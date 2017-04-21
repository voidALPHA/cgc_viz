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
using UnityEngine;
using Utility.JobManagerSystem;

namespace Utility.PerfMetricSystem
{
    public class PerfMetric : MonoBehaviour
    {
        public static PerfMetric Instance { get; private set; }

        public class FrameMetric
        {
            public float unscaledDeltaTime;
            public int unityFrameCount;
            public int numJobsRegistered;
            public int numJobsRunning;
            public long timeSpentInJobsTicks;
        }

        private const int MaxFrames = 1000; // At 60 fps this is 16.67 seconds

        private List<FrameMetric> m_FramesBuffer = new List< FrameMetric >();
        private List<FrameMetric> FramesBuffer { get { return m_FramesBuffer; } set { m_FramesBuffer = value; } }

        private int CurFrameIndex { get; set; }
        private bool WrappedAround { get; set; }

        private bool m_IsMeasuring;
        private bool IsMeasuring
        {
            get { return m_IsMeasuring; }
            set
            {
                RecordingIndicator.enabled = RecordingIndicatorDisplayOverride ? value : false;
                m_IsMeasuring = value;
            }
        }

        private Canvas m_RecordingIndicator;
        private Canvas RecordingIndicator { get { return m_RecordingIndicator; } set { m_RecordingIndicator = value; } }

        private bool m_RecordingIndicatorDisplayOverride = true;
        public bool RecordingIndicatorDisplayOverride
        {
            get
            {
                return m_RecordingIndicatorDisplayOverride;
            }
            set
            {
                if ( value && IsMeasuring )
                    RecordingIndicator.enabled = true;
                else if ( !value )
                    RecordingIndicator.enabled = false;

                m_RecordingIndicatorDisplayOverride = value;
            }
        }


        private void Awake()
        {
            Instance = this;

            RecordingIndicator = GetComponentInChildren<Canvas>();
            RecordingIndicator.enabled = false;

            for (int i = 0; i < MaxFrames; i++) // Is there a better way to do this?  (Initialize a fixed-size list with initialized objects?)
                FramesBuffer.Add(new FrameMetric());
        }

        private void Update()
        {
            if ( IsMeasuring )
            {
                var frame = FramesBuffer[ CurFrameIndex ];

                frame.unscaledDeltaTime = Time.unscaledDeltaTime;
                frame.unityFrameCount = Time.frameCount;

                var jm = JobManager.Instance;
                frame.numJobsRegistered = jm.NumJobsRegistered;
                frame.numJobsRunning = jm.NumJobsRunning;
                frame.timeSpentInJobsTicks = jm.TimeSpentInJobsTicks;

                if ( ++CurFrameIndex >= MaxFrames )
                {
                    CurFrameIndex = 0;
                    WrappedAround = true;
                }
            }
        }

        public bool StartMeasuring()
        {
            if ( IsMeasuring )
                return false;

            IsMeasuring = true;
            CurFrameIndex = 0;
            WrappedAround = false;

            return true;
        }

        public bool RestartMeasuring()
        {
            StopMeasuring();
            StartMeasuring();

            return true;    // Restart always works
        }

        public bool StopMeasuring()
        {
            if ( !IsMeasuring )
                return false;

            IsMeasuring = false;

            return true;
        }

        public void Dump(bool dumpAll = false)
        {
            var numFrames = WrappedAround ? MaxFrames : CurFrameIndex;
            var frameIndex = WrappedAround ? CurFrameIndex : 0;
            var totalDeltaTime = 0.0f;
            var longestDeltaTime = 0.0f;

            for ( int i = 0; i < numFrames; i++ )
            {
                var frame = FramesBuffer[ frameIndex ];

                totalDeltaTime += frame.unscaledDeltaTime;
                if ( frame.unscaledDeltaTime > longestDeltaTime )
                    longestDeltaTime = frame.unscaledDeltaTime;

                if ( dumpAll )
                {
                    var line = string.Format( "Frame:{0,3}  FrameCount:{1,5}  Duration:{2,6:N2}ms  Jobs:{3,2} ({4,2} running)", //"  TimeInJobs:{5,6:N2}ms",
                        i, frame.unityFrameCount, frame.unscaledDeltaTime * 1000.0f, frame.numJobsRegistered, frame.numJobsRunning /*,
                        ((float)frame.timeSpentInJobsTicks / (float)JobManager.Instance.TicksPerMS)*/);
                    Debug.Log(line);
                }

                if ( ++frameIndex >= MaxFrames )
                    frameIndex = 0;
            }

            Debug.Log( "Total frames recorded: " + numFrames + "; Total time: " + totalDeltaTime + " seconds; Ave fps: " + ((float)numFrames / totalDeltaTime) + "; Ave mspf: " + (totalDeltaTime / (float)numFrames * 1000.0f));
            Debug.Log( "Longest frame: " + longestDeltaTime * 1000.0f + " ms" );
            if (WrappedAround)
                Debug.Log("Note that the buffer overflowed and thus wrapped around; data above is for the LAST " + numFrames + " frames");
        }
    }
}

