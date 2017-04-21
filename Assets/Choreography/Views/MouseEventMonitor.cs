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

namespace Choreography.Views
{
    public class MouseEventMonitor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public event Action MouseEntered = delegate { };
        public event Action MouseExited = delegate { };
        public event Action MouseDown = delegate { };
        public event Action MouseUp = delegate { };
        public event Action MouseClicked = delegate { };

        public void OnPointerEnter( PointerEventData eventData )
        {
            MouseEntered();
        }

        public void OnPointerExit( PointerEventData eventData )
        {
            MouseExited();
        }

        public void OnPointerDown( PointerEventData eventData )
        {
            MouseDown();
        }

        public void OnPointerUp( PointerEventData eventData )
        {
            MouseUp();
        }

        public void OnPointerClick( PointerEventData eventData )
        {
            MouseClicked();
        }
    }
}