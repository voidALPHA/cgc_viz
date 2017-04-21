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
using ChainViews;
using Choreography;
using Choreography.Steps;
using Choreography.Views;
using UnityEngine;

namespace Utility.DevCommand
{
    public class DevCommandChoreography : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "choreography";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private const string m_HelpTextBrief = "Choreography control";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Option m_StartPlayback = new DevCommandManager.Option("startPlayback", "Start playback of choreography in the currently-loaded Haxxis package", "mutuallyExclusive");
        [DevCommandOption]
        public DevCommandManager.Option StartPlayback { get { return m_StartPlayback; } set { m_StartPlayback = value; } }

        private DevCommandManager.Option m_StopPlayback = new DevCommandManager.Option("stopPlayback", "Stops playback of choreography", "mutuallyExclusive");
        [DevCommandOption]
        public DevCommandManager.Option StopPlayback { get { return m_StopPlayback; } set { m_StopPlayback = value; } }

        private DevCommandManager.Option m_OnOption = new DevCommandManager.Option("timelineOn", "Turn on choreography timeline display", "mutuallyExclusiveOnOff");
        [DevCommandOption]
        public DevCommandManager.Option OnOption { get { return m_OnOption; } set { m_OnOption = value; } }

        private DevCommandManager.Option m_OffOption = new DevCommandManager.Option("timelineOff", "Turn off choreography timeline display", "mutuallyExclusiveOnOff");
        [DevCommandOption]
        public DevCommandManager.Option OffOption { get { return m_OffOption; } set { m_OffOption = value; } }

        private DevCommandManager.Option m_InfoOption = new DevCommandManager.Option("info", "Dump information about the choreography in the currently-loaded Haxxis package");
        [DevCommandOption]
        public DevCommandManager.Option InfoOption { get { return m_InfoOption; } set { m_InfoOption = value; } }


        private TimelineViewBehaviour TimelineView { get { return TimelineViewBehaviour.Instance; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand( this );
        }

        [SerializeField]
        private bool m_DoCommandWaitOnPlay = true;
        private bool DoCommandWaitOnPlay { get { return m_DoCommandWaitOnPlay; } set { m_DoCommandWaitOnPlay = value; } }

        public bool Execute()
        {
            if ( OnOption.IsPresent )
                TimelineView.Show();

            if ( OffOption.IsPresent )
                TimelineView.Hide();

            if ( StartPlayback.IsPresent )
            {
                if ( !TimelineView.Timeline.IsBusy )
                {
                    if ( DoCommandWaitOnPlay )
                    {
                        DevCommandManager.Instance.StartWait();

                        TimelineView.PlayStopped += HandleTimelineViewPlayStopped;
                    }

                    TimelineView.Play();
                }
                else
                {
                    Debug.Log( "Error:  Choreography already in progress" );
                    return false;
                }
            }

            // Currently this option would only work if you started choreography with the UI button (which works fine),
            // because if you start it with the dev command we set the 'wait gate' on dev commands (above)
            if (StopPlayback.IsPresent)
            {
                TimelineView.Stop();
            }

            if (InfoOption.IsPresent)
            {
                // NOTE: time does not accomodate for the eval
                var stepCount = TimelineView.Timeline.RecursiveStepCount;
                var duration = TimelineView.Timeline.RecursiveDuration;
                var estimated = TimelineView.Timeline.RecursiveDurationIsEstimated;

                Debug.Log( "Total steps: " + stepCount + "; total time: " + duration + " seconds; estimated:" + estimated );
            }

            return true;
        }

        private void HandleTimelineViewPlayStopped()
        {
            if ( DoCommandWaitOnPlay )
            {
                DevCommandManager.Instance.EndWait();

                TimelineView.PlayStopped -= HandleTimelineViewPlayStopped;
            }
        }
    }
}
