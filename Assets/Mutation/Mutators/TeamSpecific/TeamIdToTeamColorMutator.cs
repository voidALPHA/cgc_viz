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

using Mutation.Mutators.ColorMutators;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.TeamSpecific
{
    public class TeamIdToTeamColorMutator : DataMutator
    {
        private MutableField<int> m_TeamIndex = new MutableField<int>() 
        { LiteralValue = 0 };
        [Controllable(LabelText = "Team Index")]
        public MutableField<int> TeamIndex { get { return m_TeamIndex; } }

        private MutableTarget m_ColorTarget = new MutableTarget() { AbsoluteKey = "NewColor" };
        [Controllable(LabelText = "New Color Field")]
        public MutableTarget ColorTarget { get { return m_ColorTarget; } }

        public void Start()
        {
            ColorTarget.SchemaParent = TeamIndex;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            foreach (var entry in TeamIndex.GetEntries(mutable))
                ColorTarget.SetValue(
                    TeamSpecificDataHider.USE_REAL_TEAM_DATA?
                    TeamColorPalette.ColorFromIndex(TeamIndex.GetValue(entry)):
                    ColorPalette.ColorFromIndex(TeamIndex.GetValue(entry)),
                    entry);

            return mutable;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            foreach (var entry in TeamIndex.GetEntries(newSchema))
                ColorTarget.SetValue(
                    Color.magenta,
                    entry);

            Router.TransmitAllSchema(newSchema);
        }
    }
}
