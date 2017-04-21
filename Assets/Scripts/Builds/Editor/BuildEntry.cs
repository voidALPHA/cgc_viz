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
using System.Diagnostics;
using System.IO;
using System.Linq;
using Assets.Builds.Editor;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Utility.DevCommand;
using Debug = UnityEngine.Debug;

namespace Builds.Editor
{
    [Serializable]
    public class BuildEntry
    {
        private string m_Name = string.Empty;
        public string Name { get { return m_Name; } set { m_Name = value; } }

        private List < string > m_Scenes = new List < string >();
        public List < string > Scenes
        {
            get { return m_Scenes; }
        }

        private List<BuildTarget> m_Targets = new List < BuildTarget >();
        public List < BuildTarget > Targets
        {
            get { return m_Targets; }
        }

        private readonly List<string> m_FoldersToCopy = new List<string>();
        public List <string> FoldersToCopy { get { return m_FoldersToCopy; } }

        private readonly List<string> m_LocalFoldersToCopy = new List<string>();
        public List<string> LocalFoldersToCopy { get { return m_LocalFoldersToCopy; } }

        private readonly List<string> m_LocalFilesToCopy = new List< string >();
        public List<string> LocalFilesToCopy { get { return m_LocalFilesToCopy; } }

        private int m_Target = 0;
        public int Target { get { return m_Target; } set { m_Target = value; } }

        #region Build-Time Options

        //private bool m_DoCopyFolders = true;
        //[JsonIgnore]
        //public bool DoCopyFolders { get { return m_DoCopyFolders; } set { m_DoCopyFolders = value; } }

        private bool m_DoCopyLocalFolders = true;
        [JsonIgnore]
        public bool DoCopyLocalFolders { get { return m_DoCopyLocalFolders; } set { m_DoCopyLocalFolders = value; } }

        private bool m_DoCopyLocalFiles = true;
        [JsonIgnore]
        public bool DoCopyLocalFiles { get { return m_DoCopyLocalFiles; } set { m_DoCopyLocalFiles = value; } }

        private bool m_DoDeploy = false;
        [JsonIgnore]
        public bool DoDeploy { get { return m_DoDeploy; } set { m_DoDeploy = value; } }

        #endregion


        #region Flags for registering changes controlled by elsewhere

        [JsonIgnore]
        public bool PendingBuild { get; set; }

        [JsonIgnore]
        public bool PendingDelete { get; set; }

        #endregion

        public const string LastBuildMadeFilename = "LastBuildMade.txt";

        public void Update()
        {
            if ( PendingBuild )
            {
                PendingBuild = false;

                Build();
            }
        }

        public bool Build(BuildPathEnvironmentVariable buildEnvVar = null)
        {
            Log( "Starting build process for {0}.", Name );

            try
            {
                var relativePath = GenerateRelativeBuildPath();

                if(buildEnvVar == null) buildEnvVar = BuildManagerWindow.LocalBuildRootEnvVar;
                var buildRootFolder = buildEnvVar.Value;

                LocalBuild(relativePath, buildRootFolder);

                //CopyFolders(relativePath, buildRootFolder);

                CopyLocalFolders(relativePath, buildRootFolder);

                CopyLocalFiles(relativePath, buildRootFolder);

                Deploy( relativePath );
            }
            catch ( Exception ex )
            {
                Debug.LogErrorFormat( "Error during build, state unknown. Exception: {0}", ex );
                return false;
            }

            Log( "Ending build process for {0}.", Name );
            return true;
        }

        private string GenerateRelativeBuildPath()
        {
            var pathDate = DateTime.Now.ToString( "yyyy.MM.dd_HH.mm.ss" );

            var relativePath = Path.Combine( Name, pathDate );

            return relativePath;
        }

