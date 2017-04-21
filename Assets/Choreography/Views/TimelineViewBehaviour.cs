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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChainViews;
using Choreography.Recording;
using Choreography.Steps;
using Choreography.Views.StepEditor;
using JetBrains.Annotations;
using Scripts.Utility.Misc;
using Ui;
using Ui.TypePicker;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using Network = Utility.NetworkSystem.Network;

namespace Choreography.Views
{
    public class TimelineViewBehaviour : MonoBehaviour, IResizeHandleTarget
    {
        private struct TimelineViewState
        {
            public string Name { get; private set; }
            public bool PlayButtonEnabled { get; private set; }
            public bool PauseButtonEnabled { get; private set; }
            public bool StopButtonEnabled { get; private set; }
            public bool CanChangeTimeline { get; private set; }
            //public bool OptionsEnabled { get; private set; }
            //public bool EditingEnabled { get; private set; }


            public TimelineViewState( string name, bool playButtonEnabled, bool pauseButtonEnabled, bool stopButtonEnabled, bool canChangeTimeline ) :
                this()
            {
                Name = name;
                PlayButtonEnabled = playButtonEnabled;
                PauseButtonEnabled = pauseButtonEnabled;
                StopButtonEnabled = stopButtonEnabled;
                CanChangeTimeline = canChangeTimeline;
            }

            public static bool operator ==( TimelineViewState a, TimelineViewState b )
            {
                return a.Equals( b );
            }

            public static bool operator !=( TimelineViewState a, TimelineViewState b )
            {
                return !(a == b);
            }
        }

        public event Action PlayStarted = delegate { };

        public event Action PlayStopped = delegate { };


        #region Inspectable Properties

        [Header("Component References")]

        [SerializeField]
        private Text m_MinimizeRestoreButtonTextComponent = null;
        private Text MinimizeRestoreButtonTextComponent { get { return m_MinimizeRestoreButtonTextComponent; } }

        [SerializeField]
        private RectTransform m_TitleBarTransform = null;
        private RectTransform TitleBarTransform { get { return m_TitleBarTransform; } }

        [SerializeField]
        private RectTransform m_WorkspacePanel = null;
        private RectTransform WorkspacePanel { get { return m_WorkspacePanel; } }

        [SerializeField]
        private ScrollRect m_WorkspaceScrollRect = null;
        private ScrollRect WorkspaceScrollRect { get { return m_WorkspaceScrollRect; } }

        [SerializeField]
        private RectTransform m_StepViewParent = null;
        public RectTransform StepViewParent { get { return m_StepViewParent; } }

        [SerializeField]
        private EditorViewBehaviour m_StepEditorView = null;
        private EditorViewBehaviour StepEditorView { get { return m_StepEditorView; } }

        [SerializeField]
        private TypePickerBehaviour m_StepPicker = null;
        public static TypePickerBehaviour StepPicker { get { return Instance.m_StepPicker; } }

        [SerializeField]
        private Toggle m_OptionsToggle = null;
        private Toggle OptionsToggle { get { return m_OptionsToggle; } }

        [SerializeField]
        private Graphic m_OptionsPanel = null;
        private Graphic OptionsPanel { get { return m_OptionsPanel; } }

        [SerializeField]
        private Toggle m_MinimizeOnPlayToggle = null;
        private Toggle MinimizeOnPlayToggle { get { return m_MinimizeOnPlayToggle; } }

        [SerializeField]
        private Toggle m_HideChainViewOnPlayToggle = null;
        private Toggle HideChainViewOnPlayToggle { get { return m_HideChainViewOnPlayToggle; } }

        [SerializeField]
        private Toggle m_EvaluateOnPlayToggle = null;
        private Toggle EvaluateOnPlayToggle { get { return m_EvaluateOnPlayToggle; } }


        [Header("Transport Component References")]

        [SerializeField]
        private Button m_PlayButton = null;
        private Button PlayButton { get { return m_PlayButton; } }

        [SerializeField]
        private Button m_PauseButton = null;
        private Button PauseButton { get { return m_PauseButton; } }

        [SerializeField]
        private Button m_StopButton = null;
        private Button StopButton { get { return m_StopButton; } }


        [Header("Status Indicating Component References")]

