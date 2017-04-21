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
using Bounds;
using Choreography.Recording;
using Choreography.Steps;
using Choreography.Steps.Timeline;
using Choreography.Views;
using Newtonsoft.Json;
using Scripts.Utility.Misc;
using UnityEngine;

namespace Choreography
{
    [JsonObject( MemberSerialization.OptIn )]
    public class Timeline
    {
        //public event Action Started = delegate { };
        //public event Action Paused = delegate { };
        //public event Action Resumed = delegate { };
        //public event Action Ended = delegate { };

        public event Action< TimelineState > StateChanged = delegate { };

        public event Action< Step > StepUnregistered = delegate { };


        public bool IsBusy { get { return State == TimelineState.Paused || State == TimelineState.Playing; } }

        public bool IsEmpty { get { return RegisteredSteps.Count( step => step != StartStep ) == 0; } }


        private TimelineState m_State = TimelineState.Stopped;
        private TimelineState State
        {
            get
            {
                return m_State;
            }
             set
             {
                 if ( m_State == value )
                     return;

                 //var oldState = m_State;

                 m_State = value;

                 StateChanged( m_State );

                 //if ( oldState == TimelineState.Stopped && m_State == TimelineState.Playing )
                 //    Started();
                 //else if ( m_State == TimelineState.Stopped )
                 //    Ended();
             }
        }

        private readonly List< Step > m_RegisteredSteps = new List< Step >();
        private List< Step > RegisteredSteps
        {
            get { return m_RegisteredSteps; }
        }

        //private TimelineStartStep m_StartStep;
        [JsonProperty( TypeNameHandling = TypeNameHandling.All )]
        public TimelineStartStep StartStep { get; set; }


        // Call only from Steps!
        public void RegisterStep( Step step )
        {
            step.StateChanged += HandleStepStateChanged;

            step.SeekToRequested += HandleStepSeekToRequested;
            step.SeekThroughRequested += HandleStepSeekThroughRequested;

            if ( !step.IsJustMovingNotBeingDeleted )
            {
                var boundsProvider = step as IBoundsProvider;
                if ( boundsProvider != null )
                    BoundsProviderRepository.Add( boundsProvider );
            }

            RegisteredSteps.Add( step );
        }

        
        // Call only from Steps!
        public void UnregisterStep( Step step )
        {
            step.StateChanged -= HandleStepStateChanged;

            step.SeekToRequested -= HandleStepSeekToRequested;
            step.SeekThroughRequested -= HandleStepSeekThroughRequested;

            RegisteredSteps.Remove( step );

            if ( !step.IsJustMovingNotBeingDeleted )
            {
                var boundsProvider = step as IBoundsProvider;
                if ( boundsProvider != null )
                    BoundsProviderRepository.Remove( boundsProvider );
            }

            step.CleanUp();

            StepUnregistered( step );
        }

        private void HandleStepStateChanged( Step step, TimelineState state )
        {
            // If playing, check if we're done.

            if ( !RegisteredSteps.Any( s => s.IsBusy ) )
            {
                State = TimelineState.Stopped;
                
                RecordingLord.StopRecording();

                Camera.main.orthographic = false;

                if ( HaxxisGlobalSettings.Instance.IsVgsJob == true )
                {
                    if ( TimelineViewBehaviour.Instance.NumRecordingsStartedThisPlayback == 0 )
                    {
                        HaxxisGlobalSettings.Instance.ReportVgsError( 6, "Choreography had no recording step" );
                    }
                }

                if (HaxxisGlobalSettings.Instance.IsVgsJob == true)
                    HaxxisGlobalSettings.Instance.ReportVgsVideoDuration();
            }

            
            // Check for seek arrival

            if ( IsSeeking )
            {
                if ( step == SeekTarget )
                {
                    // Hmm I thought something like this was going to be needed...?
                    //if ( state == TimelineState.Playing )
                    
                    IsSeeking = false;

                    if ( SeekThrough )
                        SetNormalSpeed();
                    else
                        Pause();
                }
            }
        }

