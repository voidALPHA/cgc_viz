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
using System.Linq;
using Bounds;
using Choreography.CameraControl;
using Choreography.CameraControl.SplineFunctionListings;
using JetBrains.Annotations;
using Kinetics;
using Newtonsoft.Json;
using UnityEngine;
using Visualizers;

namespace Choreography.Steps.SplineCameraTransforms
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LookAtBoundsRelatedPositionStep : Step
    {
        private const string EndEventName = "EndLook";

        [UsedImplicitly]
        public event Action LookDirty = delegate { };

        // In the "MoveToBoundsRelated..." steps, these are in a parent class (which parent this step does not have)
        [Controllable, UsedImplicitly]
        protected void FromCamera()
        {
            OnSavePosition(SplineCameraControlLord.CameraParent);
        }

        //protected virtual void OnSavePosition(GameObject cameraParent)
        //{
        //}


        [Controllable, UsedImplicitly]
        protected void ToCamera()
        {
            OnLoadPosition(SplineCameraControlLord.CameraParent);
        }

        //protected virtual void OnLoadPosition(GameObject cameraParent)
        //{
        //}

        private IBoundsProvider m_BoundsProvider;
        [Controllable(LabelText = "Bounds Provider")]
        public IBoundsProvider BoundsProvider { get { return m_BoundsProvider; } set { m_BoundsProvider = value; } }

        private Vector3 m_TargetOffset;
        [Controllable(LabelText = "Target Offset", ObservableEventName = "LookDirty")]
        public Vector3 TargetOffset
        {
            get { return m_TargetOffset; }
            set
            {
                m_TargetOffset = value;
                LookDirty();
            }
        }

        private SplineEvaluationFunctionTypes m_SplineType = SplineEvaluationFunctionTypes.Bezier;
        [Controllable(ObservableEventName = "LookDirty")]
        public SplineEvaluationFunctionTypes SplineType
        {
            get { return m_SplineType; }
            set
            {
                m_SplineType = value;
                LookDirty();
            }
        }

        private SplineComputeFocusFunctionTypes m_FocusType = SplineComputeFocusFunctionTypes.Linear;
        [Controllable(ObservableEventName = "LookDirty")]
        public SplineComputeFocusFunctionTypes FocusType
        {
            get { return m_FocusType; }
            set
            {
                m_FocusType = value;
                LookDirty();
            }
        }


        public override float BaseDuration
        {
            get { return Duration; }
        }

        private float m_Duration = 1f;
        [Controllable]
        public float Duration { get { return m_Duration; } set { m_Duration = value; } }

        private string m_TimeCurve = "EaseBoth";
        [Controllable]
        public string TimeCurve { get { return m_TimeCurve; } set { m_TimeCurve = value; } }

        public LookAtBoundsRelatedPositionStep()
        {
            Router.AddEvent( EndEventName );
        }

        private SplineMovement LookMovement { get; set; }

        private bool StepCompleted { get; set; }

        private float DurationOverage { get; set; }

        private GameObject m_TargetObject;
        protected GameObject TargetObject
        {
            get
            {
                return m_TargetObject ?? (m_TargetObject = new GameObject("Camera Look Target"));
            }
            set { m_TargetObject = value; }
        }

        protected override IEnumerator ExecuteStep()
        {
            StepCompleted = false;
            LookMovement = CreateLookMovement();
            LookMovement.OnMovementCompleted += HandleMovementCompleted;
            SplineCameraControlLord.EnqueueLookMovement(LookMovement);
            
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

        protected /*override*/ void OnSavePosition(GameObject cameraParent)
        {
            var targetPosition = cameraParent.transform.position;

            if (BoundsProvider != null && BoundsProvider.Bounds != null && BoundsProvider.Bounds.Any())
            {
                targetPosition = BoundsProvider.Bounds.First().transform.worldToLocalMatrix.MultiplyPoint(cameraParent.transform.position);
            }

            TargetOffset = targetPosition;
        }

        protected /*override*/ void OnLoadPosition(GameObject cameraParent)
        {
            var targetPosition = TargetOffset;

            if (BoundsProvider != null && BoundsProvider.Bounds != null && BoundsProvider.Bounds.Any())
            {
                targetPosition = BoundsProvider.Bounds.First().transform.localToWorldMatrix.MultiplyPoint(TargetOffset);
            }

            cameraParent.transform.position = targetPosition;
        }

        private SplineMovement CreateLookMovement()
        {
            if (BoundsProvider==null || !BoundsProvider.Bounds.Any())
                throw new Exception("Cannot focus on a bounds group!");
            var parentObject = BoundsProvider.Bounds.First();

            var lookModule = new SplineMovement();

            lookModule.ComputeImaginaryFocus = SplineFocusTypes.GetFocusFunction(FocusType);
            lookModule.EvaluateSpline = SplineTypes.GetEvaluationFunction(SplineType);

            if (TargetObject == null)
                TargetObject = new GameObject("Camera Look Point");
            TargetObject.transform.parent = parentObject.transform;
            TargetObject.transform.localPosition = TargetOffset;
            lookModule.TargetTransform = TargetObject.transform;

            var startTime = Time.time - DelayOverage;
            var timeCurve = CurveFactory.GetCurve(TimeCurve);

            lookModule.RealtimeToLocalTime = (realTime) => (realTime - startTime) / Duration;
            lookModule.ModulateLocalTime = (timeProportion) => timeCurve.Evaluate(timeProportion);

            //moveModule.OnMovementCompleted += ()=>Object.Destroy( targetObject );

            return lookModule;
        }

    }
}
