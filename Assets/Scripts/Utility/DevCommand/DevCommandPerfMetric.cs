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
using Utility.PerfMetricSystem;
using Debug = UnityEngine.Debug;

namespace Utility.DevCommand
{
    public class DevCommandPerfMetric : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "perf";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Performance metrics control";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }

        private DevCommandManager.Option m_StartOption = new DevCommandManager.Option("start", "Start measurement", "mutuallyExclusive");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option StartOption { get { return m_StartOption; } set { m_StartOption = value; } }

        private DevCommandManager.Option m_RestartOption = new DevCommandManager.Option("restart", "Restart measurement", "mutuallyExclusive");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option RestartOption { get { return m_RestartOption; } set { m_RestartOption = value; } }

        private DevCommandManager.Option m_StopOption = new DevCommandManager.Option("stop", "Stop measurement", "mutuallyExclusive");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option StopOption { get { return m_StopOption; } set { m_StopOption = value; } }

        private DevCommandManager.Option m_DumpOption = new DevCommandManager.Option("dump", "Dump measurement data");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option DumpOption { get { return m_DumpOption; } set { m_DumpOption = value; } }

        private DevCommandManager.Option m_DumpAllOption = new DevCommandManager.Option("dumpAll", "Dump all measurement data (includes data for each frame)");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option DumpAllOption { get { return m_DumpAllOption; } set { m_DumpAllOption = value; } }

        private DevCommandManager.Option m_RecordingIndicatorOnOption = new DevCommandManager.Option("recordingIndicatorOn", "Turns on recording indicator display", "mutuallyExclusiveRI");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option RecordingIndicatorOnOption { get { return m_RecordingIndicatorOnOption; } set { m_RecordingIndicatorOnOption = value; } }

        private DevCommandManager.Option m_RecordingIndicatorOffOption = new DevCommandManager.Option("recordingIndicatorOff", "Turns off recording indicator display", "mutuallyExclusiveRI");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option RecordingIndicatorOffOption { get { return m_RecordingIndicatorOffOption; } set { m_RecordingIndicatorOffOption = value; } }

        private DevCommandManager.Option m_SetVsyncOption = new DevCommandManager.Option
            ("setVsync", "Sets vsync (0 for off, 1, or 2)", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "vsyncCount", typeof(int), true )
            });
        [DevCommandOptionAttribute]
        private DevCommandManager.Option SetVsyncOption { get { return m_SetVsyncOption; } set { m_SetVsyncOption = value; } }


        [SerializeField]
        private PerfMetric m_PerfMetric;
        private PerfMetric PerfMetric { get { return m_PerfMetric; } set { m_PerfMetric = value; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);

            PerfMetric = FindObjectOfType<PerfMetric>();
        }

        public bool Execute()
        {
            var pm = PerfMetric.Instance;

            if ( StartOption.IsPresent )
            {
                if ( !pm.StartMeasuring() )
                {
                    Debug.Log( "Error:  Already measuring" );
                    return false;
                }
            }

            if (RestartOption.IsPresent)
            {
                pm.RestartMeasuring();
            }

            if (StopOption.IsPresent)
            {
                if ( !pm.StopMeasuring() )
                {
                    Debug.Log("Error:  No measurement in progress");
                    return false;
                }
            }

            if (DumpOption.IsPresent)
                pm.Dump();

            if (DumpAllOption.IsPresent)
                pm.Dump(true);

            if (RecordingIndicatorOnOption.IsPresent)
                pm.RecordingIndicatorDisplayOverride = true;

            if (RecordingIndicatorOffOption.IsPresent)
                pm.RecordingIndicatorDisplayOverride = false;

            if ( SetVsyncOption.IsPresent )
            {
                var value = (int) SetVsyncOption.Arguments[ 0 ].Value;
                if (value >= 0 && value <= 2)
                    QualitySettings.vSyncCount = value;
                else
                    Debug.Log("Error:  Vsync value out of range (must be 0, 1 or 2)");
            }

            return true;
        }
    }
}
