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
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Builds.Editor
{
    public class BuildManifestView
    {

        public BuildManifestView( BuildManifest manifest )
        {
            Manifest = manifest;
        }

        private BuildManifest m_Manifest;
        private BuildManifest Manifest
        {
            get { return m_Manifest; }
            set
            {
                m_Manifest = value;

                foreach ( var build in m_Manifest.Builds )
                    BuildViews.Add( new BuildEntryView( build ) );

                m_Manifest.BuildAdded += HandleBuildAdded;
                m_Manifest.BuildRemoved += HandleBuildRemoved;
            }
        }

        private Vector2 ScrollVector2 = Vector2.zero;


        private void HandleBuildAdded( BuildEntry build )
        {
            BuildViews.Add( new BuildEntryView( build ) );
        }

        private void HandleBuildRemoved( BuildEntry build )
        {
            var view = BuildViews.First( b => b.Build == build );

            BuildViews.Remove( view );
        }

        private List< BuildEntryView > m_BuildViews = new List < BuildEntryView >();
        private List<BuildEntryView> BuildViews
        {
            get { return m_BuildViews; }
        }

        public void DrawGui()
        {
            DrawMainButtons();

            DrawBuilds();
        }
        
        private void DrawMainButtons()
        {
            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal(  );


            if ( GUILayout.Button( "Add Build" ) )
            {
                Manifest.AddBuild();
            }


            //EditorGUILayout.Space();


            GUI.enabled = BuildViews.Any(view => view.BuildChecked);

            if ( GUILayout.Button( "Build Checked Builds" ) )
            {
                foreach(var buildView in BuildViews)
                {
                    if(!buildView.BuildChecked) continue;

                    buildView.Build.Build();
                }
            }

            GUI.enabled = true;


            GUILayout.EndHorizontal();
        }

        private void DrawBuilds()
        {
            ScrollVector2 = GUILayout.BeginScrollView(ScrollVector2, false, true);

            foreach ( var buildView in BuildViews )
            {
                buildView.DrawGui();
            }

            GUILayout.EndScrollView();
        }
    }
}
