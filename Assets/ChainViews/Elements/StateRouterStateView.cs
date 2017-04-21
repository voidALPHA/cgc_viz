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

using System.Collections.Generic;
using Chains;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Utility.Undo;

namespace ChainViews.Elements
{
    public class StateRouterStateView : MonoBehaviour
    {
        [Header("Component References")]

        [SerializeField]
        private RectTransform m_TargetItemRootTransform = null;
        private RectTransform TargetItemRootTransform { get { return m_TargetItemRootTransform; } }

        [SerializeField]
        private Text m_NameTextComponent = null;
        private Text NameTextComponent { get { return m_NameTextComponent; } }

        [SerializeField]
        private Button m_AddButtonComponent = null;
        private Button AddButtonComponent { get { return m_AddButtonComponent; } }

        [SerializeField]
        private GameObject m_SelectedIndicatorComponent = null;
        private GameObject SelectedIndicatorComponent { get { return m_SelectedIndicatorComponent; } }


        [Header("Prefab References")]
    
        [SerializeField]
        private GameObject m_TargetUiPrefab = null;
        private GameObject TargetUiPrefab { get { return m_TargetUiPrefab; } }

        
        private SelectionState m_SelectionState;
        public SelectionState SelectionState
        {
            get { return m_SelectionState; }
            set
            {
                m_SelectionState = value;

                m_SelectionState.TargetRemoved += HandleStateTargetRemoved;
                m_SelectionState.TargetAdded += HandleStateTargetAdded;

                NameTextComponent.text = m_SelectionState.Name;// + " : " + m_SelectionState.GroupId;

                m_SelectionState.SelectedChanged += HandleSelectionStateSelectedChanged;
            }
        }

        private void HandleStateTargetRemoved(SelectionState state, ChainNode target )
        {
            if (!TargetsToTargetCheckboxes.ContainsKey(target))
                return;

            var checkbox = TargetsToTargetCheckboxes[target];

            checkbox.Checked = false;

            UpdateCheckboxes();
        }

        private void HandleStateTargetAdded( SelectionState arg1, ChainNode arg2, List<UndoItem> returnUndos )
        {
            UpdateCheckboxes();
        }

        private void UpdateCheckboxes()
        {
            foreach ( var checkbox in TargetsToTargetCheckboxes.Values )
                checkbox.Checked = false;

            foreach ( var target in SelectionState.TargetsEnumerable )
            {
                TargetsToTargetCheckboxes[ target ].Checked = true;
            }
        }

        private void HandleSelectionStateSelectedChanged( bool selected )
        {
            SelectedIndicatorComponent.SetActive( selected );
        }


        private readonly Dictionary< ChainNode, StateRouterTargetCheckboxView > m_TargetsToTargetCheckboxes = new Dictionary< ChainNode, StateRouterTargetCheckboxView >();
        private Dictionary< ChainNode, StateRouterTargetCheckboxView > TargetsToTargetCheckboxes { get { return m_TargetsToTargetCheckboxes; } }

        
        public void AddTargetCheckbox( ChainNode target )
        {
            CreateTargetCheckbox( target );

            AddButtonComponent.transform.SetAsLastSibling();
        }

        private void CreateTargetCheckbox( ChainNode target )
        {
            var newTargetItemGo = Instantiate( TargetUiPrefab );
            var newTargetItem = newTargetItemGo.GetComponent<StateRouterTargetCheckboxView>();

            newTargetItem.transform.SetParent( TargetItemRootTransform, false );

            if ( SelectionState != null && SelectionState.Contains( target ) )
                newTargetItem.Checked = true;

            newTargetItem.CheckedChanged += isChecked => HandleTargetChecked( target, isChecked );
            
            TargetsToTargetCheckboxes.Add( target, newTargetItem );
        }


        private void HandleTargetChecked( ChainNode target, bool isChecked )
        {
            // This is setting actual targets in the state.

            if ( isChecked && !SelectionState.Contains( target ) )
                SelectionState.AddTarget( target );

            else if ( !isChecked && SelectionState.Contains( target ) )
                SelectionState.RemoveTarget( target );
        }


        public void RemoveTargetCheckbox( ChainNode targetNode )
        {
            var targetCheckbox = TargetsToTargetCheckboxes[ targetNode ];

            Destroy( targetCheckbox.gameObject );

            TargetsToTargetCheckboxes.Remove( targetNode );
        }


        #region NEW target stuff...

        [UsedImplicitly]
        public void HandleAddTargetButtonClicked()
        {
            ChainView.SelectedSelectionState = ChainView.SelectedSelectionState == SelectionState ? null : SelectionState;
        }

        #endregion   
    }
}
