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
using Mutation;
using UnityEngine;
using Utility;
using Utility.JobManagerSystem;
using Visualizers;

namespace Kinetics.BoundMovements
{
    public class LerpToTargetOverPreciseTime : KineticNode
    {
        private MutableField<Vector3> m_StartPosition = new MutableField<Vector3>() { LiteralValue = Vector3.zero };
        [Controllable(LabelText = "Start Position")]
        public MutableField<Vector3> StartPosition { get { return m_StartPosition; } }

        private MutableField<BoundingBox> m_TargetBound = new MutableField<BoundingBox>() { AbsoluteKey = "TargetBound" };
        [Controllable(LabelText = "Target Bound")]
        public MutableField<BoundingBox> TargetBound { get { return m_TargetBound; } }

        private MutableField<float> m_StartTime = new MutableField<float>() 
        { AbsoluteKey = "Start Time" };
        [Controllable(LabelText = "Start Time")]
        public MutableField<float> StartTime { get { return m_StartTime; } }

        private MutableField<float> m_TransitionDuration = new MutableField<float>() { LiteralValue = 1f };
        [Controllable(LabelText = "Transition Duration")]
        public MutableField<float> TransitionDuration { get { return m_TransitionDuration; } }


        public override void StartKinetic(VisualPayload payload, Func<float, float> translateTime)
        {
            var transitionTimeInverse = 1f / TransitionDuration.GetFirstValue(payload.Data);
            var startTime = StartTime.GetFirstValue(payload.Data);

            var startParent = payload.VisualData.Bound.transform.parent;

            var localStart = payload.VisualData.Bound.transform.localPosition;

            var targetBound = TargetBound.GetFirstValue(payload.Data);

            var bound = payload.VisualData.Bound.CreateDependingBound("Lerp to Target");

            bound.transform.parent = targetBound.transform;

            bound.transform.position = StartPosition.GetFirstValue(payload.Data);

            payload.VisualData.Bound.transform.parent = bound.transform;

            payload.VisualData.Bound.transform.localPosition = Vector3.zero;


            var movementSatellite = bound.gameObject.AddComponent<BoundMovementSatellite>();

            var newPayload = new VisualPayload(payload.Data, new VisualDescription(bound));

            movementSatellite.MovementFunc = (trans) =>
            {
                float proportion = (Time.time - startTime) * transitionTimeInverse;

                if (proportion >= 1)
                {
                    trans.position = targetBound.transform.position;

                    movementSatellite.Cleanup();
                    return;
                }

                proportion = translateTime(proportion);

                var startPos = startParent.PiecewiseMultiply(localStart);
                var endPos = targetBound.transform.position;

                trans.position = Vector3.Lerp(startPos, endPos, proportion);
            };

            movementSatellite.CleanupFunc = (trans) =>
            {
                JobManager.Instance.StartJob(
                    Finished.Transmit(newPayload), jobName: "Kinetic Finished", startImmediately: true, maxExecutionsPerFrame: 1);
            };


            // execute first step immediately to set initial position
            movementSatellite.MovementFunc(movementSatellite.transform);
        }

    }
}
