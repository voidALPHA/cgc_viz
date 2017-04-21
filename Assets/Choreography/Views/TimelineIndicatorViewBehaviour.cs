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
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Choreography.Views
{
    public class TimelineIndicatorViewBehaviour : MonoBehaviour
    {

        [Serializable]
        public class State
        {
            [SerializeField]
            private string m_Key = null;
            public string Key { get { return m_Key; } }

            [SerializeField]
            private Sprite m_DotSprite = null;
            public Sprite DotSprite { get { return m_DotSprite; } }

            [SerializeField]
            private Color m_DotColor = Color.white;
            public Color DotColor { get { return m_DotColor; } }

            [SerializeField]
            private Color m_BackgroundColor;
            public Color BackgroundColor { get { return m_BackgroundColor; } }

            [SerializeField]
            private Color m_TextColor = Color.white;
            public Color TextColor { get { return m_TextColor; } }
        }




        [Header( "Component References" )]

        [SerializeField]
        private Image m_BackgroundComponent = null;
        private Image BackgroundComponent { get { return m_BackgroundComponent; } }

        [SerializeField]
        private Image m_DotImageComponent = null;
        private Image DotImageComponent { get { return m_DotImageComponent; } }

        [SerializeField]
        private Text m_LabelTextComponent = null;
        private Text LabelTextComponent { get { return m_LabelTextComponent; } }

        

        [Header("State Configuration")]

        [SerializeField]
        private State m_TurnedOnState = new State();
        private State TurnedOnState { get { return m_TurnedOnState; } }

        [SerializeField]
        private State m_TurnedOffState = new State();
        private State TurnedOffState { get { return m_TurnedOffState; } }

        [SerializeField]
        private List< State > m_OtherStates = null;
        private List< State > OtherStates { get { return m_OtherStates; } }


        private bool m_IsTurnedOn;
        public bool IsTurnedOn
        {
            get { return m_IsTurnedOn; }
            set
            {
                m_IsTurnedOn = value;

                if ( m_IsTurnedOn )
                    TurnOn();
                else
                    TurnOff();
            }
        }

        public void SetOtherState( string stateKey )
        {
            var state = OtherStates.FirstOrDefault( s => s.Key == stateKey );

            ApplyState( state );
        }

        private void TurnOn()
        {
            ApplyState( TurnedOnState );
        }

        private void TurnOff()
        {
            ApplyState( TurnedOffState );
        }

        private void ApplyState( State state )
        {
            BackgroundComponent.color = state.BackgroundColor;

            LabelTextComponent.color = state.TextColor;

            DotImageComponent.sprite = state.DotSprite;

            DotImageComponent.color = state.DotColor;
        }
    }
}
