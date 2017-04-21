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

using System.Collections.Generic;
using Choreography;
using Choreography.Steps;
using Choreography.Steps.CameraTransform;
using Choreography.Steps.Recording;
using Choreography.Views;
using UnityEngine;

namespace Experimental
{
    public class ExperimentalStepRouterTest : MonoBehaviour
    {
        [SerializeField]
        private int m_NumberOfMovements = 10;
        private int NumberOfMovements { get { return m_NumberOfMovements; } }

        [SerializeField]
        private float m_SphereSize = 15f;
        private float SphereSize { get { return m_SphereSize; } }

        private void Start()
        {
            //CreateTimeline();
        }

        private void Update()
        {
            if ( Input.GetKeyDown( KeyCode.PageUp ) )
                CreateTimeline();
        }

        private void CreateTimeline()
        {
            var startRecordingStep = new StartRecordingStep
            {
                Delay = 2.0f
            };


            var stopRecordingStep = new StopRecordingStep
            {
                Delay = 10.0f
            };

            var cameraSteps = new List< Step >();
            for ( int i = 0; i < NumberOfMovements; i++ )
            {
                var offsetVector = UnityEngine.Random.onUnitSphere * SphereSize;
                var goSomewhereStep = new CameraMoveOverTimeStep();
                goSomewhereStep.Duration = UnityEngine.Random.Range( 1.5f, 3.5f );
                goSomewhereStep.MoveTarget = offsetVector;
                goSomewhereStep.LookAtTarget = Vector3.zero;
                cameraSteps.Add( goSomewhereStep );
            }

            startRecordingStep.Router.AddTarget( "End", stopRecordingStep );

            Step lastStep = startRecordingStep;
            var lastEventName = "End";
            foreach ( var step in cameraSteps )
            {
                lastStep.Router.AddTarget( lastEventName, step );
                lastStep = step;
                lastEventName = "EndMovement";
            }

            var timeline = new Timeline();

            //timeline.Started += HandleTimelineStarted;
            //timeline.Ended += HandleTimelineEnded;

            timeline.AddStartEventTarget( startRecordingStep );

            TimelineViewBehaviour.Instance.Timeline = timeline;
        }

        //private void HandleTimelineStarted()
        //{
        //    foreach ( var child in transform )
        //        ( child as Transform ).gameObject.SetActive( true );
        //}

        //public void HandleTimelineEnded( bool cancelled )
        //{
        //    foreach ( var child in transform )
        //        ( child as Transform ).gameObject.SetActive( false );
        //}
    }
}
