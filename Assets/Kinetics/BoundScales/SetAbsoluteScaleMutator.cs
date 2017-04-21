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
using Mutation;
using Mutation.Mutators;
using UnityEngine;
using Utility;
using Visualizers;

namespace Kinetics.BoundScales
{
    public class SetAbsoluteScaleMutator : Mutator
    {
        private MutableField<Vector3> m_AbsoluteScale = new MutableField<Vector3>() 
        { LiteralValue = Vector3.one };
        [Controllable(LabelText = "Absolute Scale")]
        public MutableField<Vector3> AbsoluteScale { get { return m_AbsoluteScale; } }

        private MutableField<bool> m_PostOperation = new MutableField<bool>() { LiteralValue = true };
        [Controllable(LabelText = "Post Operation")]
        public MutableField<bool> PostOperation { get { return m_PostOperation; } }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var newBound = payload.VisualData.Bound.CreateDependingBound( Name );
            //var bound = payload.VisualData.Bound;

            var isPreOperation = !PostOperation.GetFirstValue( payload.Data );

            if ( isPreOperation )
            {
                newBound.transform.parent = payload.VisualData.Bound.transform.parent;

                payload.VisualData.Bound.transform.parent = newBound.transform;
            }

            var scaleToSet = AbsoluteScale.GetFirstValue( payload.Data );

            if ( Mathf.Abs( scaleToSet.x ) < .000001f || Mathf.Abs( scaleToSet.y ) < .000001f ||
                 Mathf.Abs( scaleToSet.z ) < .000001f )
            {
                throw new Exception("Shouldn't be setting scale to zeroish!");
            }

        newBound.transform.SetWorldScale( scaleToSet );

            VisualPayload newPayload = payload;
            if (!isPreOperation)
                newPayload = new VisualPayload(payload.Data, new VisualDescription(newBound));

            var iterator = Router.TransmitAll( newPayload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