        [SerializeField]
        private TimelineIndicatorViewBehaviour m_EvaluatingIndicatorComponent = null;
        private TimelineIndicatorViewBehaviour EvaluatingIndicatorComponent { get { return m_EvaluatingIndicatorComponent; } }
        
        [SerializeField]
        private TimelineIndicatorViewBehaviour m_RecordingIndicatorComponent = null;
        private TimelineIndicatorViewBehaviour RecordingIndicatorComponent { get { return m_RecordingIndicatorComponent; } }



        [Header("Prefab References")]

        [SerializeField]
        private GameObject m_StepViewPrefab = null;
        private GameObject StepViewPrefab { get { return m_StepViewPrefab; } }



        [Header("Hide Options")]

        [SerializeField]
        private AnimationCurve m_HideAnimationCurve = null;
        private AnimationCurve HideAnimationCurve { get { return m_HideAnimationCurve; } }

        [SerializeField]
        private float m_HideAnimationDuration = 1.0f;
        private float HideAnimationDuration { get { return m_HideAnimationDuration; } }


        [Header("Timeline Arrangement Options")]
        [SerializeField]
        private float m_HorizontalEventSpacing = 12.0f;
        public static float HorizontalEventSpacing { get { return Instance.m_HorizontalEventSpacing; } }

        [SerializeField]
        private float m_SpaceBetweenStepsWithNoEvents = 10.0f;
        public static float SpaceBetweenStepAndStep { get { return Instance.m_SpaceBetweenStepsWithNoEvents; } }

        [SerializeField]
        private float m_SpaceBetweenEventAndEvent = 10.0f;
        public static float SpaceBetweenEventAndEvent { get { return Instance.m_SpaceBetweenEventAndEvent; } }

        [SerializeField]
        private float m_SpaceBetweenEventAndStep = 10.0f;
        public static float SpaceBetweenEventBadgeAndStep { get { return Instance.m_SpaceBetweenEventAndStep; } }

        [SerializeField]
        private float m_SpaceBetweenStepAndEvent = 10.0f;
        public static float SpaceBetweenStepAndEventBadge { get { return Instance.m_SpaceBetweenStepAndEvent; } }

        #endregion


        private readonly TimelineViewState m_NoTimelineState = new TimelineViewState(
            name: "NoTimelineState",
            playButtonEnabled: false,
            pauseButtonEnabled: false,
            stopButtonEnabled: false,
            canChangeTimeline: true );
        private TimelineViewState NoTimelineState { get { return m_NoTimelineState; } }

        private readonly TimelineViewState m_PlayingState = new TimelineViewState (
            name: "PlayingState",
            playButtonEnabled: false,
            pauseButtonEnabled: true,
            stopButtonEnabled: true,
            canChangeTimeline: false );
        private TimelineViewState PlayingState { get { return m_PlayingState; } }

        private readonly TimelineViewState m_WaitingForEvalState = new TimelineViewState(
            name: "PlayingState",
            playButtonEnabled: false,
            pauseButtonEnabled: false,
            stopButtonEnabled: false,
            canChangeTimeline: false);
        private TimelineViewState WaitingForEvalState { get { return m_WaitingForEvalState; } }

        [SerializeField]
        private TimelineViewState m_PausedState = new TimelineViewState(
            name: "PausedState",
            playButtonEnabled: true,
            pauseButtonEnabled: false,
            stopButtonEnabled: true,
            canChangeTimeline: false );
        private TimelineViewState PausedState { get { return m_PausedState; } }

        [SerializeField]
        private TimelineViewState m_StoppedState = new TimelineViewState(
            name: "StoppedState",
            playButtonEnabled: true,
            pauseButtonEnabled: false,
            stopButtonEnabled: false,
            canChangeTimeline: true );
        private TimelineViewState StoppedState { get { return m_StoppedState; } }

        [SerializeField]
        private TimelineViewState m_LockedState = new TimelineViewState(
            name: "LockedState",
            playButtonEnabled: false,
            pauseButtonEnabled: false,
            stopButtonEnabled: false,
            canChangeTimeline: true );
        private TimelineViewState LockedState { get { return m_LockedState; } }




        private RectTransform m_RectTransform;
        private RectTransform RectTransform { get { return m_RectTransform ?? ( m_RectTransform = GetComponent< RectTransform >() ); } }




        

        #region Step Views

        #region Start Step View

        private TimelineStepViewBehaviour StartStepView { get; set; }

