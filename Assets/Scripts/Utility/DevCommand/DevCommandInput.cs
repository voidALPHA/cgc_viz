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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CameraScripting;
using UnityEngine;

namespace Utility.DevCommand
{
    public class DevCommandInput : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "input";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Input (keyboard and mouse) control";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Option m_FreeCameraOnOption = new DevCommandManager.Option("freeCameraOn", "Turn on the Free Fly Camera  ");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option FreeCameraOnOption { get { return m_FreeCameraOnOption; } set { m_FreeCameraOnOption = value; } }

        private DevCommandManager.Option m_FreeCameraOffOption = new DevCommandManager.Option("freeCameraOff", "Turn off the Free Fly Camera  ");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option FreeCameraOffOption { get { return m_FreeCameraOffOption; } set { m_FreeCameraOffOption = value; } }


        private DevCommandManager.Option m_MouseOnOption = new DevCommandManager.Option("mouseCursorOn", "Turn on the mouse cursor");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option MouseOnOption { get { return m_MouseOnOption; } set { m_MouseOnOption = value; } }

        private DevCommandManager.Option m_MouseOffOption = new DevCommandManager.Option("mouseCursorOff", "Turn off the mouse cursor");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option MouseOffOption { get { return m_MouseOffOption; } set { m_MouseOffOption = value; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);
        }

        public bool Execute()
        {
            if (MouseOnOption.IsPresent)
            {
                Cursor.visible = true;
                Debug.Log( "Mouse cursor turned on" );
            }

            if (MouseOffOption.IsPresent)
            {
                Cursor.visible = false;
                Debug.Log("Mouse cursor turned off");
            }

            if ( FreeCameraOffOption.IsPresent )
                FreeCameraMode.FreeCameraDisabled = true;
            if (FreeCameraOnOption.IsPresent)
                FreeCameraMode.FreeCameraDisabled = false;

            return true;
        }
    }
}
