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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Builds.Editor;
using UnityEditor;
using UnityEngine;
using Utility;

namespace Builds.Editor
{
    public class BuildManagerWindow : EditorWindow
    {
        [MenuItem("Window/Build Manager")]
        static void ShowWindow()
        {
            var window = (BuildManagerWindow)GetWindow(typeof(BuildManagerWindow));

            window.titleContent = new GUIContent("Build Manager");

            window.Show();
        }

        private string m_ManifestPath;
        private string ManifestPath
        {
            get
            {
                // Must lazy-init, as class-init is too soon for app.datapath.
                if(m_ManifestPath == null)
                    m_ManifestPath = Path.Combine(Application.dataPath, "buildManifest.json");

                return m_ManifestPath;
            }
            set
            {
                m_ManifestPath = value;
            }
        }

        private BuildManifest m_Manifest;
        private BuildManifest Manifest
        {
            get
            {
                return m_Manifest;
            }
            set
            {
                if(m_Manifest == value)
                    return;

                m_Manifest = value;

                ManifestView = new BuildManifestView(m_Manifest);
            }
        }


        private BuildManifestView ManifestView { get; set; }

        public static string LocalBuildRoot
        {
            get
            {
                if(!EditorPrefs.HasKey("VA.LocalBuildRoot"))
                {
                    if(LocalBuildRootEnvVar.Validated)
                    {
                        LocalBuildRoot = LocalBuildRootEnvVar.Value;
                    }
                    else
                    {
                        LocalBuildRoot = Directory.GetCurrentDirectory();
                    }
                }
                return EditorPrefs.GetString("VA.LocalBuildRoot");
            }
            set
            {
                EditorPrefs.SetString("VA.LocalBuildRoot", value);
            }
        }

        public static string RemoteBuildRoot
        {
            get
            {
                if(!EditorPrefs.HasKey("VA.RemoteBuildRoot"))
                {
                    if(RemoteBuildRootEnvVar.Validated)
                    {
                        RemoteBuildRoot = RemoteBuildRootEnvVar.Value;
                    }
                    else
                    {
                        RemoteBuildRoot = Directory.GetCurrentDirectory();
                    }
                }
                return EditorPrefs.GetString("VA.RemoteBuildRoot");
            }
            set
            {
                EditorPrefs.SetString("VA.RemoteBuildRoot", value);
            }
        }


        private void OnEnable()
        {
            LoadManifest();
        }

        private void OnGUI()
        {
            if(!DependentEnvironmentVariablesAreValid)
            {
                DrawDependentEnvironmentVariableGuis();
                return;
            }

            DrawManifestPath();

            if(ManifestView != null)
            {
                ManifestView.DrawGui();
            }
            else
            {
                EditorGUILayout.HelpBox("No manifest loaded. Create or load one.", MessageType.Warning);
                return;
            }

            if(Manifest != null)
                Manifest.Update();
        }