        private void GenerateStartStepView()
        {
            StartStepView = Instantiate(StepViewPrefab).GetComponent<TimelineStepViewBehaviour>();

            StartStepView.RectTransform.SetParent(StepViewParent, false);
            StartStepView.RectTransform.anchoredPosition = Vector2.zero;

            StartStepView.Step = Timeline.StartStep;

            RegisterStepView(StartStepView, false);
        }

        private void ClearStartStepView()
        {
            if ( StartStepView == null )
                return;

            var ssv = StartStepView;

            StartStepView = null;

            UnregisterStepView(ssv);

            ssv.Shutdown();

            Destroy(ssv.gameObject);
        }

        #endregion

        private readonly List< TimelineStepViewBehaviour > m_StepViews = new List< TimelineStepViewBehaviour >();
        private List< TimelineStepViewBehaviour > StepViews { get { return m_StepViews; } }

        public static void RegisterStepView( TimelineStepViewBehaviour stepView, bool select = true )
        {
            if ( Instance.StepViews.Contains( stepView ) )
                throw new InvalidOperationException( "Step view already registered." );

            stepView.Clicked += Instance.HandleStepViewClicked;
            stepView.DragStarted += Instance.HandleStepViewDragStarted;
            stepView.DragEnded += Instance.HandleStepViewDragEnded;

            Instance.StepViews.Add( stepView );
            
            Instance.Arrange();

            if ( select )
                Instance.SelectedStep = stepView.Step;
        }

        public static void UnregisterStepView( TimelineStepViewBehaviour stepView )
        {
            if ( !Instance.StepViews.Contains( stepView ) )
                throw new InvalidOperationException( "Step view not registered." );

            stepView.Clicked -= Instance.HandleStepViewClicked;
            stepView.DragStarted -= Instance.HandleStepViewDragStarted;
            stepView.DragEnded -= Instance.HandleStepViewDragEnded;

            Instance.StepViews.Remove( stepView );
            
            Instance.Arrange();
        }


        #region Drag-to-move step views

        private void HandleStepViewDragStarted( TimelineStepViewBehaviour stepView )
        {
            WorkspaceScrollRect.enabled = false;
        }

        private void HandleStepViewDragEnded( TimelineStepViewBehaviour stepView )
        {
            Arrange();

            WorkspaceScrollRect.enabled = true;
        }

        #endregion

        #endregion


        


        private Timeline m_Timeline;
        public Timeline Timeline
        {
            get
            {
                return m_Timeline;
            }
            set
            {
                if ( !State.CanChangeTimeline )
                    throw new InvalidOperationException( "Cannot set Timeline in state " + State.Name );

                if ( m_Timeline != null )
                {
                    m_Timeline.StateChanged -= HandleTimelineStateChanged;

                    m_Timeline.Unload();

                    m_Timeline.StepUnregistered -= HandleTimelineStepUnregistered;

                    ClearStartStepView();
                }

                
                m_Timeline = value ?? new Timeline();


                if ( State != LockedState )
                    State = StoppedState;

                if ( m_Timeline.IsBusy )
                    throw new InvalidOperationException( "Cannot load a timeline that's busy." );

                m_Timeline.Load();

                GenerateStartStepView();

                m_Timeline.StateChanged += HandleTimelineStateChanged;

                m_Timeline.StepUnregistered += HandleTimelineStepUnregistered;


                Arrange();
            }
        }

        public bool IsPlayingState
        {
            get { return State == PlayingState; }
        }

        private TimelineViewState m_State;
        private TimelineViewState State
        {
            get { return m_State; }
            set
            {
                if ( m_State == value )
                    return;

                m_State = value;

                PlayButton.interactable = m_State.PlayButtonEnabled;
                PauseButton.interactable = m_State.PauseButtonEnabled;
                StopButton.interactable = m_State.StopButtonEnabled;
            }
        }

        private void HandleTimelineStateChanged( TimelineState timelineState  )
        {
            if ( State == LockedState )
                throw new InvalidOperationException("Timeline changed state while View was locked.");

            if ( timelineState == TimelineState.Playing )
            {
                State = PlayingState;
                
                SeekEnabled = false;

                PlayStarted();

            }
            if ( timelineState == TimelineState.Paused )
            {
                State = PausedState;
            }
            if ( timelineState == TimelineState.Stopped )
            {
                State = StoppedState;

                SeekEnabled = true;

                if ( MinimizeOnPlay )
                    Restore( false );

                if ( HideChainViewOnPlay )
                    ChainView.Instance.Visible = WasChainViewOpen;

                PlayStopped();
            }
        }

