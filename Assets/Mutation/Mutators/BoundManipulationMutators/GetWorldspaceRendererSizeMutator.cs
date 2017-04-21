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
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.BoundManipulationMutators
{
    public class GetWorldspaceRendererSizeMutator : Mutator
    {
        private MutableTarget m_ScaleTarget = new MutableTarget() { AbsoluteKey = "Size Vector" };
        [Controllable(LabelText = "Size Target")]
        public MutableTarget ScaleTarget { get { return m_ScaleTarget; } }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var combinedBounds = new UnityEngine.Bounds(payload.VisualData.Bound.transform.position, Vector3.zero);

            foreach ( var rend in payload.VisualData.Bound.GetComponentsInChildren< Renderer >() )
            {
                combinedBounds.Encapsulate( rend.bounds );
            }

            ScaleTarget.SetValue(combinedBounds.size, payload.Data);

            var iterator = Router.TransmitAll(payload);
            while (iterator.MoveNext())
                yield return null;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            ScaleTarget.SetValue(Vector3.zero, newSchema);

            Router.TransmitAllSchema(newSchema);
        }
    }
}
