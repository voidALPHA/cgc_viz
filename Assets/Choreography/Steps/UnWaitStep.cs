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
using Sequencers;
using Visualizers;

namespace Choreography.Steps
{
    [UsedImplicitly]
    public class UnWaitStep : Step
    {
        private const string EndEventName = "Unwait";

        private string m_GroupId = "";
        [Controllable(LabelText = "Group ID to Unwait")]
        public string GroupId { get{return m_GroupId;
        } set { m_GroupId = value; } }

        public UnWaitStep()
        {
            Router.AddEvent(EndEventName);
        }

        protected override IEnumerator ExecuteStep()
        {
            WaitNode.UnwaitGroupId(GroupId);

            Router.FireEvent(EndEventName);
            yield return null;
        }
    }
}
