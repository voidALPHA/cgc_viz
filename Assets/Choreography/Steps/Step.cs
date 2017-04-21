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
using Choreography.Steps.Timeline;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Utility;
using Utility.JobManagerSystem;
using Visualizers;

namespace Choreography.Steps
{
    [JsonObject( MemberSerialization.OptIn, IsReference = true )]
    [JsonConverter( typeof( ChoreographyStepConverter ) )]
    public abstract class Step
    {
        public event Action DelayStarted = delegate { };
        public event Action ExecutionStarted = delegate { };
        public event Action< bool > ExecutionEnded = delegate { };

        public event Action Paused = delegate { };
        public event Action Resumed = delegate { };


        public event Action< Step, TimelineState > StateChanged = delegate { };

        
        public event Action< string > NameChanged = delegate { };
        public event Action< string > NoteChanged = delegate { };
        public event Action< float > DelayChanged = delegate { };


        public event Action< Step > RemovalRequested = delegate { };
        public event Action< Step > MoveUpRequested = delegate { };
        public event Action< Step > MoveDownRequested = delegate { };
        public event Action< Step > DuplicationRequested = delegate { };
        public event Action< Step, Step > ReplacementRequested = delegate { };

        public event Action< Step > SeekToRequested = delegate { };
        public event Action< Step > SeekThroughRequested = delegate { };

        private TimelineState m_State = TimelineState.Stopped;
        private TimelineState State
        {
            get { return m_State; }
            set
            {
                if ( m_State == value )
                    return;

                m_State = value;

                StateChanged( this, m_State );
            }
        }

        public bool IsBusy
        {
            get { return State == TimelineState.Paused || State == TimelineState.Playing; }
        }




        private Choreography.Timeline m_Timeline;
        public Choreography.Timeline Timeline
        {
            get { return m_Timeline; }
            set
            {
                if ( m_Timeline != null )
                {
                    m_Timeline.UnregisterStep( this );
                }

                m_Timeline = value;

                Router.Timeline = m_Timeline;

                if ( m_Timeline != null )
                {
                    m_Timeline.RegisterStep( this );

                    m_Timeline.StateChanged += HandleTimelineStateChanged;
                }
            }
        }

        private void HandleTimelineStateChanged( TimelineState state )
        {
            if ( state == TimelineState.Stopped )
            {
                OnTimelineStopped();
            }
        }

        protected virtual void OnTimelineStopped()
        {
        }

        public virtual void CleanUp()
        {
        }


        private float m_Delay;
        [Controllable]
        public float Delay
        {
            get { return m_Delay; }
            set
            {
                if ( Mathf.Approximately( m_Delay, value ) )
                    return;

                m_Delay = value;

                DelayChanged( m_Delay );
            }
        }

        private string m_Note = "";
        [JsonProperty]  // Not controllable; has a dedicated component in the UI
        public string Note
        {
            get
            {
                return m_Note;
            }
            set
            {
                if ( m_Note == value )
                    return;

                m_Note = value;

                NoteChanged( m_Note );
            }
        }


