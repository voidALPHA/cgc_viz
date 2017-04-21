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
using ChainViews.Elements;
using JetBrains.Annotations;
using Mutation;
using PayloadSelection.CriterionStatements;
using UnityEngine;
using UnityEngine.UI;

namespace PayloadSelection.UI
{
    public class CriterionStatementViewBehaviour : MonoBehaviour, IBoundsChanger
    {

        public event Action BoundsChanged = delegate { };

        public RectTransform BoundsRectTransform { get { return GetComponent<RectTransform>(); } }

        [Header("Component References")]
        
        [SerializeField]
        private RectTransform m_ElementAttachmentPoint = null;
        private RectTransform ElementAttachmentPoint { get { return m_ElementAttachmentPoint; } }


        [Header("Prefab References")]

        [SerializeField]
        private GameObject m_MutableBoxPrefab = null;
        private GameObject MutableBoxPrefab { get { return m_MutableBoxPrefab; } }

        [SerializeField]
        private GameObject m_SymbolViewPrefab = null;
        private GameObject SymbolViewPrefab { get { return m_SymbolViewPrefab; } }



        public void Initialize( CriterionStatement criterionStatement, ISchemaProvider schemaProvider )
        {
            SchemaProvider = schemaProvider;

            CriterionStatement = criterionStatement;
        }



        #region Model

        private CriterionStatement m_CriterionStatement;
        public CriterionStatement CriterionStatement
        {
            get { return m_CriterionStatement; }
            private set
            {
                if ( m_CriterionStatement != null )
                {
                    throw new InvalidOperationException("Cannot reuse this view.");
                }

                m_CriterionStatement = value;

                if ( m_CriterionStatement != null )
                {
                    GenerateElements();
                }
            }
        }

        #endregion


        private ISchemaProvider SchemaProvider { get; set; }


        #region Elements

        private readonly List< MonoBehaviour > m_Elements = new List< MonoBehaviour >();
        private List< MonoBehaviour > Elements
        {
            get { return m_Elements; }
        }

        private void GenerateElements()
        {
            var propertiesToDisplay =
                CriterionStatement.GetType()
                    .GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where( e => Attribute.IsDefined( e, typeof( CriterionDescriptionElementAttribute ), true ) );

            foreach ( var propertyInfo in propertiesToDisplay )
            {
                if ( typeof( IMutableField ).IsAssignableFrom( propertyInfo.PropertyType ) )
                    GenerateMutableBox( propertyInfo );
                else if ( propertyInfo.PropertyType == typeof( string ) )
                    GenerateSymbol( propertyInfo );
                else
                    throw new NotImplementedException( "Cannot handle type " + propertyInfo.PropertyType );
            }

            Resize();
        }

        private void GenerateMutableBox( PropertyInfo propertyInfo )
        {
            var go = Instantiate( MutableBoxPrefab );
            var mutableBox = go.GetComponent< MutableBoxBehaviour >();

            mutableBox.BoundsChanged += Resize;

            mutableBox.ShowLabel = false;

            mutableBox.Initialize( propertyInfo, CriterionStatement, SchemaProvider );

            mutableBox.transform.SetParent( ElementAttachmentPoint, false );


            Elements.Add( mutableBox );
        }

        private void GenerateSymbol( PropertyInfo propertyInfo )
        {
            var go = Instantiate( SymbolViewPrefab );
            var symbolView = go.GetComponent< Text >();

            
            var symbolText = (string)propertyInfo.GetValue( CriterionStatement, null );
            symbolView.text = symbolText;


            go.transform.SetParent( ElementAttachmentPoint, false );


            Elements.Add( symbolView );
        }


        #endregion


        private void Resize()
        {
            var height = 0.0f;
            if ( Elements.Any() )
                height = Elements.Max( e => e.GetComponent< LayoutElement >().preferredHeight );

            height += 24.0f;

            GetComponent< LayoutElement >().preferredHeight = height;

            BoundsChanged();
        }

        [UsedImplicitly]
        public void HandleRemoveButtonClicked()
        {
            CriterionStatement.RequestRemoval();
        }
    }
}
