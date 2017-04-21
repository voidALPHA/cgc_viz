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

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    [UsedImplicitly]
    public class DisablePixelPerfectOnDrag : MonoBehaviour
    {
        [SerializeField]
        private bool m_DoDisable = true;
        private bool DoDisable { get { return m_DoDisable; } set { m_DoDisable = value; } }
        
        [SerializeField]
        private Canvas m_CanvasToDisablePixelPerfectOn = null;
        private Canvas CanvasToDisablePixelPerfectOn { get { return m_CanvasToDisablePixelPerfectOn; } }


        private bool LookedForScrollRect { get; set; }
        private ScrollRect m_ScrollRect;
        private ScrollRect ScrollRect
        {
            get
            {
                if ( !LookedForScrollRect )
                {
                    m_ScrollRect = GetComponent< ScrollRect >();

                    LookedForScrollRect = true;
                }

                return m_ScrollRect;
            }
        }


        private bool LookedForDraggable { get; set; }
        private Draggable m_Draggable;
        private Draggable Draggable
        {
            get
            {
                if ( !LookedForDraggable )
                {
                    m_Draggable = GetComponent<Draggable>();

                    LookedForDraggable = true;
                }

                return m_Draggable;
            }
        }



        private bool m_IsScrolling;
        private bool IsScrolling
        {
            get { return m_IsScrolling; }
            set
            {
                if ( m_IsScrolling == value )
                    return;

                m_IsScrolling = value;

                CanvasToDisablePixelPerfectOn.pixelPerfect = !m_IsScrolling;
            }
        }


        [UsedImplicitly]
        private void Update()
        {
            var isScrolling = false;

            if ( ScrollRect != null )
                if ( ScrollRect.velocity.sqrMagnitude >= 0.0001f )
                    isScrolling = true;

            if ( Draggable != null )
                if ( m_Draggable.IsDragging )
                    isScrolling = true;

            IsScrolling = isScrolling;
        }
    }
}
