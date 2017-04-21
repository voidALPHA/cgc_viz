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
using UnityEngine.EventSystems;

namespace Ui
{
    [UsedImplicitly]
    public class ResizeHandleBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {

        // TODO: This does not support pivots other than top/left (or just top if stretching in X, or just left if stretching in Y).


        private enum ResizeHandleRegion
        {
            Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left, TopLeft
        }

        [Header( "Project References" )]


        [SerializeField]
        private Texture2D m_VerticalCursor = null;
        private Texture2D VerticalCursor { get { return m_VerticalCursor; } }

        [SerializeField]
        private Texture2D m_HorizontalCursor = null;
        private Texture2D HorizontalCursor { get { return m_HorizontalCursor; } }

        [SerializeField]
        private Texture2D m_TopRightCursor = null;
        private Texture2D TopRightCursor { get { return m_TopRightCursor; } }

        [SerializeField]
        private Texture2D m_TopLeftCursor = null;
        private Texture2D TopLeftCursor { get { return m_TopLeftCursor; } }


        [Header( "Configuration" )]

        
        [SerializeField]
        private RectTransform m_TargetTransform = null;
        private RectTransform TargetTransform { get { return m_TargetTransform; } }


        [SerializeField]
        private ResizeHandleRegion m_Region = ResizeHandleRegion.Top;
        private ResizeHandleRegion Region { get { return m_Region; } }


        private IResizeHandleTarget m_Target;
        private IResizeHandleTarget TargetBehaviour
        {
            get
            {
                if ( m_Target == null )
                {
                    m_Target = TargetTransform.GetComponent<IResizeHandleTarget>();
                }

                return m_Target;
            }
        }



        private bool MouseDownOnMe { get; set; }

        private Vector2 MouseStartPosition { get; set; }

        private Vector2 OriginalTargetSize { get; set; }

        private Vector2 OriginalTargetPosition { get; set; }


        public void OnPointerEnter( PointerEventData eventData )
        {
            // Use event data?

            if ( MouseDownOnMe )
                return;

            if ( TargetBehaviour != null)
                if ( !TargetBehaviour.SuppressResize )
                    UseResizeCursor();
        }

        
        public void OnPointerExit( PointerEventData eventData )
        {
            // Use event data?

            if ( MouseDownOnMe )
                return;

            if ( TargetBehaviour != null )
                if ( !TargetBehaviour.SuppressResize )
                    UseDefaultCursor();
        }

        public void OnPointerDown( PointerEventData eventData )
        {
            if ( TargetBehaviour != null )
                if ( TargetBehaviour.SuppressResize )
                    return;

            eventData.Use();

            MouseDownOnMe = true;


            MouseStartPosition = eventData.position;

            OriginalTargetSize = TargetTransform.sizeDelta;

            OriginalTargetPosition = TargetTransform.anchoredPosition;
        }


        public void OnPointerUp( PointerEventData eventData )
        {
            if ( !MouseDownOnMe )
                return;

            eventData.Use();

            MouseDownOnMe = false;

            if ( TargetBehaviour != null )
                TargetBehaviour.OnFinalResize();

            UseDefaultCursor();
        }

        private void UseResizeCursor()
        {
            Texture2D cursor = null;

            switch ( Region )
            {
                case ResizeHandleRegion.Top:
                case ResizeHandleRegion.Bottom:
                    cursor = VerticalCursor;
                    break;
                case ResizeHandleRegion.Left:
                case ResizeHandleRegion.Right:
                    cursor = HorizontalCursor;
                    break;
                case ResizeHandleRegion.TopLeft:
                case ResizeHandleRegion.BottomRight:
                    cursor = TopLeftCursor;
                    break;
                case ResizeHandleRegion.TopRight:
                case ResizeHandleRegion.BottomLeft:
                    cursor = TopRightCursor;
                    break;
            }

            Cursor.SetCursor( cursor, new Vector2( 16, 16 ), CursorMode.Auto );
        }

        private static void UseDefaultCursor()
        {
            Cursor.SetCursor( null, Vector2.zero, CursorMode.Auto );
        }

