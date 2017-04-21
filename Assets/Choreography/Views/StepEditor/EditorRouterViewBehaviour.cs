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
using Choreography.Steps;
using UnityEngine;

namespace Choreography.Views.StepEditor
{
    public class EditorRouterViewBehaviour : MonoBehaviour
    {
        public event Action< Step > TargetClicked = delegate { };

        [Header("Component References")]
        [SerializeField]
        private RectTransform m_AddButtonRootTransform = null;
        private RectTransform AddButtonRootTransform { get { return m_AddButtonRootTransform; } }


        [Header("Prefab References")]

        [SerializeField]
        private GameObject m_StepEventPrefab = null;
        private GameObject StepEventPrefab { get { return m_StepEventPrefab; } }



        private readonly Dictionary< StepEvent, EditorEventViewBehaviour > m_EventsToEventViews = new Dictionary< StepEvent, EditorEventViewBehaviour >();
        private Dictionary<StepEvent, EditorEventViewBehaviour > EventsToEventViews { get { return m_EventsToEventViews; } }



        private StepRouter m_Router;
        public StepRouter Router
        {
            get
            {
                return m_Router;
            }
            set
            {
                DestroyEventViews();

                m_Router = value;

                GenerateEventViews();
            }
        }

        private void DestroyEventViews()
        {
            foreach ( var view in EventsToEventViews.Values )
            {
                view.TargetClicked -= HandleEventTargetClicked;

                Destroy( view.gameObject );
            }

            EventsToEventViews.Clear();
        }

        private void GenerateEventViews()
        {
            foreach ( var stepEvent in m_Router.EventsEnumerable )
            {
                var eventViewGo = Instantiate( StepEventPrefab );
                var eventView = eventViewGo.GetComponent< EditorEventViewBehaviour >();

                eventView.transform.SetParent( transform, false );
                eventView.transform.SetAsLastSibling();
                AddButtonRootTransform.SetAsLastSibling();

                eventView.Event = stepEvent;

                eventView.TargetClicked += HandleEventTargetClicked;

                EventsToEventViews.Add( stepEvent, eventView );
            }
        }

        private void HandleEventTargetClicked( Step target )
        {
            TargetClicked( target );
        }
    }
}
