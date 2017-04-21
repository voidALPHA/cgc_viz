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
using Bounds;
using Choreography.CameraControl;
using Choreography.CameraControl.Modules;
using JetBrains.Annotations;
using UnityEngine;
using Visualizers;

namespace Choreography.Steps.CameraTransform
{
    public class CameraFocusOnTargetsOverTime : CameraMovementStep
    {
        [Controllable(LabelText = "Look Direction")]
        public Vector3 LookDirection { get; set; }

        [Controllable]
        public float Duration { get; set; }

        [Controllable]
        public float FocusPadding { get; set; }

        [Controllable(LabelText = "Minimum Camera Distance")]
        public float MinCameraDistance { get; set; }

        public override float BaseDuration
        {
            get { return Duration; }
        }

        [Controllable, UsedImplicitly]
        public IBoundsProvider BoundsToFocus { get; set; }


        protected override ICameraMovement CreateCameraMovement()
        {
            var moveModule = new MoveToPositionInTimeModule(
                ComputeViewPosition(
                    LookDirection, 
                    BoundsToFocus.Bounds,
                    CameraControlLord.MainCamera.fieldOfView,
                    CameraControlLord.MainCamera.aspect,
                    FocusPadding, 
                    MinCameraDistance),
                Duration, true);

            var rotateModule = new RotateToFacePointInTimeModule(
                LookDirection, Duration, true);

            var cameraMove = new CameraMovement();
            cameraMove.MovementModules.Add(moveModule);
            cameraMove.MovementModules.Add(rotateModule);

            return cameraMove;
        }

        protected override void OnSavePosition(Camera camera)
        {
            LookDirection = camera.transform.forward;
        }

        protected override void OnLoadPosition(Camera camera)
        {
            camera.transform.forward = LookDirection;
        }

        public static Vector3 ComputeViewPosition(Vector3 cameraForward, IEnumerable<BoundingBox> targets, float fieldOfView, float aspectRatio, float focusPadding=10f, float minCameraDistance=1f)
        {
            if ( !targets.Any() )
                return Vector3.zero;

            var center = targets.Aggregate(Vector3.zero, (current, bound) => current + bound.Centerpoint) / ((float)targets.Count());

            var w = Vector3.Cross(cameraForward, Vector3.up).normalized;

            var v = Vector3.Cross(cameraForward, w).normalized;

            var thetaVRadians = fieldOfView/2f*Mathf.Deg2Rad;
            var vMult = Mathf.Tan(thetaVRadians);

            var wMult = vMult*aspectRatio;

            var maxX = 0f;
            var maxY = 0f;

            foreach (var bound in targets)
            {
                var output = new Vector3();


                var widthCompensatedPosition = bound.Centerpoint;
                var delta = widthCompensatedPosition - center;

                var localU = Vector3.Dot(cameraForward, delta);
                output.z = localU;

                output.x = Vector3.Dot(w, delta);
                output.y = Vector3.Dot(v, delta);

                var nVx = output.x + Mathf.Sign(output.x) * wMult * -localU + Mathf.Sign(output.x) * focusPadding;
                var nVy = output.y + Mathf.Sign(output.y) * vMult * -localU + Mathf.Sign(output.y) * focusPadding;

                output.x = nVx * output.x < 0 ? 0 : nVx;
                output.y = nVy * output.y < 0 ? 0 : nVy;


                var x = Math.Abs(output.x);
                if (x > maxX)
                    maxX = x;

                var y = Math.Abs(output.y);
                if (y > maxY)
                    maxY = y;
            }

            maxX = maxX / wMult;
            maxY = maxY / vMult;


            var distance = Mathf.Max(maxX, maxY) + focusPadding;

            if (distance < minCameraDistance)
                distance = minCameraDistance;

            return center - cameraForward * distance;
        }

    }
}
