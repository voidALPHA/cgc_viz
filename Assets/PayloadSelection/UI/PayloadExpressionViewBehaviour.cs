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
using ChainViews;
using JetBrains.Annotations;
using Mutation;
using UnityEngine;
using Utility;
using VoidAlpha.Utilities;

namespace PayloadSelection.UI
{
    public class PayloadExpressionViewBehaviour : MonoBehaviour, IEscapeQueueHandler
    {

        #region Serialized Properties

        
        [Header("Component References")]

        [SerializeField]
        private Canvas m_Canvas = null;
        private Canvas Canvas { get { return m_Canvas; } }

        [SerializeField]
        private RectTransform m_CriteriaGroupAttachmentPoint = null;
        private RectTransform CriteriaGroupAttachmentPoint { get { return m_CriteriaGroupAttachmentPoint; } }

        [SerializeField]
        private RectTransform m_AddGroupButtonWrapperComponent = null;
        private RectTransform AddGroupButtonWrapperComponent { get { return m_AddGroupButtonWrapperComponent; } }

        [SerializeField]
        private AddCriterionStatementDialogBehaviour m_AddCriterionStatementDialog = null;
        private AddCriterionStatementDialogBehaviour AddCriterionStatementDialog { get { return m_AddCriterionStatementDialog; } }

        [Header( "Prefab References" )]

        [SerializeField]
        private GameObject m_CriteriaGroupViewPrefab = null;
        private GameObject CriteriaGroupViewPrefab { get { return m_CriteriaGroupViewPrefab; } }

        

        #endregion

        private static PayloadExpressionViewBehaviour s_Instance;
        public static PayloadExpressionViewBehaviour Instance
        {
            get
            {
                if ( s_Instance == null )
                    s_Instance = FindObjectOfType< PayloadExpressionViewBehaviour >();

                return s_Instance;
            }
        }

        
        private readonly List< CriteriaGroupViewBehaviour > m_CriteriaGroupViews = new List< CriteriaGroupViewBehaviour >();
        private List< CriteriaGroupViewBehaviour > CriteriaGroupViews { get { return m_CriteriaGroupViews; } }
        
        
        private PayloadExpression m_PayloadExpression;
        public PayloadExpression PayloadExpression
        {
            get
            {
                return m_PayloadExpression;
            }
            set
            {
                if ( m_PayloadExpression != null )
                {
                    m_PayloadExpression.CriteriaGroupAdded -= HandlePayloadExpressionCriteriaGroupAdded;
                    m_PayloadExpression.CriteriaGroupRemoved -= HandlePayloadExpressionCriteriaGroupRemoved;
                    m_PayloadExpression.CriteriaGroupsCleared -= HandlePayloadExpressionCriteriaGroupsCleared;

                    DestroyCriteriaGroupViews();
                }

                m_PayloadExpression = value;

                if ( m_PayloadExpression != null )
                {
                    ComputeSchema();

                    GenerateCriteriaGroupViews();

                    m_PayloadExpression.CriteriaGroupAdded += HandlePayloadExpressionCriteriaGroupAdded;
                    m_PayloadExpression.CriteriaGroupRemoved += HandlePayloadExpressionCriteriaGroupRemoved;
                    m_PayloadExpression.CriteriaGroupsCleared += HandlePayloadExpressionCriteriaGroupsCleared;
                }
            }
        }

        public ISchemaProvider ExternalSchemaProvider { get; set; }


        private void HandlePayloadExpressionCriteriaGroupAdded( CriteriaGroup criteriaGroup )
        {
            AddCriteriaGroupView( criteriaGroup );
        }

        private void HandlePayloadExpressionCriteriaGroupRemoved( CriteriaGroup criteriaGroup )
        {
            RemoveCriteriaGroupView( criteriaGroup );
        }

        private void HandlePayloadExpressionCriteriaGroupsCleared()
        {
            ClearCriteriaGroupViews();
        }


        private void GenerateCriteriaGroupViews()
        {
            foreach ( var criteriaGroup in PayloadExpression.CriteriaGroupsEnumerable )
            {
                AddCriteriaGroupView( criteriaGroup, true);
            }

            ComputeSchema();
            
            // update mutable boxes

            ShowNewSchema();
        }

        private void ShowNewSchema()
        {

            if (ExternalSchemaProvider!=null)
                ExternalSchemaProvider.CacheSchema();

            foreach (var group in CriteriaGroupViews)
                foreach (var statement in group.CriteriaGroup.CriteriaEnumerable)
                    statement.UpdateSchema(group.Schema);

            if (ExternalSchemaProvider!=null)
                ExternalSchemaProvider.UnCacheSchema();
        }

        private void DestroyCriteriaGroupViews()
        {
            foreach ( var view in CriteriaGroupViews.ToList() )    // ToList to avoid modifying collection
            {
                RemoveCriteriaGroupView( view.CriteriaGroup, true);
            }
            ComputeSchema();
        }

