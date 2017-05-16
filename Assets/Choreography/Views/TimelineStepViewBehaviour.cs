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
using Choreography.Steps.Timeline;
using JetBrains.Annotations;
using Ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility;

namespace Choreography.Views
{
    public class TimelineStepViewBehaviour : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public event Action< TimelineStepViewBehaviour > Clicked = delegate { };

        public event Action< TimelineStepViewBehaviour > DragStarted = delegate { };

        public event Action< TimelineStepViewBehaviour > DragEnded = delegate { };


        #region Serialized Properties

        [Header("Component References")]

        [SerializeField]
        private RectTransform m_ContentsVolumeTransform = null;
        private RectTransform ContentsVolumeTransform { get { return m_ContentsVolumeTransform; } }

        [SerializeField]
        private RectTransform m_DelayVolumeTransform = null;
        private RectTransform DelayVolumeTransform { get { return m_DelayVolumeTransform; } }

        [SerializeField]
        private Text m_StepNameTextComponent = null;
        private Text StepNameTextComponent { get { return m_StepNameTextComponent; } }

        [SerializeField]
        private InputField m_StepNoteInputFieldComponent = null;
        private InputField StepNoteInputFieldComponent { get { return m_StepNoteInputFieldComponent; } }

        [SerializeField]
        private Text m_StepDelayTextComponent = null;
        private Text StepDelayTextComponent { get { return m_StepDelayTextComponent; } }

        [SerializeField]
        private RectTransform m_EventAttachmentPoint = null;
        public RectTransform EventAttachmentPoint { get { return m_EventAttachmentPoint; } }
        
        [SerializeField]
        private Draggable m_Draggable = null;
        private Draggable Draggable { get { return m_Draggable; } }

        [SerializeField]
        private List< Selectable > m_SeekButtons = new List< Selectable >();
        private List< Selectable > SeekButtons { get { return m_SeekButtons; } }



        [SerializeField]
        private List< Graphic > m_StepColoredComponents = new List< Graphic >();
        private List< Graphic > StepColoredComponents { get { return m_StepColoredComponents; } }

        [SerializeField]
        private List<Graphic> m_EventColoredComponents = new List<Graphic>();
        private List<Graphic> EventColoredComponents { get { return m_EventColoredComponents; } }

        [SerializeField]
        private List<Graphic> m_StatusColoredComponents = new List<Graphic>();
        private List<Graphic> StatusColoredComponents { get { return m_StatusColoredComponents; } }

        [SerializeField]
        private Graphic m_BackgroundGraphic = null;
        private Graphic BackgroundGraphic { get { return m_BackgroundGraphic; } }


        [Header("Prefab References")]
        [SerializeField]
        private GameObject m_EventViewPrefab = null;
        private GameObject EventViewPrefab { get { return m_EventViewPrefab; } }



        [Header("Configuration")]

        [SerializeField]
        private Color m_PlayingStatusColor = Color.green;
        private Color PlayingStatusColor { get { return m_PlayingStatusColor; } }

        [SerializeField]
        private Color m_DelayingStatusColor = Color.yellow;
        private Color DelayingStatusColor { get { return m_DelayingStatusColor; } }

        [SerializeField]
        private Color m_PausedStatusColor = new Color( 0.0f, 0.6f, 0.0f, 1.0f );
        private Color PausedStatusColor { get { return m_PausedStatusColor; } }

        [SerializeField]
        private Color m_IdleStatusColor = Color.black;
        private Color IdleStatusColor { get { return m_IdleStatusColor; } }

        [SerializeField]
        private Color m_SelectedColor = new Color( 0.8f, 0.8f, 0.8f, 1.0f );
        private Color SelectedColor { get { return m_SelectedColor; } }

        [SerializeField]
        private Color m_UnselectedColor = new Color( 0.9f, 0.9f, 0.9f, 1.0f );
        private Color UnselectedColor { get { return m_UnselectedColor; } }

        #endregion


        private RectTransform m_RectTransform;
        public RectTransform RectTransform
        {
            get { return m_RectTransform ?? (m_RectTransform = GetComponent< RectTransform >()); }
        }

        private readonly List< TimelineEventViewBehaviour > m_EventViews = new List< TimelineEventViewBehaviour >();

        private List< TimelineEventViewBehaviour > EventViews
        {
            get { return m_EventViews; }
        }

        public IEnumerable< TimelineEventViewBehaviour > EventViewsEnumerable { get { return m_EventViews.AsReadOnly(); } }


        #region Step Model

