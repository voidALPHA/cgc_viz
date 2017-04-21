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
    public class DevCommandException : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "exception";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Throws an exception";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);
        }

        public bool Execute()
        {
            Debug.Log("Throwing an exception");
            throw new Exception("Exception intentionally thrown from 'exception' dev command");
        }
    }
}
