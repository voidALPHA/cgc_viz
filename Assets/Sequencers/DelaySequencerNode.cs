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
using Mutation;
using UnityEngine;
using Utility.JobManagerSystem;
using Visualizers;

namespace Sequencers
{
    public class DelaySequencerNode : SequencerNode
    {
        private MutableField<float> m_DelayTime = new MutableField<float>() 
        { LiteralValue = 1f };
        [Controllable(LabelText = "Delay Time")]
        public MutableField<float> DelayTime { get { return m_DelayTime; } }


        private List<uint> m_DelayJobs = new List<uint>();
        private List<uint> DelayJobs { get { return m_DelayJobs; } set { m_DelayJobs = value; } }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            float targetTime = Time.time + DelayTime.GetFirstValue( payload.Data );

            DelayJobs.Add(JobManager.Instance.StartJob(
                Delay(payload, targetTime), jobName: "Delay", startImmediately: true, maxExecutionsPerFrame: 1));    // mex is 1 for testing
           // if (JobManager.Instance.IsJobRegistered(jobId))
           //     yield return null;

            yield break;
        }
        
        public override void Unload()
        {
            foreach (var job in DelayJobs)
                JobManager.Instance.CancelJob(job, true);

            DelayJobs.Clear();

            base.Unload();
        }


        private IEnumerator Delay(VisualPayload payload, float targetTime)
        {
            while (Time.time < targetTime)
                yield return null;

            var iterator = Router.TransmitAll(payload);
            while (iterator.MoveNext())
                yield return null;
        }
    }
}
