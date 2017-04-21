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
using Choreography.CameraControl;
using Choreography.CameraControl.Modules;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using Visualizers;

namespace Choreography.Steps.CameraTransform
{
    [JsonObject( MemberSerialization.OptIn )]
    public class CameraMoveOverTimeStep : CameraMovementStep
    {
        [UsedImplicitly]
        public event Action LookDirty = delegate { };

        private readonly ControllableCondition m_IsLookDirection = new ControllableCondition( false );
        [Controllable]
        private ControllableCondition IsLookDirection { get { return m_IsLookDirection; } }

        private Vector3 m_LookAtTarget;
        [Controllable( ConditionalPropertyName = "IsLookDirection", InvertConditional = true, ObservableEventName = "LookDirty" )]
        public Vector3 LookAtTarget
        {
            get { return m_LookAtTarget; }
            set
            {
                m_LookAtTarget = value;
                LookDirty();
            }
        }

        private Vector3 m_LookDirection;
        [Controllable( ConditionalPropertyName = "IsLookDirection", ObservableEventName = "LookDirty" )]
        private Vector3 LookDirection
        {
            get { return m_LookDirection; }
            set
            {
                m_LookDirection = value;
                LookDirty();
            }
        }


        [UsedImplicitly]
        public event Action MoveDirty = delegate { };

        private readonly ControllableCondition m_IsMoveDirection = new ControllableCondition( false );
        [Controllable]
        public ControllableCondition IsMoveDirection { get { return m_IsMoveDirection; } }


        private Vector3 m_MoveTarget;
        [Controllable( ConditionalPropertyName = "IsMoveDirection", InvertConditional = true,
            ObservableEventName = "MoveDirty" )]
        public Vector3 MoveTarget
        {
            get { return m_MoveTarget; }
            set
            {
                m_MoveTarget = value;
                MoveDirty();
            }
        }

        private Vector3 m_MoveDirection;
        [Controllable( ConditionalPropertyName = "IsMoveDirection", ObservableEventName = "MoveDirty" )]
        public Vector3 MoveDirection
        {
            get { return m_MoveDirection; }
            set
            {
                m_MoveDirection = value;
                MoveDirty();
            }
        }


        public override float BaseDuration
        {
            get { return Duration; }
        }

        [Controllable]
        public float Duration { get; set; }

        protected override ICameraMovement CreateCameraMovement()
        {
            throw new Exception("Non-spline-based camera transition!");

            //var moveModule = new MoveToPositionInTimeModule(
            //    IsMoveDirection?MoveDirection:MoveTarget, Duration, !IsMoveDirection);

            //var rotateModule = new RotateToFacePointInTimeModule(
            //    IsLookDirection ? LookDirection : LookAtTarget, Duration, IsLookDirection );

            //var cameraMove = new CameraMovement();
            //cameraMove.MovementModules.Add( moveModule );
            //cameraMove.MovementModules.Add( rotateModule );

            //return cameraMove;
        }

        protected override void OnSavePosition( Camera camera )
        {
            IsMoveDirection.ConditionValid = false;
            MoveTarget = camera.transform.position;

            IsLookDirection.ConditionValid = true;
            LookDirection = camera.transform.forward;
        }

        protected override void OnLoadPosition( Camera camera )
        {
            if ( IsMoveDirection.ConditionValid )
            {
                Debug.Log( "Load camera pos with IsMoveDirection true is not implemented..." );
            }
            else
            {
                camera.transform.position = MoveTarget;
            }

            if ( IsLookDirection.ConditionValid )
            {
                camera.transform.forward = LookDirection;
            }
            else
            {
                Debug.Log( "Load camera pos with IsLookDirection true is not implemented..." );
            }
        }
    }
}
