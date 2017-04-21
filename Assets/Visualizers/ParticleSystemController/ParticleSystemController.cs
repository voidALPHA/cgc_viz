// Copyright 2017 voidALPHA, Inc.
// This file is part of the Haxxis video generation system and is provided
// by voidALPHA in support of the Cyber Grand Challenge.
// Haxxis is free software: you can redistribute it and/or modify it under the terms
// of the GNU General Public License as published by the Free Software Foundation.
// Haxxis is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
// PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with
// Haxxis. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Visualizers.ParticleSystemController;
using Chains;
using Mutation;
using UnityEngine;
using Visualizers.IsoGrid;

namespace Visualizers.ParticleSystemController
{
    public class ParticleSystemController : VisualizerController
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<string> m_ParticleSystemPrefabName = new MutableField<string>() 
        { LiteralValue = "Default" };
        [Controllable(LabelText = "System Prefab Name")]
        public MutableField<string> ParticleSystemPrefabName { get { return m_ParticleSystemPrefabName; } }

        private MutableField<Color> m_MainColor = new MutableField<Color>() 
        { LiteralValue = Color.white };
        [Controllable(LabelText = "MainColor")]
        public MutableField<Color> MainColor { get { return m_MainColor; } }

        private MutableField<float> m_LifeTime = new MutableField<float>() 
        { LiteralValue = 1f };
        [Controllable(LabelText = "Life Time")]
        public MutableField<float> LifeTime { get { return m_LifeTime; } }

        private MutableField<float> m_EmitRate = new MutableField<float>() 
        { LiteralValue = 1.0f };
        [Controllable(LabelText = "Emit Rate")]
        public MutableField<float> EmitRate { get { return m_EmitRate; } }

        private MutableField<bool> m_OverrideColorGradient = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "Override Color Gradient")]
        public MutableField<bool> OverrideColorGradient { get { return m_OverrideColorGradient; } }




        public SelectionState DefaultState { get { return Router["Default"]; } }

        public SelectionState PerParticleSystem { get { return Router["PerParticleSystem"]; } }

        public ParticleSystemController()
        {
            ParticleSystemPrefabName.SchemaParent = Scope;
            MainColor.SchemaParent = Scope;
            LifeTime.SchemaParent = Scope;
            EmitRate.SchemaParent = Scope;
            OverrideColorGradient.SchemaParent = Scope;

            Router.AddSelectionState("Default");
            Router.AddSelectionState("PerParticleSystem");
        }


        protected override IEnumerator ProcessPayload(VisualPayload payload)
        {
            foreach (var entry in Scope.GetEntries(payload.Data))
            {
                var newSystem = ParticleSystemFactory.GenerateParticleSystem(ParticleSystemPrefabName.GetValue(entry));

                var mainColor = MainColor.GetValue(entry);

                newSystem.startColor = mainColor;

                if ( OverrideColorGradient.GetValue( entry ) )
                {
                    var newGradient = new Gradient();
                    newGradient.colorKeys = new []
                    {
                        new GradientColorKey(mainColor, 0f), 
                        new GradientColorKey(mainColor, 1f), 
                    };
                    newGradient.alphaKeys = new []
                    {
                        new GradientAlphaKey(1f, 0f), 
                        new GradientAlphaKey(1f, .15f), 
                        new GradientAlphaKey(0f, 1f)
                    };

                    var col = newSystem.colorOverLifetime;
                    col.enabled = true;
                    col.color = new ParticleSystem.MinMaxGradient(
                        newGradient);
                }


                newSystem.startLifetime = LifeTime.GetValue(entry);

                // this allegedly works, see http://forum.unity3d.com/threads/enabling-emission.364258/#post-2356966
                var emission = newSystem.emission;
                emission.rate = new ParticleSystem.MinMaxCurve( EmitRate.GetValue( entry ) );

                
                var newBound = newSystem.GetComponent<BoundingBox>();
                if (newBound == null)
                    newBound = newSystem.gameObject.AddComponent<BoundingBox>();
                
                payload.VisualData.Bound.ChildWithinBound(newBound.transform);

                var newVisualizer = newSystem.GetComponent<ParticleSystemVisualizer>();
                if (newVisualizer == null)
                    newVisualizer = newSystem.gameObject.AddComponent<ParticleSystemVisualizer>();
                
                var newPayload = new VisualPayload(entry.Last(), new VisualDescription(newBound));

                newVisualizer.Initialize(this, newPayload);

                var perParticleSystemIterator = PerParticleSystem.Transmit(newPayload);
                while (perParticleSystemIterator.MoveNext())
                    yield return null;
            }

            var defaultIterator = DefaultState.Transmit(payload);
            while (defaultIterator.MoveNext())
                yield return null;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            DefaultState.TransmitSchema(newSchema);

            var entry = Scope.GetEntries(newSchema).FirstOrDefault();
            
            PerParticleSystem.TransmitSchema(entry.LastOrDefault());
        }
    }
}
