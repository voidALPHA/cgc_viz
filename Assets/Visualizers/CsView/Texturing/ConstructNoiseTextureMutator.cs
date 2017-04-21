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

using System.Collections;
using Mutation;
using Mutation.Mutators;
using UnityEngine;
using Utility;

namespace Visualizers.CsView.Texturing
{
    public class ConstructNoiseTextureMutator : Mutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableTarget m_TextureTarget = new MutableTarget() 
        { AbsoluteKey = "Noise Texture" };
        [Controllable(LabelText = "Texture Target")]
        public MutableTarget TextureTarget { get { return m_TextureTarget; } }


        private MutableField<ColorGradient> m_BandingGradient = new MutableField<ColorGradient>() 
        { AbsoluteKey = "Banding Gradient"};
        [Controllable(LabelText = "Banding Gradient")]
        public MutableField<ColorGradient> BandingGradient { get { return m_BandingGradient; } }

        private MutableField<int> m_TextureSize = new MutableField<int>() 
        { LiteralValue = 1024 };
        [Controllable(LabelText = "Texture Size")]
        public MutableField<int> TextureSize { get { return m_TextureSize; } }

        private MutableField<int> m_LowerOctave = new MutableField<int>() 
        { LiteralValue = 3 };
        [Controllable(LabelText = "Lower Octave")]
        public MutableField<int> LowerOctave { get { return m_LowerOctave; } }

        private MutableField<int> m_UpperOctave = new MutableField<int>() 
        { LiteralValue = 6 };
        [Controllable(LabelText = "Upper Octave")]
        public MutableField<int> UpperOctave { get { return m_UpperOctave; } }

        private MutableField<float> m_Persistence = new MutableField<float>() 
        { LiteralValue = .5f };
        [Controllable(LabelText = "Persistence")]
        public MutableField<float> Persistence { get { return m_Persistence; } }

        private MutableField<int> m_BitDepth = new MutableField<int>() 
        { LiteralValue = 64 };
        [Controllable(LabelText = "Bit Depth")]
        public MutableField<int> BitDepth { get { return m_BitDepth; } }



        public ConstructNoiseTextureMutator()
        {
            TextureTarget.SchemaParent = Scope;
            BandingGradient.SchemaPattern = Scope;
            TextureSize.SchemaPattern = Scope;
            LowerOctave.SchemaPattern = Scope;
            UpperOctave.SchemaPattern = Scope;
            Persistence.SchemaPattern = Scope;
            BitDepth.SchemaPattern = Scope;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            foreach (var entry in Scope.GetEntries( newSchema ))
                TextureTarget.SetValue( new Texture2D(1,1), entry );
            
            Router.TransmitAllSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach ( var entry in Scope.GetEntries( payload.Data ) )
            {
                var noiseTexture = ConstructNoiseTexture.GenerateNoiseTexture(
                    TextureSize.GetValue( entry ),
                    LowerOctave.GetValue( entry ),
                    UpperOctave.GetValue( entry ),
                    Persistence.GetValue( entry ),
                    BandingGradient.GetValue( entry ),
                    BitDepth.GetValue(entry),
                    1f );

                TextureTarget.SetValue( noiseTexture, entry );
            }

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
