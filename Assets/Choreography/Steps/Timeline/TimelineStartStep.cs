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
using System.Collections.Generic;
using ChainViews;
using Choreography.CameraControl;
using Newtonsoft.Json;
using Ui.TypePicker;
using Visualizers;

namespace Choreography.Steps.Timeline
{
    [TypePickerIgnore]
    public class TimelineStartStep : Step
    {
        private string StartEventName { get { return "Start"; } }

        
        public override float BaseDuration { get { return 0.0f; } }
        
        
        public TimelineStartStep()
        {
            Router.AddEvent( StartEventName );
        }


        protected override IEnumerator ExecuteStep()
        {
            SplineCameraControlLord.Instance.StartPlayback();

            Router.FireEvent( StartEventName );

            yield break;
        }

        public void AddStartStepTarget( Step target )
        {
            Router.AddTarget( StartEventName, target );
        }

        public IEnumerable< Step > StartStepTargets
        {
            get { return Router.GetTargetsEnumerable( StartEventName ); }
        }
    }
}
