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
using UnityEngine;

namespace Choreography.CameraControl
{
    public class SplineMovement
    {
        public Transform TargetTransform { get; set; }

        public Func<Vector3, Vector3, Vector3, float, Vector3> ComputeImaginaryFocus { get; set; }

        public Func<Vector3, Vector3, Vector3, float, Vector3> EvaluateSpline { get; set; }

        public Func<float, float> RealtimeToLocalTime { get; set; }

        public Func< float, float > ModulateLocalTime { get; set; }

        public event Action<float> OnMovementCompleted = delegate { };

        public virtual Vector3 MovementUpdate( Transform priorPoint, Transform currentPoint, Transform nextPoint, float realTime )
        {
            var proportion = RealtimeToLocalTime( realTime );
            var localTime = ModulateLocalTime( proportion );

            var focus = ComputeImaginaryFocus( priorPoint.position, currentPoint.position, nextPoint.position, localTime );

            if ( localTime >= 1 )
            {
                //Debug.Log("Final proportion: " + proportion);
                CompleteMovement( proportion );

                return nextPoint.position;
            }

            return EvaluateSpline( currentPoint.position, nextPoint.position, focus, localTime );
        }

        protected virtual void CompleteMovement( float finalProportion )
        {
            // other movement step cleanup?

            OnMovementCompleted( finalProportion );
        }
    }
}
