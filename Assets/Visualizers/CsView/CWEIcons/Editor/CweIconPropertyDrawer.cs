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

namespace Visualizers.CsView.CWEIcons.Editor
{
    [CustomPropertyDrawer( typeof( CWEIcon ) )]
    public class CweIconPropertyDrawer : PropertyDrawer
    {
        private float PropertyHeight
        {
            get { return 80.0f; }
        }

        public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
        {
            return PropertyHeight;
        }

        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
        {
            EditorGUI.BeginProperty( position, label, property );

            var labelRect = new Rect( position.x, position.y, position.width, 16 );
            EditorGUI.LabelField( labelRect, property.displayName );

            var oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel ++;

            var widthToTexture = position.width - 54;

            var length = property.FindPropertyRelative( "m_Description" ).stringValue.Length;

            var idRect = new Rect( position.x, position.y + 18, widthToTexture, 16 );
            var descriptionRect = new Rect( position.x, position.y + 36, widthToTexture-20, 16 );
            var lengthRect = new Rect( position.x + widthToTexture - 50, position.y + 36, 60, 16 );
            var textureRect = new Rect( position.x, position.y + 54, widthToTexture, 16 );
            var texturePreviewRect = new Rect( position.x + widthToTexture + 8, position.y + 18 + 5, 42, 42 );


            EditorGUI.PropertyField( idRect, property.FindPropertyRelative( "m_Name" ), new GUIContent("Name") );
            
            EditorGUI.PropertyField( descriptionRect, property.FindPropertyRelative( "m_Description" ), new GUIContent( "Description" ) );
            
            var oldColor = GUI.contentColor;
            GUI.contentColor = length <= 36 ? Color.green : length <= 40 ? Color.yellow : Color.red;
            EditorGUI.LabelField( lengthRect, length.ToString() );
            GUI.contentColor = oldColor;
            
            EditorGUI.PropertyField( textureRect, property.FindPropertyRelative( "m_Texture" ), new GUIContent( "Texture" ) );
            
            var texture = property.FindPropertyRelative( "m_Texture" ).objectReferenceValue as Texture2D;
            if ( texture != null)
                EditorGUI.DrawPreviewTexture( texturePreviewRect, texture, null, ScaleMode.ScaleToFit );
            else
                EditorGUI.DrawRect( texturePreviewRect, Color.black );


            EditorGUI.indentLevel = oldIndent;

            EditorGUI.EndProperty();
        }
    }
}
