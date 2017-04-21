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
    public class MoveToBoundsRelatedPositionStep : SplineCameraMovementStep
    {
        [UsedImplicitly]
        public event Action LookDirty = delegate { };

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

        private SplineComputeFocusFunctionTypes m_FocusType = SplineComputeFocusFunctionTypes.Projection;
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


        private string m_TimeCurve = "Linear";
        [Controllable]
        public string TimeCurve { get { return m_TimeCurve; } set { m_TimeCurve = value; } }



        protected override void OnSavePosition(GameObject cameraParent)
        {
            var targetPosition = cameraParent.transform.position;

            if (BoundsProvider != null && BoundsProvider.Bounds != null && BoundsProvider.Bounds.Any())
            {
                targetPosition = BoundsProvider.Bounds.First().transform.worldToLocalMatrix.MultiplyPoint(cameraParent.transform.position);
            }

            TargetOffset = targetPosition;
        }

        protected override void OnLoadPosition(GameObject cameraParent)
        {
            var targetPosition = TargetOffset;

            if (BoundsProvider != null && BoundsProvider.Bounds != null && BoundsProvider.Bounds.Any())
            {
                targetPosition = BoundsProvider.Bounds.First().transform.localToWorldMatrix.MultiplyPoint(TargetOffset);
            }

            cameraParent.transform.position = targetPosition;
        }

        protected override SplineMovement CreateCameraMovement()
        {
            if (!BoundsProvider.Bounds.Any())
                throw new Exception("No bound objects available!");

            var parentObject = BoundsProvider.Bounds.First();

            var moveModule = new SplineMovement();

            moveModule.ComputeImaginaryFocus = SplineFocusTypes.GetFocusFunction(FocusType);
            moveModule.EvaluateSpline = SplineTypes.GetEvaluationFunction(SplineType);

            if (TargetObject==null)
                TargetObject = new GameObject("Camera movement Target");
            TargetObject.transform.parent = parentObject.transform;
            TargetObject.transform.localPosition = TargetOffset;
            moveModule.TargetTransform = TargetObject.transform;

            var startTime = Time.time - DelayOverage;
            var timeCurve = CurveFactory.GetCurve(TimeCurve);

            moveModule.RealtimeToLocalTime = (realTime) => (realTime - startTime) / Duration;
            moveModule.ModulateLocalTime = (timeProportion) => timeCurve.Evaluate(timeProportion);


            //moveModule.OnMovementCompleted += ()=>Object.Destroy( targetObject );

            return moveModule;
        }
    }
}
