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
using UnityEngine.UI;

namespace Visualizers.DisassemblyWindow
{
    public class CommsEntryVisualizer : MonoBehaviour
    {
        [Header("Component References")]

        [SerializeField]
        private Text m_TextComponent = null;
        private Text TextComponent { get { return m_TextComponent; } }
        
        [SerializeField]
        private Graphic m_BackgroundComponent = null;
        private Graphic BackgroundComponent { get { return m_BackgroundComponent; } }

        [SerializeField]
        private LayoutGroup m_GroupToAlign = null;
        private LayoutGroup GroupToAlign { get { return m_GroupToAlign; } }


        private string m_Text;
        public string Text
        {
            get { return m_Text; }
            set
            {
                m_Text = value ?? string.Empty;

                TextComponent.text = m_Text;
            }
        }

        private bool m_IsFromRequestSide;
        public bool IsFromRequestSide
        {
            get { return m_IsFromRequestSide; }
            set
            {
                m_IsFromRequestSide = value;

                var alignment = m_IsFromRequestSide ? TextAnchor.LowerLeft : TextAnchor.LowerRight;

                GroupToAlign.childAlignment = alignment;
            }
        }

        private Color m_Color;
        public Color Color
        {
            get { return m_Color; }
            set
            {
                m_Color = value;

                BackgroundComponent.color = m_Color;
            }
        }

        public void Embiggen()
        {
            TextComponent.fontSize = 40;
            //GetComponent< LayoutElement >().preferredHeight = 42;
        }
    }
}
