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
    public class DevCommandLog : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "log";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Outputs the specified text";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Argument m_OutputText = new DevCommandManager.Argument("textToOutput", typeof(string), true);
        [DevCommandArgumentAttribute]
        public DevCommandManager.Argument OutputText { get { return m_OutputText; } set { m_OutputText = value; } }

        // This is an option that has an argument:
        private DevCommandManager.Option m_RepeatCountOption = new DevCommandManager.Option
            ( "count", "(This option is here for testing)", new List<DevCommandManager.Argument>( )
            {
                new DevCommandManager.Argument( "repeatCount", typeof (int), true )
            } );
        [DevCommandOptionAttribute]
        public DevCommandManager.Option RepeatCountOption { get { return m_RepeatCountOption; } set { m_RepeatCountOption = value; } }

        private DevCommandManager.Option m_TestStringOption = new DevCommandManager.Option
            ("foo-bar", "(This option is here for testing)", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "testString", typeof (string), true )
            });
        [DevCommandOptionAttribute]
        public DevCommandManager.Option TestStringOption { get { return m_TestStringOption; } set { m_TestStringOption = value; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);
        }

        public bool Execute()
        {
            int count = RepeatCountOption.IsPresent ? (int)RepeatCountOption.Arguments[0].Value : 1;
            //Debug.Log("Repeat count is " + count);

            for (int i = 0; i < count; i++)
                Debug.Log((string)OutputText.Value);

            if ( TestStringOption.IsPresent )
            {
                Debug.Log("TestStringOption is present, and the value is " + (string)TestStringOption.Arguments[0].Value );
            }

            return true;
        }
    }
}
