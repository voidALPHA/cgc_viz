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
using Choreography.Steps;
using Mutation;
using Mutation.Mutators;
using Visualizers;

namespace Sequencers
{
    public class UnWaitNode : Mutator
    {
        private MutableField<string> m_GroupId = new MutableField<string>() 
        { LiteralValue = "Unwait" };
        [Controllable(LabelText = "GroupId")]
        public MutableField<string> GroupId { get { return m_GroupId; } }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            WaitStep.UnwaitGroupId( GroupId.GetFirstValue( payload.Data ) );

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
