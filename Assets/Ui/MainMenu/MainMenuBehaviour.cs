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
using Adapters.TraceAdapters.Commands;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using Utility.Undo;
#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Ui.MainMenu
{
    public class MainMenuBehaviour : MonoBehaviour, IEscapeQueueHandler
    {
        
        [SerializeField]
        private Canvas m_Canvas = null;
        private Canvas Canvas { get { return m_Canvas; } }
        
        [SerializeField]
        private InputField m_UrlInputField = null;
        private InputField UrlInputField { get { return m_UrlInputField; } }
        
        [SerializeField]
        private InputField m_UndoCapacityInputField = null;
        private InputField UndoCapacityInputField { get { return m_UndoCapacityInputField; } }


        private bool IsShowing { get; set; }

        private void Show()
        {
            Canvas.gameObject.SetActive( true );

            EscapeQueue.AddHandler( this );

            IsShowing = true;
        }

        private void Hide()
        {
            Canvas.gameObject.SetActive( false );

            EscapeQueue.RemoveHandler( this );

            IsShowing = false;
        }

        public void HandleEscape()
        {
            Hide();
        }

        #region Unity Callbacks

        [UsedImplicitly]
        private void Start()
        {
            Hide();

            UrlInputField.text = CommandProcessor.Url;
            UndoCapacityInputField.text = "" + UndoLord.Instance.UndoCapacity;
        }

        [UsedImplicitly]
        public void Update()
        {
            if ( Input.GetKeyDown( KeyCode.F2 ) )
            {
                if ( IsShowing )
                    Hide();
                else
                    Show();
            }
        }

        #endregion



        #region UI Callbacks

        [UsedImplicitly]
        public void HandleCloseButtonClicked()
        {
            Hide();
        }

        [UsedImplicitly]
        public void HandleUrlSubmitted()
        {
            CommandProcessor.Url = UrlInputField.text;
        }

        [UsedImplicitly]
        public void HandleUndoCapcitySubmitted()
        {
            try
            {
                UndoLord.Instance.UndoCapacity = int.Parse(UndoCapacityInputField.text);
            } catch(Exception) { }
        }

        [UsedImplicitly]
        public void HandleExitHaxxisClicked()
        {
        #if UNITY_EDITOR
            if ( EditorApplication.isPlaying )
                EditorApplication.isPlaying = false;
            else
        #endif
                Application.Quit();
        }

        #endregion
    }
}
