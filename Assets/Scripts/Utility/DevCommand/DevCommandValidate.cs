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
using System.Linq;
using ChainViews;
using Choreography.Views;
using Utility.JobManagerSystem;
using UnityEngine;

namespace Utility.DevCommand
{
    public class DevCommandValidate : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "validate";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private const string m_HelpTextBrief = "Haxxis Package validation tool";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Argument m_HPNamesArg = new DevCommandManager.Argument("HPNames", typeof(string), false, "Name of Haxxis Package(s) with optional wildcards, e.g. score*.json");
        [DevCommandArgumentAttribute]
        private DevCommandManager.Argument HPNamesArg { get { return m_HPNamesArg; } set { m_HPNamesArg = value; } }

        private DevCommandManager.Option m_FromCmdLineArgOption = new DevCommandManager.Option( "fromCmdLineArg", "Use the name(s) specified by the command line argument" );
        [DevCommandOption]
        private DevCommandManager.Option FromCmdLineArgOption { get { return m_FromCmdLineArgOption; } set { m_FromCmdLineArgOption = value; } }

        private DevCommandManager.Option m_ListOnlyOption = new DevCommandManager.Option("listOnly", "Just list the files and don't do any validation", "MExclusive");
        [DevCommandOption]
        private DevCommandManager.Option ListOnlyOption { get { return m_ListOnlyOption; } set { m_ListOnlyOption = value; } }

        private DevCommandManager.Option m_VerboseOption = new DevCommandManager.Option("verbose", "Show full exception messages and callstacks");
        [DevCommandOption]
        private DevCommandManager.Option VerboseOption { get { return m_VerboseOption; } set { m_VerboseOption = value; } }

        private DevCommandManager.Option m_LoadOnlyOption = new DevCommandManager.Option("loadOnly", "Only validate the load", "MExclusive");
        [DevCommandOption]
        private DevCommandManager.Option LoadOnlyOption { get { return m_LoadOnlyOption; } set { m_LoadOnlyOption = value; } }

        private DevCommandManager.Option m_IncludeChoreographyOption = new DevCommandManager.Option("includeChoreography", "After validating evaluation, play the choreography", "MExclusive");
        [DevCommandOption]
        private DevCommandManager.Option IncludeChoreographyOption { get { return m_IncludeChoreographyOption; } set { m_IncludeChoreographyOption = value; } }

        private DevCommandManager.Option m_SaveFileUpdateOption = new DevCommandManager.Option("saveFileUpdate", "Just do the load, and if no errors, save the file out", "MExclusive");
        [DevCommandOption]
        private DevCommandManager.Option SaveFileUpdateOption { get { return m_SaveFileUpdateOption; } set { m_SaveFileUpdateOption = value; } }


        [SerializeField]
        private ChainView m_ChainView;
        private ChainView ChainView { get { return m_ChainView; } set { m_ChainView = value; } }

        private TimelineViewBehaviour TimelineView { get { return TimelineViewBehaviour.Instance; } }

        private enum ResultCode
        {
            Success,
            FailedLoad,
            HasChainErrors,
            FailedEval,
            FailedSave
        }

        private class Package
        {
            public string Filename;
            public ResultCode Result;
        }

        private List<Package> m_Packages= new List< Package >();
        private List<Package> Packages { get { return m_Packages; } set { m_Packages = value; } }

        private int CurrentFileIndex { get; set; }

        private string CurrentFileName { get; set; }

        private bool DoingJustCurrentHPValidation { get; set; }

        private enum ValidationStage
        {
            Load,
            Eval,
            Choreography,
            Save    // Used for the 'update save file' option
        }

        private ValidationStage m_Stage;
        private ValidationStage Stage { get { return m_Stage; } set { m_Stage = value; } }

        private bool ErrorInLastStage { get; set; }

        private int SavedJobManagerVerbosityLevel { get; set; }

