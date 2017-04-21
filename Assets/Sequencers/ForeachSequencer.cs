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

using System.Collections.Generic;
using System.Linq;
using Mutation;
using Utility.JobManagerSystem;
using Visualizers;

namespace Sequencers
{
    public class ForeachSequencer : WaitSequencer
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable( LabelText = "Scope" )]
        public MutableScope Scope
        {
            get { return m_Scope; }
        }

        protected override SequencedRepeatPair ProduceTransmissionFunc( VisualPayload payload )
        {
            var sequencedPair = new SequencedRepeatPair();
            
            var transmissionStack = new Queue<VisualPayload>();

            PopulateTransmissions(transmissionStack, payload);

            //var finalTransmission = new TransmitWaitPair(payload, EndSequenceState);

            if ( !transmissionStack.Any() )
            {
                sequencedPair.PerStepFuncComplete = () => true;

                sequencedPair.RepeatPairIsCompleted = true;
            }
            else
            {

                sequencedPair.PerStepFuncComplete = () =>
                {
                    var transmit = transmissionStack.Dequeue();

                    JobManager.Instance.StartJob(
                        PerSequenceState.Transmit( transmit ), jobName: "Per Step", startImmediately: true,
                        maxExecutionsPerFrame: 100 );

                    return !transmissionStack.Any();
                };
            }

            sequencedPair.ConcludeFunc = ProduceDefaultConclude( payload );

            return sequencedPair;
        }

        protected override void ProcessPerStepSchema( MutableObject newSchema )
        {
            PerSequenceState.TransmitSchema( newSchema );
        }

        protected virtual void PopulateTransmissions(Queue<VisualPayload> waitQueue, VisualPayload payload)
        {
            // ReSharper disable once UnusedVariable
            foreach (var entry in Scope.GetEntries(payload.Data))
            {
                waitQueue.Enqueue(payload);
            }
        }
    }
}
