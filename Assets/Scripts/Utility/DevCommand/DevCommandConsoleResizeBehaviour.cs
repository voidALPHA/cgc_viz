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
    public class DevCommandConsoleResizeBehaviour : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [SerializeField]
        private Vector2 m_MinSize = new Vector2(80.0f, 40.0f);
        private Vector2 MinSize { get { return m_MinSize; } set { m_MinSize = value; } }

        private Vector2 m_MaxSize = new Vector2(500.0f, 500.0f);
        private Vector2 MaxSize { get { return m_MaxSize; } set { m_MaxSize = value; } }

        private RectTransform rectTransform;
        private Vector2 currentPointerPosition;
        private Vector2 previousPointerPosition;

        private void Awake()
        {
            rectTransform = transform.parent.GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData data)
        {
            rectTransform.SetAsLastSibling();  // Ensure this is in front of everything
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out previousPointerPosition);
        }

        public void OnDrag(PointerEventData data)
        {
            MaxSize = SetMaxSize( );

            Vector2 sizeDelta = rectTransform.sizeDelta;

            RectTransformUtility.ScreenPointToLocalPointInRectangle( rectTransform, data.position, data.pressEventCamera, out currentPointerPosition );
            Vector2 resizeValue = currentPointerPosition - previousPointerPosition;

            sizeDelta += new Vector2(resizeValue.x, -resizeValue.y);
            sizeDelta = new Vector2( Mathf.Clamp( sizeDelta.x, MinSize.x, MaxSize.x ), Mathf.Clamp( sizeDelta.y, MinSize.y, MaxSize.y ) );

            rectTransform.sizeDelta = sizeDelta;

            previousPointerPosition = currentPointerPosition;

            var panelBehaviour = transform.parent.parent.GetComponent< DevCommandConsoleBehaviour >();
            var rt = panelBehaviour.TextInputField.GetComponent<RectTransform>();
            var newWidth = rt.rect.width + resizeValue.x;
            if ( newWidth < 28.0f ) // Cheesy I know
                newWidth = 28.0f;
            rt.sizeDelta = new Vector2(newWidth, rt.rect.height);
        }

        // Set max size to the current canvas size
        private Vector2 SetMaxSize()
        {
            Vector3[] canvasCorners = new Vector3[4];
            RectTransform canvasRectTransform = rectTransform.parent.GetComponent<RectTransform>( );
            canvasRectTransform.GetWorldCorners(canvasCorners);

            return new Vector2(canvasCorners[2].x, canvasCorners[2].y);
        }
    }
}
