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
using System.Text;
using UnityEngine;

namespace Choreography.CameraControl.Modules
{
    public class RotateToFacePointInTimeModule : ICameraMovementModule
    {
        private Camera Cam { get { return CameraControlLord.MainCamera; } }

        private bool RotationCompleted = false;

        public bool ModuleConcluded { get { return RotationCompleted; } }


        private Quaternion StartRotation { get; set; }
        private Quaternion EndRotation { get
        {
            return Quaternion.LookRotation( TargetPosition - 
                (IsLookDirection?Vector3.zero:Cam.transform.position), 
                Vector3.up );
        } }

        private bool IsLookDirection = false;
        public Vector3 TargetPosition { get; set; }
        public float RotationDuration { get; set; }
        private float RotationTime { get; set; }

        public RotateToFacePointInTimeModule(Vector3 targetPosition, float rotationDuration, bool isLookDirection = false)
        {
            TargetPosition = targetPosition;
            RotationDuration = rotationDuration;
            IsLookDirection = isLookDirection;
        }

        public void PreMove()
        {
            RotationTime = 0f;

            StartRotation = Cam.transform.rotation;

            RotationCompleted = false;
        }

        public bool IsDoneMovingTowardTarget
        {
            get
            {
                //if (RotationCompleted)
                //    return true;

                RotationTime += Time.deltaTime;

                float proportion = RotationTime / RotationDuration;

                if ( proportion >= 1f )
                {
                    proportion = 1f;
                    //Debug.Log( "Rotation complete!" );
                    RotationCompleted = true;
                }

                Cam.transform.rotation = Quaternion.Slerp( StartRotation, EndRotation, proportion );

                return RotationCompleted;
            }
        }

        public void PostMove()
        {
            
        }
    }
}
