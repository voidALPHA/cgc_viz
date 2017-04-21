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
using UnityEngine.UI;

namespace Utility.DevCommand
{
    public class DevCommandConsole : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "console";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Dev command console control";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Option m_ClearOption = new DevCommandManager.Option("clear", "Clear the dev command console");
        [DevCommandOptionAttribute]
        public DevCommandManager.Option ClearOption { get { return m_ClearOption; } set { m_ClearOption = value; } }

        private DevCommandManager.Option m_OnOption = new DevCommandManager.Option("on", "Turn the dev command console on", "mutuallyExclusive");
        [DevCommandOptionAttribute]
        public DevCommandManager.Option OnOption { get { return m_OnOption; } set { m_OnOption = value; } }

        private DevCommandManager.Option m_OffOption = new DevCommandManager.Option("off", "Turn the dev command console off", "mutuallyExclusive");
        [DevCommandOptionAttribute]
        public DevCommandManager.Option OffOption { get { return m_OffOption; } set { m_OffOption = value; } }


        [SerializeField]
        private Text m_OutputText;
        private Text OutputText { get { return m_OutputText; } set { m_OutputText = value; } }

        [SerializeField]
        private DevCommandConsoleBehaviour m_DevCommandConsoleBehaviour;
        private DevCommandConsoleBehaviour DevCommandConsoleBehaviour { get { return m_DevCommandConsoleBehaviour; } set { m_DevCommandConsoleBehaviour = value; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand( this );
        }

        public bool Execute()
        {
            if (ClearOption.IsPresent)
                OutputText.text = string.Empty;

            if (OnOption.IsPresent)
                DevCommandConsoleBehaviour.ShowConsole = true;

            if (OffOption.IsPresent)
                DevCommandConsoleBehaviour.ShowConsole = false;

            return true;
        }
    }
}