        private void HandleTimelineStepUnregistered( Step step )
        {
            if ( StepEditorView.Step == step )
                StepEditorView.Step = null;
        }

        private bool SeekEnabled
        {
            set { StepViews.ForEach( view => view.SeekEnabled = value ); }
        }


        private void HandleStepViewClicked( TimelineStepViewBehaviour stepView )
        {
            SelectedStep = stepView.Step;
        }

        private void ToggleSelectedStep( Step step)
        {
            SelectedStep = SelectedStep == step ? null : step;
        }



        private TimelineStepViewBehaviour m_SelectedStepView;
        private Step m_SelectedStep;
        public Step SelectedStep
        {
            get { return m_SelectedStep; }
            private set
            {
                // Step

                m_SelectedStep = value;

                StepEditorView.Step = m_SelectedStep;
                

                // StepView

                if (m_SelectedStepView != null)
                {
                    m_SelectedStepView.Selected = false;
                }
                
                m_SelectedStepView = StepViews.FirstOrDefault( view => view.Step == m_SelectedStep );

                if (m_SelectedStepView != null)
                {
                    m_SelectedStepView.Selected = true;
                }

                // Other

                OptionsToggle.isOn = false;
            }
        }



        [UsedImplicitly]
        private void Start()
        {
            GraphicRaycasterVA.Instance.ChoreoCanvas = GetComponentInParent<Canvas>();
            State = NoTimelineState;

            RecordingLord.RecordingStarted += HandleRecordingLordRecordingStarted;
            RecordingLord.RecordingPaused += HandleRecordingLordRecordingPaused;
            RecordingLord.RecordingResumed += HandleRecordingLordRecordingResumed;
            RecordingLord.RecordingStopped += HandleRecordingLordRecordingStopped;

            ChainView.IsEvaluatingChanged += HandleChainViewIsEvaluatingChanged;
            ChainView.IsBusyChanged += HandleChainViewIsBusyChanged;


            StepEditorView.TargetClicked += HandleEditorTargetClicked;

            if ( Timeline == null )
                Timeline = new Timeline();

            RestoreSavedHeight();


            InitializeOptionsToggleStates();
            

            if ( IsHidden )
                Minimize( true );
        }

        private void HandleChainViewIsBusyChanged( bool chainViewBusy )
        {
            if ( chainViewBusy && State == StoppedState )
                Lock();
            else
                Unlock();
        }


        [UsedImplicitly]
        private void OnApplicationQuit()
        {
            ClearStartStepView();

            // Unbind from events from externally managed things...!
            //RecordingLord.RecordingStarted -= HandleRecordingLordRecordingStarted;
            //RecordingLord.RecordingStopped -= HandleRecordingLordRecordingStopped;

            //ChainView.IsBusyChanged -= HandleChainNodeIsBusyChanged;
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            ClearStartStepView();
        }


        private void HandleEditorTargetClicked( Step step )
        {
            SelectedStep = step;
        }



        #region Status Indicators

        private void HandleRecordingLordRecordingStarted()
        {
            RecordingIndicatorComponent.IsTurnedOn = true;
        }

        private void HandleRecordingLordRecordingPaused()
        {
            RecordingIndicatorComponent.SetOtherState("Paused");
        }

        private void HandleRecordingLordRecordingResumed()
        {
            RecordingIndicatorComponent.IsTurnedOn = true;
        }

        private void HandleRecordingLordRecordingStopped()
        {
            RecordingIndicatorComponent.IsTurnedOn = false;
        }

        public string VgsJobId { get; set; }
        public string MasterServerSessionId { get; set; }
        private float TimelineDuration { get; set; }
        private bool TimelineDurationIsEstimated { get; set; }
        private float TimeStarted { get; set; }

        public int NumRecordingsStartedThisPlayback { get; set; }

