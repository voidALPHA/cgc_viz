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
using Mutation;
using UnityEngine;
using Utility.JobManagerSystem;
using Visualizers;

namespace Kinetics.BoundMovements
{
    public class LerpToPosition : KineticNode
    {
        private MutableField<Vector3> m_StartPosition = new MutableField<Vector3>() 
        { LiteralValue = Vector3.zero };
        [Controllable(LabelText = "Start Position")]
        public MutableField<Vector3> StartPosition { get { return m_StartPosition; } }

        private MutableField<Vector3> m_EndPosition = new MutableField<Vector3>() 
        { LiteralValue = Vector3.one };
        [Controllable(LabelText = "End Position")]
        public MutableField<Vector3> EndPosition { get { return m_EndPosition; } }

        private MutableField<float> m_TransitionTime = new MutableField<float>() 
        { LiteralValue = 1f };
        [Controllable(LabelText = "Transition Time")]
        public MutableField<float> TransitionTime { get { return m_TransitionTime; } }

        private MutableField<bool> m_LocalSpace = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "Use Local Space?")]
        public MutableField<bool> LocalSpace { get { return m_LocalSpace; } }


        public override void StartKinetic( VisualPayload payload, Func<float, float> translateTime )
        {
            var localSpace = LocalSpace.GetFirstValue( payload.Data );

            var startVector = StartPosition.GetFirstValue( payload.Data );
            var endVector = EndPosition.GetFirstValue( payload.Data );
            var transitionTimeInverse = 1f/TransitionTime.GetFirstValue( payload.Data );
            var startTime = Time.time;

            var bound = payload.VisualData.Bound.CreateDependingBound( "Lerp Position" );

            bound.transform.parent = payload.VisualData.Bound.transform.parent;

            payload.VisualData.Bound.transform.parent = bound.transform;

            var movementSatellite =
                bound.gameObject.AddComponent<BoundMovementSatellite>();

            var newPayload = new VisualPayload( payload.Data, new VisualDescription( bound ) );

            movementSatellite.MovementFunc = ( trans ) =>
            {
                float proportion = ( Time.time - startTime ) * transitionTimeInverse;

                if ( proportion >= 1 )
                {
                    if ( localSpace )
                        trans.localPosition = endVector;
                    else 
                        trans.position = endVector;
                    
                    movementSatellite.Cleanup();
                    return;
                }

                proportion = translateTime(proportion);

                var currentPos = Vector3.Lerp(startVector, endVector, proportion);

                if (localSpace)
                    trans.localPosition = currentPos;
                else
                    trans.position = currentPos;
            };

            movementSatellite.CleanupFunc = ( trans ) =>
            {
                JobManager.Instance.StartJob(
                    Finished.Transmit(newPayload), jobName: "Kinetic Finished", startImmediately: true, maxExecutionsPerFrame: 1);
            };

            // execute first step immediately to set initial position
            movementSatellite.MovementFunc( movementSatellite.transform );

            //throw new System.NotImplementedException();
        }
    }
}
