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
using Kinetics.BoundMovements;
using Mutation;
using UnityEngine;
using Utility.JobManagerSystem;
using Visualizers;

namespace Kinetics.BoundScales
{
    public class PulseObjectNode : KineticNode
    {
        private MutableField<float> m_PulseAmplitude = new MutableField<float>() 
        { LiteralValue = .2f };
        [Controllable(LabelText = "Pulse Amplitude (percent)")]
        public MutableField<float> PulseAmplitude { get { return m_PulseAmplitude; } }

        private MutableField<float> m_PulseWavelength = new MutableField<float>() 
        { LiteralValue = 1f };
        [Controllable(LabelText = "Pulse Wavelength (seconds)")]
        public MutableField<float> PulseWavelength { get { return m_PulseWavelength; } }

        private MutableField<float> m_PulseDuration = new MutableField<float>() 
        { LiteralValue = 5f };
        [Controllable(LabelText = "PulseDuration")]
        public MutableField<float> PulseDuration { get { return m_PulseDuration; } }

        public override void StartKinetic( VisualPayload payload, Func<float, float> translateTime )
        {
            var pulseSatellite = payload.VisualData.Bound.gameObject.AddComponent< BoundMovementSatellite >();
            var waveLength = PulseWavelength.GetFirstValue( payload.Data );
            var amplitude = PulseAmplitude.GetFirstValue( payload.Data );
            var startTime = Time.time;
            var endTime = Time.time + PulseDuration.GetFirstValue( payload.Data );
            var startScale = payload.VisualData.Bound.transform.localScale;
            var startPosition = payload.VisualData.Bound.transform.localPosition;

            pulseSatellite.MovementFunc = ( trans ) =>
            {
                if ( Time.time > endTime )
                {
                    trans.localScale = startScale;
                    trans.localPosition = startPosition;
                    pulseSatellite.Cleanup();
                    return;
                }

                float timeProportion = (Time.time - startTime) / waveLength;
                timeProportion = translateTime(timeProportion);

                float pulseOffset = Mathf.Sin( Mathf.PI * 2f * timeProportion );

                float scaleMult = pulseOffset * amplitude + 1f;

                trans.localScale = startScale * scaleMult;

                trans.localPosition = startPosition + .5f*(1-scaleMult)*startScale;
            };

            pulseSatellite.CleanupFunc = (trans) =>
            {
                JobManager.Instance.StartJob(
                    Finished.Transmit(payload), jobName: "Kinetic Finished", startImmediately: true, maxExecutionsPerFrame: 1);
            };
        }
    }
}
