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
using JetBrains.Annotations;
using Mutation;
using Utility.JobManagerSystem;
using Visualizers;

namespace Sequencers
{
    public class SequencedRepeatPair
    {
        private bool m_RepeatPairIsCompleted = false;
        public bool RepeatPairIsCompleted { get { return m_RepeatPairIsCompleted;} set { m_RepeatPairIsCompleted = value; } }
        public Func< bool > PerStepFuncComplete { get; set; }
        public Action ConcludeFunc { get; set; }
    }

    public abstract class WaitSequencer : SequencerNode
    {
        //[Controllable, UsedImplicitly]
        //protected void Conclude()
        //{
        //    ForceConclude = true;
        //}
        
        //private bool ForceConclude { get; set; }

        //private IEnumerable PutThisSomewhere()
        //{
        //    if ( ForceConclude )
        //    {
        //        ConcludeSequence( groupId );
        //        ForceConclude = false;
        //        yield break;
        //    }
        //}


        private MutableField<string> m_GroupId = new MutableField<string>()
        { LiteralValue = "Unwait" };
        [Controllable(LabelText = "Shared Unwait Id")]
        public MutableField<string> GroupId
        {
            get { return m_GroupId; }
        }

        private static NodeDataShare<List<SequencedRepeatPair>> m_WaitingTransmissions
            //Queue< TransmitWaitPair > > m_WaitingTransmissions
            = new NodeDataShare<List<SequencedRepeatPair>>();

        public static NodeDataShare<List<SequencedRepeatPair>> WaitingTransmissions
        {
            get { return m_WaitingTransmissions; }
            set { m_WaitingTransmissions = value; }
        }

        protected SelectionState BeginSequenceState { get { return Router["Begin Sequence"]; } }
        protected SelectionState PerSequenceState { get { return Router["Per Step"]; } }
        protected SelectionState EndSequenceState { get { return EndState; } }

        protected WaitSequencer()
        {
            Router.AddSelectionState("Begin Sequence", "Main group");
            Router.AddSelectionState("Per Step", "Per Step");
        }

        protected abstract SequencedRepeatPair ProduceTransmissionFunc( VisualPayload payload );
        
        protected Action ProduceDefaultConclude( VisualPayload payload )
        {
            return ()=>JobManager.Instance.StartJob(
                EndSequenceState.Transmit( payload ), jobName: "Conclude", startImmediately: true,
                maxExecutionsPerFrame: 100);
        }

        protected Func< bool > ProduceDefaultPerStep( VisualPayload payload )
        {
            return ()=>
            {
                JobManager.Instance.StartJob(
                    PerSequenceState.Transmit( payload ), jobName: "Per Step", startImmediately: true,
                    maxExecutionsPerFrame: 100 );
                return false;
            };
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var groupId = GroupId.GetFirstValue( payload.Data );

            if (!WaitingTransmissions.ContainsKey( groupId ))
                WaitingTransmissions[groupId] = new List<SequencedRepeatPair>();
            
            WaitingTransmissions[groupId].Add(ProduceTransmissionFunc(payload));

            var iterator = BeginSequenceState.Transmit(payload);
            while (iterator.MoveNext())
                yield return null;

            //start sequence
            UnwaitGroupId( groupId );
        }

        // unwait one groupID
        public static void UnwaitGroupId(string groupId)
        {
            if (!WaitingTransmissions.ContainsKey(groupId))
                return;

            var waitingList = WaitingTransmissions[groupId];
            for (int i = 0; i < waitingList.Count; i++)
            {
                if ( waitingList[ i ].RepeatPairIsCompleted )
                {
                    waitingList[ i ].ConcludeFunc();
                    WaitingTransmissions[groupId].Remove(waitingList[i--]);
                    continue;
                }

                if ( waitingList[ i ].PerStepFuncComplete() )
                    waitingList[ i ].RepeatPairIsCompleted = true;
            }

            if (!WaitingTransmissions.ContainsKey(groupId))
                return;
            
            if (WaitingTransmissions[groupId].Count == 0)
                WaitingTransmissions.Remove(groupId);
        }

        // conclude a sequence
        public static void ConcludeSequence( string groupId )
        {
            if ( !WaitingTransmissions.ContainsKey( groupId ) )
                return;

            for ( int i = 0; i < WaitingTransmissions[ groupId ].Count; i++ )
            {
                WaitingTransmissions[ groupId ][ i ].ConcludeFunc();
            }

            WaitingTransmissions.Remove( groupId );
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            BeginSequenceState.TransmitSchema( newSchema );
            EndSequenceState.TransmitSchema( newSchema );
            ProcessPerStepSchema( newSchema );
        }

        protected abstract void ProcessPerStepSchema( MutableObject newSchema );

        public override void Unload()
        {
            WaitingTransmissions.Clear();

            base.Unload();
        }
        
        //protected static IEnumerator TransmitPair(TransmitWaitPair transmit)
        //{
        //    return transmit.TransmitState.Transmit(transmit.Payload);
        //}

    }
}