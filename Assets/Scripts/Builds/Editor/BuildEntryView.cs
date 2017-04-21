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
using UnityEditor;
using UnityEngine;
using Utility.Editor;

namespace Builds.Editor
{
    public class BuildEntryView
    {
        public BuildEntry Build { get; private set; }

        public BuildEntryView( BuildEntry build )
        {
            Build = build;

            ShowScenes = true;

            //ShowFoldersToCopy = true;

            ShowLocalFoldersToCopy = true;

            ShowLocalFilesToCopy = true;

            ShowBuild = true;

            BuildChecked = false;
        }

        
        #region Drawing

        public void DrawGui()
        {
            GUILayout.BeginVertical( GUI.skin.box );

            DrawHeading();

            if ( ShowBuild )
            {
                MoreEditorStyles.Indent();

                GUILayout.BeginHorizontal();

                DrawOptions();

                EditorGUILayout.Separator();

                DrawBuildControls();

                GUILayout.EndHorizontal();

                GUILayout.Space( 3 );

                MoreEditorStyles.Outdent();
            }

            GUILayout.EndVertical();
        }

        private void DrawOptions()
        {
            GUILayout.BeginVertical();

            DrawScenes();

            //DrawTargets();

            //DrawFolderCopies();

            DrawLocalFolderCopies();

            DrawLocalFileCopies( );

            GUILayout.EndVertical();
        }

        private bool ShowConfirmDelete { get; set; }

        private bool ShowBuild { get; set; }

        public bool BuildChecked { get; private set; }

        private void DrawHeading()
        {
            GUILayout.BeginHorizontal();

            BuildChecked = EditorGUILayout.Toggle(BuildChecked, GUILayout.Width(16));
            ShowBuild = EditorGUILayout.Toggle(ShowBuild, EditorStyles.foldout, GUILayout.Width(16));

            if ( ShowBuild )
            {
                Build.Name = GUILayout.TextField( Build.Name );

                if ( GUILayout.Button( "X", GUI.skin.label, GUILayout.ExpandWidth( false ) ) )
                {
                    ShowConfirmDelete = !ShowConfirmDelete;
                }
            }
            else
            {
                GUILayout.Label( Build.Name );
            }
            
            GUILayout.EndHorizontal();

            DrawConfirmDelete();
        }


        private void DrawConfirmDelete()
        {
            if ( !ShowBuild )
                return;

            if ( !ShowConfirmDelete )
                return;

            GUILayout.BeginHorizontal();
            
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal( MoreEditorStyles.RecessedStyle );

            GUILayout.Label( "Confirm Delete: ", EditorStyles.miniLabel );

            var originalColor = GUI.color;

            GUI.color = Color.red;
            if ( GUILayout.Button( "Delete", EditorStyles.miniButton ) )
                Build.PendingDelete = true;

            GUI.color = originalColor;
            if ( GUILayout.Button( "Cancel", EditorStyles.miniButton ) )
                ShowConfirmDelete = false;

            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();

            GUILayout.Space( 2 );
        }

        private void DrawBuildControls()
        {
            GUILayout.BeginVertical( MoreEditorStyles.RecessedStyle, GUILayout.Width( 150 ) );//, GUILayout.Height( 150 ) );

            Build.Target = EditorGUILayout.Popup(Build.Target, new[] { "Windows 64", "Mac OS Universal", "Linux Universal" });

            GUILayout.Space( 8 );

            Build.DoDeploy = GUILayout.Toggle( Build.DoDeploy, "Deploy on Build" );
            //Build.DoCopyFolders = GUILayout.Toggle( Build.DoCopyFolders, "Copy Folders" );
            Build.DoCopyLocalFolders = GUILayout.Toggle( Build.DoCopyLocalFolders, "Copy Local Folders" );
            Build.DoCopyLocalFiles = GUILayout.Toggle(Build.DoCopyLocalFiles, "Copy Local Files");
            

            if ( GUILayout.Button( "Build", GUILayout.ExpandWidth( true ) ) )
            {
                Build.PendingBuild = true;
            }

            EditorGUILayout.Separator();

            if ( GUILayout.Button( "Reveal Local", MoreEditorStyles.SmallButtonStyle ) )
            {
                Build.OpenLocal();
            }

            if ( GUILayout.Button( "Reveal Remote", MoreEditorStyles.SmallButtonStyle ) )
            {
                Build.OpenRemote();
            }

            GUILayout.Space( 8 );

            GUILayout.EndVertical();
        }



