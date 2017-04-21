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
using Chains;
using ChainViews;
using UnityEngine;

// test 3

namespace Utility.DevCommand
{
    public class DevCommandChainView : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "chainview";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private const string m_HelpTextBrief = "Chain view control";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Option m_LoadOption = new DevCommandManager.Option("load", "Load Haxxis package", "mutuallyExclusive",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "fileName", typeof (string), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option LoadOption { get { return m_LoadOption; } set { m_LoadOption = value; } }

        private DevCommandManager.Option m_LoadFromCmdLineArgOption = new DevCommandManager.Option("loadFromCmdLineArg", "Load Haxxis package, using the name specified by the named command line argument", "mutuallyExclusive",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option LoadFromCmdLineArgOption { get { return m_LoadFromCmdLineArgOption; } set { m_LoadFromCmdLineArgOption = value; } }

        private DevCommandManager.Option m_ImportOption = new DevCommandManager.Option("import", "Import Haxxis package", "mutuallyExclusive",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "fileName", typeof (string), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option ImportOption { get { return m_ImportOption; } set { m_ImportOption = value; } }

        private DevCommandManager.Option m_SaveOption = new DevCommandManager.Option("save", "Save Haxxis package with current filename", "mutuallyExclusive");
        [DevCommandOption]
        private DevCommandManager.Option SaveOption { get { return m_SaveOption; } set { m_SaveOption = value; } }

        private DevCommandManager.Option m_SaveAsOption = new DevCommandManager.Option("saveAs", "Save Haxxis package with specified filename", "mutuallyExclusive",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "fileName", typeof (string), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option SaveAsOption { get { return m_SaveAsOption; } set { m_SaveAsOption = value; } }

        private DevCommandManager.Option m_EvalOption = new DevCommandManager.Option("eval", "Evaluate current Haxxis package", "mutuallyExclusive");
        [DevCommandOption]
        private DevCommandManager.Option EvalOption { get { return m_EvalOption; } set { m_EvalOption = value; } }

        private DevCommandManager.Option m_NewOption = new DevCommandManager.Option("new", "Remove current Haxxis package and start new", "mutuallyExclusive");
        [DevCommandOption]
        private DevCommandManager.Option NewOption { get { return m_NewOption; } set { m_NewOption = value; } }

        private DevCommandManager.Option m_OnOption = new DevCommandManager.Option("on", "Turn on workspace display", "mutuallyExclusiveOnOff");
        [DevCommandOption]
        private DevCommandManager.Option OnOption { get { return m_OnOption; } set { m_OnOption = value; } }

        private DevCommandManager.Option m_OffOption = new DevCommandManager.Option("off", "Turn off workspace display", "mutuallyExclusiveOnOff");
        [DevCommandOption]
        private DevCommandManager.Option OffOption { get { return m_OffOption; } set { m_OffOption = value; } }

        private DevCommandManager.Option m_InfoOption = new DevCommandManager.Option("info", "Dump info about currently-loaded Haxxis package");
        [DevCommandOption]
        private DevCommandManager.Option InfoOption { get { return m_InfoOption; } set { m_InfoOption = value; } }


        private ChainView ChainView { get { return ChainView.Instance; } }

        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand( this );

            ChainView.IsBusyChanged += HandleIsBusyChanged;
        }

        private void HandleIsBusyChanged(bool isBusy)
        {
            if ( isBusy )
                DevCommandManager.Instance.StartWait();
            else
                DevCommandManager.Instance.EndWait();
        }

        public bool Execute()
        {
            if (NewOption.IsPresent)
                ChainView.LoadNewChain();

            if ( ChainView == null )
                return false;

            var loadFilename = "";

            if ( LoadOption.IsPresent )
            {
                loadFilename = (string)LoadOption.Arguments[0].Value;
            }

            if ( LoadFromCmdLineArgOption.IsPresent )
            {
                loadFilename = CommandLineArgs.GetArgumentValue( (string)LoadFromCmdLineArgOption.Arguments[0].Value );

                if ( String.IsNullOrEmpty( loadFilename ) )
                {
                    Debug.Log( "Cannot load because that command line argument was not found or had no value" );
                    return false;
                }
            }

            if ( !String.IsNullOrEmpty( loadFilename ) )
            {
                var fullPath = Path.Combine( HaxxisPackage.RootPath, loadFilename );

                ChainView.LoadPackage( new ChainView.PackageRequest( fullPath, null ) );
            }


            if ( ImportOption.IsPresent )
            {
                var filename = (string)LoadOption.Arguments[0].Value;

                var fullPath = Path.Combine( HaxxisPackage.RootPath, filename );

                ChainView.ImportFile( fullPath );
            }

            if (SaveOption.IsPresent)
            {
                if (string.IsNullOrEmpty( ChainView.LoadedPackagePath ))
                {
                    Debug.Log("Cannot save because there is no file name; try -saveAs instead");
                    return false;
                }
                ChainView.SavePackage( new ChainView.PackageRequest( ChainView.LoadedPackagePath, null ) );
            }

            if ( SaveAsOption.IsPresent )
            {
                var filename = (string)SaveAsOption.Arguments[0].Value;

                var fullPath = Path.Combine( HaxxisPackage.RootPath, filename );

                ChainView.SavePackage( new ChainView.PackageRequest( fullPath, null ) );
            }

            if (EvalOption.IsPresent)
                if (!ChainView.Chain.HasError)
                    ChainView.EvaluateChain();
                else
                    Debug.Log("Cannot eval because chain has error");

            if (OnOption.IsPresent)
                ChainView.Visible = true;

            if (OffOption.IsPresent)
                ChainView.Visible = false;

            if (InfoOption.IsPresent)
                ChainView.DumpInfo();

            return true;
        }
    }
}
