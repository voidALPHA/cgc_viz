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
using UnityEngine;

namespace ChainViews.Editor
{

    // IngredientDrawer
    [CustomPropertyDrawer( typeof( ChainNodeView.TypeColorPair ) )]
    public class TypeColorPairPropertyDrawer : PropertyDrawer
    {

        // Draw the property inside the given rect
        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty( position, label, property );

            // Draw label
            position = EditorGUI.PrefixLabel( position, GUIUtility.GetControlID( FocusType.Passive ), label );

            // Don't make child fields be indented
            var oldIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            var colorRect = new Rect( position.x, position.y, position.width, position.height );
            EditorGUI.PropertyField( colorRect, property.FindPropertyRelative( "m_Color" ), GUIContent.none );

            // Set indent back to what it was
            EditorGUI.indentLevel = oldIndentLevel;

            EditorGUI.EndProperty();
        }
    }

}