        private void DrawManifestPath()
        {
            ManifestPath = EditorGUILayout.TextField("Manifest Path", ManifestPath);

            GUILayout.BeginHorizontal();

            if(GUILayout.Button("Create"))
            {
                CreateManifest();
            }

            if(GUILayout.Button("Load"))
            {
                LoadManifest();
            }

            if(GUILayout.Button("Save"))
            {
                SaveManifest();
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            LocalBuildRoot = EditorGUILayout.TextField("Local Build Root", LocalBuildRoot);
            RemoteBuildRoot = EditorGUILayout.TextField("Remote Build Root", RemoteBuildRoot);
        }


        #region Environment Variables

        public static readonly BuildPathEnvironmentVariable AuxiliaryDataRootEnvVar = new BuildPathEnvironmentVariable(
            @"CGC_AUXILIARY_BUILD_DATA_ROOT",
            "Value should be the path to root of the shared folder which is designated for hosting contents of post-build copies.");

        public static readonly BuildPathEnvironmentVariable LocalBuildRootEnvVar = new BuildPathEnvironmentVariable(
            @"CGC_LOCAL_BUILD_ROOT",
            "Value should be the path where local builds should be placed.");

        public static readonly BuildPathEnvironmentVariable RemoteBuildRootEnvVar = new BuildPathEnvironmentVariable(
            @"CGC_REMOTE_BUILD_ROOT",
            "Value should be the path where remote builds should be placed.");

        private static List<BuildPathEnvironmentVariable> m_DependentEnvironmentVariables = null;
        private static List<BuildPathEnvironmentVariable> DependentEnvironmentVariables
        {
            get
            {
                if(m_DependentEnvironmentVariables == null)
                    m_DependentEnvironmentVariables = new List<BuildPathEnvironmentVariable>
                    {
                        //AuxiliaryDataRootEnvVar,
                        //LocalBuildRootEnvVar,
                        //RemoteBuildRootEnvVar
                    };

                return m_DependentEnvironmentVariables;
            }
        }

        private static bool DependentEnvironmentVariablesAreValid
        {
            get { return DependentEnvironmentVariables.All(envVar => envVar.Validated); }
        }

        private static void DrawDependentEnvironmentVariableGuis()
        {
            foreach(var ev in DependentEnvironmentVariables)
            {
                ev.DrawStatusGui();
            }
        }

        #endregion


        #region Link to external automation

        public static void PerformAutoBuild()
        {
            var buildName = CommandLineArgs.GetArgumentValue("buildName");
            Debug.Log("PerformAutoBuild running for build name \"" + buildName + "\"");

            BuildManagerWindow bmw = ScriptableObject.CreateInstance("BuildManagerWindow") as BuildManagerWindow;

            Debug.Log("Loading manifest at " + bmw.ManifestPath);
            bmw.LoadManifest();

            BuildEntry buildEntry = bmw.Manifest.Builds.Find(e => e.Name == buildName);
            if(buildEntry == null)
            {
                Debug.Log("Failure:  Haxxis build entry not found.");
                EditorApplication.Exit(1);
                return;
            }

            //buildEntry.DoCopyFolders = true;
            buildEntry.DoCopyLocalFolders = true;
            buildEntry.DoCopyLocalFiles = true;
            buildEntry.DoDeploy = false;

            Debug.Log("Starting the build");
            BuildPathEnvironmentVariable candidateBuildRootEnvVar = new BuildPathEnvironmentVariable(
                @"CGC_CANDIDATE_BUILD_ROOT",
                "Value should be the path where candidate builds should be placed (only used by Build Server).");
            var success = buildEntry.Build(candidateBuildRootEnvVar);

            if(!success)
            {
                Debug.Log("PerformAutoBuild failed");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("Finished running PerformAutoBuild");
        }

        public static void PerformAutoDeploy()
        {
            // This is a call designed to be made externally, to do just the 'deploy' step on an already-built local build.
            var buildName = CommandLineArgs.GetArgumentValue("buildName");
            Debug.Log("PerformAutoDeploy running for build name \"" + buildName + "\"");

            BuildManagerWindow bmw = ScriptableObject.CreateInstance("BuildManagerWindow") as BuildManagerWindow;

            Debug.Log("Loading manifest at " + bmw.ManifestPath);
            bmw.LoadManifest();

            BuildEntry buildEntry = bmw.Manifest.Builds.Find(e => e.Name == buildName);
            if(buildEntry == null)
            {
                Debug.Log("Failure:  Haxxis build entry not found.");
                EditorApplication.Exit(1);
                return;
            }

            buildEntry.DoDeploy = true;
            BuildPathEnvironmentVariable candidateBuildRootEnvVar = new BuildPathEnvironmentVariable(
                @"CGC_CANDIDATE_BUILD_ROOT",
                "Value should be the path where candidate builds should be placed (only used by Build Server).");
            var path = Path.Combine(candidateBuildRootEnvVar.Value, buildEntry.Name);
            var filename = Path.Combine(path, BuildEntry.LastBuildMadeFilename);
            string lastBuildDirName = File.ReadAllText(filename);

            Debug.Log("Starting the deploy");
            var relativePath = Path.Combine(buildEntry.Name, lastBuildDirName);
            buildEntry.Deploy(relativePath, candidateBuildRootEnvVar);

            Debug.Log("Finished running PerformAutoDeploy");
        }

        #endregion


        #region Manifest Lifecycle

        private void CreateManifest()
        {
            Manifest = new BuildManifest();
        }

        private void LoadManifest()
        {
            if(!File.Exists(ManifestPath))
            {
                Debug.LogErrorFormat("No manifest at ManifestPath {0}.", ManifestPath);
            }

            Manifest = BuildManifest.ReadFromFile(ManifestPath);
        }

        private void SaveManifest()
        {
            BuildManifest.WriteToFile(Manifest, ManifestPath);
        }

        #endregion


        #region Helpers

        private static IEnumerable<string> s_AllSceneNames = null;
        public static IEnumerable<string> AllSceneNames
        {
            get
            {
                if(s_AllSceneNames == null)
                {
                    s_AllSceneNames =
                        Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories)
                        .Select(s =>
                       {
                           var relativePath = s.Replace(Application.dataPath, string.Empty);

                           relativePath = relativePath.TrimStart('\\', '/');

                           return relativePath.Replace("\\", "/");
                       });
                }

                return s_AllSceneNames;
            }
        }

        #endregion
    }
}
