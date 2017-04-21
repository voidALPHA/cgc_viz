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
using System.Text;
using Chains;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.SocketConnection
{
    public class SendStringThroughDataSharedSocketMutator : ChainNode
    {
        private MutableField<string> m_GroupId = new MutableField<string>() 
        { LiteralValue = "Group ID" };
        [Controllable(LabelText = "Group Id")]
        public MutableField<string> GroupId { get { return m_GroupId; } }


        private MutableField<string> m_DataToSend = new MutableField<string>() 
        { LiteralValue = "New Data" };
        [Controllable(LabelText = "Data To Send")]
        public MutableField<string> DataToSend { get { return m_DataToSend; } }


        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var groupId = GroupId.GetFirstValue( payload.Data );

            if ( !ConnectToDataSharedSocketMutator.DataShare.ContainsKey( groupId ) )
            {
                Debug.LogError( "Error: could not find socket with group ID " + groupId + " to transmit through." );
                yield break;
            }

            var socket = ConnectToDataSharedSocketMutator.DataShare[ groupId ];

            foreach ( var entry in DataToSend.GetEntries( payload.Data ) )
            {
                var data = DataToSend.GetValue( entry );

                socket.Send( Encoding.ASCII.GetBytes( data) );
            }


        }
    }
}
