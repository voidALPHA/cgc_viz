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
using Kinetics.BoundMovements;
using Mutation;
using UnityEngine;
using Utility.JobManagerSystem;
using Visualizers;

namespace Kinetics.VisualKinetics
{
    public class LerpToColor : KineticNode
    {
        private MutableField< Color > m_StartColor = new MutableField< Color >()
        { LiteralValue = Color.white };

        [Controllable( LabelText = "Start Color" )]
        public MutableField< Color > StartColor
        {
            get { return m_StartColor; }
        }

        private MutableField< Color > m_EndColor = new MutableField< Color >()
        { LiteralValue = Color.black };

        [Controllable( LabelText = "End Color" )]
        public MutableField< Color > EndColor
        {
            get { return m_EndColor; }
        }

        private MutableField< float > m_TransitionTime = new MutableField< float >()
        { LiteralValue = 10f };

        [Controllable( LabelText = "TransitionTime" )]
        public MutableField< float > TransitionTime
        {
            get { return m_TransitionTime; }
        }

        public override void StartKinetic( VisualPayload payload, Func<float, float> translateTime)
        {
            var startColor = StartColor.GetFirstValue( payload.Data );
            var endColor = EndColor.GetFirstValue( payload.Data );

            var transitionTimeInverse = 1f / TransitionTime.GetFirstValue( payload.Data );
            var startTime = Time.time;


            var colorSatellite =
                payload.VisualData.Bound.gameObject.AddComponent< BoundMovementSatellite >();

            var newPayload = new VisualPayload( payload.Data, new VisualDescription( payload.VisualData.Bound ) );

            var materialsList =
                payload.VisualData.Bound.GetComponentsInChildren< Renderer >().Select( rend => rend.material ).ToList();

            colorSatellite.MovementFunc = ( trans ) =>
            {
                float proportion = ( Time.time - startTime ) * transitionTimeInverse;

                if ( proportion >= 1f )
                {
                    foreach ( var mat in materialsList )
                        mat.color = endColor;

                    colorSatellite.Cleanup();
                    return;
                }

                proportion = translateTime( proportion );

                var toColor = Color.Lerp( startColor, endColor, proportion );

                foreach ( var mat in materialsList )
                    mat.color = toColor;
            };

            colorSatellite.CleanupFunc = ( trans ) =>
            {
                JobManager.Instance.StartJob(
                    Finished.Transmit( newPayload ), jobName: "Kinetic Finished", startImmediately: true,
                    maxExecutionsPerFrame: 1 );
            };

            // execute first step immediately to set initial position
            colorSatellite.MovementFunc( colorSatellite.transform );

        }
    }

}