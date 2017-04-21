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

using Scripts.Utility.Misc;
using UnityEngine;
using Utility.Misc;

namespace Visualizers.AnnotationIcon
{
    public class AnnotationTooltippedIcon : AnnotationIcon
    {
        // tooltip referenced roughly from http://answers.unity3d.com/questions/44811/tooltip-when-mousing-over-a-game-object.html

        private string m_AnnotationText = "";
        public override string AnnotationText { get { return m_AnnotationText; } set { m_AnnotationText = value; } }

        private string m_CurrentToolTipText = "";
        private string CurrentToolTipText { get { return m_CurrentToolTipText; } set { m_CurrentToolTipText = value; } }

        private GUIStyle GuiStyleFore { get; set; }
        private GUIStyle GuiStyleBack { get; set; }

        private void Start()
        {
            GuiStyleBack = new GUIStyle
            {
                normal = { textColor = Color.white, background = WhiteTex()},
                
                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
                fontSize = 18
            };
            GuiStyleFore = new GUIStyle
            {
                normal = { textColor = Color.black},
                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
                fontSize = 18
            };

        }

        private Texture2D WhiteTex()
        {
            var newTexture = new Texture2D(1, 1);
            newTexture.SetPixel(0,0,Color.gray);
            return newTexture;
        }

        private void OnMouseEnter()
        {
            if ( HaxxisGlobalSettings.Instance.IsVgsJob == false && HaxxisGlobalSettings.Instance.DisableEditor == false)  // We don't want inadvertent rollovers during video generation.  At some level we may not want rollovers at all anymore.
            {
                CurrentToolTipText = AnnotationText;
            }
        }

        private void OnMouseExit()
        {
            CurrentToolTipText = "";
        }

        private void OnGUI()
        {
            if (CurrentToolTipText != "")
            {
                var x = Event.current.mousePosition.x;
                var y = Event.current.mousePosition.y;

                var UiString = CurrentToolTipText.Substring(0, Mathf.Min(140, CurrentToolTipText.Length));

                var labelSize = GuiStyleFore.CalcSize(new GUIContent(UiString));

                GUI.Box(new Rect(x - labelSize.x / 2 - 5, y + 15, labelSize.x + 10, labelSize.y + 10),
                    "", GuiStyleBack);
                GUI.Box(new Rect(x - labelSize.x/2, y + 20, labelSize.x, labelSize.y),
                    UiString, GuiStyleFore);

            }
        }

    }
}
