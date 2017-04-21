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
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Utility.JobManagerSystem;

namespace Utility.SystemStats
{
    public class SystemStatsJobManager : ISystemStats
    {
        public GameObject UnityObject { get; set; }

        public Text TextComponent { get; set; }

        public string StatString { get; set; }

        private readonly long m_TicksPerMS = Stopwatch.Frequency / 1000L;

        public void Update()
        {
            var jm = JobManager.Instance;

            StatString = String.Format("Jobs:  Frame Budget:{0,5:N2}ms  Job Budget:{1,5:N2}ms  Running:{2,2}  Paused:{3,3}  Waiting:{4,2} ",
                jm.FrameBudgetMS, (float)jm.JobBudget / (float)m_TicksPerMS,
                jm.NumJobsRunning, jm.NumJobsPaused, jm.NumJobsRegistered - jm.NumJobsRunning - jm.NumJobsPaused);

            var count = jm.MaxJobsInStatsDisplay;
            if ( count > 0 )
            {
                foreach (var job in JobManager.Instance.RegisteredJobs.Keys)
                {
                    StatString += " " + job;
                    if (--count <= 0)
                        break;
                }
                var othersCount = jm.NumJobsRegistered - jm.MaxJobsInStatsDisplay;
                if ( othersCount > 0 )
                    StatString += " ...and " + othersCount + " more";
            }
        }
    }
}
