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
using Newtonsoft.Json;

namespace Choreography.Steps
{
    [JsonObject( MemberSerialization.OptIn )]
    public class StepRouter
    {
        public event Action< StepEvent > EventAdded = delegate { };
        public event Action< StepEvent > EventRemoved = delegate { };

        private List<StepEvent> m_Events = new List<StepEvent>();
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        private List<StepEvent> Events
        {
            get { return m_Events; }
            set
            {
                m_Events = value;
                foreach ( var @event in m_Events )
                {
                    @event.Timeline = Timeline;
                }
            }
        }

        public IEnumerable<StepEvent> EventsEnumerable { get { return Events.AsReadOnly(); } }

        public StepEvent this[ String key ]
        {
            get
            {
                var foundEvent = Events.FirstOrDefault( e => e.Name == key );

                if ( foundEvent == null )
                    throw new KeyNotFoundException("Could not find StepEvent with key " + key);

                return foundEvent;
            }
        }

        private Choreography.Timeline m_Timeline;

        public Choreography.Timeline Timeline
        {
            get
            {
                return m_Timeline;
            }
            set
            {
                m_Timeline = value;

                foreach ( var @event in Events )
                {
                    @event.Timeline = m_Timeline;
                }
            }
        }

        public StepRouter()
        {
            
        }

        #region Add / Remove Events

        public void AddEvent( string stepEventName )
        {
            AddEvent( new StepEvent( stepEventName ) );
        }

        public void RemoveEvent( string stepEventName )
        {
            RemoveEvent( this[ stepEventName ] );
        }


        private void AddEvent( StepEvent stepEvent )
        {
            if ( Events.Any(s=>s.Name == stepEvent.Name) )
                return;

            stepEvent.Timeline = Timeline;

            Events.Add( stepEvent );

            EventAdded( stepEvent );
        }

        private void RemoveEvent( StepEvent stepEvent )
        {
            if ( !Events.Contains( stepEvent ) )
                return;

            stepEvent.Timeline = null;

            Events.Remove( stepEvent );

            EventRemoved( stepEvent );
        }
        #endregion

        #region Add / Remove Targets to Events

        public void AddTarget(string stepEventName, Step target)
        {
            this[stepEventName].AddTarget(target);
        }

        public void RemoveTarget(string stepEventName, Step target)
        {
            this[stepEventName].RemoveTarget(target);
        }

        public IEnumerable< Step > GetTargetsEnumerable( string eventName )
        {
            return this[ eventName ].TargetsEnumerable;
        }

        #endregion

        public void FireEvent(string stepEventName, float durationOverage = 0.0f)
        {
            if (Events.All(s => s.Name != stepEventName))
                return;

            this[stepEventName].FireEvent(durationOverage);
        }


        public float RecursiveDelayedDuration
        {
            get { return EventsEnumerable.Select( e => e.RecursiveDelayedDuration ).DefaultIfEmpty(0).Max(); }
        }

        public int RecursiveCount
        {
            get { return EventsEnumerable.Select( e => e.RecursiveCount ).DefaultIfEmpty(0).Sum(); }
        }

        public bool RecursiveDurationIsEstimated
        {
            get { return EventsEnumerable.Any( e => e.RecursiveDurationIsEstimated ); }
        }

    }
}