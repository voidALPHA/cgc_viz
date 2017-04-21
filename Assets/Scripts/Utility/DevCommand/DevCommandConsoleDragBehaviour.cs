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

using UnityEngine;
using UnityEngine.EventSystems;

namespace Utility.DevCommand
{
    public class DevCommandConsoleDragBehaviour : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        private Vector2 pointerOffset;
        private RectTransform canvasRectTransform;
        private RectTransform panelRectTransform;

        private void Awake()
        {
            Canvas canvas = GetComponentInParent<Canvas>( );
            canvasRectTransform = canvas.transform as RectTransform;
            panelRectTransform = transform.parent as RectTransform;
        }

        public void OnPointerDown(PointerEventData data)
        {
            panelRectTransform.SetAsLastSibling();  // Ensure this is in front of everything
            RectTransformUtility.ScreenPointToLocalPointInRectangle( panelRectTransform, data.position, data.pressEventCamera, out pointerOffset );
        }

        public void OnDrag(PointerEventData data)
        {
            Vector2 pointerPosition = ClampToWindow( data );

            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, pointerPosition, data.pressEventCamera, out localPointerPosition))
            {
                panelRectTransform.localPosition = localPointerPosition - pointerOffset;
            }
        }

        // Don't allow user to drag the dev command console ALL the way off the screen
        private Vector2 ClampToWindow(PointerEventData data)
        {
            Vector3[] canvasCorners = new Vector3[4];
            canvasRectTransform.GetWorldCorners( canvasCorners );

            Vector2 rawPointerPosition = data.position;
            float clampedX = Mathf.Clamp(rawPointerPosition.x, canvasCorners[0].x, canvasCorners[2].x);
            float clampedY = Mathf.Clamp(rawPointerPosition.y, canvasCorners[0].y, canvasCorners[2].y);

            Vector2 newPointerPosition = new Vector2(clampedX, clampedY);
            return newPointerPosition;
        }
    }
}
