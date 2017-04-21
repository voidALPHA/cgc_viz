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
using System.Net;
using System.Net.Sockets;
using Chains;
using Sequencers;
using Visualizers;

namespace Mutation.Mutators.SocketConnection
{
    public class OpenSocketAdapter : ChainNode
    {
        public SelectionState SocketOpened { get { return Router[ "Socket Open" ]; } }
        public SelectionState ReceivedData { get { return Router["Received Data"]; } }
        
        private MutableField<int> m_Port = new MutableField<int>() 
        { LiteralValue = 9999 };
        [Controllable(LabelText = "Port")]
        public MutableField<int> Port { get { return m_Port; } }
        
        private MutableTarget m_StringDataTarget = new MutableTarget() 
        { AbsoluteKey = "String Data" };
        [Controllable(LabelText = "String Data Target")]
        public MutableTarget StringDataTarget { get { return m_StringDataTarget; } }

        protected OpenSocketAdapter()
        {
            Router.AddSelectionState("Socket Open", "Ready Group" );
            Router.AddSelectionState("Received Data", "Receiving Group");
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var ipHostInfo = Dns.GetHostEntry( "127.0.0.1" );
            var ipAddress = ipHostInfo.AddressList[ 0 ];
            var remoteEP = new IPEndPoint( ipAddress, Port.GetFirstValue( payload.Data ) );

            var newSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            //newSocket.BeginConnect(remoteEP,
            //    new AsyncCallback(ConnectCallback), client);
            
            yield break;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            SocketOpened.TransmitSchema( newSchema );

            var perStepSchema = newSchema.CloneKeys();

            StringDataTarget.SetValue( "String Data", perStepSchema);

            ReceivedData.TransmitSchema( perStepSchema );
        }
    }
}
