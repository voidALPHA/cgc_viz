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

namespace Utility.DevCommand
{
    public class DevCommandWait : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "wait";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Waits the specified amount of seconds before running the next dev command";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Argument m_DurationArg = new DevCommandManager.Argument("durationInSeconds", typeof(float), true);
        [DevCommandArgumentAttribute]
        public DevCommandManager.Argument DurationArg { get { return m_DurationArg; } set { m_DurationArg = value; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);
        }

        public bool Execute()
        {
            if (DevCommandManager.Instance.WaitTimeRemaining > 0.0f)
                throw new Exception("Got a wait command but we're already waiting...");

            DevCommandManager.Instance.WaitTimeRemaining = (float)DurationArg.Value;

            return true;
        }
    }
}
