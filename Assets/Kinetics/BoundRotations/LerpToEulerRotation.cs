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
using Kinetics.BoundMovements;
using Mutation;
using UnityEngine;
using Utility.JobManagerSystem;
using Visualizers;

namespace Kinetics.BoundRotations
{
    public class LerpToEulerRotation : KineticNode
    {
        private MutableField<Vector3> m_StartEuler = new MutableField<Vector3>() 
        { LiteralValue = Vector3.zero };
        [Controllable(LabelText = "Start Euler")]
        public MutableField<Vector3> StartEuler { get { return m_StartEuler; } }

        private MutableField<Vector3> m_EndEuler = new MutableField<Vector3>() 
        { LiteralValue = Vector3.zero };
        [Controllable(LabelText = "End Euler")]
        public MutableField<Vector3> EndEuler { get { return m_EndEuler; } }

        private MutableField<float> m_TransitionTime = new MutableField<float>() 
        { LiteralValue = 1f };
        [Controllable(LabelText = "TransitionTime")]
        public MutableField<float> TransitionTime { get { return m_TransitionTime; } }


        public override void StartKinetic(VisualPayload payload, Func<float, float> translateTime)
        {
            var startRotation = Quaternion.Euler(StartEuler.GetFirstValue(payload.Data));
            var endRotation = Quaternion.Euler(EndEuler.GetFirstValue(payload.Data));
            var transitionTimeInverse = 1f / TransitionTime.GetFirstValue(payload.Data);
            var startTime = Time.time;

            var bound = payload.VisualData.Bound.CreateDependingBound("Lerp Rotation");

            bound.transform.parent = payload.VisualData.Bound.transform.parent;

            payload.VisualData.Bound.transform.parent = bound.transform;

            var rotationSatellite =
                bound.gameObject.AddComponent<BoundMovementSatellite>();

            var newPayload = new VisualPayload(payload.Data, new VisualDescription(bound));

            rotationSatellite.MovementFunc = (trans) =>
            {
                float proportion = (Time.time - startTime) * transitionTimeInverse;

                if (proportion >= 1)
                {
                    trans.localRotation = endRotation;

                    rotationSatellite.Cleanup();
                    return;
                }

                proportion = translateTime(proportion);

                trans.localRotation = Quaternion.Lerp(startRotation, endRotation, proportion);
            };

            rotationSatellite.CleanupFunc = (trans) =>
            {
                JobManager.Instance.StartJob(
                    Finished.Transmit(newPayload), jobName: "Kinetic Finished", startImmediately: true, maxExecutionsPerFrame: 1);
            };

            // execute first step immediately to set initial position
            rotationSatellite.MovementFunc(rotationSatellite.transform);

            //throw new System.NotImplementedException();
        }

    }
}