        public IEnumerator PeriodicProgressUpdate(string vgsJobId, string masterServerSessionId, float frequencySeconds)
        {
            while ( true )
            {
                var elapsed = Time.time - TimeStarted;
                var progressPct = elapsed / TimelineDuration * 100.0f;

                if ( progressPct > 100.0f )
                    progressPct = 100.0f;

                Network.Instance.SendRequest("localhost:8004/JobProgress/" + vgsJobId + "/" + masterServerSessionId + "/" + progressPct + "/Playing", true);  // Silent=true so we don't spam the log

                yield return new WaitForSeconds(frequencySeconds);  // frequencySeconds is in UnityTime, not unscaled time; so frequency depends on how fast or slow we are going
            }
        }

        private void HandleChainViewIsEvaluatingChanged( bool chainViewIsEvaluating )
        {
            EvaluatingIndicatorComponent.IsTurnedOn = chainViewIsEvaluating;

            if ( chainViewIsEvaluating == false )
            {
                if ( State == WaitingForEvalState )
                {
                    if ( HaxxisGlobalSettings.Instance.IsVgsJob == true )
                    {
                        NumRecordingsStartedThisPlayback = 0;

                        VgsJobId = CommandLineArgs.GetArgumentValue( "jobID" );
                        MasterServerSessionId = CommandLineArgs.GetArgumentValue( "MSSID" );
                        if ( !string.IsNullOrEmpty( VgsJobId ) && !string.IsNullOrEmpty( MasterServerSessionId ) )
                        {
                            TimelineDuration = Timeline.RecursiveDuration;
                            if ( TimelineDuration <= 0.0f )
                                TimelineDuration = 0.001f;   // Avoid divide by zero later

                            TimelineDurationIsEstimated = Timeline.RecursiveDurationIsEstimated;

                            var stepCount = Timeline.RecursiveStepCount;
                            if ( stepCount <= 1 )
                            {
                                HaxxisGlobalSettings.Instance.ReportVgsError( 5, "HP has no choreography" );
                            }

                            TimeStarted = Time.time;

                            StartCoroutine(PeriodicProgressUpdate(VgsJobId, MasterServerSessionId, 3.0f));
                        }
                    }

                    Play();
                }
            }
        }

        #endregion



        #region Playback Controls

        [UsedImplicitly]
        public void HandlePlayButtonPressed()
        {
            if(Timeline.IsBusy || ChainView.Instance.IsBusy) return;

            if ( State == StoppedState )
                Play();
            else if ( State == PausedState )
                Resume();
        }

        [UsedImplicitly]
        public void HandlePausePressed()
        {
            Pause();
        }
        
        [UsedImplicitly]
        public void HandleStopButtonPressed()
        {
            Stop();
        }

        public void Play()
        {
            if ( State == LockedState )
                return;

            if ( EvaluateOnPlay && State != WaitingForEvalState )
            {
                if ( ChainView.Instance.Chain.HasError )
                {
                    if (HaxxisGlobalSettings.Instance.IsVgsJob == true)
                    {
                        VgsJobId = CommandLineArgs.GetArgumentValue("jobID");
                        MasterServerSessionId = CommandLineArgs.GetArgumentValue( "MSSID" );
                        HaxxisGlobalSettings.Instance.ReportVgsError( 8, "HP has chain errors" );
                    }

                    PlayStopped();

                    return;
                }

                State = WaitingForEvalState;

                ChainView.Instance.EvaluateChain();

                return;
            }

            if ( HideChainViewOnPlay )
            {
                WasChainViewOpen = ChainView.Instance.Visible;
                ChainView.Instance.Visible = false;
            }

            if ( MinimizeOnPlay )
            {
                Minimize( true );
            }

            Timeline.Play();
        }

        private void Pause()
        {
            Timeline.Pause();
        }

        private void Resume()
        {
            Timeline.Resume();
        }

        public void Stop()
        {
            Timeline.Cancel();
        }

        public void Lock()
        {
            if ( State == LockedState )
                return;

            if ( State != StoppedState )
                throw new InvalidOperationException( "Cannot lock timeline view when timeline is not stopped. Current state is " + State.Name + "." );

            State = LockedState;
        }

        public void Unlock()
        {
            if ( State != LockedState )
                return;

            State = StoppedState;
        }

        #endregion


        #region Options

        [UsedImplicitly]
        public void HandleOptionsToggleChanged()
        {
            OptionsPanel.gameObject.SetActive( OptionsToggle.isOn );
        }


        private bool m_MinimizeOnPlay = false;
        private bool MinimizeOnPlay { get { return m_MinimizeOnPlay; } set { m_MinimizeOnPlay = value; } }

