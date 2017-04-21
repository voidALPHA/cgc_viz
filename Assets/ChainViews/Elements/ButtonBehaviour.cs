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
using UnityEngine;
using UnityEngine.UI;

namespace ChainViews.Elements
{
    public class ButtonBehaviour : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField]
        private Text m_TextComponent = null;
        private Text TextComponent { get { return m_TextComponent; } }

        [SerializeField]
        private Button m_ButtonComponent = null;
        private Button ButtonComponent { get { return m_ButtonComponent; } }

        public bool Enabled
        {
            get { return ButtonComponent.interactable; }
            set { ButtonComponent.interactable = value; }
        }

        public string LabelText
        {
            get { return TextComponent.text; }
            set { TextComponent.text = value; }
        }

        public event Action Clicked = delegate { };

        public void HandleClicked()
        {
            Debug.Log( "Button clicked, text is: " + LabelText );

            Clicked();
        }
    }
}
