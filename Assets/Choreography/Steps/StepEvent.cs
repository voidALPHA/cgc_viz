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
using System.Configuration;
using System.Linq;
using Chains;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using Utility.JobManagerSystem;

namespace Choreography.Steps
{
    [JsonObject( MemberSerialization.OptIn )]
    public class StepEvent
    {
        public event Action< Step > TargetAdded = delegate { };

        public event Action< Step > TargetRemoved = delegate { };

        public event Action TargetOrderChanged = delegate { };


        private string m_Name = "";

        [JsonProperty]
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        private Choreography.Timeline m_Timeline;

        public Choreography.Timeline Timeline
        {
            get { return m_Timeline; }
            set
            {
                m_Timeline = value;

                foreach ( var target in Targets )
                {
                    target.Timeline = m_Timeline;
                }
            }
        }


        private List< Step > m_Targets = new List< Step >();

        [JsonProperty( ObjectCreationHandling = ObjectCreationHandling.Replace )]
        private List< Step > Targets
        {
            get { return m_Targets; }
            [UsedImplicitly]
            set
            {
                foreach ( var target in value )
                {
                    AddTarget( target );

                    target.Timeline = Timeline;
                }
            }
        }

        public IEnumerable< Step > TargetsEnumerable
        {
            get { return Targets.AsReadOnly(); }
        }


        public void AddTarget( Step step )
        {
            if ( Targets.Contains( step ) )
                return;

            step.Timeline = Timeline;

            step.RemovalRequested += HandleStepRemovalRequested;
            step.MoveUpRequested += HandleStepMoveUpRequested;
            step.MoveDownRequested += HandleStepMoveDownRequested;
            step.DuplicationRequested += HandleStepDuplicationRequested;
            step.ReplacementRequested += HandleStepReplacementRequested;

            Targets.Add( step );

            TargetAdded( step );
        }

        public void RemoveTarget( Step step )
        {
            if ( !Targets.Contains( step ) )
                return;

            step.Timeline = null;

            step.RemovalRequested -= HandleStepRemovalRequested;
            step.MoveUpRequested -= HandleStepMoveUpRequested;
            step.MoveDownRequested -= HandleStepMoveDownRequested;
            step.DuplicationRequested -= HandleStepDuplicationRequested;
            step.ReplacementRequested -= HandleStepReplacementRequested;

            Targets.Remove( step );

            TargetRemoved( step );
        }

        private void HandleStepRemovalRequested( Step step )
        {
            RemoveTarget( step );
        }

        private void HandleStepMoveDownRequested( Step step )
        {
            MoveStep( step, +1 );
        }

        private void HandleStepMoveUpRequested( Step step )
        {
            MoveStep( step, -1 );
        }

        private void HandleStepDuplicationRequested( Step step )
        {
            var settings = HaxxisPackage.GetSerializationSettings( TypeNameHandling.All );

            var json = JsonConvert.SerializeObject( step, Formatting.Indented, settings );
            var dupe = JsonConvert.DeserializeObject< Step >( json, settings );

            AddTarget( dupe );
        }

        private void HandleStepReplacementRequested( Step oldStep, Step newStep )
        {
            // TODO: Get old step index, move new one to there.

            var oldIndex = Targets.IndexOf( oldStep );

            AddTarget( newStep );

            RemoveTarget( oldStep );

            var newIndex = Targets.IndexOf( newStep );

            var moveCount = oldIndex - newIndex;
            var direction = Math.Sign( moveCount );

            for ( var i = 0; i < moveCount * direction; i++ )
            {
                MoveStep( newStep, Math.Sign( moveCount ) );
            }
        }

        private void MoveStep( Step step, int direction )
        {
            direction = Math.Sign( direction );

            var startIndex = Targets.IndexOf( step );

            if ( startIndex == -1 )
                return;


            var newIndex = startIndex + direction;

            if ( newIndex == Targets.Count )
                return;

            if ( newIndex == -1 )
                return;


            Targets.RemoveAt( startIndex );
            Targets.Insert( newIndex, step );

            TargetOrderChanged();
        }

        public StepEvent( string name )
        {
            Name = name;
        }

        public void FireEvent( float durationOverage )
        {
            foreach ( var target in Targets ) // start job
                target.Play( durationOverage );
        }

        public float RecursiveDelayedDuration
        {
            get { return TargetsEnumerable.Select( t => t.RecursiveDelayedDuration ).DefaultIfEmpty( 0 ).Max(); }
        }

        public int RecursiveCount
        {
            get { return TargetsEnumerable.Select( t => t.RecursiveCount ).DefaultIfEmpty( 0 ).Sum(); }
        }

        public bool RecursiveDurationIsEstimated
        {
            get { return TargetsEnumerable.Any( t => t.RecursiveDurationIsEstimated ); }
        }
    }
}