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

using Visualizers;

namespace Mutation.Mutators.TeamSpecific
{
    public class TeamNameToTeamIdMutator : DataMutator
    {
        private MutableField<string> m_TeamName = new MutableField<string>() { AbsoluteKey = "Team Name" };
        [Controllable(LabelText = "Team Name")]
        public MutableField<string> TeamName { get { return m_TeamName; } }

        private MutableTarget m_TeamIdTarget = new MutableTarget() { AbsoluteKey = "Team Id" };
        [Controllable(LabelText = "Team Id Target")]
        public MutableTarget TeamIdTarget { get { return m_TeamIdTarget; } }

        public TeamNameToTeamIdMutator()
        {
            TeamIdTarget.SchemaParent = TeamName;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            foreach (var entry in TeamName.GetEntries(mutable))
            {
                TeamIdTarget.SetValue(
                    TeamSpecificDataHider.USE_REAL_TEAM_DATA?
                    TeamIdToTeamNameCorrelation.TeamNameToId(
                    TeamName.GetValue(entry)):
                    1
                    , entry);
            }

            return mutable;
        }
    }
}
