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
using Ui.TypePicker;
using UnityEngine;

namespace Choreography.Steps
{
    // This step tests the Pause functionality of a Step. It should remain unimpacted by timescale.
    [TypePickerIgnore]
    public class PauseTestStep : Step
    {
        private const string EndEventName = "End";

        public PauseTestStep()
        {
            Router.AddEvent(EndEventName);
        }

        protected override IEnumerator ExecuteStep()
        {
            // Use real time to ensure timescale pause isn't impacting the test...
            var startTime = Time.realtimeSinceStartup;

            Debug.Log("PauseTestStep Starting");
            while ( Time.realtimeSinceStartup < startTime + 5.0f )
            {
                Debug.Log( "PauseTestStep Delaying" );

                yield return null;
            }

            Router.FireEvent(EndEventName);

            yield return null;
        }

    }
}
