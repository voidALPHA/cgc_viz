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
using System.Collections.Generic;
using UnityEngine;
using Utility.SystemStats;

namespace Utility.DevCommand
{
    public class DevCommandStats : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "stats";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Stats system control";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Option m_DumpOption = new DevCommandManager.Option("dump", "Dump current stats lines");
        [DevCommandOptionAttribute]
        public DevCommandManager.Option DumpOption { get { return m_DumpOption; } set { m_DumpOption = value; } }

        private DevCommandManager.Option m_DisplayOnOption = new DevCommandManager.Option("displayOn", "Turn on stats lines display", "mutuallyExclusive");
        [DevCommandOptionAttribute]
        public DevCommandManager.Option DisplayOnOption { get { return m_DisplayOnOption; } set { m_DisplayOnOption = value; } }

        private DevCommandManager.Option m_DisplayOffOption = new DevCommandManager.Option("displayOff", "Turn off stats lines display", "mutuallyExclusive");
        [DevCommandOptionAttribute]
        public DevCommandManager.Option DisplayOffOption { get { return m_DisplayOffOption; } set { m_DisplayOffOption = value; } }


        [SerializeField]
        private SystemStatsBehaviour m_SystemStats;
        private SystemStatsBehaviour SystemStats { get { return m_SystemStats; } set { m_SystemStats = value; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);
        }

        public bool Execute()
        {
            if (DumpOption.IsPresent)
            {
                var text = String.Empty;
                foreach (var stat in SystemStats.StatsLines)
                    text += ( stat.StatString + "\n" );
                Debug.Log(text);
            }

            if (DisplayOnOption.IsPresent)
                SystemStats.ShowStats = true;

            if (DisplayOffOption.IsPresent)
                SystemStats.ShowStats = false;

            return true;
        }
    }
}
