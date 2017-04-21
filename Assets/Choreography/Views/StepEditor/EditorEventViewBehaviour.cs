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
using Choreography.Steps;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Choreography.Views.StepEditor
{
    public class EditorEventViewBehaviour : MonoBehaviour
    {
        public event Action< Step > TargetClicked = delegate { };

        [Header("Component References")]

        [SerializeField]
        private Text m_EventNameTextComponent = null;
        private Text EventNameTextComponent { get { return m_EventNameTextComponent; } }

        [SerializeField]
        private RectTransform m_AddButtonRootTransform = null;
        private RectTransform AddButtonRootTransform { get { return m_AddButtonRootTransform; } }

        [SerializeField]
        private Graphic m_ColoredBarComponent = null;
        private Graphic ColoredBarComponent { get { return m_ColoredBarComponent; } }


        [Header("Prefab References")]

        [SerializeField]
        private GameObject m_TargetViewPrefab = null;
        private GameObject TargetViewPrefab { get { return m_TargetViewPrefab; } }


        private List< EditorTargetViewBehaviour > m_TargetViews = new List< EditorTargetViewBehaviour >();
        private List< EditorTargetViewBehaviour > TargetViews
        {
            get { return m_TargetViews; }
            set { m_TargetViews = value; }
        }


        private StepEvent m_Event;
        public StepEvent Event
        {
            get { return m_Event; }
            set
            {
                if ( m_Event != null )
                {
                    m_Event.TargetAdded -= HandleEventTargetAdded;
                    m_Event.TargetRemoved -= HandleEventTargetRemoved;
                    m_Event.TargetOrderChanged -= HandleTargetOrderChanged;

                    if ( value != null )
                        throw new InvalidOperationException( "Cannot reuse this view." );
                }

                m_Event = value;

                if ( m_Event == null )
                    return;

                EventNameTextComponent.text = m_Event.Name;

                ColoredBarComponent.color = TimelineColorManager.GetEventColor( m_Event );

                m_Event.TargetAdded += HandleEventTargetAdded;
                m_Event.TargetRemoved += HandleEventTargetRemoved;
                m_Event.TargetOrderChanged += HandleTargetOrderChanged;

                GenerateTargets();
            }
        }

        private void HandleEventTargetRemoved( Step target )
        {
            RemoveTargetView( target );
        }

        private void HandleEventTargetAdded( Step target )
        {
            AddTargetView( target );
        }

        private void HandleTargetOrderChanged()
        {
            foreach ( var target in TargetViews.ToList() )
            {
                RemoveTargetView( target.Target );
            }

            GenerateTargets();
        }

        private void GenerateTargets()
        {
            foreach ( var target in Event.TargetsEnumerable )
            {
                AddTargetView( target );
            }
        }

        private void AddTargetView( Step target )
        {
            //Debug.Log( "Adding editor target view" );

            var targetViewGo = Instantiate(TargetViewPrefab);
            var targetView = targetViewGo.GetComponent< EditorTargetViewBehaviour >();

            targetView.transform.SetParent( transform, false );
            targetView.transform.SetAsLastSibling();
            AddButtonRootTransform.SetAsLastSibling();

            targetView.Clicked += HandleTargetViewClicked;
            //targetView.RemovalRequested += HandleTargetRemovalRequested;

            targetView.Target = target;

            TargetViews.Add( targetView );
        }

        private void RemoveTargetView( Step target )
        {
            var targetView = TargetViews.FirstOrDefault( view => view.Target == target );

            if ( targetView == null )
                return;

            targetView.Target = null;

            targetView.Clicked -= HandleTargetViewClicked;
            //targetView.RemovalRequested -= HandleTargetRemovalRequested;

            Destroy( targetView.gameObject );

            TargetViews.Remove( targetView );
        }


        #region Target Event Handling

        private void HandleTargetViewClicked( EditorTargetViewBehaviour targetView )
        {
            TargetClicked( targetView.Target );
        }

        private void HandleTargetRemovalRequested( EditorTargetViewBehaviour targetView )
        {
            Event.RemoveTarget( targetView.Target );
        }

        #endregion


        [UsedImplicitly]
        public void HandleAddButtonClicked()
        {
            TimelineViewBehaviour.StepPicker.TypeSelected += HandlePickerTypeSelected;
            TimelineViewBehaviour.StepPicker.Hidden += HandlePickerHidden;

            TimelineViewBehaviour.StepPicker.Show();
        }

        private void HandlePickerHidden()
        {
            UnbindFromPicker();
        }

        private void HandlePickerTypeSelected( Type type )
        {
            UnbindFromPicker();

            Event.AddTarget( Activator.CreateInstance( type ) as Step );
        }

        private void OnDestroy()
        {
            Event = null;

            //Debug.Log( "EventView OnDestroy" );
            UnbindFromPicker();

            TimelineViewBehaviour.StepPicker.Hide();
        }

        private void UnbindFromPicker()
        {
            //Debug.Log( "Unbinding from Picker." );
            TimelineViewBehaviour.StepPicker.TypeSelected -= HandlePickerTypeSelected;
            TimelineViewBehaviour.StepPicker.Hidden -= HandlePickerHidden;
        }
    }
}
