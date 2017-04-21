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
using Choreography.CameraControl.SplineFunctionListings;
using JetBrains.Annotations;
using Kinetics;
using Newtonsoft.Json;
using UnityEngine;
using Visualizers;

namespace Choreography.Steps.SplineCameraTransforms
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MoveToPositionAndFacingStep : SplineCameraMovementStep
    {
        public override void CleanUp()
        {
            GameObject.Destroy( FacingObject );
            GameObject.Destroy( TargetObject );

            FacingObject = null;
            TargetObject = null;
        }

        [UsedImplicitly]
        public event Action LookDirty = delegate { };


        protected override void OnSavePosition(GameObject cameraParent)
        {
            TargetPosition = cameraParent.transform.position;
            TargetFacing = SplineCameraControlLord.MainCamera.transform.forward;
        }

        protected override void OnLoadPosition(GameObject cameraParent)
        {
            cameraParent.transform.position = TargetPosition;
            SplineCameraControlLord.MainCamera.transform.LookAt( TargetPosition + TargetFacing);
        }

        private Vector3 m_TargetPosition = Vector3.zero;
        [Controllable(ObservableEventName = "LookDirty")]
        public Vector3 TargetPosition
        {
            get { return m_TargetPosition; }
            set
            {
                m_TargetPosition = value;
                LookDirty();
            }
        }
        private Vector3 m_TargetFacing = Vector3.one;
        [Controllable(ObservableEventName = "LookDirty")]
        public Vector3 TargetFacing { get { return m_TargetFacing; } set { m_TargetFacing = value; LookDirty(); } }


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

        private string m_TimeCurve = "EaseBoth";
        [Controllable]
        public string TimeCurve { get { return m_TimeCurve; } set { m_TimeCurve = value; } }


        private GameObject m_FacingObject;
        protected GameObject FacingObject
        {
            get
            {
                return m_FacingObject ?? (m_FacingObject = new GameObject("Camera Facing Target"));
            }
            set { m_FacingObject = value; }
        }

        protected override SplineMovement CreateCameraMovement()
        {
            var moveModule = new SplineMovement();

            moveModule.ComputeImaginaryFocus = SplineFocusTypes.GetFocusFunction(FocusType);
            moveModule.EvaluateSpline = SplineTypes.GetEvaluationFunction(SplineType);

            TargetObject.transform.position = TargetPosition;
            TargetObject.transform.parent = null;
            moveModule.TargetTransform = TargetObject.transform;

            var startTime = Time.time - DelayOverage;
            var timeCurve = CurveFactory.GetCurve(TimeCurve);

            moveModule.RealtimeToLocalTime = (realTime) => (realTime - startTime) / Duration;
            moveModule.ModulateLocalTime = (timeProportion) => timeCurve.Evaluate(timeProportion);


            //moveModule.OnMovementCompleted += ()=>Object.Destroy( targetObject );

            return moveModule;
        }

        protected override void ConductOtherFunctionality()
        {
            var lookModule = new SplineMovement();

            //lookModule.ComputeImaginaryFocus = SplineFocusTypes.GetFocusFunction(FocusType);
            lookModule.ComputeImaginaryFocus = SplineFocusTypes.GetFocusFunction(SplineComputeFocusFunctionTypes.Linear);
            lookModule.EvaluateSpline = SplineTypes.GetEvaluationFunction(SplineType);

            FacingObject.transform.parent = SplineCameraControlLord.CameraParent.transform;
            FacingObject.transform.localPosition = TargetFacing;
            lookModule.TargetTransform = FacingObject.transform;

            var startTime = Time.time - DelayOverage;
            //var timeCurve = CurveFactory.GetCurve(TimeCurve);
            var timeCurve = CurveFactory.GetCurve("EaseBoth");

            lookModule.RealtimeToLocalTime = (realTime) => (realTime - startTime) / Duration;
            lookModule.ModulateLocalTime = (timeProportion) => timeCurve.Evaluate(timeProportion);

            //moveModule.OnMovementCompleted += ()=>Object.Destroy( FacingObject );

            SplineCameraControlLord.EnqueueLookMovement( lookModule );
        }
    }
}
