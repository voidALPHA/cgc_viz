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
using Chains;
using Mutation;
using UnityEngine;
using Utility;
using Visualizers;

namespace Experimental
{
    public class ExperimentalListOfBoundNumbers : ChainNode
    {
        private MutableField<int> m_NumberOfBounds = new MutableField<int>() 
        { LiteralValue = 4 };
        [Controllable(LabelText = "Number Of Bounds")]
        public MutableField<int> NumberOfBounds { get { return m_NumberOfBounds; } }

        private MutableTarget m_BoundsTarget = new MutableTarget() 
        { AbsoluteKey = "BoundsList" };
        [Controllable(LabelText = "Bounds List Target")]
        public MutableTarget BoundsTarget { get { return m_BoundsTarget; } }
        
        public SelectionState DefaultState { get { return Router["Default"]; } }

        public ExperimentalListOfBoundNumbers()
        {
            Router.AddSelectionState( "Default" );
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            Router.TransmitAllSchema( AddBoundsList( newSchema ) );
        }

        private MutableObject AddBoundsList( MutableObject mutable )
        {
            var boundsList = new List<BoundingBox>();
            for (int i = 0; i < NumberOfBounds.GetFirstValue(mutable); i++)
            {
                var newBound = BoundingBox.ConstructBoundingBox("Bound " + (i + 1),
                    Vector3.right * i,
                    Quaternion.identity,
                    Vector3.one);

                //newBound.Data.Add("BoundNumber", i);
                boundsList.Add(newBound);
            }

            BoundsTarget.SetValue(boundsList, mutable);

            return mutable;
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            payload.Data = AddBoundsList( payload.Data );

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
