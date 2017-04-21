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
using UnityEngine;
using Utility;
using Visualizers;

namespace Mutation.Mutators.BoundManipulationMutators
{
    public class RotateBoundToEulerAngles : Mutator
    {
        private MutableField<float> m_XEuler = new MutableField<float>() 
        { LiteralValue = 0f};
        [Controllable(LabelText = "X Euler")]
        public MutableField<float> XEuler { get { return m_XEuler; } }

        private MutableField<float> m_YEuler = new MutableField<float>() { LiteralValue = 0f };
        [Controllable(LabelText = "Y Euler")]
        public MutableField<float> YEuler { get { return m_YEuler; } }

        private MutableField<float> m_ZEuler = new MutableField<float>() { LiteralValue = 0f };
        [Controllable(LabelText = "Z Euler")]
        public MutableField<float> ZEuler { get { return m_ZEuler; } }


        private MutableField<bool> m_PostOperation = new MutableField<bool>() { LiteralValue = true };
        [Controllable(LabelText = "Post Operation")]
        public MutableField<bool> PostOperation { get { return m_PostOperation; } }


        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var newBound = payload.VisualData.Bound.CreateDependingBound(Name);
            //var bound = payload.VisualData.Bound;

            var isPreOperation = !PostOperation.GetFirstValue(payload.Data);

            if (isPreOperation)
            {
                newBound.transform.parent = payload.VisualData.Bound.transform.parent;

                payload.VisualData.Bound.transform.parent = newBound.transform;
            }

            //newBound.transform.parent = payload.VisualData.Bound.transform.parent;
            //payload.VisualData.Bound.transform.parent = newBound.transform;

            newBound.transform.localRotation *= Quaternion.Euler(
                XEuler.GetFirstValue(payload.Data),
                YEuler.GetFirstValue(payload.Data),
                ZEuler.GetFirstValue(payload.Data));

            VisualPayload newPayload = payload;
            if (!isPreOperation)
                newPayload = new VisualPayload(payload.Data, new VisualDescription(newBound));

            var iterator = Router.TransmitAll(newPayload);
            while (iterator.MoveNext())
                yield return null;
        }
    }
}