        [UsedImplicitly]
        public void HandleMinimizeOnPlayToggleChanged( bool value )
        {
            //Debug.Log( "Toggle set to " + value );
            MinimizeOnPlay = value;
        }


        private bool m_EvaluateOnPlay = true;
        private bool EvaluateOnPlay { get { return m_EvaluateOnPlay; } set { m_EvaluateOnPlay = value; } }

        [UsedImplicitly]
        public void HandleEvaluateOnPlayToggleChanged( bool value )
        {
            //Debug.Log("Toggle set to " + value);
            EvaluateOnPlay = value;
        }


        private bool m_HideChainViewOnPlay = true;
        private bool HideChainViewOnPlay { get { return m_HideChainViewOnPlay; } set { m_HideChainViewOnPlay = value; } }
        private bool WasChainViewOpen { get; set; }

        [UsedImplicitly]
        public void HandleHideChainViewOnPlayToggleChanged( bool value )
        {
            //Debug.Log("Toggle set to " + value);
            HideChainViewOnPlay = value;
        }

        private void InitializeOptionsToggleStates()
        {
            HideChainViewOnPlayToggle.isOn = HideChainViewOnPlay;
            EvaluateOnPlayToggle.isOn = EvaluateOnPlay;
            MinimizeOnPlayToggle.isOn = MinimizeOnPlay;
        }

        #endregion


        [UsedImplicitly]
        private void Update()
        {
            if ( Input.GetKeyDown( KeyCode.PageDown ) )
            {
                Arrange();
            }
            else if ( Input.GetKeyDown( KeyCode.D ) )
            {
                if ( Input.GetKey( KeyCode.LeftControl ) || Input.GetKey( KeyCode.RightControl ) )
                    RequestDuplicationOnSelectedStep();
            }

            //Is handled by a KeyMnemonic on the Play button
            //if ( Input.GetButtonDown( "Play Choreography" ) && ( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) ) && !Timeline.IsBusy && !ChainView.Instance.IsBusy )
            //{
            //    Play();
            //}
        }

        private void RequestDuplicationOnSelectedStep()
        {
            if ( SelectedStep == null )
                return;

            SelectedStep.RequestDuplication();
        }

        #region Arrangement

        private void Arrange()
        {
            if ( StartStepView == null )
                return;

            WorkspaceMax = WorkspacePanel.rect.size;
            StartStepView.RectTransform.anchoredPosition = new Vector2( 10.0f, -16.0f );

            var height = StartStepView.Arrange();

            // Why is this necessary? (it is...)
            StepViewParent.sizeDelta = new Vector2( 100.0f, height );

            StepViewParent.sizeDelta = WorkspaceMax;
        }

        private static Vector2 WorkspaceMax { get; set; }
        public static void BoostWorkspaceMax( Vector2 newMax )
        {
            WorkspaceMax = new Vector2(
                Mathf.Max( WorkspaceMax.x, newMax.x ),
                Mathf.Max( WorkspaceMax.y, newMax.y )
                );

            //Debug.LogFormat( "Workspace boosted to {0} with addition of {1}.", WorkspaceMax, newMax );
        }

        public static Vector2 GetStepViewPositionRelativeToWorkspace( TimelineStepViewBehaviour stepView )
        {
            return Instance.StepViewParent.position - stepView.RectTransform.position;
        }

        private void EnsureStepAttachmentPointFillsWorkspace()
        {
            StepViewParent.sizeDelta = new Vector2(
                Mathf.Max( StepViewParent.sizeDelta.x, WorkspacePanel.rect.width ),
                Mathf.Max( StepViewParent.sizeDelta.y, WorkspacePanel.rect.height ) );
        }

        #endregion


        #region Visibility and Sizing

        private bool IsHidden
        {
            get { return PlayerPrefs.GetInt( "HideTimelineWindow", 0 ) == 1; }
            set
            {
                PlayerPrefs.SetInt( "HideTimelineWindow", value ? 1 : 0 );

                UpdateHideArrow();
            }
        }

        private void UpdateHideArrow()
        {
            var text = IsHidden ? "▲" : "▼";

            MinimizeRestoreButtonTextComponent.text = text;
        }


        private float SavedHeight
        {
            get { return PlayerPrefs.GetFloat( "TimelineSavedHeight", 300 ); }
            set { PlayerPrefs.SetFloat( "TimelineSavedHeight", value ); }
        }

