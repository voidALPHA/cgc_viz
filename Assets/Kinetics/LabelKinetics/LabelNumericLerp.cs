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
using Visualizers.LabelController;

namespace Kinetics.LabelKinetics
{
    public class LabelNumericLerp : KineticNode
    {
        private MutableField<float> m_StartValue = new MutableField<float>() 
        { LiteralValue = 0f };
        [Controllable(LabelText = "Start Value")]
        public MutableField<float> StartValue { get { return m_StartValue; } }

        private MutableField<float> m_EndValue = new MutableField<float>() 
        { LiteralValue = 1f };
        [Controllable(LabelText = "End Value")]
        public MutableField<float> EndValue { get { return m_EndValue; } }

        private MutableField<float> m_TransitionTime = new MutableField<float>() 
        { LiteralValue = 1f };
        [Controllable(LabelText = "Transition Time")]
        public MutableField<float> TransitionTime { get { return m_TransitionTime; } }

        private MutableField<string> m_FormatString = new MutableField<string>() 
        { LiteralValue = "{0}" };
        [Controllable(LabelText = "FormatString")]
        public MutableField<string> FormatString { get { return m_FormatString; } }

        
        public override void StartKinetic(VisualPayload payload, Func<float, float> translateTimeToEffectProportion)
        {
            var labelVis = payload.VisualData.Bound.GetComponent<LabelVisualizer>();

            var startValue = StartValue.GetFirstValue(payload.Data);
            var endValue = EndValue.GetFirstValue(payload.Data);
            var transitionTimeInverse = 1f / TransitionTime.GetFirstValue(payload.Data); ;
            var formatString = FormatString.GetFirstValue(payload.Data);
            var startTime = Time.time;

            var bound = payload.VisualData.Bound.CreateDependingBound("Lerp Value");

            bound.transform.parent = payload.VisualData.Bound.transform.parent;
            payload.VisualData.Bound.transform.parent = bound.transform;

            var valueSatellite =
                bound.gameObject.AddComponent<SetLabelSatellite>();

            valueSatellite.LabelVis = labelVis;

            var newPayload = new VisualPayload(payload.Data, new VisualDescription(bound));


            valueSatellite.UpdateFunc = (label) =>
            {
                float proportion = (Time.time - startTime) * transitionTimeInverse;

                if (proportion >= 1)
                {
                    label.Text = string.Format(formatString, endValue);

                    valueSatellite.Cleanup();
                    return;
                }

                proportion = translateTimeToEffectProportion(proportion);

                var currentValue = Mathf.Lerp(startValue, endValue, proportion);
                
                label.Text = string.Format(formatString, currentValue);
            };

            valueSatellite.CleanupFunc = (label) =>
            {
                JobManager.Instance.StartJob(
                    Finished.Transmit(newPayload), jobName: "Kinetic Finished", startImmediately: true,
                    maxExecutionsPerFrame: 1);
            };

            //throw new NotImplementedException();
        }
    }
}
