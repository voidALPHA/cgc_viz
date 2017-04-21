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
using Visualizers;

namespace Mutation.Mutators.Enumeration
{
    public class GetCurrentBoundMutator : Mutator
    {
        private MutableTarget m_BoundListTarget = new MutableTarget { AbsoluteKey = "Bounds" };
        [Controllable( LabelText = "Bound List Target" )]
        private MutableTarget BoundListTarget { get { return m_BoundListTarget; } }


        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var bound = payload.VisualData.Bound;

            BoundListTarget.SetValue( new List< BoundingBox > { bound }, payload.Data );

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            
            BoundListTarget.SetValue( new List< BoundingBox >(), newSchema );

            Router.TransmitAllSchema( newSchema );
        }
    }
}