        private Step m_Step;
        public Step Step
        {
            get
            {
                return m_Step;
            }
            set
            {
                if ( m_Step != null )
                    throw new InvalidOperationException("Cannot reuse this view.");

                m_Step = value;

                if ( m_Step != null )
                {
                    if ( m_Step.GetType() == typeof( ErrorStep ) )
                        IndicateError();

                    StepNameTextComponent.text = m_Step.Name;
                    StepNoteInputFieldComponent.text = m_Step.Note;
                    IndicateDelay( m_Step.Delay );


                    //StepColoredComponents.ForEach( c => c.color = TimelineColorManager.GetStepColor( m_Step ) );
                    //EventColoredComponents.ForEach( c => c.color = TimelineColorManager.GetStepColor( m_Step ) );
                    EventColoredComponents.ForEach( c => c.color = UnselectedColor );


                    m_Step.NameChanged += HandleStepNameChanged;
                    m_Step.NoteChanged += HandleStepNoteChanged;
                    m_Step.DelayChanged += HandleStepDelayChanged;


                    m_Step.DelayStarted += HandleStepDelayStarted;
                    m_Step.ExecutionStarted += HandleStepExecutionStarted;
                    m_Step.ExecutionEnded += HandleStepExecutionEnded;
            
                    m_Step.Paused += HandleStepPaused;
                    m_Step.Resumed += HandleStepResumed;

                    gameObject.name = "[STEP] " + m_Step.Name;

                    foreach ( var stepEvent in m_Step.Router.EventsEnumerable )
                    {
                        AddEventView( stepEvent );
                    }
                }
            }
        }

        private void IndicateError()
        {
            BackgroundGraphic.color = Color.red;
        }

        private void UnbindStep()
        {
            if ( Step == null )
                return;

            Step.NameChanged -= HandleStepNameChanged;
            Step.NoteChanged -= HandleStepNoteChanged;
            Step.DelayChanged -= HandleStepDelayChanged;

            Step.DelayStarted -= HandleStepDelayStarted;
            Step.ExecutionStarted -= HandleStepExecutionStarted;
            Step.ExecutionEnded -= HandleStepExecutionEnded;

            Step.Paused -= HandleStepPaused;
            Step.Resumed -= HandleStepResumed;
        }


        private void HandleStepDelayStarted()
        {
            StatusColoredComponents.ForEach( g => g.color = DelayingStatusColor );
        }

        private void HandleStepExecutionStarted()
        {
            StatusColoredComponents.ForEach( g => g.color = PlayingStatusColor );
        }

        private void HandleStepExecutionEnded( bool obj )
        {
            StatusColoredComponents.ForEach( g => g.color = IdleStatusColor );
        }

        private void HandleStepNameChanged( string stepName )
        {
            StepNameTextComponent.text = stepName;
        }

        private void HandleStepNoteChanged( string note )
        {
            StepNoteInputFieldComponent.text = note;
        }

        private void HandleStepDelayChanged( float delay )
        {
            IndicateDelay( delay );
        }

        private void HandleStepPaused()
        {
            StatusColoredComponents.ForEach( g => g.color = PausedStatusColor );
        }

        private void HandleStepResumed()
        {
            StatusColoredComponents.ForEach( g => g.color = PlayingStatusColor );
        }

        #endregion

        public Color EventColor
        {
            set { EventColoredComponents.ForEach( c => c.color = value ); }
        }

        private void IndicateDelay( float delay )
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ( delay == 0.0f )
            {
                ContentsVolumeTransform.offsetMin = new Vector2( 32.0f, ContentsVolumeTransform.offsetMin.y );
                DelayVolumeTransform.sizeDelta = new Vector2( 32.0f, DelayVolumeTransform.sizeDelta.y );
                StepDelayTextComponent.gameObject.SetActive( false );
            }
            else
            {
                ContentsVolumeTransform.offsetMin = new Vector2( 80.0f, ContentsVolumeTransform.offsetMin.y );
                DelayVolumeTransform.sizeDelta = new Vector2( 80.0f, DelayVolumeTransform.sizeDelta.y );
                StepDelayTextComponent.gameObject.SetActive( true );
            }