        [Controllable]
        private string NameOverride { get; set; }
        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(NameOverride) ? 
                    GetType().Name:
                    NameOverride;
            }
            set
            {
                if (NameOverride == value)
                    return;

                NameOverride = value;
                
                NameChanged(NameOverride);
            }
        }

        public bool IsJustMovingNotBeingDeleted { get; set; }



        public virtual float BaseDuration { get { return 0.0f; } }

        public float DelayedDuration { get { return BaseDuration + Delay; } }
        
        public virtual float RecursiveDelayedDuration { get { return DelayedDuration + Router.RecursiveDelayedDuration; } }

        public virtual int RecursiveCount { get { return Router.RecursiveCount + 1; } }

        public virtual bool BaseDurationIsEstimated { get { return false; } }

        public virtual bool RecursiveDurationIsEstimated { get { return BaseDurationIsEstimated || Router.RecursiveDurationIsEstimated; } }

        protected float DelayOverage { get; set; }


        private StepRouter m_Router;
        //[JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        [JsonProperty]
        public StepRouter Router
        {
            get
            {
                return m_Router;
            }
            set
            {
                if ( m_Router != null )
                {
                    // Do we even care about these in here?
                    m_Router.EventAdded -= HandleRouterEventAdded;
                    m_Router.EventRemoved -= HandleRouterEventRemoved;

                    m_Router.Timeline = null;
                }

                m_Router = value;

                if ( m_Router != null )
                {
                    m_Router.EventAdded += HandleRouterEventAdded;
                    m_Router.EventRemoved += HandleRouterEventRemoved;

                    m_Router.Timeline = Timeline;
                }
            }
        }

        protected Step()
        {
            Router = new StepRouter();
        }

        private void HandleRouterEventAdded( StepEvent @event )
        {
        }

        private void HandleRouterEventRemoved( StepEvent @event )
        {
        }


        private uint JobId { get; set; }
        
        public void Play( float durationOverage = 0.0f )
        {
            if ( State != TimelineState.Stopped )
                throw new InvalidOperationException("Cannot play if not stopped.");
        
            State = TimelineState.Playing;

            JobId = JobManager.Instance.StartJob( JobImplementation( durationOverage ),
                jobName: "Execute Step " + Name, startImmediately: true, maxExecutionsPerFrame: 1 );
        }

        public void Pause()
        {
            if ( State != TimelineState.Playing )
                return;
                //throw new InvalidOperationException("Cannot pause if not playing.");

            State = TimelineState.Paused;

            OnPause();

            Paused();
        }

        protected virtual void OnPause()
        {
        }

        public void Resume()
        {
            if ( State != TimelineState.Paused )
                return;
            //new InvalidOperationException("Cannot resume if not paused.");

            State = TimelineState.Playing;

            OnResume();

            Resumed();
        }

        protected virtual void OnResume()
        {
        }

        public void Cancel()
        {
            if ( State != TimelineState.Paused && State != TimelineState.Playing )
                return;

            JobManager.Instance.CancelJob( JobId );

            OnCancel();

            ExecutionEnded( true );

            State = TimelineState.Stopped;
        }

        protected virtual void OnCancel()
        {
        }

        private IEnumerator JobImplementation( float durationOverage )
        {
            DelayStarted();

            State = TimelineState.Playing;

            //Debug.Log("Starting step " + Name + ", coming in with durationOverage: " + durationOverage );

            var targetTime = Time.time + Delay - durationOverage;
            while (Time.time < targetTime)
                yield return null;

            DelayOverage = Time.time - targetTime;

            ExecutionStarted();

            var iterator = ExecuteStep();
            while ( iterator.MoveNext() )
            {
                // If paused, cycle without moving ExecuteStep iterator.
                while ( State == TimelineState.Paused )
                    yield return null;

                yield return null;
            }

            State = TimelineState.Stopped;
            
            ExecutionEnded(false);
        }

        protected abstract IEnumerator ExecuteStep();

        #region Request Methods (remove, move, seek)

        public void RequestRemoval()
        {
            RemovalRequested( this );
        }

        public void RequestMoveUp()
        {
            MoveUpRequested( this );
        }

        public void RequestMoveDown()
        {
            MoveDownRequested( this );
        }

        public void RequestDuplication()
        {
            if ( GetType() == typeof( TimelineStartStep ) )
                return;

            DuplicationRequested( this );
        }

        public void RequestSeekTo()
        {
            SeekToRequested( this );
        }

        public void RequestSeekThrough()
        {
            SeekThroughRequested( this );
        }

        protected void RequestReplacement( Step newStep )
        {
            ReplacementRequested( this, newStep );
        }

        #endregion
    }

    public class ChoreographyStepConverter : JsonConverter
    {
        public override bool CanRead { get { return true; } }

        public override bool CanWrite { get { return false; } }

        public override bool CanConvert( Type objectType ) { return typeof( Step ).IsAssignableFrom( objectType ); }


        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            throw new NotImplementedException( "If CanWrite is false; this won't be called." );
        }


        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            // This converter exists to replace steps which would not load with dedicated error-indicating steps; allowing a package to load even with errors.

            // NOTE: The deal with TimelineStartSteps:
            //          This type-error-recovery paradigm seems to only work with json elements with a $type property.
            //          The serializer was only adding the $type property to derived types held polymorphically.
            //          The StartStep in the Timeline is strongly typed as a TimelineStartStep. Thus, it didn't
            //              receive the $type property in json, and thus, would not work with this paradigm.
            //          The fix is to inject the $type property into the json object before it's deserialized.
            //              Note: This depends on a step which does not have a $type property, actually being a start step.

            var jToken = JToken.ReadFrom( reader );

            var refString = jToken.Value<string>( "$ref" );
            var firstReference = refString == null;

            if ( firstReference )
            {
                var typeString = jToken.Value<string>( "$type" );

                // Aforementioned fix.
                if ( typeString == null )
                {
                    System.Diagnostics.Debug.Assert( objectType == typeof( TimelineStartStep ) );

                    var jObject = jToken as JObject;
                    if ( jObject != null )
                    {
                        var t = typeof( TimelineStartStep );

                        typeString = t.ShortAssemblyQualifiedName();

                        jObject.InsertItem( 1, new JProperty( "$type", typeString ) );
                    }
                }

                if ( typeString != null )
                {
                    var type = Type.GetType( typeString );

                    if ( type == null )
                    {
                        var errorStep = new ErrorStep();

                        errorStep.JToken = jToken;

                        errorStep.FailedTypeString = typeString;

                        serializer.Populate( jToken.CreateReader(), errorStep );

                        return errorStep;
                    }
                }
            }

            return serializer.Deserialize( jToken.CreateReader() );
        }
    }
}