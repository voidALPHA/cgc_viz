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
using Kinetics.BoundMovements;
using Mutation;
using UnityEngine;
using Utility.JobManagerSystem;
using Visualizers;

namespace Kinetics.BoundRotations
{
    public class LerpToRotationOverPreciseTime : KineticNode
    {
        private MutableField<Quaternion> m_StartRotation = new MutableField<Quaternion>() { LiteralValue = Quaternion.identity };
        [Controllable(LabelText = "Start Rotation")]
        public MutableField<Quaternion> StartRotation { get { return m_StartRotation; } }

        private MutableField<Quaternion> m_EndRotation = new MutableField<Quaternion>() { LiteralValue = Quaternion.identity };
        [Controllable(LabelText = "End Rotation")]
        public MutableField<Quaternion> EndRotation { get { return m_EndRotation; } }

        private MutableField<float> m_StartTime = new MutableField<float>() { AbsoluteKey = "Start Time" };
        [Controllable(LabelText = "Start Time")]
        public MutableField<float> StartTime { get { return m_StartTime; } }

        private MutableField<float> m_TransitionDuration = new MutableField<float>() { LiteralValue = 1f };
        [Controllable(LabelText = "Transition Duration")]
        public MutableField<float> TransitionDuration { get { return m_TransitionDuration; } }


        public override void StartKinetic(VisualPayload payload, Func<float, float> translateTime)
        {
            var startRotation = StartRotation.GetFirstValue(payload.Data);
            var endRotation = EndRotation.GetFirstValue(payload.Data);
            var transitionTimeInverse = 1f / TransitionDuration.GetFirstValue(payload.Data);
            var startTime = StartTime.GetFirstValue(payload.Data);

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
                    trans.rotation = endRotation;

                    rotationSatellite.Cleanup();
                    return;
                }

                proportion = translateTime(proportion);

                trans.rotation = Quaternion.Lerp(startRotation, endRotation, proportion);
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
