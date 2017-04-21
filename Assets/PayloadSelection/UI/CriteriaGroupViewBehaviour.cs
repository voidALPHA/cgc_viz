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
using JetBrains.Annotations;
using Mutation;
using PayloadSelection.CriterionStatements;
using UnityEngine;
using UnityEngine.UI;

namespace PayloadSelection.UI
{
    public class CriteriaGroupViewBehaviour : MonoBehaviour , ISchemaProvider
    {

        [Header("Component References")]
        
        [SerializeField]
        private RectTransform m_CriterionStatementViewAttachmentPoint = null;
        private RectTransform CriterionStatementViewAttachmentPoint { get { return m_CriterionStatementViewAttachmentPoint; } }
        
        [SerializeField]
        private RectTransform m_AddButtonWrapper = null;
        private RectTransform AddButtonWrapper { get { return m_AddButtonWrapper; } }

        [SerializeField]
        private Text m_ConjuctionButtonText = null;
        private Text ConjuctionButtonText { get { return m_ConjuctionButtonText; } }


        [Header("Prefab References")]
        
        [SerializeField]
        private GameObject m_CriterionStatementViewPrefab = null;
        private GameObject CriterionStatementViewPrefab { get { return m_CriterionStatementViewPrefab; } }

        public event Action CriteriaChanged = delegate { };
        
        #region Model Binding

        private CriteriaGroup m_CriteriaGroup;
        public CriteriaGroup CriteriaGroup
        {
            get
            {
                return m_CriteriaGroup;
            }
            set
            {
                if ( m_CriteriaGroup != null )
                {
                    throw new InvalidOperationException("Cannot reuse this view.");
                }

                m_CriteriaGroup = value;

                if ( m_CriteriaGroup != null )
                {
                    m_CriteriaGroup.CriterionAdded += HandleCriteriaGroupCriterionAdded;
                    m_CriteriaGroup.CriterionRemoved += HandleCriteriaGroupCriterionRemoved;
                    m_CriteriaGroup.ConjuctionChanged += HandleCriteriaGroupConjuctionChanged;
                    m_CriteriaGroup.CriteriaChanged += HandleCriteriaChanged;
                    // Adding more events? Hit up Unbind...

                    ConjuctionButtonText.text = m_CriteriaGroup.Conjunction.ToString().ToUpper();

                    GenerateCriterionStatementViews();
                }
            }
        }

        private void Unbind()
        {
            m_CriteriaGroup.CriterionAdded -= HandleCriteriaGroupCriterionAdded;
            m_CriteriaGroup.CriterionRemoved -= HandleCriteriaGroupCriterionRemoved;
            m_CriteriaGroup.ConjuctionChanged -= HandleCriteriaGroupConjuctionChanged;
            m_CriteriaGroup.CriteriaChanged -= HandleCriteriaChanged;
        }


        private void HandleCriteriaGroupCriterionAdded( CriterionStatement criterionStatement )
        {
            AddCriterionStatementView( criterionStatement );

            CriteriaChanged();
        }

        private void HandleCriteriaGroupCriterionRemoved( CriterionStatement criterionStatement )
        {
            RemoveCriterionStatementView( criterionStatement );

            CriteriaChanged();
        }

        private void HandleCriteriaGroupConjuctionChanged( CriteriaConjunction conjuction )
        {
            ConjuctionButtonText.text = conjuction.ToString().ToUpper();

            CriteriaChanged();
        }

        private void HandleCriteriaChanged(CriterionStatement updatedStatement)
        {
            CriteriaChanged();
        }


        #endregion


        #region Child (Criterion Statement) View Management

        private readonly List<CriterionStatementViewBehaviour> m_CriterionStatementViews = new List<CriterionStatementViewBehaviour>();
        private List<CriterionStatementViewBehaviour> CriterionStatementViews { get { return m_CriterionStatementViews; } }

        private void GenerateCriterionStatementViews()
        {
            foreach ( var criterionStatement in m_CriteriaGroup.CriteriaEnumerable )
            {
                AddCriterionStatementView( criterionStatement );
            }
        }

        private void DestroyCriterionStatementViews()
        {
            foreach ( var view in CriterionStatementViews.ToList() )
            {
                RemoveCriterionStatementView( view.CriterionStatement );
            }
        }

        private void AddCriterionStatementView( CriterionStatement criterionStatement )
        {
            var go = Instantiate( CriterionStatementViewPrefab );
            var view = go.GetComponent< CriterionStatementViewBehaviour >();

            view.BoundsChanged += HandleCriteriaStatementViewBoundsChanged;

            view.Initialize( criterionStatement, this );

            view.transform.SetParent( CriterionStatementViewAttachmentPoint, false );

            CriterionStatementViews.Add( view );
            

            AddButtonWrapper.SetAsLastSibling();

            UpdateHeight();
        }

        private void RemoveCriterionStatementView( CriterionStatement criterionStatement )
        {
            var view = CriterionStatementViews.FirstOrDefault( v => v.CriterionStatement == criterionStatement );

            if ( view == null )
                throw new InvalidOperationException("Could not find view for criterion statement.");

            
            view.BoundsChanged -= HandleCriteriaStatementViewBoundsChanged;

            Destroy( view.gameObject );

            CriterionStatementViews.Remove( view );

            UpdateHeight();
        }

        private void HandleCriteriaStatementViewBoundsChanged()
        {
            UpdateHeight();
        }

        #endregion


        private void UpdateHeight()
        {
            var height = 50.0f;    // Titlebar, add-button, padding...

            foreach ( var view in CriterionStatementViews )
            {
                height += view.GetComponent< LayoutElement >().preferredHeight;
                //height += 2.0f;
            }

            GetComponent< LayoutElement >().preferredHeight = height;
        }


        #region Buttons

        [UsedImplicitly]
        public void HandleDeleteButtonPressed()
        {
            if ( CriteriaGroup != null )
                CriteriaGroup.RequestDeletion();
        }

        [UsedImplicitly]
        public void HandleAddButtonPressed()
        {
            PayloadExpressionViewBehaviour.Instance.ShowAddCriterionStatementDialog( CriteriaGroup );
        }

        [UsedImplicitly]
        public void HandleConjuctionButtonClicked()
        {
            CriteriaGroup.Conjunction = CriteriaGroup.Conjunction == CriteriaConjunction.And
                ? CriteriaConjunction.Or 
                : CriteriaConjunction.And;
        }

        #endregion

        [UsedImplicitly]
        private void OnDestroy()
        {
            Unbind();

            DestroyCriterionStatementViews();
        }

        private MutableObject m_Schema = new MutableObject
        {
            { "Empty Schema", "Yup.  Still empty."}
        };

        public MutableObject Schema
        {
            get { return m_Schema; }
            set { m_Schema = value; }
        }

        public string SchemaName { get { return "Criteria Group Schema"; }}

        private ISchemaProvider m_ExternalSchemaProvider = null;
        public ISchemaProvider ExternalSchemaProvider { get { return m_ExternalSchemaProvider; } set
        {
            m_ExternalSchemaProvider = value;
        } }


        public virtual void CacheSchema()
        {
            if (ExternalSchemaProvider!=null)
                ExternalSchemaProvider.CacheSchema();
        }

        public virtual void UnCacheSchema()
        {
            if (ExternalSchemaProvider != null)
                ExternalSchemaProvider.UnCacheSchema();
        }
    }
}