        public bool IsSeeking { get; set; }  //TODO: this is to be a state
        private Step SeekTarget { get; set; }
        private bool SeekThrough { get; set; }

        private void HandleStepSeekThroughRequested( Step step )
        {
            if ( IsBusy )
                return;

            SeekTarget = step;
            SeekThrough = true;

            Play( true );
        }

        private void HandleStepSeekToRequested( Step step )
        {
            if ( IsBusy )
                return;

            SeekTarget = step;
            SeekThrough = false;

            Play( true );
        }

        public Timeline()
        {
            StartStep = new TimelineStartStep();
        }

        private void HandleBoundsProviderRemoved( IBoundsProvider removedBoundsProvider )
        {
            // Iterate over all our steps; if any use the removed bounds provider, set that reference to null.

            // Using reflection for this may seem hacky, but there are a lot of infrastructure overhead (and timing issues) with a more concrete approach.
            //   The main advantage here is there is no need to enforce every consumer of a bounds provider
            //   listening to the bounds provider repo or its individual bounds providers for removal handling.

            foreach ( var step in RegisteredSteps )
            {
                var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                var boundsProviderInfos = step.GetType()
                    .GetProperties( flags )
                    .Where( p => typeof( IBoundsProvider ).IsAssignableFrom( p.PropertyType ) );

                foreach ( var boundsProviderInfo in boundsProviderInfos )
                {
                    var boundsProvider = boundsProviderInfo.GetValue( step, null ) as IBoundsProvider;

                    if ( boundsProvider == removedBoundsProvider )
                        boundsProviderInfo.SetValue( step, null, null );
                }
            }
        }

        public void Unload()
        {
            BoundsProviderRepository.BoundsProviderRemoved -= HandleBoundsProviderRemoved;

            StartStep.Timeline = null;
        }

        public void Load()
        {
            StartStep.Timeline = this;

            BoundsProviderRepository.BoundsProviderRemoved += HandleBoundsProviderRemoved;
        }

        public void AddStartEventTarget( Step step )
        {
            StartStep.AddStartStepTarget( step );
        }

        public void Play( bool seek = false )
        {
            IsSeeking = seek;


            if ( seek )
                SetFastSpeed();
            else
                SetNormalSpeed();


            State = TimelineState.Playing;

            StartStep.Play();
        }

        public void Pause()
        {
            if ( State == TimelineState.Paused )
                return;

            foreach ( var step in RegisteredSteps )
                step.Pause();

            RecordingLord.PauseRecording();

            SetPausedSpeed();

            State = TimelineState.Paused;
        }

        public void Resume()
        {
            if ( State != TimelineState.Paused )
                return;

            foreach ( var step in RegisteredSteps )
                step.Resume();

            
            
            SetNormalSpeed();
            // ^ and v order dependent
            RecordingLord.ResumeRecording();


            State = TimelineState.Playing;
        }

        public void Cancel()
        {
            foreach ( var step in RegisteredSteps )
                step.Cancel();


            SetNormalSpeed();


            RecordingLord.StopRecording();


            State = TimelineState.Stopped;
        }

        public void SetNormalSpeed()
        {
            Time.timeScale = 1.0f;

            Time.captureFramerate = 0;
        }

        public void SetPausedSpeed()
        {
            Time.timeScale = 0.0f;
        }

        public void SetFastSpeed()
        {
            Time.captureFramerate = 1;
        }


        public void ImportTimeline( Timeline timeline )
        {
            foreach ( var target in timeline.StartStep.StartStepTargets )
            {
                StartStep.AddStartStepTarget( target );
            }
        }

        public int RecursiveStepCount
        {
            get { return StartStep.RecursiveCount; }
        }

        public float RecursiveDuration
        {
            get { return StartStep.RecursiveDelayedDuration; }
        }

        public bool RecursiveDurationIsEstimated
        {
            get { return StartStep.RecursiveDurationIsEstimated; }
        }
    }
}