        private void LocalBuild(string relativePath, string buildRootFolder)
        {
            Log( "Starting local build." );

            var path = Path.Combine(buildRootFolder, relativePath);

            // Race condition between existence check and create, could use lock file if this becomes an issue...
            if ( Directory.Exists( path ) )
                throw new InvalidOperationException( "Target directory (" + path + ") already exists." );
            
            Directory.CreateDirectory( path );

            var executableName = Name + ".exe";
            var exePath = Path.Combine( path, executableName );

            var scenesToBuild = Scenes.Select( s => Path.Combine( Application.dataPath, s ).Replace( "\\", "/" ) ).ToArray();

            Log( "Preparing scenes:" );

            foreach ( var s in scenesToBuild )
                Log( "...scene {0}", s  );

            var bpo = new BuildPlayerOptions {
                scenes = scenesToBuild, locationPathName = exePath, options = BuildOptions.None
            };
            switch(Target) 
            {
                default: // 0: Win64
                    bpo.target = BuildTarget.StandaloneWindows64;
                    break;
                case 1: // 1: OSX
                    bpo.target = BuildTarget.StandaloneOSXUniversal;
                    break;
                case 2: // 2: Linux Universal Headful
                    bpo.target = BuildTarget.StandaloneLinuxUniversal;
                    break;
            }

            var error = BuildPipeline.BuildPlayer(bpo);

            if ( !string.IsNullOrEmpty( error ) )
            {
                throw new InvalidOperationException("Build failed with message: " + error );
            }

            AddIdentifierFile( path );

            var buildsPath = Path.Combine(buildRootFolder, Name);
            UpdateLastBuildMadeFile( buildsPath, path );

            Log( "Ending local build." );
        }


        private void AddIdentifierFile( string path )
        {
            var filename = Path.Combine( path, "version.txt" );

            var identifier = LastFolderName( path );

            File.WriteAllText( filename, identifier );
        }

        private void UpdateLastBuildMadeFile( string buildsPath, string path )
        {
            var filename = Path.Combine(buildsPath, LastBuildMadeFilename);

            File.WriteAllText( filename, LastFolderName( path ) );
        }

        private string LastFolderName(string path)
        {
            var lastBackslashIndex = path.LastIndexOf('\\');
            if (lastBackslashIndex >= 0)
                return path.Substring(lastBackslashIndex + 1, ( path.Length - lastBackslashIndex ) - 1);
            else
                return path;
        }


        //private void CopyFolders(string relativePath, string buildRootFolder)
        //{
        //    if ( !DoCopyFolders )
        //    {
        //        Log( "Skipping auxiliary asset folder copy." );
        //        return;
        //    }

        //    Log("Starting auxiliary asset folder copy with {0} folders.", FoldersToCopy.Count);

        //    foreach ( var relativeSourcePath in FoldersToCopy )
        //    {
        //        var sourcePath = relativeSourcePath.Replace( "\\", "/" );

        //        // Strip any parent dirs from source path, we copy only the selected directory itself straight into build location.
        //        var relativeDestinationPath = Path.GetFileName( relativeSourcePath.TrimEnd( Path.DirectorySeparatorChar ) );
        //        var destinationParent = Path.Combine(buildRootFolder, relativePath);
        //        var destinationPath = Path.Combine( destinationParent, relativeDestinationPath ).Replace( "\\", "/" );
   
        //        FileUtil.CopyFileOrDirectory( sourcePath, destinationPath );

        //        Log( "...copied {0} to {1}", sourcePath, destinationPath );
        //    }

        //    Log("Ending auxiliary asset folder copy.");
        //}

        private void CopyLocalFolders(string relativePath, string buildRootFolder)
        {
            if (!DoCopyLocalFolders)
            {
                Log("Skipping local asset folder copy.");
                return;
            }

            Log("Starting local asset folder copy with {0} folders.", LocalFoldersToCopy.Count);

            CopyLocalFoldersOrFilesInternal(relativePath, buildRootFolder, LocalFoldersToCopy);

            AddVersionScript(relativePath, buildRootFolder);

            Log("Ending local asset folder copy.");
        }

        private void CopyLocalFiles(string relativePath, string buildRootFolder)
        {
            if (!DoCopyLocalFiles)
            {
                Log("Skipping auxiliary local asset file copy.");
                return;
            }

            Log("Starting auxiliary local asset file copy with {0} files.", LocalFilesToCopy.Count);

            CopyLocalFoldersOrFilesInternal(relativePath, buildRootFolder, LocalFilesToCopy);

            Log("Ending auxiliary local asset file copy.");
        }

