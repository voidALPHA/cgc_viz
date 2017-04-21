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
    public class TeamIdToStageOrderMutator : DataMutator
    {
        private MutableField<int> m_TeamId = new MutableField<int>()
        { AbsoluteKey = "TeamId" };
        [Controllable(LabelText = "Team Id")]
        public MutableField<int> TeamId { get { return m_TeamId; } }

        private MutableTarget m_StageOrder = new MutableTarget()
        { AbsoluteKey = "Stage Order" };
        [Controllable(LabelText = "Stage Order Target")]
        public MutableTarget StageOrder { get { return m_StageOrder; } }

        public TeamIdToStageOrderMutator()
        {
            StageOrder.SchemaParent = TeamId;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            foreach (var entry in TeamId.GetEntries(mutable))
            {
                StageOrder.SetValue(
                     TeamSpecificDataHider.USE_REAL_TEAM_DATA ?
                    TeamIdToStageOrderCorrelation.TeamIdToStageOrder(
                        TeamId.GetValue(entry)) :
                    TeamId.GetValue(entry), entry);
            }

            return mutable;
        }
    }
}