        public float CurrentHeight { get { return IsHidden ? TitleBarTransform.sizeDelta.y : RectTransform.sizeDelta.y; } }

        private static TimelineViewBehaviour s_Instance;
        public static TimelineViewBehaviour Instance
        {
            get
            {
                if ( s_Instance == null )
                    s_Instance = FindObjectOfType<TimelineViewBehaviour>();

                //Debug.Log("Timelineview instance is " + s_Instance );

                return s_Instance;
            }
        }


        private void RestoreSavedHeight()
        {
            RectTransform.sizeDelta = new Vector2( RectTransform.sizeDelta.x, SavedHeight );
            RectTransform.anchoredPosition = new Vector2( RectTransform.anchoredPosition.x, SavedHeight );
            EnsureStepAttachmentPointFillsWorkspace();
        }


        [UsedImplicitly]
        public void HandleMinimizeRestoreButtonPressed()
        {
            //Debug.Log("Is Hidden is " + IsHidden  );
            if ( IsHidden )
                Restore();
            else
                Minimize();
        }

        // Someday, this should probably move to the root Choreography View.
        public void Hide()
        {
            GetComponentInParent<Canvas>().enabled = false;
        }

        public void Show()
        {
            GetComponentInParent<Canvas>().enabled = true;
        }

        private void Minimize( bool immediate = false )
        {
            MinimizeRestoreButtonTextComponent.text = "▲";

            var targetY = TitleBarTransform.sizeDelta.y;

            StartCoroutine( MinimizeRestoreCoroutine( targetY, true, immediate ) );
        }

        private void Restore( bool immediate = false )
        {
            MinimizeRestoreButtonTextComponent.text = "▼";

            var targetY = RectTransform.sizeDelta.y;

            // If we're being asked to Show, but there's no height...
            if ( Mathf.Approximately( RectTransform.sizeDelta.y, TitleBarTransform.sizeDelta.y ) )
            {
                var newHeight = 300.0f;

                RectTransform.sizeDelta = new Vector2( RectTransform.sizeDelta.x, newHeight );

                SavedHeight = newHeight;

                targetY = newHeight;

                EnsureStepAttachmentPointFillsWorkspace();
            }

            StartCoroutine( MinimizeRestoreCoroutine( targetY, false, immediate ) );
        }

        private IEnumerator MinimizeRestoreCoroutine( float targetY, bool willBeHidden, bool immediate )
        {
            var startY = RectTransform.anchoredPosition.y;

            var deltaY = targetY - startY;

            var startTime = Time.time;
            
            while ( true )
            {                
                if ( immediate )
                    break;

                if ( Mathf.Approximately( HideAnimationDuration, 0.0f ) )
                    break;

                var progressThroughAnimation = ( Time.time - startTime ) / HideAnimationDuration;

                if ( progressThroughAnimation >= 1.0f )
                    break;


                var progressThroughMotion = HideAnimationCurve.Evaluate( progressThroughAnimation );

                var newY = deltaY * progressThroughMotion + startY;

                RectTransform.anchoredPosition = new Vector2( RectTransform.anchoredPosition.x, newY );


                yield return null;
            }

            RectTransform.anchoredPosition = new Vector2( RectTransform.anchoredPosition.x, targetY );

            IsHidden = willBeHidden;
        }

        #endregion

        #region IResizeTarget Implementation

        public void OnPartialResize()
        {
            //if ( IsHidden )
            //    RectTransform.sizeDelta = new Vector2( RectTransform.sizeDelta.x, TitleBarTransform.sizeDelta.y );

            //IsHidden = Mathf.Approximately( RectTransform.sizeDelta.y, TitleBarTransform.sizeDelta.y );

            // Can't set to automatically stretch, as it's sized manually in Arrange.
            EnsureStepAttachmentPointFillsWorkspace();
        }

        public void OnFinalResize()
        {
            SavedHeight = RectTransform.sizeDelta.y;
        }

        public bool SuppressResize { get { return IsHidden; } }

        public Vector2 MinResizeSize
        {
            get { return new Vector2( -1.0f, TitleBarTransform.sizeDelta.y ); }
        }

        public Vector2 MaxResizeSize
        {
            get { return new Vector2( -1.0f, Screen.height ); }
        }

        #endregion

    }
}
