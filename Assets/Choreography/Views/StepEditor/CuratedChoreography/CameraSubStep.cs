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
using Assets.Utility;
using Choreography.CameraControl;
using Choreography.CameraControl.SplineFunctionListings;
using Kinetics;
using UnityEngine;

namespace Choreography.Views.StepEditor.CuratedChoreography
{
    public class CameraSubStep
    {
        public float Duration { get; set; }
        public float Delay { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Facing { get; set; }

        public string ToString() { return Position.ToString()+":"+Facing.ToString()+":"+Duration+":"+Delay; }

        public static CameraSubStep GenerateFromString( string input )
        {
            var tokens = input.Split( ':' );

            if (tokens.Length<4)
                throw new Exception("Cannot convert string " + input + " into a substep!");

            var subStep = new CameraSubStep();

            Vector3 vectorBuffer = Vector3.zero;
            if (!VectorUtility.TryParseVector( tokens[ 0 ], ref vectorBuffer ))
                throw new Exception("Cannot convert string " + tokens[0] + " into a Vector3!");
            subStep.Position = vectorBuffer;

            if (!VectorUtility.TryParseVector(tokens[1], ref vectorBuffer))
                throw new Exception("Cannot convert string " + tokens[1] + " into a Vector3!");
            subStep.Facing = vectorBuffer;

            float floatBuffer = 0f;
            if (!float.TryParse( tokens[2], out floatBuffer ))
                throw new Exception("Cannot convert string " + tokens[2] + " into a float!");
            subStep.Duration = Mathf.Max(floatBuffer,.001f);

            if (!float.TryParse(tokens[3], out floatBuffer))
                throw new Exception("Cannot convert string " + tokens[3] + " into a float!");
            subStep.Delay = Mathf.Max(floatBuffer, .001f);

            return subStep;
        }

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

        private GameObject m_FacingObject;
        protected GameObject FacingObject
        {
            get
            {
                return m_FacingObject ?? (m_FacingObject = new GameObject("Camera Facing Target"));
            }
            set { m_FacingObject = value; }
        }

        public SplineMovement GenerateTranslationMovement(float startTime)
        {
            var cameraMovement = new SplineMovement();

            cameraMovement.ComputeImaginaryFocus = SplineFocusTypes.GetFocusFunction( SplineComputeFocusFunctionTypes.Linear );
            cameraMovement.EvaluateSpline = SplineTypes.GetEvaluationFunction(SplineEvaluationFunctionTypes.Bezier);

            TargetObject.transform.position = Position;
            TargetObject.transform.parent = null;
            cameraMovement.TargetTransform = TargetObject.transform;
            
            var timeCurve = CurveFactory.GetCurve("EaseBoth");

            cameraMovement.RealtimeToLocalTime = (realTime) => (realTime - startTime) / Duration;
            cameraMovement.ModulateLocalTime = (timeProportion) => timeCurve.Evaluate(timeProportion);

            return cameraMovement;
        }

        public SplineMovement GenerateLookMovement( float startTime )
        {
            var lookModule = new SplineMovement();

            //lookModule.ComputeImaginaryFocus = SplineFocusTypes.GetFocusFunction(FocusType);
            lookModule.ComputeImaginaryFocus = SplineFocusTypes.GetFocusFunction(SplineComputeFocusFunctionTypes.Linear);
            lookModule.EvaluateSpline = SplineTypes.GetEvaluationFunction(SplineEvaluationFunctionTypes.Bezier);

            FacingObject.transform.parent = SplineCameraControlLord.CameraParent.transform;
            FacingObject.transform.localPosition = Facing;
            lookModule.TargetTransform = FacingObject.transform;
            
            var timeCurve = CurveFactory.GetCurve("EaseBoth");

            lookModule.RealtimeToLocalTime = (realTime) => (realTime - startTime) / Duration;
            lookModule.ModulateLocalTime = (timeProportion) => timeCurve.Evaluate(timeProportion);
            
            return lookModule;
        }

    }
}
