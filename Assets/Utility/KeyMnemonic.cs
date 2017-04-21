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
using UnityEngine.Events;
using UnityEngine.UI;
using Utility.InputManagement;

namespace Utility
{
    public class KeyMnemonic : MonoBehaviour
    {
        [SerializeField]
        private KeyCode m_Key = KeyCode.A;
        private KeyCode Key { get { return m_Key; } }

        [SerializeField]
        private string m_buttonName = "";

        private bool PressingButton
        {
            get
            {
                return Key == KeyCode.None
                    ? Input.GetButtonDown(m_buttonName) // Requiring user-configurable button, return that
                    : Input.GetKeyDown(Key); // Requiring a specific key, return that
            }
        }

        [SerializeField]
        private bool m_requireCtrl = false;
        private bool RequireCtrl { get { return m_requireCtrl; } }
        private bool Ctrl { get { return !(RequireCtrl ^ (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))); } }

        [SerializeField]
        private bool m_requireShift = false;
        [SerializeField]
        private bool m_requireShiftInEditor = false;
        private bool Shift
        {
            get
            {
#if UNITY_EDITOR
                return !(m_requireShiftInEditor ^ (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)));
#else
                return !(m_requireShift ^ (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)));
#endif
            }
        }

        [SerializeField]
        private bool m_selfTest = false;
        private bool SelfTest { get { return m_selfTest; } }

        private Button Button { get; set; }

        [Serializable]
        private class MnemonicKeypressEvent : UnityEvent {}

        [SerializeField] private MnemonicKeypressEvent KeyPressed;

        public void Start()
        {
            Button = GetComponent< Button >();
        }

        public void Update()
        {
            if(SelfTest)
                Test();
        }

        /// <summary>
        /// <para>Performs the keybinding test.  Is called by Update if SelfTest is true, but will require being called elsewhere if SelfTest is false.</para>
        /// <para>See ChainView.Update for an example of SelfTest being false.</para>
        /// </summary>
        public void Test()
        {
            if (PressingButton // Pressing required key
                && Shift // Pressing either Shift if required, not holding Shift if not required
                && Ctrl // Pressing either Ctrl if required, not holding Ctrl if not required
                && (RequireCtrl || !InputFocusManager.Instance.IsAnyInputFieldInFocus())) // Either we require Ctrl (and thus don't care if we're in an input field) or we're not in a field)
                if (Button && Button.interactable) // Make sure the button could be clicked normally...
                    Button.onClick.Invoke( );
                else
                    KeyPressed.Invoke();
        }
    }
}
