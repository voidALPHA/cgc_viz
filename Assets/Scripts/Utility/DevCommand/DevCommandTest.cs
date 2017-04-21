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
using System.IO;
using UnityEngine;

namespace Utility.DevCommand
{
    public class DevCommandTest : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "test";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Executes the specified test script (any text file containing dev commands)";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Argument m_TestScriptName = new DevCommandManager.Argument("testScriptName", typeof(string), false, "Script filename, or path and filename");
        [DevCommandArgumentAttribute]
        public DevCommandManager.Argument TestScriptName { get { return m_TestScriptName; } set { m_TestScriptName = value; } }

        private DevCommandManager.Option m_ListOption = new DevCommandManager.Option("list", "List all test scripts");
        [DevCommandOptionAttribute]
        public DevCommandManager.Option ListOption { get { return m_ListOption; } set { m_ListOption = value; } }

        private DevCommandManager.Option m_IfExistsOption = new DevCommandManager.Option("ifExists", "Ignore if the test script does not exist");
        [DevCommandOptionAttribute]
        public DevCommandManager.Option IfExistsOption { get { return m_IfExistsOption; } set { m_IfExistsOption = value; } }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        public static readonly string TestScriptsDirectoryPath = "TestScripts\\";
#else
        public static readonly string TestScriptsDirectoryPath = "TestScripts/";
#endif

        private const string StartupScriptCommandLineArg = "RunOnStartup";


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);

            var scriptName = CommandLineArgs.GetArgumentValue( StartupScriptCommandLineArg );
            if ( !string.IsNullOrEmpty( scriptName ) )
            {
                DevCommandManager.Instance.QueueDevCommand( "test " + scriptName );
            }
            else
            {
                if (File.Exists(TestScriptsDirectoryPath + "StartupLocal.txt"))
                    DevCommandManager.Instance.QueueDevCommand("test StartupLocal.txt");
                else
                    DevCommandManager.Instance.QueueDevCommand("test Startup.txt");
            }
        }

        public bool Execute()
        {
            if (ListOption.IsPresent)
            {
                return ListTestScripts(String.IsNullOrEmpty((string)TestScriptName.Value) ? TestScriptsDirectoryPath : (string)TestScriptName.Value);
            }

            if (!TestScriptName.IsPresent)
            {
                Debug.Log( "Specify a script to run, or use -" + ListOption.Name + " to list available scripts\n" );
                return false;
            }

            // Conveniences:
            var pathAndFile = (string)TestScriptName.Value;

            if (pathAndFile.IndexOf("/") < 0)
                pathAndFile = TestScriptsDirectoryPath + pathAndFile;

            if (!Path.HasExtension(pathAndFile))
                pathAndFile += ".txt";

            if (!File.Exists(pathAndFile))
            {
                if (!IfExistsOption.IsPresent)
                {
                    Debug.Log( "Error:  Test script \"" + pathAndFile + "\" not found" );
                    return false;
                }
                return true;
            }

            // To support test scripts calling other test scripts:
            // We are going to >insert< this test script's commands into the command queue
            // This gives us full nestability, although there is no error-checking for 'infinite loops' (e.g. script A calls script B, which calls script A)

            DevCommandManager.Instance.SaveAndResetQueuedDevCommands();
            
            TextReader textReader = new StreamReader(pathAndFile);

            while (true)
            {
                var curLine = textReader.ReadLine( );
                if (curLine == null)
                    break;
                if (curLine.Length == 0)    // Ignore blank lines
                    continue;

                DevCommandManager.Instance.QueueDevCommand(curLine);
            }

            DevCommandManager.Instance.AppendSavedQueuedDevCommands();

            return true;
        }

        private bool ListTestScripts(string path)
        {
            if (File.Exists( path ))
                ProcessFile( path );
            else if (Directory.Exists( path ))
                ProcessDirectory( path );
            else
            {
                Debug.Log( "Directory not found" );
                return false;
            }

            return true;
        }

        private void ProcessDirectory(string path)
        {
            string[] fileEntries = Directory.GetFiles( path );
            foreach (var file in fileEntries)
                ProcessFile(file );

            string[] subdirectoryEntires = Directory.GetDirectories( path );
            foreach (var subdirectory in subdirectoryEntires)
                ProcessDirectory( subdirectory );
        }

        private void ProcessFile(string path)
        {
            Debug.Log(path + "\n");
        }
    }
}
