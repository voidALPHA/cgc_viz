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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Choreography.Views.StepEditor.ControllableViews
{
    public class ControllableDropDownItemBehaviour : MonoBehaviour, IPointerClickHandler//, IPointerUpHandler, IPointerDownHandler
    {
        public event Action< ControllableDropDownItemBehaviour > Clicked = delegate { };

        [Header("Component References")]

        [SerializeField]
        private Text m_TextComponent = null;
        private Text TextComponent { get { return m_TextComponent; } }


        public string Text { get { return TextComponent.text; } }

        public void Initialize( string text, System.Object model )
        {
            Model = model;

            TextComponent.text = text;
        }

        public System.Object Model { get; private set; }


        public void OnPointerClick( PointerEventData eventData )
        {
            Clicked( this );
        }

        //public void OnPointerUp( PointerEventData eventData )
        //{
        //}

        //public void OnPointerDown( PointerEventData eventData )
        //{
        //}
    }
}