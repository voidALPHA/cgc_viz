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
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utility.Editor
{
    public static class MoreEditorStyles
    {
     
        private class TextureIdentifier
        {
            public Color Foreground { get; set; }

            public Color Background { get; set; }

            public TextureIdentifier( Color foregroundColor, Color backgroundColor )
            {
                Foreground = foregroundColor;

                Background = backgroundColor;
            }
        }


        [MenuItem( "Utility/MoreEditorStyles/Reset" )]
        public static void Reset()
        {
            Debug.Log( "Resetting MoreEditorStyles." );

            FreeAllTextures();
        }

        private static Dictionary < TextureIdentifier, Texture2D > TextureRecords { get; set; }

        static MoreEditorStyles()
        {
            //Debug.Log( "Constructing MoreEditorStyles." );

            TextureRecords = new Dictionary < TextureIdentifier, Texture2D >();
        }

        private static void FreeAllTextures()
        {
            //Debug.Log( "Freeing all textures." );
            foreach ( var texture in TextureRecords.Values )
            {
                if ( texture != null )
                    Object.DestroyImmediate( texture );
            }

            TextureRecords.Clear();
        }

        public static void SetStyleBorder( GUIStyle style, Color foregroundColor )
        {
            SetStyleBorder( style, foregroundColor, new Color(0,0,0,0) );
        }

        public static void SetStyleBorder( GUIStyle style, Color foregroundColor, Color backgroundColor )
        {
            var identifier = new TextureIdentifier( foregroundColor, backgroundColor );

            Texture2D tex;

            if ( TextureRecords.ContainsKey( identifier ) )
            {
                tex = TextureRecords[identifier];
            }
            else
            {
                var texSize = 16;

                tex = new Texture2D( texSize, texSize, TextureFormat.ARGB32, false, true );

                for ( int y = 0; y < texSize; y++ )
                    for ( int x = 0; x < texSize; x++ )
                        if ( x == 0 || y == 0 || x == texSize - 1 || y == texSize - 1 )
                            tex.SetPixel( x, y, foregroundColor );
                        else
                            tex.SetPixel( x, y, backgroundColor );

                tex.Apply( false );

                style.border = new RectOffset( 4, 4, 4, 4 );
            }

            style.normal.background = tex;
            style.active.background = tex;
            style.hover.background = tex;
            style.focused.background = tex;
        }

        #region Universal Styles

        private static GUIStyle s_RecessedStyle = null;
        public static GUIStyle RecessedStyle
        {
            get
            {
                if ( s_RecessedStyle == null )
                {
                    s_RecessedStyle = new GUIStyle();
                    SetStyleBorder( s_RecessedStyle, new Color( 0, 0, 0, 0.5f ), new Color( 0, 0, 0, 0.5f ) );
                }

                return s_RecessedStyle;
            }
        }

        private static GUIStyle s_HyperlinkStyle = null;
        public static GUIStyle HyperlinkStyleLabel
        {
            get
            {
                if ( s_HyperlinkStyle == null )
                {
                    var color = new Color( 0.5f, 0.5f, 0.9f );

                    s_HyperlinkStyle = new GUIStyle( EditorStyles.label );

                    s_HyperlinkStyle.normal.textColor = color;
                    s_HyperlinkStyle.active.textColor = color;
                    s_HyperlinkStyle.hover.textColor = color;
                    s_HyperlinkStyle.focused.textColor = color;

                    s_HyperlinkStyle.fontStyle = FontStyle.Italic;

                    s_HyperlinkStyle.margin = new RectOffset();
                }

                return s_HyperlinkStyle;
            }
        }

        private static GUIStyle s_SmallButtonStyle = null;
        public static GUIStyle SmallButtonStyle
        {
            get
            {
                if ( s_SmallButtonStyle == null )
                {
                    s_SmallButtonStyle = new GUIStyle( EditorStyles.miniButton );

                    s_SmallButtonStyle.margin = new RectOffset(15,15,0,0);
                }

                return s_SmallButtonStyle;
            }
        }

        private static Stack < bool > EnabledStates = new Stack < bool >();

        public static void PushEnabled( bool enabled )
        {
            EnabledStates.Push( GUI.enabled );
            GUI.enabled = enabled;
        }

        public static void PopEnabled()
        {
            if ( EnabledStates.Count <= 0 )
                GUI.enabled = true;

            GUI.enabled = EnabledStates.Pop();
        }

        #endregion

        #region

        public static void Indent()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space( 15 );
            GUILayout.BeginVertical();
        }

        public static void Outdent()
        {
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        #endregion
    }
}
