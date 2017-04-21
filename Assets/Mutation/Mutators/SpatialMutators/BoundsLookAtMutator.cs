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

namespace Mutation.Mutators.SpatialMutators
{
    public class BoundsLookAtMutator : Mutator
    {
        private MutableField<Vector3> m_LookTarget = new MutableField<Vector3>() 
        { LiteralValue = Vector3.zero };
        [Controllable(LabelText = "Look At Focus")]
        public MutableField<Vector3> LookTarget { get { return m_LookTarget; } }

        private MutableField<Vector3> m_UpVector = new MutableField<Vector3>() 
        { LiteralValue = Vector3.up };
        [Controllable(LabelText = "Up Vector")]
        public MutableField<Vector3> UpVector { get { return m_UpVector; } }


        private MutableField<bool> m_AbsolutePosition = new MutableField<bool>() 
        { LiteralValue = true };
        [Controllable(LabelText = "Is Focus Absolute")]
        public MutableField<bool> AbsolutePosition { get { return m_AbsolutePosition; } }


        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            payload.VisualData.Bound.transform.LookAt( 
                AbsolutePosition.GetFirstValue(payload.Data)?
                LookTarget.GetFirstValue( payload.Data ):
                payload.VisualData.Bound.transform.position +
                payload.VisualData.Bound.transform.PiecewiseMultiply( LookTarget.GetFirstValue( payload.Data ) ),
                
                AbsolutePosition.GetFirstValue(payload.Data)?
                UpVector.GetFirstValue( payload.Data ):
                payload.VisualData.Bound.transform.position +
                payload.VisualData.Bound.transform.PiecewiseMultiply( UpVector.GetFirstValue( payload.Data ))
                );

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
