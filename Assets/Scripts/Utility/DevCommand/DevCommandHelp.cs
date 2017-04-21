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
    public class DevCommandHelp : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "help";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Lists all registered dev commands, or a specific one";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        /// The goal here is to have a way to use attributes to define an 'arg spec' for the arguments, options, and option arguments, and associated dev command
        ///    member variables, and help text.  At registration time, use this data to (a) generate full help text, and (b) generate the arg spec data that can be used at command
        ///    entry time to parse and validate the entered arguments/options, AND set the command's bound member variables to the entered argument values just prior to "Execute".
        //      Sample:  help [<commandName:string>] -full
        // Full sample:  blah <slot:int> [<name:string>] -count <int> [<secondaryCount:int>] -optionB

        private DevCommandManager.Argument m_CommandForHelpArg = new DevCommandManager.Argument("commandName", typeof(string), false, "Show help for the specified dev command");
        [DevCommandArgumentAttribute]
        private DevCommandManager.Argument CommandForHelpArg { get { return m_CommandForHelpArg; } set { m_CommandForHelpArg = value; } }

        private DevCommandManager.Option m_FullHelpOption = new DevCommandManager.Option("full", "Show all arguments and options");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option FullHelpOption { get { return m_FullHelpOption; } set { m_FullHelpOption = value; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);
        }

        public bool Execute()
        {
            bool fullHelp = FullHelpOption.IsPresent || CommandForHelpArg.Value != null;

            var text = string.Empty;    // This string building is cheesy but we're doing it now just to output to Debug.Log
            bool atLeastOneMatch = false;
            var devCommands = DevCommandManager.Instance.DevCommands.Values.OrderBy(c => c.Name).ToList();

            var commandForHelpArg = CommandForHelpArg.Value == null ? null : ((string)(CommandForHelpArg.Value)).ToLower();
            foreach (var cmd in devCommands)
            {
                if ((commandForHelpArg == null) || (commandForHelpArg.Equals(cmd.Name)))
                {
                    if (fullHelp)
                        text += ( cmd.HelpTextFull + "\n" );
                    else
                        text += String.Format("{0,14} - {1}\n", cmd.Name, cmd.HelpTextBrief);
                    atLeastOneMatch = true;
                }
            }
            if ((commandForHelpArg != null) && (atLeastOneMatch == false))
                text += ("There is no registered dev command called \"" + commandForHelpArg + "\"");
            if (!fullHelp && commandForHelpArg == null)
                text += "Type \"help -full\" for full help, or \"help <command>\" for full help on a particular dev command";

            Debug.Log(text);

            return true;
        }
    }
}
