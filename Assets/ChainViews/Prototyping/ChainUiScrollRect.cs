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
using UnityEngine.UI;

namespace ChainViews
{
    public class ChainUiScrollRect : ScrollRect
    {
        [SerializeField]
        private RectTransform m_ItemRootRect = null;
        private RectTransform ItemRootRect { get { return m_ItemRootRect; } }

        private RectTransform m_RectTransform;
        private RectTransform RectTransform
        {
            //get { return transform.GetChild( 0 ).GetComponent<RectTransform>(); }
            get { return m_RectTransform ?? (m_RectTransform = GetComponent< RectTransform >()); }
        }
        
        public override void OnScroll( PointerEventData data )
        {
            var rectSize = RectTransform.rect.size;

            Vector2 localRectPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle( RectTransform, Input.mousePosition, null, out localRectPos );

            Debug.Log( localRectPos );

            var pivotPosition = new Vector2
            {
                x = RectTransform.pivot.x * rectSize.x,
                y = RectTransform.pivot.y * rectSize.y
            };

            var rectMousePos = new Vector2
            {
                x = localRectPos.x + pivotPosition.x,
                y = localRectPos.y + pivotPosition.y
            };



            Debug.Log( "Rect mousePos:" + rectMousePos );
            

            var relativeRectMousePos = new Vector2( rectMousePos.x / RectTransform.sizeDelta.x, rectMousePos.y / RectTransform.sizeDelta.y );

            Debug.Log( "Relative pos " + relativeRectMousePos );

            var pivot = new Vector2( relativeRectMousePos.x, relativeRectMousePos.y );


            RectTransform.SetPivot( pivot );

            


            var scrollDirection = Mathf.Sign( data.scrollDelta.y );

            var scaleFactor = 1.0f + scrollDirection * 0.1f;

            var finalScale = RectTransform.localScale.x * scaleFactor;

            if ( finalScale > 1.0f )
                finalScale = 1.0f;

            //transform.parent.GetComponent< CanvasScaler >().scaleFactor = finalScale; //

            RectTransform.localScale = new Vector3(finalScale, finalScale, 1.0f);
        }
    }

    public static class RectTransformExtensions
    {
        public static void SetPivot( this RectTransform rectTransform, Vector2 pivot )
        {
            if ( rectTransform == null ) return;

            Vector2 size = rectTransform.rect.size;
            Vector2 deltaPivot = rectTransform.pivot - pivot;
            Vector3 deltaPosition = new Vector3( deltaPivot.x * size.x, deltaPivot.y * size.y );
            rectTransform.pivot = pivot;
            rectTransform.localPosition -= deltaPosition;
        }
    }
}
