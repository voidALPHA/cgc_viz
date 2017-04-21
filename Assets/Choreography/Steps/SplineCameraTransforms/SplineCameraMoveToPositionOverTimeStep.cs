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
    public class SplineCameraMoveToPositionOverTimeStep : SplineCameraMovementStep
    {
        [UsedImplicitly]
        public event Action LookDirty = delegate { };

        
        protected override void OnSavePosition(GameObject cameraParent)
        {
            TargetPosition = cameraParent.transform.position;
        }

        protected override void OnLoadPosition(GameObject cameraParent)
        {
            cameraParent.transform.position = TargetPosition;
        }

        private Vector3 m_TargetPosition = Vector3.zero;
        [Controllable( ObservableEventName = "LookDirty" )]
        public Vector3 TargetPosition
        {
            get { return m_TargetPosition;}
            set
            {
                m_TargetPosition = value;
                LookDirty();
            }
        }


        private SplineEvaluationFunctionTypes m_SplineType = SplineEvaluationFunctionTypes.Bezier;
        [Controllable( ObservableEventName = "LookDirty" )]
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
        [Controllable( ObservableEventName = "LookDirty" )]
        public SplineComputeFocusFunctionTypes FocusType
        {
            get { return m_FocusType; }
            set { m_FocusType = value;
                LookDirty();
            }
        }
        
        private string m_TimeCurve = "Linear";
        [Controllable]
        public string TimeCurve { get {return m_TimeCurve;} set { m_TimeCurve = value; } }


        protected override SplineMovement CreateCameraMovement()
        {
            var moveModule = new SplineMovement();
            
            moveModule.ComputeImaginaryFocus = SplineFocusTypes.GetFocusFunction( FocusType );
            moveModule.EvaluateSpline = SplineTypes.GetEvaluationFunction( SplineType );
            
            TargetObject.transform.position = TargetPosition;
            TargetObject.transform.parent = null;
            moveModule.TargetTransform = TargetObject.transform;

            var startTime = Time.time - DelayOverage;
            var timeCurve = CurveFactory.GetCurve( TimeCurve );

            moveModule.RealtimeToLocalTime = ( realTime ) => ( realTime - startTime ) / Duration;
            moveModule.ModulateLocalTime = ( timeProportion ) => timeCurve.Evaluate( timeProportion );


            //moveModule.OnMovementCompleted += ()=>Object.Destroy( targetObject );

            return moveModule;
        }
    }
}
