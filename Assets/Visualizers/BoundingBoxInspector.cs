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


using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Visualizers
{
#if UNITY_EDITOR
    [CustomEditor(typeof(BoundingBox))]
    public class BoundingBoxInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            BoundingBox bound = (BoundingBox)target;

            GUILayout.Label( "Discriminated Values" );

            EditorGUILayout.BeginVertical();
            foreach (var discriminatedValue in bound.Data)
                GUILayout.Label( discriminatedValue.Key + ": " + discriminatedValue.Value);
            EditorGUILayout.EndVertical();
        }
    }
#endif
}