        [UsedImplicitly]
        private void Update()
        {
            if ( !MouseDownOnMe )
                return;

            var desiredSizeDelta = (Vector2)Input.mousePosition - MouseStartPosition;

            if ( desiredSizeDelta.magnitude < 1.0f )
                return;

            if ( IsLeft )
                desiredSizeDelta.x *= -1;
            if ( IsBottom )
                desiredSizeDelta.y *= -1;

            if ( !IsHorizontal || TargetStretchingInX )
                desiredSizeDelta.x = 0;

            if ( !IsVertical || TargetStretchingInY)
                desiredSizeDelta.y = 0;

            
            var newSize = new Vector2( OriginalTargetSize.x + desiredSizeDelta.x, OriginalTargetSize.y + desiredSizeDelta.y );


            newSize = ClampSize( newSize );

            TargetTransform.sizeDelta = newSize;


            // Size done, on to position...


            var actualSizeDelta = OriginalTargetSize - newSize;

            var newPosition = OriginalTargetPosition;

            if ( IsTop  && !TargetStretchingInY )
                newPosition.y -= actualSizeDelta.y;
            if ( IsLeft && !TargetStretchingInX )
                newPosition.x += actualSizeDelta.x;

            TargetTransform.anchoredPosition = newPosition;
            
            
            
            //Debug.LogFormat( "AnchorMin[{0},{1}] AnchorMax[{2},{3}]", TargetTransform.anchorMin.x,
            //    TargetTransform.anchorMin.y, TargetTransform.anchorMax.x, TargetTransform.anchorMax.y );

            if ( TargetBehaviour != null )
                TargetBehaviour.OnPartialResize();
        }

        private Vector2 ClampSize( Vector2 newSize )
        {
            // Clamping if stretching is bad!  Stretched dimensions are often negative. Who knows why, but, don't clamp...

            if ( TargetBehaviour != null )
            {
                if ( !TargetStretchingInY )
                {
                    if ( TargetBehaviour.MaxResizeSize.y > 0.0f )
                        if ( TargetBehaviour.MaxResizeSize.y < newSize.y )
                            newSize.y = TargetBehaviour.MaxResizeSize.y;

                    if ( TargetBehaviour.MinResizeSize.y > 0.0f )
                        if ( TargetBehaviour.MinResizeSize.y > newSize.y )
                            newSize.y = TargetBehaviour.MinResizeSize.y;
                }

                if ( !TargetStretchingInX )
                {
                    if ( TargetBehaviour.MaxResizeSize.x > 0.0f )
                        if ( TargetBehaviour.MaxResizeSize.x < newSize.x )
                            newSize.x = TargetBehaviour.MaxResizeSize.x;

                    if ( TargetBehaviour.MinResizeSize.x > 0.0f )
                        if ( TargetBehaviour.MinResizeSize.x > newSize.x )
                            newSize.x = TargetBehaviour.MinResizeSize.x;
                }
            }

            return newSize;
        }

        private bool IsVertical
        {
            get { return Region != ResizeHandleRegion.Left && Region != ResizeHandleRegion.Right; }
        }
        private bool IsHorizontal
        {
            get { return Region != ResizeHandleRegion.Top && Region != ResizeHandleRegion.Bottom; }
        }

        private bool IsRight
        {
            get { return Region == ResizeHandleRegion.TopRight || Region == ResizeHandleRegion.Right || Region == ResizeHandleRegion.BottomRight; }
        }

        private bool IsLeft
        {
            get { return Region == ResizeHandleRegion.TopLeft || Region == ResizeHandleRegion.Left || Region == ResizeHandleRegion.BottomLeft; }
        }

        private bool IsTop
        {
            get { return Region == ResizeHandleRegion.TopRight || Region == ResizeHandleRegion.Top || Region == ResizeHandleRegion.TopLeft; }
        }

        private bool IsBottom
        {
            get { return Region == ResizeHandleRegion.BottomRight || Region == ResizeHandleRegion.Bottom || Region == ResizeHandleRegion.BottomLeft; }
        }

        private bool TargetStretchingInX
        {
            get
            {
                var isStretchingInX = Mathf.Approximately( TargetTransform.anchorMin.x, 0.0f ) && Mathf.Approximately( TargetTransform.anchorMax.x, 1.0f );
                //Debug.Log("Is stretching in X: " + isStretchingInX );
                return isStretchingInX;
            }
        }

        private bool TargetStretchingInY
        {
            get
            {
                var isStretchingInY = Mathf.Approximately( TargetTransform.anchorMin.y, 0.0f ) && Mathf.Approximately( TargetTransform.anchorMax.y, 1.0f );
                //Debug.Log( "Is stretching in Y: " + isStretchingInY );
                return isStretchingInY;
            }
        }
    }
}
