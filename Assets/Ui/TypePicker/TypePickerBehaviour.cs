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
using System.Reflection;
using ChainViews;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility;

namespace Ui.TypePicker
{
    public class TypePickerBehaviour : MonoBehaviour, IEscapeQueueHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Component References")]

        [SerializeField]
        private RectTransform m_ItemsRoot = null;
        private RectTransform ItemsRoot { get { return m_ItemsRoot; } }


        [SerializeField]
        private InputField m_SearchInputField = null;
        private InputField SearchInputField { get { return m_SearchInputField; } }

        [SerializeField]
        private string m_TypeName = "Chains.ChainNode";
        private string TypeName { get { return m_TypeName; } }

        [SerializeField]
        private Text m_TitleTextComponent = null;
        private Text TitleTextComponent { get { return m_TitleTextComponent; } }

        [SerializeField]
        private ScrollRect m_ScrollRectComponent = null;
        private ScrollRect ScrollRectComponent { get { return m_ScrollRectComponent; } }


        [Header("Prefab References")]

        [SerializeField]
        private GameObject m_ItemPrefab = null;
        private GameObject ItemPrefab { get { return m_ItemPrefab; } }


        [Header("Configuration")]

        [SerializeField]
        private string m_TitleString = "Choose a Type";
        private string TitleString
        {
            set
            {
                m_TitleString = value;

                TitleTextComponent.text = value;
            }
        }


        private readonly List< TypePickerItemBehaviour > m_Items = new List< TypePickerItemBehaviour >();
        private List< TypePickerItemBehaviour > Items { get { return m_Items; } }


        public event Action< Type > TypeSelected = delegate { };

        public event Action Hidden = delegate { };

        public event Action Shown = delegate { };

        [UsedImplicitly]
        private void Start()
        {
            InitializeItems();

            InitializeItemNavigation();

            EnsureSomeItemIsHovered();

            TitleString = m_TitleString;
        }

        private void InitializeItems()
        {
            var assy = Assembly.GetExecutingAssembly();

            var typeToShow = assy.GetType( TypeName, throwOnError: false, ignoreCase: true );

            var allTypes = assy.GetTypes().Where(
                t => typeToShow.IsAssignableFrom( t )
                    && !t.IsAbstract
                    && !Attribute.IsDefined( t, typeof( TypePickerIgnore ) ) ).OrderBy( t => t.Name );

            Debug.LogFormat( "TypePicker found {0} concrete types that inherit from {1}", allTypes.Count(), TypeName );

            foreach ( var t in allTypes )
                AddItem( t );
        }

        private void InitializeItemNavigation()
        {
            for ( var curIndex = 0; curIndex < Items.Count; curIndex++ )
            {
                var current = Items[curIndex];

                var lastIndex = curIndex - 1;
                current.PreviousItem = lastIndex < 0 ? null : Items[lastIndex];

                var nextIndex = curIndex + 1;
                current.NextItem = nextIndex >= Items.Count ? null : Items[nextIndex];
            }
        }


        private void AddItem( Type nodeType )
        {
            var itemGo = Instantiate( ItemPrefab );
            var item = itemGo.GetComponent< TypePickerItemBehaviour >();

            item.NodeType = nodeType;

            item.Selected += () => HandleItemSelected( item );
            item.HoverRequested += () => HoveredItem = item;

            Items.Add( item );

            item.transform.SetParent( ItemsRoot, false );
        }

        private void HandleItemSelected( TypePickerItemBehaviour item )
        {
            var type = item.NodeType;

            TypeSelected( type );

            Hide();
        }

        private TypePickerItemBehaviour m_HoveredItem;
        private TypePickerItemBehaviour HoveredItem
        {
            get { return m_HoveredItem; }
            set
            {
                if ( m_HoveredItem != null )
                    m_HoveredItem.Hovered = false;

                m_HoveredItem = value;

                if ( m_HoveredItem != null )
                    m_HoveredItem.Hovered = true;
            }
        }

        [UsedImplicitly]
        public void HandleSearchTextChanged()
        {
            var searchTerm = SearchInputField.text;

            foreach ( var item in Items )
            {
                item.SearchTerm = searchTerm;
            }

            EnsureSomeItemIsHovered();

            EnsureHoveredItemIsVisible();
        }

        private void EnsureSomeItemIsHovered()
        {
            if ( HoveredItem == null || !HoveredItem.Visible )
                HoveredItem = Items.FirstOrDefault( i => i.Visible );
        }


