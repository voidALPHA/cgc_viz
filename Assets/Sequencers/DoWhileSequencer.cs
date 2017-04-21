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

using Mutation;
using Utility.JobManagerSystem;

namespace Sequencers
{
    public class DoWhileSequencer : WaitSequencer
    {
        protected override SequencedRepeatPair ProduceTransmissionFunc( VisualPayload payload )
        {
            var sequencedPair = new SequencedRepeatPair();
            sequencedPair.PerStepFuncComplete = () =>
            {
                JobManager.Instance.StartJob(
                    PerSequenceState.Transmit( payload ), jobName: "Per Step", startImmediately: true,
                    maxExecutionsPerFrame: 100 );

                return false;
            };

            sequencedPair.ConcludeFunc = ProduceDefaultConclude( payload );

            return sequencedPair;
        }

        protected override void ProcessPerStepSchema( MutableObject newSchema )
        {
            PerSequenceState.TransmitSchema( newSchema );
        }
    }
}
