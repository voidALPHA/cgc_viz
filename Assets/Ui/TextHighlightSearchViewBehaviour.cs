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
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace Ui
{
    public class TextHighlightSearchViewBehaviour : MonoBehaviour, IEscapeQueueHandler
    {
        [Header("Component References")]

        [SerializeField]
        private GameObject m_VisibilityToggleGameObject = null;
        private GameObject VisibilityToggleGameObject { get { return m_VisibilityToggleGameObject; } }

        [SerializeField]
        private InputField m_InputFieldComponent = null;
        private InputField InputFieldComponent { get { return m_InputFieldComponent; } }


        [Header("Prefab References")]
        
        [SerializeField]
        private GameObject m_HighlightPrefab = null;
        private GameObject HighlightPrefab { get { return m_HighlightPrefab; } }


        [Header("Configuration")]

        [SerializeField]
        private List< GameObject > m_SearchTargets = null;
        private List< GameObject > SearchTargets { get { return m_SearchTargets; } }



        [UsedImplicitly]
        public void Start()
        {
            Hide();
        }

        [UsedImplicitly]
        public void Update()
        {
            if ( Input.GetButtonDown( "Show Find Menu" ) )
            {
                if ( Showing )
                    Hide();
                else
                    Show();
            }
        }

        private bool Showing { get; set; }

        private void Show()
        {
            EscapeQueue.AddHandler( this );

            VisibilityToggleGameObject.SetActive( true );

            InputFieldComponent.Select();

            Showing = true;
        }

        private void Hide()
        {
            EscapeQueue.RemoveHandler( this );

            VisibilityToggleGameObject.SetActive( false );

            EndFind();

            Showing = false;
        }

        public void HandleEscape()
        {
            Hide();
        }


        #region UI Callbacks

        [UsedImplicitly]
        public void HandleFindButtonPressed()
        {
            HandleFindSubmitted( true );
        }

        [UsedImplicitly]
        public void HandleFindSubmitted( bool force = false )
        {
            if ( !(force || Input.GetKeyDown( KeyCode.Return ) || Input.GetKeyDown( KeyCode.KeypadEnter ) ) )
                return;

            var text = InputFieldComponent.text;

            if ( string.IsNullOrEmpty( text ) )
            {
                EndFind();
                return;
            }

            Find( text );
        }

        [UsedImplicitly]
        public void HandleClearPressed()
        {
            EndFind();

            InputFieldComponent.text = string.Empty;
        }

        [UsedImplicitly]
        public void HandleClosePressed()
        {
            Hide();
        }

        #endregion


        
        private readonly List< GameObject > m_Highlights = new List< GameObject >();
        private List< GameObject > Highlights { get { return m_Highlights; } }


        private void Find( string text )
        {
            EndFind();

            var matchingTextComponents = new List< Text >();
            foreach ( var searchTarget in SearchTargets )
            {
                var textComponents = searchTarget.GetComponentsInChildren< Text >();

                foreach ( var component in textComponents )
                {
                    if ( component.text.Contains( text, StringComparison.InvariantCultureIgnoreCase ) )
                        matchingTextComponents.Add( component );
                }
            }

            foreach ( var textComponent in matchingTextComponents )
            {
                var highlight = Instantiate( HighlightPrefab );

                highlight.transform.SetParent( textComponent.transform, false );

                Highlights.Add( highlight );
            }

            Debug.Log( matchingTextComponents.Count + " matching occurrences of \"" + text + "\" were found" );
        }

        private void EndFind()
        {
            foreach ( var highlight in Highlights )
            {
                Destroy( highlight.gameObject );
            }

            Highlights.Clear();
        }
    }
}
