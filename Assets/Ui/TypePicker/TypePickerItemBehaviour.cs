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
using System.Text;
using Assets.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility;

namespace Ui.TypePicker
{
    public class TypePickerItemBehaviour : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {

        public event Action Selected = delegate { };

        public event Action HoverRequested = delegate { };


        [Header("Component References")]

        [SerializeField]
        private Text m_TextComponent = null;
        private Text TextComponent { get { return m_TextComponent; } }

        [SerializeField]
        private Image m_BackgroundImageComponent = null;
        private Image BackgroundImageComponent { get { return m_BackgroundImageComponent; } }


        [Header("Configuration")]

        [SerializeField]
        private Color m_NormalBackgroundColor = new Color( 0.9f, 0.9f, 0.9f );
        private Color NormalBackgroundColor { get { return m_NormalBackgroundColor; } }

        [SerializeField]
        private Color m_HoveredBackgroundColor = new Color( 0.7f, 0.7f, 0.7f );
        private Color HoveredBackgroundColor { get { return m_HoveredBackgroundColor; } }


        #region Type Stuff

        private Type m_NodeType;
        public Type NodeType
        {
            get
            {
                return m_NodeType;
            }
            set
            {
                m_NodeType = value;
                
                GenerateTypeHierarchyString();

                UpdateLabelText();
            }
        }

        private void GenerateTypeHierarchyString()
        {
            var str = string.Empty;

            for ( var curType = NodeType; curType != null; curType = curType.BaseType )
                str = str.Insert( 0, curType.Name + "." );

            TypeHierarchyString = str;
        }

        private string TypeHierarchyString { get; set; }

        #endregion


        private string m_SearchTerm = string.Empty;
        public string SearchTerm
        {
            get { return m_SearchTerm; }
            set
            {
                m_SearchTerm = value;

                Visible = TypeHierarchyString.Contains( m_SearchTerm, StringComparison.InvariantCultureIgnoreCase );

                UpdateLabelText();
            }
        }

        private bool m_Visible = true;

        public bool Visible
        {
            get { return m_Visible; }
            set
            {
                m_Visible = value;

                gameObject.SetActive( m_Visible );
            }
        }



        private void UpdateLabelText()
        {
            if ( !Visible )
                return;

            if ( string.IsNullOrEmpty( SearchTerm ) || !NodeType.Name.Contains( SearchTerm, StringComparison.InvariantCultureIgnoreCase ) )
            {
                TextComponent.text = NodeType.Name;
                return;
            }


            var indices = new List< int >();

            var foundIndex = 0;
            while ( true )
            {
                foundIndex = NodeType.Name.IndexOf( SearchTerm, foundIndex, StringComparison.InvariantCultureIgnoreCase );

                if ( foundIndex == -1 )
                    break;
                
                indices.Add( foundIndex );

                foundIndex += SearchTerm.Length;
            }

            // Reverse so we can add from the back, not mess up indices as we go...
            indices.Reverse();

            var builder = new StringBuilder( NodeType.Name );

            foreach ( var openingIndex in indices )
            {
                var closingIndex = openingIndex + SearchTerm.Length;

                builder.Insert( closingIndex, "</b>" );
                builder.Insert( openingIndex, "<b>" );
            }

            TextComponent.text = builder.ToString();
        }


        
        
        
        public void OnPointerClick( PointerEventData eventData )
        {
            if ( eventData.used )
                return;

            eventData.Use();

            Selected();
        }

        private bool m_Hovered;
        public bool Hovered
        {
            get { return m_Hovered; }
            set
            {
                if ( m_Hovered == value )
                    return;

                m_Hovered = value;

                BackgroundImageComponent.color = m_Hovered ? HoveredBackgroundColor : NormalBackgroundColor;
            }
        }

        public TypePickerItemBehaviour NextItem { get; set; }

        public TypePickerItemBehaviour PreviousItem { get; set; }

        public void HoverNextVisibleItem()
        {
            var next = NextItem;
            while ( next != null && !next.Visible )
                next = next.NextItem;

            if ( next == null )
                return;

            next.HoverRequested();
        }

        public void HoverPreviousVisibleItem()
        {
            var previous = PreviousItem;
            while ( previous != null && !previous.Visible )
                previous = previous.PreviousItem;

            if ( previous == null )
                return;

            previous.HoverRequested();
        }

        public void Select()
        {
            Selected();
        }

        public void OnPointerEnter( PointerEventData eventData )
        {
            HoverRequested();
        }
    }
}