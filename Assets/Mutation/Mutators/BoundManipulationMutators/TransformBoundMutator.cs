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
using Utility;
using Visualizers;

namespace Mutation.Mutators.BoundManipulationMutators
{
    public class TransformBoundMutator : Mutator
    {
        private MutableField<Vector3> m_ScaleMultiplier = new MutableField<Vector3>()
        {LiteralValue = Vector3.one};
        [Controllable(LabelText = "Scale Multiplier")]
        public MutableField<Vector3> ScaleMultiplier
        {
            get { return m_ScaleMultiplier; }
        }

        private MutableField<Quaternion> m_RotationMultiplier = new MutableField<Quaternion>()
        {LiteralValue = Quaternion.identity};
        [Controllable(LabelText = "Rotation Offset")]
        public MutableField<Quaternion> RotationMultiplier
        {
            get { return m_RotationMultiplier; }
        }

        private MutableField<Vector3> m_TranslationOffset = new MutableField<Vector3>()
        {LiteralValue = Vector3.zero};
        [Controllable(LabelText = "Translation Offset")]
        public MutableField<Vector3> TranslationOffset
        {
            get { return m_TranslationOffset; }
        }

        private MutableField<bool> m_PostOperation = new MutableField<bool>() 
        { LiteralValue = true };
        [Controllable(LabelText = "Post Operation?")]
        public MutableField<bool> PostOperation { get { return m_PostOperation; } }

        private MutableField<bool> m_LocalSpace = new MutableField<bool>()
        { LiteralValue = true };
        [Controllable(LabelText = "Local Space?")]
        public MutableField<bool> LocalSpace { get { return m_LocalSpace; } }


        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var newBound = payload.VisualData.Bound.CreateDependingBound(Name);
            //var bound = payload.VisualData.Bound;

            var isPreOperation = !PostOperation.GetFirstValue( payload.Data );

            if ( isPreOperation )
            {
                newBound.transform.parent = payload.VisualData.Bound.transform.parent;

                payload.VisualData.Bound.transform.parent = newBound.transform;
            }

            //newBound.transform.parent = payload.VisualData.Bound.transform.parent;
            //payload.VisualData.Bound.transform.parent = newBound.transform;

            if (LocalSpace.GetFirstValue(payload.Data))
            {
                newBound.transform.localScale =
                    newBound.transform.localScale.PiecewiseMultiply(ScaleMultiplier.GetFirstValue(payload.Data).MinAtEpsilon());

                newBound.transform.position +=
                    payload.VisualData.Bound.transform.PiecewiseMultiply(TranslationOffset.GetFirstValue(payload.Data));
            }
            else
            {
                newBound.transform.localScale = ScaleMultiplier.GetFirstValue(payload.Data).MinAtEpsilon();

                newBound.transform.position += TranslationOffset.GetFirstValue(payload.Data);
            }

            // rotation is already a quaternion, so it's already localized or not
            newBound.transform.localRotation = RotationMultiplier.GetFirstValue(payload.Data);

            VisualPayload newPayload = payload;
            if (!isPreOperation)
                newPayload = new VisualPayload( payload.Data, new VisualDescription( newBound ) );

            var iterator = Router.TransmitAll(newPayload);
            while (iterator.MoveNext())
                yield return null;
        }
    }
}
