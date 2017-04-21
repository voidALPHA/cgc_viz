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
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Chains;
using UnityEngine;
using Utility.Undo;
using Visualizers;

namespace Mutation.Mutators.SocketConnection
{
    public class ConnectToDataSharedSocketMutator : ChainNode
    {
        public static NodeDataShare<Socket> DataShare { get; set; }

        private MutableField<string> m_GroupId = new MutableField<string>() 
        { LiteralValue = "Group ID" };
        [Controllable(LabelText = "Group Id")]
        public MutableField<string> GroupId { get { return m_GroupId; } }

        private MutableField<string> m_TargetAddress = new MutableField<string>() 
        { LiteralValue = "127.0.0.1" };
        [Controllable(LabelText = "Target Address")]
        public MutableField<string> TargetAddress { get { return m_TargetAddress; } }

        private MutableField<int> m_TargetPort = new MutableField<int>() 
        { LiteralValue = 9999 };
        [Controllable(LabelText = "Target Port")]
        public MutableField<int> TargetPort { get { return m_TargetPort; } }
        

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            if (DataShare == null)
                DataShare = new NodeDataShare< Socket >();

            var IPs = Dns.GetHostAddresses( TargetAddress.GetFirstValue( payload.Data ) );

            var newSocket = new Socket( 
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            try
            {
                newSocket.Connect( IPs.First(), TargetPort.GetFirstValue( payload.Data ) );

                DataShare[ GroupId.GetFirstValue( payload.Data ) ] = newSocket;

            }
            catch ( Exception e )
            {
                Debug.LogError( "Could not connect to socket!  Error: " + e );
            }

            yield break;
        }

        public override NodeDelete PrepareForDestruction(bool recurse)
        {
            if ( DataShare.Any() )
            {
                foreach ( var kvp in DataShare )
                {
                    kvp.Value.Close();
                }
                DataShare.Clear();
            }
            return base.PrepareForDestruction(recurse);
        }
    }
}