        private void AddCriteriaGroupView( CriteriaGroup criteriaGroup, bool ignoreSchemaChange=false )
        {
            var viewGo = Instantiate( CriteriaGroupViewPrefab );
            var view = viewGo.GetComponent<CriteriaGroupViewBehaviour>();
            

            // set external schemas
            view.ExternalSchemaProvider = ExternalSchemaProvider;

            view.CriteriaGroup = criteriaGroup;

            view.transform.SetParent( CriteriaGroupAttachmentPoint, false );

            CriteriaGroupViews.Add( view );


            AddGroupButtonWrapperComponent.transform.SetAsLastSibling();

            view.CriteriaChanged += HandleCriteriaGroupChanged;

            if (!ignoreSchemaChange)
            {
                HighlightSelection();
                ComputeSchema();
                
                ShowNewSchema();
            }
        }

        private void RemoveCriteriaGroupView(CriteriaGroup criteriaGroup, bool ignoreSchemaChange = false)
        {
            var view = CriteriaGroupViews.FirstOrDefault( v => v.CriteriaGroup == criteriaGroup );

            if ( view == null )
                return;

            Destroy( view.gameObject );

            CriteriaGroupViews.Remove(view);

            view.CriteriaChanged -= HandleCriteriaGroupChanged;

            if (!ignoreSchemaChange)
            {
                HighlightSelection();
                ComputeSchema();

                ShowNewSchema();
            }
        }

        private void ClearCriteriaGroupViews(bool ignoreSchemaChange=false )
        {
            foreach ( var view in CriteriaGroupViews )
            {
                if (view == null)
                    return;

                Destroy(view.gameObject);

                CriteriaGroupViews.Remove(view);

                view.CriteriaChanged -= HandleCriteriaGroupChanged;
            }
            if (!ignoreSchemaChange)
            {
                HighlightSelection();
                ComputeSchema();

                ShowNewSchema();
            }
        }

        private void HandleEvaluation(bool starting)
        {
            if ( starting )
                return;

            ComputeSchema();

            ShowNewSchema();
        }

        public void Show()
        {
            HighlightSelection();
            Canvas.gameObject.SetActive( true );

            ChainView.IsEvaluatingChanged += HandleEvaluation;

            EscapeQueue.AddHandler( this );
        }

        public void Hide()
        {
            DisableHighlights();
            Canvas.gameObject.SetActive( false );

            ChainView.IsEvaluatingChanged -= HandleEvaluation;

            EscapeQueue.RemoveHandler( this );
        }

        public void HandleEscape()
        {
            Hide();
        }

        [UsedImplicitly]
        public void HandleCloseButtonPressed()
        {
            Hide();
        }

        [UsedImplicitly]
        public void HandleAddGroupButtonPressed()
        {
            PayloadExpression.AddCriteriaGroup();
        }

        public void ShowAddCriterionStatementDialog( CriteriaGroup group )
        {
            AddCriterionStatementDialog.Show( group );
        }

        [UsedImplicitly]
        public void HandleComputeSchemaButtonPressed()
        {
            ComputeSchema();

            ShowNewSchema();
        }

        private void HandleCriteriaGroupChanged()
        {
            HighlightSelection();
            ComputeSchema();

            ShowNewSchema();
        }

        private void HighlightSelection()
        {
            DisableHighlights();

            foreach (var bound in PayloadExpression.ResolveExpression(
                ChainView.Instance.Chain.RootBoundingBoxes))
                bound.Highlight = true;
        }

        private void DisableHighlights()
        {
            foreach (var bound in ChainView.Instance.Chain.RootBoundingBoxes)
                bound.HighlightChildren = false;
        }

        private void ComputeSchema()
        {
            //var debugTimer = DiagnosticTimer.Start("Schema Timing: ");

            if (ExternalSchemaProvider!=null)
                ExternalSchemaProvider.CacheSchema();
            var schemaList = PayloadExpression.ComputeSchemaForGroups(ChainView.Instance.Chain.RootBoundingBoxes);

            List<MutableObject>.Enumerator schemaEnumerator = schemaList.GetEnumerator();
            var groupEnumerator = CriteriaGroupViews.GetEnumerator();

            while (groupEnumerator.MoveNext() && schemaEnumerator.MoveNext())
            {
                groupEnumerator.Current.Schema = schemaEnumerator.Current;
            }
            while (groupEnumerator.MoveNext())
            {
                groupEnumerator.Current.Schema = new MutableObject { };
            }

            if (ExternalSchemaProvider != null)
                ExternalSchemaProvider.UnCacheSchema();

            //Debug.Log("Debug Timer: " + debugTimer.Stop().Conclusion + ", with " + MutableObject.SchemaCounter + " schema modifications.");
        }
    }
}
