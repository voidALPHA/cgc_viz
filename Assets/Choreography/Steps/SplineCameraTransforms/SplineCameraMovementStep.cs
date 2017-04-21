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

using System.Collections;
using Choreography.CameraControl;
using JetBrains.Annotations;
using UnityEngine;
using Visualizers;

namespace Choreography.Steps.SplineCameraTransforms
{
    public abstract class SplineCameraMovementStep : Step
    {
        private const string EndEventName = "EndMovement";


        [Controllable, UsedImplicitly]
        protected void FromCamera()
        {
            OnSavePosition(SplineCameraControlLord.CameraParent);
        }

        protected virtual void OnSavePosition(GameObject cameraParent)
        {
        }


        [Controllable, UsedImplicitly]
        protected void ToCamera()
        {
            OnLoadPosition(SplineCameraControlLord.CameraParent);
        }

        protected virtual void OnLoadPosition(GameObject cameraParent)
        {
        }

        public override float BaseDuration
        {
            get { return Duration; }
        }

        private float m_Duration = 1f;

        [Controllable]
        public float Duration
        {
            get { return m_Duration; }
            set { m_Duration = Mathf.Max(value, 0.001f); }
        }


        public SplineCameraMovementStep()
        {
            Router.AddEvent(EndEventName);
        }

        public override void CleanUp()
        {
            GameObject.Destroy(TargetObject);

            TargetObject = null;
        }

        private SplineMovement Movement { get; set; }

        private bool StepCompleted { get; set; }

        private float DurationOverage { get; set; }

        private GameObject m_TargetObject;
        protected GameObject TargetObject
        {
            get
            {
                return m_TargetObject ?? (m_TargetObject = new GameObject("Camera Target"));
            }
            set { m_TargetObject = value; }
        }

        protected override IEnumerator ExecuteStep()
        {
            StepCompleted = false;
            Movement = CreateCameraMovement();
            Movement.OnMovementCompleted += HandleMovementCompleted;
            SplineCameraControlLord.EnqueueMovement(Movement);

            ConductOtherFunctionality();

            while (!StepCompleted)
                yield return null;

            Router.FireEvent(EndEventName, DurationOverage);
        }

        protected override void OnCancel()
        {
            SplineCameraControlLord.ClearCameraMovements();
        }

        private void HandleMovementCompleted( float finalProportion )
        {
            StepCompleted = true;

            var proportionOverage = finalProportion - 1.0f;

            DurationOverage = proportionOverage * Duration;
        }

        protected abstract SplineMovement CreateCameraMovement();

        protected virtual void ConductOtherFunctionality()
        {
        }
    }
}