        private bool ShowScenes { get; set; }
        private bool ShowExcludedScenes { get; set; }

        private readonly List<string> m_ScenesToRemove = new List<string>();
        private List<string> ScenesToRemove { get { return m_ScenesToRemove; } }

        private GUIStyle SceneListStyle
        {
            get { return MoreEditorStyles.RecessedStyle; }
        }


        #region Scene Drawing

        private void DrawScenes()
        {
            GUILayout.BeginHorizontal();
            
            var foldoutText = string.Format( "Scenes ({0})", Build.Scenes.Count );
            ShowScenes = EditorGUILayout.Foldout( ShowScenes, foldoutText );

            GUILayout.FlexibleSpace();

            if ( ShowScenes )
            {
                ShowExcludedScenes = GUILayout.Toggle( ShowExcludedScenes, "Edit", EditorStyles.miniButton );
            }

            GUILayout.EndHorizontal();


            if ( ShowScenes )
            {
                ScenesToRemove.Clear();

                MoreEditorStyles.Indent();
                
                GUILayout.BeginVertical( SceneListStyle );

                // Hack for empty vertical rendering poorly.
                if ( !ShowExcludedScenes && Build.Scenes.Count == 0 )
                    GUILayout.Label( "", GUILayout.Height( 6 ) );

                foreach ( var sceneName in BuildManagerWindow.AllSceneNames )
                {
                    var isIncluded = Build.Scenes.Contains( sceneName );

                    if ( !ShowExcludedScenes && !isIncluded )
                        continue;

                    var isMissing = isIncluded && !BuildManagerWindow.AllSceneNames.Contains( sceneName );

                    if ( DrawSceneLine( sceneName, isIncluded, isMissing ) != isIncluded )
                    {
                        if ( isIncluded )
                        {
                            ScenesToRemove.Add( sceneName );
                        }
                        else
                        {
                            Build.Scenes.Add( sceneName );
                        }
                    }
                }


                GUILayout.EndVertical();
                MoreEditorStyles.Outdent();
            }

            foreach ( var s in ScenesToRemove )
            {
                Build.Scenes.Remove( s );
            }
        }

        private bool DrawSceneLine( string sceneName, bool included, bool missing )
        {
            var sceneDisplayName = sceneName;

            var oldColor = GUI.color;

            if ( !included )
                GUI.color = new Color( 0.7f,0.7f,0.7f);

            if ( missing )
            {
                GUI.color = Color.red;
                sceneDisplayName += " (missing)";
            }

            var includeScene = GUILayout.Toggle( included, sceneDisplayName );

            GUI.color = oldColor;

            return includeScene;
        }

        #endregion


        //private bool ShowFoldersToCopy { get; set; }
        //private readonly List < string > FolderCopiesToDelete = new List < string >(); 
        //private void DrawFolderCopies()
        //{
        //    GUILayout.BeginHorizontal();

        //    var foldoutText = String.Format( "Folders to Copy ({0})", Build.FoldersToCopy.Count );
        //    ShowFoldersToCopy = EditorGUILayout.Foldout( ShowFoldersToCopy, foldoutText );

        //    GUILayout.FlexibleSpace();

        //    if ( ShowFoldersToCopy )
        //    {
        //        if ( GUILayout.Button( "Add", EditorStyles.miniButton ) )
        //            HandleAddFolderCopyButtonPressed();
        //    }


