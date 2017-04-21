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

namespace Choreography.Views
{
    public class TimelineEventViewBehaviour : MonoBehaviour
    {

        [Header("Component References")]
        [SerializeField]
        private RectTransform m_StepAttachmentPoint = null;

        public RectTransform StepAttachmentPoint { get { return m_StepAttachmentPoint; } }


        [SerializeField]
        private Text m_EventNameTextComponent = null;
        private Text EventNameTextComponent { get { return m_EventNameTextComponent; } }

        [SerializeField]
        private Image m_EventNameBackgroundComponent = null;
        private Image EventNameBackgroundComponent { get { return m_EventNameBackgroundComponent; } }

        [SerializeField]
        private TimelineEventViewDropTargetBehaviour m_DropTargetComponent = null;
        private TimelineEventViewDropTargetBehaviour DropTargetComponent { get { return m_DropTargetComponent; } }



        [SerializeField]
        private List< Graphic > m_ColoredComponents = new List< Graphic >();
        private List< Graphic > ColoredComponents { get { return m_ColoredComponents; } }

        [SerializeField]
        private List<GameObject> m_EnableOnStepHover = new List<GameObject>();
        private List<GameObject> EnableOnStepHover { get { return m_EnableOnStepHover; } }



        [Header("Prefab References")]

        [SerializeField]
        private GameObject m_StepViewPrefab = null;
        private GameObject StepViewPrefab { get { return m_StepViewPrefab; } }



        private RectTransform m_RectTransform;
        public RectTransform RectTransform { get { return m_RectTransform ?? ( m_RectTransform = GetComponent< RectTransform >() ); } }




        private readonly List<TimelineStepViewBehaviour > m_StepViews = new List< TimelineStepViewBehaviour >();
        private List<TimelineStepViewBehaviour > StepViews { get { return m_StepViews; } }

        public IEnumerable<TimelineStepViewBehaviour> StepViewsEnumerable
        {
            get { return StepViews.AsReadOnly(); }
        }

        private StepEvent m_Event;
        public StepEvent Event
        {
            get { return m_Event; }
            set
            {
                if ( m_Event != null )
                    throw new InvalidOperationException("Cannot re-use this view.");

                m_Event = value;

                if ( m_Event == null )
                    return;

                gameObject.name = "[EVENT] " + m_Event.Name;

                EventNameTextComponent.text = m_Event.Name;

                var color = TimelineColorManager.GetEventColor( m_Event );
                foreach ( var g in ColoredComponents )
                    g.color = color;

                GenerateTargetViews();

                Event.TargetAdded += HandleEventTargetAdded;
                Event.TargetRemoved += HandleEventTargetRemoved;
                Event.TargetOrderChanged += HandleEventTargetOrderChanged;
            }
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            Shutdown();
        }

        public void Shutdown()
        {
            foreach (var stepView in StepViews.ToList())
                RemoveStepView(stepView.Step);

            if (Event == null)
                return;

            Event.TargetAdded -= HandleEventTargetAdded;
            Event.TargetRemoved -= HandleEventTargetRemoved;
            Event.TargetOrderChanged -= HandleEventTargetOrderChanged;
        }

        private void GenerateTargetViews()
        {
            foreach ( var target in m_Event.TargetsEnumerable )
            {
                AddStepView( target );
            }
        }

        private void HandleEventTargetAdded( Step step )
        {
            AddStepView( step );
        }

        private void HandleEventTargetRemoved( Step step )
        {
            RemoveStepView( step );
        }

        private void HandleEventTargetOrderChanged()
        {
            foreach ( var view in StepViews.ToList() )
                RemoveStepView( view.Step );

            GenerateTargetViews();

        }

        private void AddStepView( Step step )
        {
            var stepViewGo = Instantiate( StepViewPrefab );
            var stepView = stepViewGo.GetComponent<TimelineStepViewBehaviour>();

            stepView.transform.SetParent( StepAttachmentPoint, false );

            stepView.Step = step;

            stepView.EventColor = TimelineColorManager.GetEventColor( Event );

            StepViews.Add( stepView );

            TimelineViewBehaviour.RegisterStepView( stepView );
        }

        private void RemoveStepView( Step step )
        {
            var stepView = StepViews.FirstOrDefault(view => view.Step == step);

            if (stepView == null)
                return;


            StepViews.Remove(stepView);

            stepView.Shutdown();

            Destroy( stepView.gameObject );


            TimelineViewBehaviour.UnregisterStepView(stepView);
        }

        public float Arrange( float startY )
        {

            // Badge Arrangement
            EventNameBackgroundComponent.rectTransform.anchoredPosition = new Vector3( EventNameBackgroundComponent.rectTransform.anchoredPosition.x, -startY );

            var nameBadgeHeight = EventNameBackgroundComponent.rectTransform.sizeDelta.y;

            var cumulativeHeight = nameBadgeHeight; 

            if ( !StepViews.Any() )
            {
                // Increase our size to encompass the badge
                RectTransform.sizeDelta = new Vector2(RectTransform.sizeDelta.x, startY + nameBadgeHeight - 1);  // -1 to hide the last bit of the line poking out the bottom of the name badge

                // Return the height of the one added element
                return cumulativeHeight;
            }

            cumulativeHeight += TimelineViewBehaviour.SpaceBetweenEventBadgeAndStep;

            float lastStepStart = 0.0f;
            foreach ( var stepView in StepViews )
            {

                stepView.RectTransform.anchoredPosition = new Vector2( 0.0f, -startY - cumulativeHeight );

                lastStepStart = cumulativeHeight;
                cumulativeHeight += stepView.Arrange();

                if ( stepView != StepViews.Last() )
                {
                    if (stepView.EventViewsEnumerable.Any())
                        cumulativeHeight += TimelineViewBehaviour.SpaceBetweenEventBadgeAndStep;
                    else
                        cumulativeHeight += TimelineViewBehaviour.SpaceBetweenStepAndStep;
                }
            }

            RectTransform.sizeDelta = new Vector2( RectTransform.sizeDelta.x, startY + lastStepStart + 18 );    // 18 for height of step

            return cumulativeHeight;
        }

        [UsedImplicitly]
        public void Start()
        {
            DropTargetComponent.StepViewDropped += HandleStepViewDropped;
        }

        
        #region Drag-move of steps...

        private bool CheckIfStepIsDescendent( TimelineStepViewBehaviour stepView )
        {
            // How can we target a descendent if it's being dragged with its parents? I'm sure we'll find a way, but until then...

            // Prevent re-attachment to self...
            return Event.TargetsEnumerable.Contains( stepView.Step );
        }


        private void HandleStepViewDropped( TimelineStepViewBehaviour stepView )
        {
            ReceiveDraggedStep( stepView );
        }

        public void ReceiveDraggedStep( TimelineStepViewBehaviour stepView )
        {
            
            if (CheckIfStepIsDescendent( stepView ))
                return;

            stepView.Step.IsJustMovingNotBeingDeleted = true;

            stepView.Step.RequestRemoval();

            Event.AddTarget( stepView.Step );

            stepView.Step.IsJustMovingNotBeingDeleted = false;
        }

        #endregion
    }
}
