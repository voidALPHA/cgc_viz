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
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.UI;

namespace ChainViews.Elements
{
    public class TextBoxBehaviour : MonoBehaviour
    {
        [Header( "Component References" )]
        [SerializeField]
        private InputField m_InputFieldComponent = null;
        private InputField InputFieldComponent { get { return m_InputFieldComponent; } }

        [SerializeField]
        private Text m_LabelTextComponent;
        private Text LabelTextComponent { get { return m_LabelTextComponent; } }


        public event Action<string> TextSubmitted = delegate { };

        public event Action<string> TextChanged = delegate { };


        public string LabelText
        {
            get { return LabelTextComponent.text; }
            set { LabelTextComponent.text = value; }
        }

        public string Text
        {
            get { return InputFieldComponent.text; }
            set { InputFieldComponent.text = value; }
        }

        public bool Interactable
        {
            set { InputFieldComponent.interactable = value; }
        }


        public void HandleTextSubmitted()
        {
            TextSubmitted( Text );
        }

        public void HandleTextChanged()
        {
            TextChanged( Text );
        }
    }
}
