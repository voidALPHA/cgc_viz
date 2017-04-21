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
using JetBrains.Annotations;
using Ui.TypePicker;
using UnityEngine;

namespace Choreography.Steps
{
    [UsedImplicitly]
    [TypePickerIgnore]
    public class TwoEventStep : Step
    {
        private const string StartEventName = "Start";
        private const string EndEventName = "End";

        public TwoEventStep()
        {
            Router.AddEvent( StartEventName );
            Router.AddEvent( EndEventName );
        }

        public override float BaseDuration
        {
            get { return 3.0f; }
        }

        protected override IEnumerator ExecuteStep()
        {
            var startTime = Time.time;

            Router.FireEvent( StartEventName );

            while ( startTime + 3.0f < Time.time )
                yield return null;

            Router.FireEvent( EndEventName );

            yield return null;
        }
    }
}