            StepDelayTextComponent.text = string.Format( "{0:F2}s", delay );
        }

        private void AddEventView( StepEvent @event )
        {
            var eventViewGo = Instantiate( EventViewPrefab );
            var eventView = eventViewGo.GetComponent< TimelineEventViewBehaviour >();

            eventView.RectTransform.SetParent( EventAttachmentPoint, false );

            EventViews.Add( eventView );

            eventView.Event = @event;
        }

        private void RemoveEventView( StepEvent @event )
        {
            var eventView = EventViews.FirstOrDefault( view => view.Event == @event );

            if ( eventView == null )
                return;

            EventViews.Remove( eventView );

            eventView.Shutdown();

            Destroy(eventView.gameObject);
        }

        private bool m_Selected;
        public bool Selected
        {
            get
            {
                return m_Selected;
            }
            set
            {
                m_Selected = value;

                IndicateSelected();
            }
        }

        private void IndicateSelected()
        {
            var color = m_Selected ? SelectedColor : UnselectedColor;

            StepColoredComponents.ForEach( g => g.color = color );

            //StepNameTextComponent.color = color;

            StepNoteInputFieldComponent.interactable = m_Selected;

            //StepNameTextComponent.fontStyle = m_Selected ? FontStyle.Bold : FontStyle.Normal;
        }

        // Without these, OnPointerClick does not seem to work...!
        public void OnPointerDown( PointerEventData eventData ) { }
        public void OnPointerUp( PointerEventData eventData ) { }

        public void OnPointerClick( PointerEventData eventData )
        {
            Clicked( this );
        }

        [UsedImplicitly]
        public void HandleNonAutomaticClickForwardingChildClicked()
        {
            if ( !StepNoteInputFieldComponent.isFocused )
                Clicked( this );
        }

        [UsedImplicitly]
        public void HandleNoteTextChanged()
        {
            Step.Note = StepNoteInputFieldComponent.text;
        }

        [UsedImplicitly]
        public void HandleRemoveButtonClicked()
        {
            Step.RequestRemoval();
        }

        [UsedImplicitly]
        public void HandleMoveUpArrowPressed()
        {
            Step.RequestMoveUp();
            GraphicRaycasterVA.Instance.ForceReset();
        }

        [UsedImplicitly]
        public void HandleMoveDownArrowPressed()
        {
            Step.RequestMoveDown();
            GraphicRaycasterVA.Instance.ForceReset();
        }

        [UsedImplicitly]
        public void HandleSeekToPressed()
        {
            Step.RequestSeekTo();
        }

        [UsedImplicitly]
        public void HandleSeekThroughPressed()
        {
            Step.RequestSeekThrough();
        }


        #region Timeline-Called Stuff

        public float Arrange()
        {
            if ( !EventViews.Any() )
            {
                var height = RectTransform.sizeDelta.y;
                
                return height;
            }

            var eventPosX = 0.0f;

            // Start by counting our own height from our top to the event attachement point
            var cumulativeHeight = 9.0f;

            // Then increment by space between bottom of step and top of event.
            cumulativeHeight += TimelineViewBehaviour.SpaceBetweenStepAndEventBadge;

            foreach ( var eventView in EventViews )
            {
                // This positions the /attachment point/
                eventView.RectTransform.anchoredPosition = new Vector2( eventPosX, 0.0f );
                eventPosX += TimelineViewBehaviour.HorizontalEventSpacing;
                
                // Ensure hierarchy order is synced with EventViews collection order.
                eventView.RectTransform.SetAsFirstSibling();
                

                // Add height of iterated event and its steps recursive
                cumulativeHeight += eventView.Arrange( cumulativeHeight );

                if ( eventView != EventViews.Last() )
                {
                    if ( eventView.StepViewsEnumerable.Any() )
                    {
                        cumulativeHeight += TimelineViewBehaviour.SpaceBetweenStepAndEventBadge;
                    }
                    else
                    {
                        cumulativeHeight += TimelineViewBehaviour.SpaceBetweenEventAndEvent;
                    }
                }
            }
            
            // Make sure workspace fits this step
            var xFromWorkspaceEdge = RectTransform.sizeDelta.x - TimelineViewBehaviour.GetStepViewPositionRelativeToWorkspace( this ).x;
            TimelineViewBehaviour.BoostWorkspaceMax( new Vector2( xFromWorkspaceEdge + 40, cumulativeHeight + 40 ) );

            // Finish by counting the height from the attachment point to the bottom.
            return cumulativeHeight + 9.0f;
        }

        public bool SeekEnabled
        {
            set { SeekButtons.ForEach( selectable => selectable.interactable = value ); }
        }

        #endregion


        [UsedImplicitly]
        private void Start()
        {
            Draggable.MouseDown += HandleDragStarted;
            Draggable.MouseUp += HandleDragEnded;

            IndicateSelected();

            //Draggable.DragStarted += HandleDragStarted;
            //Draggable.DragEnded += HandleDragEnded;
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            Shutdown();
        }

        public void Shutdown()
        {
            UnbindStep();

            foreach (var eventView in EventViews.ToList())
                RemoveEventView(eventView.Event);
        }

        private void HandleDragStarted()
        {
            DragStarted( this );
        }

        private void HandleDragEnded()
        {
            DragEnded( this );
        }
    }
}