        //    GUILayout.EndHorizontal();

        //    if ( ShowFoldersToCopy )
        //    {
        //        FolderCopiesToDelete.Clear();


        //        MoreEditorStyles.Indent();
                
        //        GUILayout.BeginVertical( SceneListStyle );

        //        // Hack for empty vertical rendering poorly.
        //        if ( Build.FoldersToCopy.Count == 0 )
        //            GUILayout.Label( "", GUILayout.Height( 6 ) );

        //        foreach ( var folderToCopy in Build.FoldersToCopy )
        //        {
        //            GUILayout.BeginHorizontal();

        //            GUILayout.Label( folderToCopy );

        //            if ( GUILayout.Button( "X", GUI.skin.label, GUILayout.ExpandWidth( false ) ) )
        //                FolderCopiesToDelete.Add( folderToCopy );

        //            GUILayout.EndHorizontal();
        //        }

        //        foreach ( var folder in FolderCopiesToDelete )
        //        {
        //            Build.FoldersToCopy.Remove( folder );
        //        }

        //        GUILayout.EndVertical();

        //        MoreEditorStyles.Outdent();
        //    }
        //}

        //private void HandleAddFolderCopyButtonPressed()
        //{
        //    var rootPath = Directory.GetCurrentDirectory();
        //    if(BuildManagerWindow.AuxiliaryDataRootEnvVar.Validated)
        //        rootPath = BuildManagerWindow.AuxiliaryDataRootEnvVar.Value;
        //    if ( !string.IsNullOrEmpty( rootPath ) )
        //        rootPath = Path.GetFullPath( rootPath );

        //    var folderPath = EditorUtility.OpenFolderPanel( "Select folder for copying", rootPath, "" );
        //    if ( !string.IsNullOrEmpty( folderPath ) )
        //        folderPath = Path.GetFullPath( folderPath );

        //    if ( string.IsNullOrEmpty( folderPath ) )
        //    {
        //        Debug.LogWarning( "Folder selection aborted." );
        //        return;
        //    }


        //    if ( !folderPath.StartsWith( rootPath ) )
        //    {
        //        Debug.LogWarning( "Selected folder must be descendent of " + rootPath + "(" + folderPath + ")" );
        //        return;
        //    }

        //    var relativeFolderPath = folderPath.Replace( rootPath, string.Empty );
        //    relativeFolderPath = relativeFolderPath.Replace( "\\", "/" );

        //    Build.FoldersToCopy.Add( relativeFolderPath );
        //}


        private bool ShowLocalFoldersToCopy { get; set; }
        private readonly List<string> LocalFolderCopiesToDelete = new List<string>();
        private void DrawLocalFolderCopies()
        {
            GUILayout.BeginHorizontal();

            var foldoutText = String.Format("Local Folders to Copy ({0})", Build.LocalFoldersToCopy.Count);
            ShowLocalFoldersToCopy = EditorGUILayout.Foldout(ShowLocalFoldersToCopy, foldoutText);

            GUILayout.FlexibleSpace();

            if (ShowLocalFoldersToCopy)
            {
                if (GUILayout.Button("Add", EditorStyles.miniButton))
                    HandleAddLocalFolderCopyButtonPressed();
            }

            GUILayout.EndHorizontal();

            if (ShowLocalFoldersToCopy)
            {
                LocalFolderCopiesToDelete.Clear();


                MoreEditorStyles.Indent();

                GUILayout.BeginVertical(SceneListStyle);

                // Hack for empty vertical rendering poorly.
                if (Build.LocalFoldersToCopy.Count == 0)
                    GUILayout.Label("", GUILayout.Height(6));

                foreach (var folderToCopy in Build.LocalFoldersToCopy)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(folderToCopy);

                    if (GUILayout.Button("X", GUI.skin.label, GUILayout.ExpandWidth(false)))
                        LocalFolderCopiesToDelete.Add(folderToCopy);

                    GUILayout.EndHorizontal();
                }

                foreach (var folder in LocalFolderCopiesToDelete)
                {
                    Build.LocalFoldersToCopy.Remove(folder);
                }

                GUILayout.EndVertical();

                MoreEditorStyles.Outdent();
            }
        }

