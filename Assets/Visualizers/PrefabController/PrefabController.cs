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
using System.Collections.Generic;
using System.Linq;
using Chains;
using JetBrains.Annotations;
using Mutation;
using UnityEngine;
using Visualizers.IsoGrid;

namespace Visualizers.PrefabController
{
    public class PrefabController : VisualizerController
    {
        private MutableField<string> m_PrefabType = new MutableField<string> { LiteralValue = "Sphere" };
        [Controllable(LabelText = "Type of Prefab to Instantiate", ValidValuesListName = "PrefabNames")]
        public MutableField<string> PrefabType
        {
            get { return m_PrefabType; }
        }

        [UsedImplicitly]
        private List<string> PrefabNames { get { return PrefabFactory.Instance.Prefabs.Keys.ToList(); } }

        private MutableTarget m_ChildBoundNameTarget = new MutableTarget() 
        { AbsoluteKey = "Bound Name" };
        [Controllable(LabelText = "Child Bound Name Target")]
        public MutableTarget ChildBoundNameTarget { get { return m_ChildBoundNameTarget; } }

        public SelectionState DefaultState { get { return Router["Default"]; } }

        public SelectionState PerChildState { get { return Router[ "Per Child" ]; } }

        public PrefabController()
        {
            Router.AddSelectionState( "Default" );
            Router.AddSelectionState( "Per Child" );
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            DefaultState.TransmitSchema( newSchema );

            ChildBoundNameTarget.SetValue( "Child Name", newSchema );

            PerChildState.TransmitSchema( newSchema );
        }

        protected override IEnumerator ProcessPayload(VisualPayload payload)
        {
            foreach (var entry in PrefabType.GetEntries(payload.Data))
            {
                var prefabType = PrefabType.GetValue(entry);

                var newVisualizer = VisualizerFactory.InstantiatePrefabVisualizer( prefabType );

                var newBound = newVisualizer.GetComponent<BoundingBox>();
                if (newBound == null)
                    newBound = newVisualizer.gameObject.AddComponent<BoundingBox>();

                newVisualizer.Initialize(this, payload);

                foreach (var discriminateChild in newVisualizer.GetComponentsInChildren<PrefabDiscriminateChild>())
                {
                    var childBound = discriminateChild.FulfillValueDiscrimination();

                    ChildBoundNameTarget.SetValue(discriminateChild.ChildName, payload.Data);

                    var childPayload = new VisualPayload(payload.Data, new VisualDescription(childBound));

                    var iterator = PerChildState.Transmit(childPayload);
                    while (iterator.MoveNext())
                        yield return null;
                }

                var newPayload = new VisualPayload(payload.Data, new VisualDescription(newBound));

                var defaultIterator = DefaultState.Transmit(newPayload);
                while (defaultIterator.MoveNext())
                    yield return null;
            }

        }
    }
}
