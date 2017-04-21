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
using Utility;
using Visualizers;

namespace Mutation.Mutators.VisualModifiers
{
    [UsedImplicitly]
    public class BillboardMutator : Mutator
    {
        private MutableField<bool> m_CameraFacing = new MutableField<bool>() 
        { LiteralValue = false};
        [Controllable(LabelText = "Camera Facing")]
        public MutableField<bool> CameraFacing { get { return m_CameraFacing; } }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var newBound = payload.VisualData.Bound.CreateDependingBound(Name);
            //var bound = payload.VisualData.Bound;

            newBound.transform.parent = payload.VisualData.Bound.transform.parent;

            payload.VisualData.Bound.transform.parent = newBound.transform;

            var cameraFacer = newBound.gameObject.AddComponent<CameraFacingObject>();

            cameraFacer.CameraFacing = CameraFacing.GetFirstValue(payload.Data);
            
            VisualPayload newPayload = new VisualPayload(payload.Data, new VisualDescription(newBound));

            var iterator = Router.TransmitAll(newPayload);
            while (iterator.MoveNext())
                yield return null;

        }
    }
}
