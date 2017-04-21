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
using Adapters.TraceAdapters.Commands;
using Mutation;
using Visualizers;

namespace Adapters.ScoreAdapters
{
    public class RankedScoreAdapter : Adapter
    {
        private MutableField<int> m_RoundIndex = new MutableField<int>() { LiteralValue = 5 };
        [Controllable(LabelText = "RoundIndex")]
        public MutableField<int> RoundIndex { get { return m_RoundIndex; } }

        private MutableTarget m_ScoresTarget = new MutableTarget() 
        { AbsoluteKey = "Scores" };
        [Controllable(LabelText = "Scores Target")]
        public MutableTarget ScoresTarget { get { return m_ScoresTarget; } }


        public RankedScoreAdapter()
        {
            Router.AddSelectionState( "All" );
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            var scoresList = new List< MutableObject >
            {
                new MutableObject() { { "Team ID", 1 }, {"Score", 1000}, {"Team Name", "Team 1"} },
                new MutableObject() { { "Team ID", 2 }, {"Score",  900}, {"Team Name", "Team 2"} },
                new MutableObject() { { "Team ID", 3 }, {"Score",  800}, {"Team Name", "Team 3"} }
            };
            
            ScoresTarget.SetValue( scoresList, newSchema );

            Router.TransmitAllSchema(newSchema);
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var roundNumber = RoundIndex.GetFirstValue( payload.Data );

            GetRankedScoresCommand command;

            if (roundNumber < 0)
                command = new GetRankedScoresCommand(0);
            else
                command = new GetRankedScoresCommand(roundNumber);

            var iterator = CommandProcessor.Execute( command );
            while ( iterator.MoveNext() )
                yield return null;
            
            var scoresList = new List< MutableObject >();
            foreach ( var scoresEntry in command.ScoresContainer.Rank )
            {
                scoresList.Add( new MutableObject()
                {
                    {"Team ID", scoresEntry.Team },
                    {"Score", (roundNumber < 0)?0:scoresEntry.Score },
                    {"Team Name", scoresEntry.Name }
                } );
            }
            
            ScoresTarget.SetValue(scoresList, payload.Data );

            iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }


    }
}
