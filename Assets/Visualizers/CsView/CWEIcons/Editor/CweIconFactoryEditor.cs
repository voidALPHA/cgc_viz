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

using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Visualizers.CsView.CWEIcons.Editor
{
    [UsedImplicitly]
    [CustomEditor( typeof( CWEIconFactory ) )]
    public class CweIconFactoryEditor : UnityEditor.Editor
    {
        public CWEIconFactory Target { get { return target as CWEIconFactory; } }

        public override void OnInspectorGUI()
        {
            DrawButtons();

            base.OnInspectorGUI();
        }

        private string CweXmlPathPrefsKey { get { return "CweXmlPath"; } }
        private string CweXmlPath
        {
            get { return PlayerPrefs.GetString( CweXmlPathPrefsKey, "" ); }
            set { PlayerPrefs.SetString( CweXmlPathPrefsKey, value ); }
        }
    
        private void DrawButtons()
        {
            if ( GUILayout.Button( "Order By CWE Number" ) )
            {
                Target.OrderByCweNumber();
            }

            if ( GUILayout.Button( "Validate" ) )
            {
                Target.Validate();
            }

            if ( GUILayout.Button( "Dump" ) )
            {
                Target.Dump();
            }

            if ( GUILayout.Button( "Dump Wrapped" ) )
            {
                Target.DumpWrapped();
            }

            CweXmlPath = EditorGUILayout.TextField( "CWE XML Retrieve Path", CweXmlPath );

            if ( GUILayout.Button( "Retrieve and Dump" ) )
            {
                Target.Retrieve( CweXmlPath );
            }

            var origColor = GUI.color;
            GUI.color = Color.red;
            if ( GUILayout.Button( "Retrieve and Overwrite (hold ctrl+shift)" ) )
            {
                Target.Overwrite( CweXmlPath );
            }
            GUI.color = origColor;

            if ( GUILayout.Button( "Update Icon Textures" ) )
            {
                Target.AssignTextures();
            }
        }
    }
}
