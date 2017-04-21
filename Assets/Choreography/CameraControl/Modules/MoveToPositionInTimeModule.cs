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

using UnityEngine;

namespace Choreography.CameraControl.Modules
{
    public class MoveToPositionInTimeModule : ICameraMovementModule
    {
        private Camera Cam { get { return CameraControlLord.MainCamera; } }
        
        private bool MovementCompleted = false;

        public bool ModuleConcluded { get { return MovementCompleted; } }

        private Vector3 StartPosition { get; set; }
        public Vector3 TargetPosition { get; set; }
        private Vector3 GlobalEndPosition { get; set; }

        public bool IsAbsolutePosition { get; set; }
        public float MovementDuration { get; set; }
        private float MovementTime { get; set; }

        public MoveToPositionInTimeModule(Vector3 targetPosition, float movementDuration, bool isAbsolutePosition = true)
        {
            TargetPosition = targetPosition;
            MovementDuration = movementDuration;
            IsAbsolutePosition = isAbsolutePosition;
        }

        public void PreMove()
        {
            MovementTime = 0f;

            StartPosition = Cam.transform.position;

            GlobalEndPosition = IsAbsolutePosition
                ? TargetPosition
                : Cam.transform.position + TargetPosition;

            MovementCompleted = false;
        }

        public bool IsDoneMovingTowardTarget
        {
            get
            {
                if ( MovementCompleted )
                    return true;

                MovementTime += Time.deltaTime;

                float proportion = MovementTime / MovementDuration;

                if ( proportion >= 1f )
                {
                    proportion = 1f;
                    //Debug.Log("Movement complete!");
                    MovementCompleted = true;
                }

                Cam.transform.position = Vector3.Lerp( StartPosition, GlobalEndPosition, proportion );

                return MovementCompleted;
            }
        }

        public void PostMove()
        {
            
        }

    }
}
