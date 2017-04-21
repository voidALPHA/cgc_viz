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
using Visualizers;

namespace Choreography.Steps.CommandLineSteps
{
    public abstract class IfCommandLineStep : Step
    {
        private const string IfEventName = "If";
        private const string ElseEventName = "Else";

        private string m_ArgumentName = "Argument";
        [Controllable]
        public string ArgumentName { get { return m_ArgumentName; } set { m_ArgumentName = value; } }

        public IfCommandLineStep()
        {
            Router.AddEvent( IfEventName );
            Router.AddEvent( ElseEventName );
        }

        protected override IEnumerator ExecuteStep()
        {
            Router.FireEvent( CheckArgument() ? IfEventName : ElseEventName );

            yield return null;
        }

        public override int RecursiveCount
        {
            get { return ( CheckArgument() ? Router[ IfEventName ] : Router[ ElseEventName ] ).RecursiveCount + 1; }
        }

        public override float RecursiveDelayedDuration
        {
            get { return CheckArgument() ? Router[IfEventName].RecursiveDelayedDuration : Router[ElseEventName].RecursiveDelayedDuration; }
        }

        public override bool RecursiveDurationIsEstimated
        {
            get { return CheckArgument() ? Router[IfEventName].RecursiveDurationIsEstimated : Router[ElseEventName].RecursiveDurationIsEstimated; }
        }

        protected abstract bool CheckArgument();

    }
}
