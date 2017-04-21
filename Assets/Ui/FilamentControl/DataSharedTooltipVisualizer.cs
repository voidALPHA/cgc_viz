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
using Visualizers;

namespace Ui.FilamentControl
{
    public class DataSharedTooltipVisualizer : Visualizer
    {
        [SerializeField]
        private string m_DisplayText = "";
        public string DisplayText { get { return m_DisplayText; } set { m_DisplayText = value; } }
        
        public Vector3 DrawPosition { get; set; }
        
        #region GUI Style
        
        private bool RedeclareStyles { get; set; }

        public void CheckRedeclareStyle()
        {
            if ( !RedeclareStyles )
                return;

            DeclareStyles();
            RedeclareStyles = false;
        }

        private int m_FontSize;

        public int FontSize
        {
            get { return m_FontSize; }
            set
            {
                if ( m_FontSize == value )
                    return;
                m_FontSize = value;
                RedeclareStyles = true;
            }
        }

        private Color m_TextColor = Color.black;

        public Color TextColor
        {
            get { return m_TextColor; }
            set
            {
                if ( m_TextColor == value ) return;
                m_TextColor = value;
                RedeclareStyles = true;
            }
        }

        private Color m_BackgroundColor = Color.gray;

        public Color BackgroundColor
        {
            get { return m_BackgroundColor; }
            set
            {
                if ( m_BackgroundColor == value )
                    return;
                m_BackgroundColor = value;
                RedeclareStyles = true;
            }
        }

        private GUIStyle GuiStyleFore { get; set; }
        private GUIStyle GuiStyleBack { get; set; }

        public void DeclareStyles()
        {
            GuiStyleBack = new GUIStyle
            {
                normal = { textColor = Color.white, background = WhiteTex() },

                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
                fontSize = FontSize
            };
            GuiStyleFore = new GUIStyle
            {
                normal = { textColor = TextColor },
                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
                fontSize = FontSize
            };
        }

        private void Start()
        {
            DeclareStyles();
        }

        private Texture2D WhiteTex()
        {
            var newTexture = new Texture2D(1, 1);
            newTexture.SetPixel(0, 0, BackgroundColor);
            return newTexture;
        }
        #endregion


        private void OnGUI()
        {
            if (DisplayText != "")
            {
                var x = DrawPosition.x;
                var y = Screen.height - DrawPosition.y;

                var UiString = DisplayText.Substring(0, Mathf.Min(140, DisplayText.Length));

                var labelSize = GuiStyleFore.CalcSize(new GUIContent(UiString));

                GUI.Box(new Rect(x - labelSize.x / 2 - 5, y + 15, labelSize.x + 10, labelSize.y + 10),
                    "", GuiStyleBack);
                GUI.Box(new Rect(x - labelSize.x / 2, y + 20, labelSize.x, labelSize.y),
                    UiString, GuiStyleFore);

            }
        }
    }
}
