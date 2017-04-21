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
using JetBrains.Annotations;
using Ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ChainViews
{
    public class ChainGroupViewBackgroundPanelBehaviour : MonoBehaviour, IPointerClickHandler//, IScrollHandler
    {
        public event Action< PointerEventData > Clicked = delegate { };

        public event Action< ChainGroupView > PreviewGroupViewDrop = delegate { };

        public event Action< ChainGroupView > GroupViewDropped = delegate { };
        
        public event Action< ChainNodeView > PreviewNodeViewDrop = delegate { };

        public event Action< ChainNodeView > NodeViewDropped = delegate { };


        #region Lazy Init Component Properties

        private Draggable m_Draggable;
        public Draggable Draggable
        {
            get { return m_Draggable ?? (m_Draggable = GetComponent< Draggable >()); }
        }

        private RectTransform m_RectTransform;
        public RectTransform RectTransform
        {
            get { return m_RectTransform ?? ( m_RectTransform = GetComponent< RectTransform >() ); }
        }

        private Image m_ImageComponent;
        private Image ImageComponent
        {
            get { return m_ImageComponent ?? ( m_ImageComponent = GetComponent< Image >() ); }
        }

        #endregion


        public Color Color { set { ImageComponent.color = value; } }


        public void OnPointerClick( PointerEventData eventData )
        {
            Clicked( eventData );
        }

        //public void OnScroll( PointerEventData eventData )
        //{
        //    if ( Math.Abs( eventData.scrollDelta.y ) < 0.001f )
        //        return;

        //    ChainView.Instance.Zoom( Math.Sign( eventData.scrollDelta.y ), eventData.position );
        //}


        #region Drag and Drop


        [UsedImplicitly]
        private void OnDragEnter( Object draggedObject )
        {
            var groupView = draggedObject as ChainGroupView;

            if ( groupView != null )
            {
                PreviewGroupViewDrop( groupView );
                return;
            }


            var nodeView = draggedObject as ChainNodeView;

            if ( nodeView != null )
            {
                PreviewNodeViewDrop( nodeView );
                return;
            }
        }

        [UsedImplicitly]
        private void OnDragExit( Object draggedObject )
        {
            var groupView = draggedObject as ChainGroupView;

            if ( groupView != null )
            {
                PreviewGroupViewDrop( null );
                return;
            }


            var nodeView = draggedObject as ChainNodeView;

            if ( nodeView != null )
            {
                PreviewNodeViewDrop( null );
                return;
            }

        }

        [UsedImplicitly]
        private void OnDrop( Object droppedObject )
        {

            var groupView = droppedObject as ChainGroupView;

            if ( groupView != null )
            {
                AcceptGroupView( groupView );
                return;
            }


            var nodeView = droppedObject as ChainNodeView;

            if ( nodeView != null )
            {
                AcceptNodeView( nodeView );
                return;
            }
        }


        private void AcceptGroupView( ChainGroupView groupView )
        {
            GroupViewDropped( groupView );
        }

        private void AcceptNodeView( ChainNodeView nodeView )
        {
            NodeViewDropped( nodeView );
        }


        #endregion
    }
}
