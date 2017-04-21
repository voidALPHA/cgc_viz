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

using System.Collections;
using Choreography.Recording;
using UnityEngine;

namespace Choreography.Steps.Recording
{
    public class StopRecordingStep : Step
    {
        private const string EndEventName = "End";

        public StopRecordingStep()
        {
            Router.AddEvent(EndEventName);
        }

        public override float BaseDuration
        {
            get { return 0.0f; }
        }

        protected override IEnumerator ExecuteStep()
        {
            Debug.Log("Stopping recording!");
            RecordingLord.StopRecording();
            Router.FireEvent(EndEventName);
            yield return null;
        }
    }
}