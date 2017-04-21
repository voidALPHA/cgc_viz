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
using Chains;

namespace Mutation.Mutators.IfMutator
{
    public abstract class IfMutator : ChainNode
    {
        protected abstract bool MeetsCriterion( VisualPayload payload );

        public SelectionState IfState { get { return Router["If"]; } }
        public SelectionState ElseState { get { return Router[ "Else" ]; } }

        public IfMutator()
        {
            Router.AddSelectionState( "If", "MainGroup" );
            Router.AddSelectionState("Else", "MainGroup");
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            IEnumerator iterator = MeetsCriterion( payload ) ? 
                IfState.Transmit( payload ) 
                : ElseState.Transmit( payload );

            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