        private void CopyLocalFoldersOrFilesInternal(string relativePath, string buildRootFolder, List<string> foldersOrFiles)
        {
            foreach (var relativeSourcePath in foldersOrFiles)
            {
                var sourcePath = Path.Combine(Directory.GetCurrentDirectory(), relativeSourcePath).Replace("\\", "/");

                // Strip any parent dirs from source path; we copy only the selected folder/file itself straight into build location
                // Note:  For folders, if copying "foo/bar", "foo" is lost in the target folder structure
                // Note:  For files, if copy "foo/bar.txt", "foo" is lost in the target folder structure
                var relativeDestinationPath = Path.GetFileName(relativeSourcePath.TrimEnd(Path.DirectorySeparatorChar));
                var destinationParent = Path.Combine(buildRootFolder, relativePath);
                var destinationPath = Path.Combine(destinationParent, relativeDestinationPath).Replace("\\", "/");

                FileUtil.CopyFileOrDirectory(sourcePath, destinationPath);

                Log("...copied {0} to {1}", sourcePath, destinationPath);
            }

        }

        private void AddVersionScript(string relativePath, string buildRootFolder)
        {
            var path = Path.Combine(buildRootFolder, relativePath);

            var scriptsPath = Path.Combine(path, DevCommandTest.TestScriptsDirectoryPath);  // Yes, unfortunately this makes the build manager dependent on the dev command system

            var filename = Path.Combine(scriptsPath, "StandaloneBuildVersion.txt");

            var text = "Log \"Standalone build version: " + LastFolderName(path) + "\"";

            File.WriteAllText(filename, text);
        }

        public void Deploy(string relativePath, BuildPathEnvironmentVariable buildEnvVar = null)
        {
            if ( !DoDeploy )
            {
                Log( "Skipping Deploy." );
                return;
            }

            Log( "Starting deploy." );
            
            var sourceRootFolder = BuildManagerWindow.LocalBuildRoot;
            if(buildEnvVar != null) sourceRootFolder = buildEnvVar.Value;

            var sourcePath = Path.Combine(sourceRootFolder, relativePath).Replace("\\", "/");

            var destinationParent = Path.Combine( BuildManagerWindow.RemoteBuildRoot, Name ).Replace( "\\", "/" );
            Directory.CreateDirectory( destinationParent );

            var destinationPath = Path.Combine( BuildManagerWindow.RemoteBuildRoot, relativePath ).Replace( "\\", "/" );

            FileUtil.CopyFileOrDirectory( sourcePath, destinationPath );

            // Copy the 'LastBuildMade' text file:
            var src1 = Path.Combine( sourceRootFolder, Name );
            var sourcePathLbm = Path.Combine( src1, LastBuildMadeFilename).Replace( "\\", "/" );

            var dest1 = Path.Combine( BuildManagerWindow.RemoteBuildRoot, Name );
            var destPathLbm = Path.Combine( dest1, LastBuildMadeFilename ).Replace( "\\", "/" );
            FileUtil.ReplaceFile( sourcePathLbm, destPathLbm );

            Log( "Ending deploy." );
        }

        public void OpenLocal()
        {
            var path = Path.Combine( BuildManagerWindow.LocalBuildRoot, Name );

            Open( path );
        }

        public void OpenRemote()
        {
            var path = Path.Combine( BuildManagerWindow.RemoteBuildRoot, Name );

            Open( path );
        }

        private void Open( string path )
        {
            if ( !Directory.Exists( path ) )
            {
                EditorUtility.DisplayDialog( "Error", "Path does not exist.", "OK" );
                return;
            }

            Process.Start( new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void Run()
        {
            // Run the game (Process class from System.Diagnostics).
            //Process proc = new Process();
            //proc.StartInfo.FileName = path + "BuiltGame.exe";
            //proc.Start();
        }


        #region Logging

        private void Log( string message, params object[] args )
        {
            Log( string.Format( message, args ) );
        }

        private void Log( string message )
        {
            Debug.LogFormat( "<color=#994ae7>{0}</color>", message );
        }

        #endregion
    }
}