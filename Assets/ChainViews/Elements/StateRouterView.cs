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
using Chains;
using UnityEngine;
using UnityEngine.UI;

namespace ChainViews.Elements
{
    public class StateRouterView : MonoBehaviour
    {

        public event Action UniqueTargetsChanged = delegate { };

        #region Reference Properties

        [Header("Component References")]
        
        [SerializeField]
        private RectTransform m_StateUiRootTransform = null;
        private RectTransform StateUiRootTransform { get { return m_StateUiRootTransform; } }


        [SerializeField]
        private RectTransform m_TargetLabelRootTransform = null;
        private RectTransform TargetLabelRootTransform { get { return m_TargetLabelRootTransform; } }



        [Header("Prefab References")]
        
        [SerializeField]
        private GameObject m_StateUiPrefab = null;
        private GameObject StateUiPrefab { get { return m_StateUiPrefab; } }

        [SerializeField]
        private GameObject m_TargetLabelPrefab = null;
        private GameObject TargetLabelPrefab { get { return m_TargetLabelPrefab; } }

        [SerializeField]
        private GameObject m_StateGroupSeparatorPrefab = null;
        private GameObject StateGroupSeparatorPrefab { get { return m_StateGroupSeparatorPrefab; } }

        #endregion


        // FYI about targets in the Model/View of Router, States, etc.: 
        //  UniqueTargets refers to the set of unique targets maintained by the Router. They are
        //    driven through events from the States, but the states don't otherwise impact UniqueTargets.
        //    These are used to drive the check boxes and labels; things which, /in the UI/, exist for all
        //    states.
        //  Otherwise, Target likely refers to the set of actual targets referred to by a state.
        //    These are tightly coupled to the /checked/ checkboxes.


        private StateRouter m_Router;
        public StateRouter Router
        {
            get { return m_Router; }
            set
            {
                m_Router = value;

                
                GenerateStateViews();

                
                foreach ( var target in m_Router.UniqueTargets )
                    AddUniqueTarget( target );


                m_Router.UniqueTargetAdded += AddUniqueTarget;

                m_Router.UniqueTargetRemoved += RemoveUniqueTarget;
            }
        }


        #region State Views

        // NOTE: States don't change; they are set at compile-time.

        private readonly List< StateRouterStateView > m_StateViews = new List< StateRouterStateView >();
        private List< StateRouterStateView > StateViews { get { return m_StateViews; } }

        private void GenerateStateViews()
        {
            if ( Router.SelectionStatesEnumerable.Count() == 0 )
                return;

            var orderedStates = Router.SelectionStatesEnumerable.OrderBy( r => r.GroupId );
            var lastGroupId = orderedStates.First().GroupId;

            var firstState = true;

            foreach ( var state in orderedStates )
            {
                if ( ( lastGroupId != state.GroupId || state.GroupId == "")&&!firstState )
                {
                    AddStateGroupSeparator();

                    lastGroupId = state.GroupId;
                }

                AddStateView(state);
                firstState = false;
            }
        }

        private void AddStateGroupSeparator()
        {
            var separatorGo = Instantiate( StateGroupSeparatorPrefab );

            separatorGo.transform.SetParent( StateUiRootTransform, false );
        }

        private void AddStateView( SelectionState state )
        {
            var newStateItemGo = Instantiate( StateUiPrefab );
            var stateView = newStateItemGo.GetComponent< StateRouterStateView >();

            stateView.transform.SetParent( StateUiRootTransform, false );
            
            stateView.SelectionState = state;

            StateViews.Add( stateView );
        }

        #endregion

        #region Target Management

        // These are only to be used in direct response to Router events and init, above

        private void AddUniqueTarget( ChainNode targetNode )
        {
            AddTargetLabel( targetNode );

            foreach ( var stateView in StateViews )
            {
                stateView.AddTargetCheckbox( targetNode );
            }

            UniqueTargetsChanged();
        }

        private void RemoveUniqueTarget( ChainNode targetNode )
        {
            RemoveTargetLabel( targetNode );

            foreach ( var stateView in StateViews )
            {
                stateView.RemoveTargetCheckbox( targetNode );
            }

            UniqueTargetsChanged();
        }

        #endregion


        #region Target Label Management

        private List<StateRouterTargetLabelView> m_TargetLabels = new List<StateRouterTargetLabelView>();
        public List<StateRouterTargetLabelView> TargetLabels { get { return m_TargetLabels; } }


        private void AddTargetLabel( ChainNode targetNode )
        {
            var targetLabelGo = Instantiate( TargetLabelPrefab );
            var targetLabel = targetLabelGo.GetComponent<StateRouterTargetLabelView>();


            targetLabel.Target = targetNode;

            targetLabel.transform.SetParent( TargetLabelRootTransform, false );
            
            TargetLabels.Add( targetLabel );


            IndexAndPositionTargetLabels();
        }
        
        private void RemoveTargetLabel( ChainNode targetNode )
        {
            var label = TargetLabels.FirstOrDefault( l => l.Target == targetNode );

            if ( label == null )
                return;

            TargetLabels.Remove(label);

            Destroy( label.gameObject );

            IndexAndPositionTargetLabels();
        }

        private void IndexAndPositionTargetLabels()
        {
            for ( var i = 0; i < TargetLabels.Count; i++ )
            {
                TargetLabels[i].TargetIndex = i;
            }

            TargetLabelRootTransform.GetComponent< LayoutElement >().preferredHeight = TargetLabels.Count*16.0f + 4;
        }

        #endregion

    }
}
