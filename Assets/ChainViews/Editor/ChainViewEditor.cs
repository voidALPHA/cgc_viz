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

using UnityEditor;

namespace ChainViews.Editor
{
    [CustomEditor( typeof( ChainView ) ) ]
    public class ChainViewEditor : UnityEditor.Editor
    {
        private ChainView Target { get { return target as ChainView; } }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DrawButtons();
        }

        private void DrawButtons()
        {
            //if ( GUILayout.Button( "Add Controller View" ) )
            //{
            //    Target.AddView( null );
            //    EditorUtility.SetDirty( Target );
            //}

            //if ( GUILayout.Button( "Remove Controller Views" ) )
            //{
            //    Target.RemoveViews();
            //    EditorUtility.SetDirty( Target );
            //}
        }
    }
}