        private void HandleAddLocalFolderCopyButtonPressed()
        {
            var rootPath = Directory.GetCurrentDirectory();

            var folderPath = EditorUtility.OpenFolderPanel("Select folder (within the project) for copying", rootPath, "");
            if (!string.IsNullOrEmpty(folderPath))
                folderPath = Path.GetFullPath(folderPath);

            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning("Folder selection aborted.");
                return;
            }


            // todo:  It would be good to *enforce* that the folder is within the actual project (and thus in version control)
            //if (!folderPath.StartsWith(rootPath))
            //{
            //    Debug.LogWarning("Selected folder must be descendent of " + rootPath + "(" + folderPath + ")");
            //    return;
            //}

            var relativeFolderPath = folderPath.Replace(rootPath, string.Empty);
            relativeFolderPath = relativeFolderPath.Replace("\\", "/");
            if (!string.IsNullOrEmpty( relativeFolderPath ) && relativeFolderPath.StartsWith( "/" ))
                relativeFolderPath = relativeFolderPath.Substring( 1 ); // Trim off the preceding slash

            Build.LocalFoldersToCopy.Add(relativeFolderPath);
        }


        private bool ShowLocalFilesToCopy { get; set; }
        private readonly List<string> LocalFileCopiesToDelete = new List<string>();
        private void DrawLocalFileCopies()
        {
            GUILayout.BeginHorizontal();

            var foldoutText = String.Format("Local Files to Copy ({0})", Build.LocalFilesToCopy.Count);
            ShowLocalFilesToCopy = EditorGUILayout.Foldout(ShowLocalFilesToCopy, foldoutText);

            GUILayout.FlexibleSpace();

            if (ShowLocalFilesToCopy)
            {
                if (GUILayout.Button("Add", EditorStyles.miniButton))
                    HandleAddLocalFileCopyButtonPressed();
            }

            GUILayout.EndHorizontal();

            if (ShowLocalFilesToCopy)
            {
                LocalFileCopiesToDelete.Clear();


                MoreEditorStyles.Indent();

                GUILayout.BeginVertical(SceneListStyle);

                // Hack for empty vertical rendering poorly.
                if (Build.LocalFilesToCopy.Count == 0)
                    GUILayout.Label("", GUILayout.Height(6));

                foreach (var fileToCopy in Build.LocalFilesToCopy)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.Label(fileToCopy);

                    if (GUILayout.Button("X", GUI.skin.label, GUILayout.ExpandWidth(false)))
                        LocalFileCopiesToDelete.Add(fileToCopy);

                    GUILayout.EndHorizontal();
                }

                foreach (var file in LocalFileCopiesToDelete)
                {
                    Build.LocalFilesToCopy.Remove(file);
                }

                GUILayout.EndVertical();

                MoreEditorStyles.Outdent();
            }
        }

        private void HandleAddLocalFileCopyButtonPressed()
        {
            var rootPath = Directory.GetCurrentDirectory();

            var filePath = EditorUtility.OpenFilePanel("Select individual file (within the project) for copying", rootPath, "*");
            if (!string.IsNullOrEmpty(filePath))
                filePath = Path.GetFullPath(filePath);  // Convert back to '\\' format that will match root path below
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogWarning("File selection aborted.");
                return;
            }

            var relativeFilePath = filePath.Replace(rootPath, string.Empty);
            relativeFilePath = relativeFilePath.Replace("\\", "/");
            if (!string.IsNullOrEmpty(relativeFilePath) && relativeFilePath.StartsWith("/"))
                relativeFilePath = relativeFilePath.Substring(1); // Trim off the preceding slash

            Build.LocalFilesToCopy.Add(relativeFilePath);
        }

        #endregion
    }
}
