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

using System.Linq;
using ChainViews;
using UnityEngine;
using UnityEngine.UI;
using Utility.InputManagement;

namespace Utility.DevCommand
{
    public class DevCommandConsoleBehaviour : MonoBehaviour
    {
        [SerializeField]
        private DevCommandManager m_DevCommandMgr;
        private DevCommandManager DevCommandMgr { get { return m_DevCommandMgr; } set { m_DevCommandMgr = value; } }

        public InputField TextInputField { get; set; }

        [SerializeField]
        private bool m_ShowConsole;
        public bool ShowConsole
        {
            get { return m_ShowConsole; }
            set
            {
                m_ShowConsole = value;
                var canvas = GetComponent<Canvas>();
                canvas.enabled = m_ShowConsole;
            }
        }

        private int CurrentHistoryIndex = 0;

        private void Start()
        {
            TextInputField = GetComponentInChildren<InputField>();

            // This doesn't quite work
            //ChainView.ErrorOccurred += HandleErrorOccurred;
        }

        private void Update()
        {
            if ( Input.GetButtonDown( "Toggle Dev Command Console" ) &&
                (!InputFocusManager.Instance.IsAnyInputFieldInFocus() || InputFocusManager.Instance.IsInputFieldInFocus(TextInputField)))
            {
                ShowConsole = !ShowConsole;
                if (ShowConsole)
                    TextInputField.ActivateInputField();
                else
                    TextInputField.DeactivateInputField();
            }

            if (TextInputField.isFocused)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    OnHistoryUpClicked();
                    TextInputField.MoveTextEnd(false);
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    OnHistoryDownClicked();
                    TextInputField.MoveTextEnd(false);
                }
            }
        }

        public void OnSubmit()
        {
            // We only want to submit when Enter is pressed, not when we simply lose focus
            if ( Input.GetKeyDown( KeyCode.Return ) || Input.GetKeyDown( KeyCode.KeypadEnter ) )
            {
                var enteredLine = TextInputField.text;
                DevCommandMgr.QueueDevCommand(enteredLine);

                CurrentHistoryIndex = DevCommandMgr.History.Count;

                TextInputField.ActivateInputField();
            }
        }

        private void OnHistoryUpClicked()
        {
            if (DevCommandMgr.History.Count > 0)
            {
                if (--CurrentHistoryIndex < 0)
                    CurrentHistoryIndex = DevCommandMgr.History.Count - 1;

                var list = DevCommandMgr.History.ToList();
                var line = list[CurrentHistoryIndex];

                TextInputField.text = line;
            }
        }

        private void OnHistoryDownClicked()
        {
            if (DevCommandMgr.History.Count > 0)
            {
                if (++CurrentHistoryIndex >= DevCommandMgr.History.Count)
                    CurrentHistoryIndex = 0;

                var list = DevCommandMgr.History.ToList();
                var line = list[CurrentHistoryIndex];

                TextInputField.text = line;
            }
        }

        //private void HandleErrorOccurred()
        //{
        //    Debug.Log("Error handling...turning on the dev console"  );
        //    ShowConsole = true;
        //}
    }
}
