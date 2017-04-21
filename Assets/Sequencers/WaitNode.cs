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
using Chains;
using Mutation;
using Mutation.Mutators;
using Utility.JobManagerSystem;
using Visualizers;

namespace Sequencers
{

    public class TransmitWaitPair
    {
        public TransmitWaitPair(VisualPayload payload, SelectionState transmitState)
        {
            Payload = payload;
            TransmitState = transmitState;
        }

        public VisualPayload Payload { get; set; }
        public SelectionState TransmitState { get; set; }
    }

    public class WaitNode : Mutator
    {
        private MutableField<string> m_GroupId = new MutableField<string>() { LiteralValue = "" };
        [Controllable(LabelText = "Shared Group Id")]
        public MutableField<string> GroupId
        {
            get { return m_GroupId; }
        }

        private static NodeDataShare<List<TransmitWaitPair>> _mWaitingTransmissions
            = new NodeDataShare<List<TransmitWaitPair>>();
        private static NodeDataShare<List<TransmitWaitPair>> WaitingTransmissions
        { get { return _mWaitingTransmissions; } set { _mWaitingTransmissions = value; } }

        public static void UnwaitGroupId(String groupId)
        {
            if (ActivatedGroupIDs.Contains(groupId))
                return;

            ActivatedGroupIDs.Add(groupId);

            if ( WaitingTransmissions.ContainsKey( groupId ) )
            {
                foreach ( var transmit in WaitingTransmissions[ groupId ] )
                {
                    //var jobId = 
                    JobManager.Instance.StartJob(
                        TransmitPair( transmit ), jobName: "Delay", startImmediately: true, maxExecutionsPerFrame: 1 );
                        // mex is 1 for testing
                }
            }

            WaitingTransmissions[groupId] = new List<TransmitWaitPair>();
        }

        private static HashSet<string> ActivatedGroupIDs = new HashSet<string>();

        private static IEnumerator TransmitPair(TransmitWaitPair transmit)
        {
            return transmit.TransmitState.Transmit(transmit.Payload);
        }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var groupId = GroupId.GetFirstValue(payload.Data);

            if (ActivatedGroupIDs.Contains(groupId))
            {
                JobManager.Instance.StartJob(
                    TransmitPair(new TransmitWaitPair(payload, DefaultState)),
                    jobName: "Delay", startImmediately: true, maxExecutionsPerFrame: 1); // mex is 1 for testing
                yield break;
            }

            if (!WaitingTransmissions.ContainsKey(groupId))
                WaitingTransmissions[groupId] = new List<TransmitWaitPair>();

            WaitingTransmissions[groupId].Add(new TransmitWaitPair(payload, DefaultState));

            
        }

        public override void Unload()
        {
            ActivatedGroupIDs.Clear();

            WaitingTransmissions.Clear();

            base.Unload();
        }
    }
}
