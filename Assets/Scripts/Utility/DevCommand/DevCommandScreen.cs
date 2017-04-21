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
using System.Linq;
using UnityEngine;

namespace Utility.DevCommand
{
    public class DevCommandScreen : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "screen";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Screen control (resolution, and full screen mode)";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Option m_DumpOption = new DevCommandManager.Option("dump", "Dumps current settings");
        [DevCommandOption]
        private DevCommandManager.Option DumpOption { get { return m_DumpOption; } set { m_DumpOption = value; } }

        private DevCommandManager.Option m_ShowAvailableResolutionsOption = new DevCommandManager.Option("showAvailableResolutions", "Dumps all available resolutions");
        [DevCommandOption]
        private DevCommandManager.Option ShowAvailableResolutionsOption { get { return m_ShowAvailableResolutionsOption; } set { m_ShowAvailableResolutionsOption = value; } }

        private DevCommandManager.Option m_SetResolutionOption = new DevCommandManager.Option
            ("resolution", "Set screen or window resolution", "mutuallyExclusiveSR", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "resolution", typeof (string), true, "Expressed as, for example: 1920x1200" )
            });
        [DevCommandOption]
        public DevCommandManager.Option SetResolutionOption { get { return m_SetResolutionOption; } set { m_SetResolutionOption = value; } }

        private DevCommandManager.Option m_SetResFromCmdLineArgOption = new DevCommandManager.Option("resolutionFromCmdLineArg", "Set screen or window resolution, using the value specified by the named command line argument", "mutuallyExclusiveSR",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "cmdLineArgName", typeof (string), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option SetResFromCmdLineArgOption { get { return m_SetResFromCmdLineArgOption; } set { m_SetResFromCmdLineArgOption = value; } }

        private DevCommandManager.Option m_FullScreenOnOption = new DevCommandManager.Option("fullScreenOn", "Turns on full screen mode", "MutualExclusiveFS");
        [DevCommandOption]
        private DevCommandManager.Option FullScreenOnOption { get { return m_FullScreenOnOption; } set { m_FullScreenOnOption = value; } }

        private DevCommandManager.Option m_FullScreenOffOption = new DevCommandManager.Option("fullScreenOff", "Turns off full screen mode (goes to windowed mode)", "MutualExclusiveFS");
        [DevCommandOption]
        private DevCommandManager.Option FullScreenOffOption { get { return m_FullScreenOffOption; } set { m_FullScreenOffOption = value; } }

        private DevCommandManager.Option m_SetFullScreenFromCmdLineArgOption = new DevCommandManager.Option("fullScreenFromCmdLineArg", "Turn on/off full screen mode, using the value (\"on\" or \"off\") specified by the named command line argument", "MutualExclusiveFS",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "cmdLineArgName", typeof (string), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option SetFullScreenFromCmdLineArgOption { get { return m_SetFullScreenFromCmdLineArgOption; } set { m_SetFullScreenFromCmdLineArgOption = value; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);
        }

        public bool Execute()
        {
            var changeRes = false;
            var changeFullScreenMode = false;
            var newFullScreenMode = Screen.fullScreen;
            int newWidth = 0;
            int newHeight = 0;

            if ( ShowAvailableResolutionsOption.IsPresent )
            {
                Resolution[] resolutions = Screen.resolutions;
                foreach ( var res in resolutions )
                    Debug.Log(res.width + "x" + res.height + "; " + res.refreshRate + "hz");
            }

            if (DumpOption.IsPresent)
            {
                var s = string.Format("Resolution: {0}x{1}; {2}", Screen.width, Screen.height, Screen.fullScreen ? "Full screen" : "Windowed");
                Debug.Log(s);
            }

            if (FullScreenOnOption.IsPresent)
            {
                changeFullScreenMode = true;
                newFullScreenMode = true;
            }

            if (FullScreenOffOption.IsPresent)
            {
                changeFullScreenMode = true;
                newFullScreenMode = false;
            }

            if ( SetFullScreenFromCmdLineArgOption.IsPresent )
            {
                var fullScreenModeArg = CommandLineArgs.GetArgumentValue( (string) SetFullScreenFromCmdLineArgOption.Arguments[ 0 ].Value ).ToLower();

                if (string.IsNullOrEmpty(fullScreenModeArg))
                {
                    Debug.Log("Cannot set full screen mode because that command line argument was not found or had no value");
                    return false;
                }

                if (fullScreenModeArg.Equals("on"))
                {
                    changeFullScreenMode = true;
                    newFullScreenMode = true;
                }
                else if (fullScreenModeArg.Equals("off"))
                {
                    changeFullScreenMode = true;
                    newFullScreenMode = false;
                }
                else
                {
                    Debug.Log("Error:  Full screen mode argument value must be \"on\" or \"off\"");
                    return false;
                }
            }

            if ( changeFullScreenMode )
            {
                if ( newFullScreenMode )
                {
                    if ( Screen.fullScreen )
                    {
                        Debug.Log("Already in full screen mode");
                        changeFullScreenMode = false;   // Cancel the operation (but don't return error...this is really just a warning)
                    }
                    else
                        Debug.Log("Setting full screen mode");
                }
                else
                {
                    if ( !Screen.fullScreen )
                    {
                        Debug.Log("Already in windowed mode");
                        changeFullScreenMode = false;   // Cancel the operation (but don't return error...this is really just a warning)
                    }
                    else
                        Debug.Log("Setting windowed mode");
                }
            }

            var resString = "";

            if (SetResolutionOption.IsPresent)
            {
                resString = ((string)SetResolutionOption.Arguments[ 0 ].Value).ToLower();
            }

            if (SetResFromCmdLineArgOption.IsPresent)
            {
                resString = CommandLineArgs.GetArgumentValue((string)SetResFromCmdLineArgOption.Arguments[0].Value);

                if (string.IsNullOrEmpty(resString))
                {
                    Debug.Log("Cannot set resolution because that command line argument was not found or had no value");
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(resString))
            {
                var numbers = resString.Split('x');
                if (numbers.Count() != 2)
                {
                    Debug.Log("Error in screen resolution specification; must use correct format as in 1920x1600");
                    return false;
                }
                newWidth = Convert.ToInt32(numbers[0]);
                newHeight = Convert.ToInt32(numbers[1]);

                Resolution[] resolutions = Screen.resolutions;
                var found = resolutions.Any(res => res.width == newWidth && res.height == newHeight);

                if (found)
                    changeRes = true;
                else
                {
                    Debug.Log("Error: That resolution is not supported");
                    return false;
                }
            }


            // We have this 'combo' logic because if we're changing full screen mode AND resolution at the same time, we have to coordinate
            if ( changeFullScreenMode && !changeRes )
            {
                Screen.fullScreen = newFullScreenMode;
            }
            else if ( changeRes )   // Res only, or Res >and< full screen mode...
            {
                Screen.SetResolution(newWidth, newHeight, newFullScreenMode);
            }

            return true;
        }
    }
}
