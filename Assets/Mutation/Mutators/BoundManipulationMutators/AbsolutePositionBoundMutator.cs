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
using JetBrains.Annotations;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.BoundManipulationMutators
{
    [UsedImplicitly]
    public class AbsolutePositionBoundMutator : Mutator
    {
        private MutableField<Vector3> m_AbsolutePosition = new MutableField<Vector3>()
        {LiteralValue = Vector3.zero};
        [Controllable(LabelText = "Absolute Position")]
        public MutableField<Vector3> AbsolutePosition
        {
            get { return m_AbsolutePosition; }
        }

        private MutableField<string> m_BoundName = new MutableField<string>()
        {
            LiteralValue = ""
        };
        [Controllable(LabelText = "Bound Name"), UsedImplicitly]
        private MutableField<string> BoundName { get { return m_BoundName; } }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            BoundingBox newBound = payload.VisualData.Bound.CreateDependingBound( BoundName.GetFirstValue(payload.Data) );
                
            newBound.transform.position = AbsolutePosition.GetLastKeyValue(payload.Data);

            var newPayload = new VisualPayload(payload.Data, new VisualDescription(newBound));

            var iterator = Router.TransmitAll(newPayload);
            while (iterator.MoveNext())
                yield return null;
        }
    }
}
