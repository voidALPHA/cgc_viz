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

using Mutation.Mutators.VisualModifiers;
using Visualizers;

namespace Mutation.Mutators.TeamSpecific
{
    public class TeamIdToTeamLogoMutator : DataMutator
    {
        private MutableField< int > m_TeamId = new MutableField< int >()
        { AbsoluteKey = "TeamId" };

        [Controllable( LabelText = "Team Id" )]
        public MutableField< int > TeamId
        {
            get { return m_TeamId; }
        }

        private MutableTarget m_LogoTarget = new MutableTarget()
        { AbsoluteKey = "Logo Target" };
        [Controllable( LabelText = "Logo Target" )]
        public MutableTarget LogoTarget
        {
            get { return m_LogoTarget; }
        }

        public TeamIdToTeamLogoMutator()
        {
            LogoTarget.SchemaParent = TeamId;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in TeamId.GetEntries( mutable ) )
            {
                LogoTarget.SetValue(
                    TeamSpecificDataHider.USE_REAL_TEAM_DATA
                        ? TeamIdToTeamLogoMaterialCorrelation.TeamIdToLogo(
                            TeamId.GetValue( entry ) )
                        : TeamIdToTeamLogoMaterialCorrelation.TeamIdToLogo( 0 ), entry );
            }

            return mutable;
        }
    }
}