        private int TotalPackagesAttempted  { get; set; }
        private int TotalSuccessfulLoads { get; set; }
        private int TotalSuccessfulChains { get; set; }
        private int TotalSuccessfulEvals { get; set; }
        private int TotalSuccessfulChoreography { get; set; }
        private int TotalSuccessfulSaves { get; set; }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand( this );
        }

        public bool Execute()
        {
            DoingJustCurrentHPValidation = !Arguments[ 0 ].IsPresent;

            if (!DoingJustCurrentHPValidation)    // (Otherwise, we want to validate just the currently loaded HP (skip the load, but do everything else))
            {
                var fileNameSpec = (string)Arguments[0].Value;

                if (FromCmdLineArgOption.IsPresent)
                {
                    fileNameSpec = CommandLineArgs.GetArgumentValue(fileNameSpec);

                    if (String.IsNullOrEmpty(fileNameSpec))
                    {
                        Debug.Log("That command line argument was not found or had no value");
                        return false;
                    }
                }

                var dir = "HaxxisPackages";
                var lastBackslashIndex = fileNameSpec.LastIndexOf('\\');
                if ( lastBackslashIndex >= 0 )
                {
                    dir += ("\\" + fileNameSpec.Substring(0, lastBackslashIndex));
                    fileNameSpec = fileNameSpec.Substring(lastBackslashIndex + 1, (fileNameSpec.Length - lastBackslashIndex) - 1);
                }

                if (!Path.HasExtension(fileNameSpec))
                    fileNameSpec += ".json";

                var files = Directory.GetFiles(dir, fileNameSpec, SearchOption.AllDirectories).ToList();
                if (files.Count == 0)
                {
                    Debug.Log("No matches found for \"" + fileNameSpec + "\"");
                    return false;
                }

                Packages.Clear();
                foreach ( var file in files )
                {
                    var package = new Package();
                    package.Filename = file;
                    package.Result = ResultCode.Success;
                    Packages.Add(package);
                }
            }

            if ( ListOnlyOption.IsPresent )
            {
                if ( DoingJustCurrentHPValidation )
                {
                    Debug.Log("Error: -listOnly option must be used with a file spec (e.g. *.json)");
                    return false;
                }

                foreach (var package in Packages)
                    Debug.Log(package.Filename);
                Debug.Log(Packages.Count + " matches found");
            }
            else
            {
                ChainView.ErrorOccurred += HandleErrorOccurred;

                ChainView.IsBusyChanged += HandleIsBusyChanged;
                TimelineView.PlayStarted += HandleTimelineViewPlayStarted;
                TimelineView.PlayStopped += HandleTimelineViewPlayStopped;

                SavedJobManagerVerbosityLevel = JobManager.Instance.ExceptionVerbosityLevel;
                JobManager.Instance.ExceptionVerbosityLevel = VerboseOption.IsPresent ? 3 : 0;

                if ( DoingJustCurrentHPValidation )
                {
                    // Just validate the currently-loaded HP; in this case, we 'pretend' we just loaded the HP:
                    Stage = ValidationStage.Load;
                    ErrorInLastStage = false;
                    CurrentFileName = "the current package";

                    HandleIsBusyChanged(false);
                }
                else
                {
                    CurrentFileIndex = 0;

                    TotalPackagesAttempted = 0;
                    TotalSuccessfulLoads = 0;
                    TotalSuccessfulChains = 0;
                    TotalSuccessfulEvals = 0;
                    TotalSuccessfulChoreography = 0;
                    TotalSuccessfulSaves = 0;

                    StartNextFileValidation();
                }
            }

            return true;
        }

        private void StartNextFileValidation( )
        {
            var filename = Packages[ CurrentFileIndex ].Filename;
            CurrentFileName = "\"" + filename + "\"";

            if (SaveFileUpdateOption.IsPresent)
                Debug.Log("Starting save file update process on " + CurrentFileName);
            else
                Debug.Log("Starting validation of " + CurrentFileName);

            TotalPackagesAttempted++;

            Stage = ValidationStage.Load;
            ErrorInLastStage = false;

            ChainView.LoadPackage( new ChainView.PackageRequest( filename, null ) );
        }

        private void HandleErrorOccurred()
        {
            ErrorInLastStage = true;
        }

        private void HandleTimelineViewPlayStarted()
        {
            DevCommandManager.Instance.StartWait();
        }

        private void HandleTimelineViewPlayStopped()
        {
            if (Stage != ValidationStage.Choreography)
                throw new Exception("Validation: Wrong state");

            DevCommandManager.Instance.EndWait();

            //var readyForNextFile = false;

            // to do: Make use of any error detection that gets put into choreography, if there ever will be, and handle that here.  Otherwise we assume it's all good
            Debug.Log("Successful CHOREOGRAPHY PLAYBACK of " + CurrentFileName);
            TotalSuccessfulChoreography++;
            //readyForNextFile = true;

            //if (readyForNextFile)
                ReadyForNextFile();
        }

        private void HandleIsBusyChanged(bool isBusy)
        {
            if (Stage == ValidationStage.Choreography)   // Doing this because choreography itself can [optionally] do an eval at the start of playback
                return;

            if ( isBusy )
            {
                DevCommandManager.Instance.StartWait();
                return;
            }

            if ( !DoingJustCurrentHPValidation || Stage != ValidationStage.Load )
                DevCommandManager.Instance.EndWait();

            var readyForNextFile = false;

            switch ( Stage )
            {
                case ValidationStage.Load:
                    if ( ErrorInLastStage )
                    {
                        Debug.Log( "Validation:  Error occurred in LOAD of " + CurrentFileName );
                        readyForNextFile = true;
                        Packages[CurrentFileIndex].Result = ResultCode.FailedLoad;
                    }
                    else
                    {
                        TotalSuccessfulLoads++;

                        if ( !DoingJustCurrentHPValidation )
                            Debug.Log( "Successful LOAD of " + CurrentFileName );

                        if ( SaveFileUpdateOption.IsPresent )
                        {
                            Stage = ValidationStage.Save;
                            ErrorInLastStage = false;

                            ChainView.SavePackage( new ChainView.PackageRequest( ChainView.LoadedPackagePath, null ) );
                            break;
                        }

                        if ( ChainView.Chain.HasError )
                        {
                            if ( DoingJustCurrentHPValidation )
                                Debug.Log( "Validation:  The current package has chain errors and thus cannot be evaluated" );
                            else
                            {
                                Debug.Log( "Validation:  The package " + CurrentFileName + " has chain errors and thus cannot be evaluated" );
                                if (!DoingJustCurrentHPValidation)
                                    Packages[CurrentFileIndex].Result = ResultCode.HasChainErrors;
                            }

                            readyForNextFile = true;
                        }
                        else
                        {
                            TotalSuccessfulChains++;

                            Debug.Log( "No errors found in node chain for " + CurrentFileName );
                            ErrorInLastStage = false;

                            if ( LoadOnlyOption.IsPresent )
                                ReadyForNextFile();
                            else
                            {
                                Stage = ValidationStage.Eval;

                                ChainView.EvaluateChain();
                            }
                        }
                    }
                    break;

                case ValidationStage.Eval:
                    if ( ErrorInLastStage )
                    {
                        Debug.Log( "Validation:  Error occurred during EVALUATION of " + CurrentFileName );
                        readyForNextFile = true;
                        if (!DoingJustCurrentHPValidation)
                            Packages[CurrentFileIndex].Result = ResultCode.FailedEval;
                    }
                    else
                    {
                        TotalSuccessfulEvals++;

                        Debug.Log( "Successful EVALUATION of " + CurrentFileName );
                        ErrorInLastStage = false;

                        if ( IncludeChoreographyOption.IsPresent )
                        {
                            Stage = ValidationStage.Choreography;

                            TimelineView.Play();
                        }
                        else
                            ReadyForNextFile();
                    }
                    break;

                case ValidationStage.Choreography: // This stage is handled in HandleTimelineViewPlayStopped
                    break;

                case ValidationStage.Save:
                    readyForNextFile = true;
                    if ( ErrorInLastStage )
                    {
                        Debug.Log( "Validation:  Error occurred in SAVE of " + CurrentFileName );
                        Packages[ CurrentFileIndex ].Result = ResultCode.FailedSave;
                    }
                    else
                    {
                        Debug.Log( "Successful SAVE of " + CurrentFileName );
                        TotalSuccessfulSaves++;
                    }
                    break;
            }

            if ( readyForNextFile )
                ReadyForNextFile();
        }

        private void ReadyForNextFile()
        {
            if ( DoingJustCurrentHPValidation )
                EndAllValidation();
            else
            {
                if (++CurrentFileIndex < Packages.Count)
                    StartNextFileValidation();
                else
                    EndAllValidation();
            }
        }

        private void EndAllValidation()
        {
            if ( SaveFileUpdateOption.IsPresent )
                Debug.Log("All save file updates complete");
            else
                Debug.Log("All validation complete");

            if ( !DoingJustCurrentHPValidation )
            {
                foreach ( var package in Packages )
                {
                    var text = string.Format("{0,16}  {1}", package.Result, package.Filename);
                    Debug.Log(text);
                }

                Debug.Log("Total packages attempted: " + TotalPackagesAttempted);
                Debug.Log("  " + TotalSuccessfulLoads + " successfully loaded (" + (TotalPackagesAttempted - TotalSuccessfulLoads) + " failed)");
                if (SaveFileUpdateOption.IsPresent)
                    Debug.Log("  " + TotalSuccessfulSaves + " successfully saved (" + (TotalPackagesAttempted - TotalSuccessfulSaves) + " failed)");
                else
                {
                    Debug.Log("  " + TotalSuccessfulChains + " loaded and had no chain errors (" + (TotalPackagesAttempted - TotalSuccessfulChains) + " failed)");
                    if (!LoadOnlyOption.IsPresent)
                        Debug.Log("  " + TotalSuccessfulEvals + " evaluated with no errors (" + (TotalPackagesAttempted - TotalSuccessfulEvals) + " failed)");
                    if (IncludeChoreographyOption.IsPresent)
                        Debug.Log("  " + TotalSuccessfulChoreography + " completed choreography (" + (TotalPackagesAttempted - TotalSuccessfulChoreography) + " failed)");
                }
            }

            JobManager.Instance.ExceptionVerbosityLevel = SavedJobManagerVerbosityLevel;

            ChainView.ErrorOccurred -= HandleErrorOccurred;

            ChainView.IsBusyChanged -= HandleIsBusyChanged;
            TimelineView.PlayStarted -= HandleTimelineViewPlayStarted;
            TimelineView.PlayStopped -= HandleTimelineViewPlayStopped;
        }
    }
}