        [UsedImplicitly]
        public void HandleSearchGotFocus()
        {
            // Don't auto-select on focus... Necessary to allow text to be added nicely when focus is just switched to it.

            this.ExecuteAtEndOfFrame( () => SearchInputField.MoveTextEnd( false ) );
        }


        // On hold (double-click-to-select-all)... Should probably detect click in text area, then forward to this...
        //[UsedImplicitly]
        //public void OnPointerDown( PointerEventData pointerEventData )
        //{
        //    Debug.Log( "CLuck of some kind" );
        //    if ( pointerEventData.clickCount == 2 )
        //    {
        //        Debug.Log( "Debule CLuck" );
        //        SearchInputField.MoveTextStart( false );
        //        SearchInputField.MoveTextEnd( true );
        //    }
        //}

        private float LastDownTime { get; set; }
        private float LastUpTime { get; set; }

        [SerializeField]
        private float m_RepeatDelay = 0.15f;
        private float RepeatDelay { get { return m_RepeatDelay; } }

        [SerializeField]
        private float m_RepeatPeriod = 0.04f;
        private float RepeatPeriod { get { return m_RepeatPeriod; } }

        [UsedImplicitly]
        private void Update()
        {
            //if ( !Input.anyKey )
            //    return;

            //if ( Input.GetKeyDown( KeyCode.Escape ) )
            //{
            //    Hide();
            //}
            if ( Input.GetKeyDown( KeyCode.Return ) )
            {
                if ( HoveredItem != null )
                    HoveredItem.Select();

                Hide();
            }
            else if ( Input.GetKey( KeyCode.DownArrow ) )
            {
                if ( Time.time > LastDownTime + RepeatPeriod )
                {
                    if ( HoveredItem != null )
                    {
                        HoveredItem.HoverNextVisibleItem();

                        EnsureHoveredItemIsVisible();
                    }

                    SearchInputField.MoveTextEnd( false );

                    LastDownTime = Time.time;

                    // Delay extra if first press
                    if ( Input.GetKeyDown( KeyCode.DownArrow ) )
                        LastDownTime += RepeatDelay;
                }
            }
            else if ( Input.GetKey( KeyCode.UpArrow ) )
            {
                if ( Time.time > LastUpTime + RepeatPeriod )
                {
                    if ( HoveredItem != null )
                    {
                        HoveredItem.HoverPreviousVisibleItem();

                        EnsureHoveredItemIsVisible();
                    }

                    SearchInputField.MoveTextEnd( false );

                    LastUpTime = Time.time;

                    if ( Input.GetKeyDown( KeyCode.UpArrow ) )
                        LastUpTime += RepeatDelay;
                }
            }
        }

        private void EnsureHoveredItemIsVisible()
        {
            if ( HoveredItem == null )
                return;

            var visibleItems = Items.Where( i => i.Visible ).ToList();
            var hoveredItemIndex = visibleItems.IndexOf( HoveredItem );
            var visibleChildCount = visibleItems.Count;

            var normalizePosition = (hoveredItemIndex) / (float)(visibleChildCount - 1);
            
            //Debug.LogFormat( "{0}/{1} is {2}", hoveredItemIndex, visibleChildCount, normalizePosition );

            SetScrollRectPosition( normalizePosition );
            this.ExecuteAtEndOfFrame( () => SetScrollRectPosition( normalizePosition ) );
        }

        private void SetScrollRectPosition( float normalizePosition )
        {
            ScrollRectComponent.verticalNormalizedPosition = 1 - normalizePosition;
        }

        #region Visbility

        [UsedImplicitly]
        public void HandleCloseClicked()
        {
            Hide();
        }

        public void Show()
        {
            gameObject.SetActive( true );

            SearchInputField.Select();

            SearchInputField.text = string.Empty;

            EscapeQueue.AddHandler( this );

            Shown();
        }

        public void Hide()
        {
            gameObject.SetActive( false );

            EscapeQueue.RemoveHandler( this );

            ChainView.Instance.AllowMouse = true;

            Hidden();
        }

        public void HandleEscape()
        {
            Hide();
        }

        #endregion


        public void OnPointerEnter(PointerEventData eventData)
        {
            ChainView.Instance.AllowMouse = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ChainView.Instance.AllowMouse = true;
        }
    }
}