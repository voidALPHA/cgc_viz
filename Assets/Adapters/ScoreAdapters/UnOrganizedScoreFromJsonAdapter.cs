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
using Assets.Utility;
using Chains;
using Filters;
using Mutation;
using Mutation.Mutators.TeamSpecific;
using UnityEngine;
using Visualizers;
using ColorUtility = Assets.Utility.ColorUtility;
using Random = System.Random;

namespace Adapters.ScoreAdapters
{
    public class UnOrganizedScoreFromJsonAdapter : Adapter
    {
        private MutableField<int> m_MinRoundNumber = new MutableField<int>() { LiteralValue = 1 };
        [Controllable(LabelText = "Min Round Number")]
        public MutableField<int> MinRoundNumber { get { return m_MinRoundNumber; } }

        private MutableField<int> m_MaxRoundNumber = new MutableField<int>() { LiteralValue = 1 };
        [Controllable(LabelText = "Max Round Number")]
        public MutableField<int> MaxRoundNumber { get { return m_MaxRoundNumber; } }


        private MutableTarget m_ScoresTarget = new MutableTarget() { AbsoluteKey = "Scores" };
        [Controllable(LabelText = "Scores Target")]
        public MutableTarget ScoresTarget { get { return m_ScoresTarget; } }


        private static NodeDataShare<int> m_DataShare
            = new NodeDataShare<int>();
        public static NodeDataShare<int> DataShare
        { get { return m_DataShare; } set { m_DataShare = value; } }

        private MutableField<string> m_GroupId = new MutableField<string>() { LiteralValue = "" };
        [Controllable(LabelText = "Shared Group Id")]
        public MutableField<string> GroupId
        {
            get { return m_GroupId; }
        }

        private MutableTarget m_DataIndexTarget = new MutableTarget() 
        { AbsoluteKey = "Data Index" };
        [Controllable(LabelText = "Data Index Target")]
        public MutableTarget DataIndexTarget { get { return m_DataIndexTarget; } }


        public SelectionState DefaultState { get { return Router["Default"]; } }


        public UnOrganizedScoreFromJsonAdapter()
        {
            Router.AddSelectionState("Default");
        }

        public override void Unload()
        {
            DataShare.Clear();

            base.Unload();
        }


        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var minRound = MinRoundNumber.GetFirstValue( payload.Data );
            var maxRound = MaxRoundNumber.GetFirstValue( payload.Data );

            var scoresList = new List< MutableObject >();
            
            for ( int roundNum = minRound; roundNum <= maxRound; roundNum++ )
            {
                var getScoresCommand = new GetScoresForRoundCommand( roundNum );

                var iterator = CommandProcessor.Execute( getScoresCommand );
                while ( iterator.MoveNext() )
                    yield return null;

                var roundList = ParseRoundData( payload.Data, getScoresCommand.Scores, roundNum );

                scoresList.AddRange(roundList);
            }

            // if we've just sampled a missing round, fill in with round zero data
            if ( scoresList.Count == 0 )
            {
                var getScoresCommand = new GetScoresForRoundCommand(0);

                var iterator = CommandProcessor.Execute(getScoresCommand);
                while (iterator.MoveNext())
                    yield return null;

                // zero out round scores
                foreach ( var score in getScoresCommand.Scores )
                {
                    foreach ( var subScore in score.SubmissionScores )
                    {
                        subScore.Evaluation.Total = 0;
                        subScore.Availability.Functionality.Total = 0;
                        subScore.Availability.Performance.Total = 0;
                        subScore.Availability.Total = 0;
                        subScore.Security.Consensus = 0;
                        subScore.Security.Reference = 0;
                        subScore.Security.Total = 0;
                        subScore.Total = 0;
                    }
                }

                var roundList = ParseRoundData(payload.Data, getScoresCommand.Scores, 0);

                scoresList.AddRange(roundList);
            }

            ScoresTarget.SetValue(scoresList, payload.Data);

            var groupId = GroupId.GetFirstValue(payload.Data);

            var index = (DataShare.ContainsKey(groupId)
                ? DataShare[groupId]
                : 0);

            if (groupId != "")
                DataShare[groupId] = index + 1;

            DataIndexTarget.SetValue(index, payload.Data);

            var transmitIterator = Router.TransmitAll( payload );
            while ( transmitIterator.MoveNext() )
                yield return null;
        }

        private List<MutableObject> ParseRoundData(MutableObject mutable, RoundScores scoresEntry, int roundNum)
        {
            //var randomGen = new Random(1337);

            var scoresList = new List<MutableObject>();

            foreach (var team in scoresEntry)
            {
                //var teamColor = ColorUtility.HsvtoRgb((float)randomGen.NextDouble(), .8f, 1f);

                var teamColor = TeamColorPalette.ColorFromIndex(team.TeamID);

                foreach (var entry in team.SubmissionScores)
                {
                    var newMutable = TurnSubmissionIntoMutable(entry, team, teamColor);

                    newMutable[ "Team Name" ] = team.TeamName;
                    newMutable[ "Team ID" ] = team.TeamID;
                    newMutable[ "Team Color" ] = teamColor;
                    newMutable[ "Round Number" ] = roundNum;

                    scoresList.Add(newMutable);
                }
            }

            return scoresList;
        }


        private static MutableObject TurnSubmissionIntoMutable(SubmissionScore entry, TeamScores teamScores, Color teamColor)
        {
            var newMutable = new MutableObject
            {
                {"Total Score", entry.Total},
                {
                    "Availability", new MutableObject
                    {
                        {
                            "Functionality", new MutableObject
                            {
                                {"Total", entry.Availability.Functionality.Total}
                            }
                        },
                        {
                            "Performance", new MutableObject
                            {
                                {"Execution Time", entry.Availability.Performance.ExecutionTime},
                                {"File Size", entry.Availability.Performance.FileSize},
                                {"Memory Use", entry.Availability.Performance.MemoryUse},
                                {"Total", entry.Availability.Performance.Total}
                            }
                        },
                        {"Total", entry.Availability.Total}
                    }
                },
                {
                    "Security", new MutableObject
                    {
                        {"Consensus", entry.Security.Consensus},
                        {"Reference", entry.Security.Reference},
                        {"Total", entry.Security.Total}
                    }
                },
                {
                    "Evaluation", new MutableObject
                    {
                        {"Total", entry.Evaluation.Total}
                    }
                },
                {"Challenge Set", entry.ChallengeSet},
                {"Challenge Shortname", entry.ShortName},
                {"Challenge Set Id", entry.ChallengeSetID},
                {"Nds Id", entry.NdsId }
            };
            return newMutable;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            // We don't modify the input schema in adapters... We may derive values from input schemas, but adapters always send a novel schema...

            var roundScores = new RoundScores();
            roundScores.Add( new TeamScores(){SubmissionScores = new List< SubmissionScore >(){new SubmissionScore()}} );

            var scoresList = ParseRoundData( newSchema, roundScores, 0 );

            ScoresTarget.SetValue(scoresList, newSchema);
            DataIndexTarget.SetValue(1, newSchema);

            Router.TransmitAllSchema(newSchema);
        }


    }
}
