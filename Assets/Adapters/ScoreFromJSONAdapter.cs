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
using Chains;
using Filters;
using Mutation;
using Mutation.Mutators.TeamSpecific;
using UnityEngine;
using Visualizers;
namespace Adapters
{
    public class ScoreFromJSONAdapter : Adapter
    {
        private MutableField<string> m_FilePath = new MutableField<string>() 
        { LiteralValue = "ScoreData\\scores.json" };
        [Controllable(LabelText = "Score File Path")]
        public MutableField<string> FilePath { get { return m_FilePath; } }

        private MutableTarget m_ScoreTarget = new MutableTarget() 
        { AbsoluteKey = "Scores" };
        [Controllable(LabelText = "Score Target")]
        public MutableTarget ScoreTarget { get { return m_ScoreTarget; } }

        private MutableField<string> m_GroupId = new MutableField<string>() { LiteralValue = "" };
        [Controllable(LabelText = "Shared Group Id")]
        public MutableField<string> GroupId
        {
            get { return m_GroupId; }
        }

        private MutableTarget m_IndexField = new MutableTarget() 
        { AbsoluteKey = "Score File Index" };
        [Controllable(LabelText = "Index Field")]
        public MutableTarget IndexField { get { return m_IndexField; } }

        public SelectionState DefaultState { get { return Router["Per File"]; } }

        private static NodeDataShare<int> m_DataShare
            = new NodeDataShare<int>();
        public static NodeDataShare<int> DataShare
        { get { return m_DataShare; } set { m_DataShare = value; } }

        private ScoreFromJSONFilter m_ScoreFromJsonFilter;
        public ScoreFromJSONFilter ScoreFromJsonFilter
        {
            get { return m_ScoreFromJsonFilter ?? (m_ScoreFromJsonFilter = new ScoreFromJSONFilter()); }
            set { m_ScoreFromJsonFilter = value; }
        }

        private VisualPayload Payload { get; set; }

        private bool DataIsReady { get; set; }

        private VisualPayload PayloadToTransmit { get; set; }


        public ScoreFromJSONAdapter()
        {
            Router.AddSelectionState("Default");
        }

        public override void Unload()
        {
            DataShare.Clear();

            base.Unload();
        }

        
        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            Payload = payload;

            ScoreFromJsonFilter.DataLoaded += DataReady;

            foreach ( var entry in FilePath.GetEntries( payload.Data ) )
            {
                DataIsReady = false;

                var fileName = FilePath.GetValue( entry );

                ScoreFromJsonFilter.LoadData( fileName );

                while ( !DataIsReady )
                    yield return null;

                yield return null;
                    // Let this breathe a bit, because currently the above is essentially a blocking operation

                var iterator = Router.TransmitAll( PayloadToTransmit );
                while ( iterator.MoveNext() )
                    yield return null;
            }

            ScoreFromJsonFilter.DataLoaded -= DataReady;
        }

        private void DataReady()
        {
            //ScoreFromJsonFilter.DataLoaded -= DataReady;

            PayloadToTransmit = ParseData();

            DataIsReady = true;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            // We don't modify the input schema in adapters... We may derive values from input schemas, but adapters always send a novel schema...

            var oneMutable = TurnSubmissionIntoMutable( new SubmissionScore(), new TeamScores(), Color.magenta );

            var mutableCollection = new List< MutableObject > { oneMutable };

            ScoreTarget.SetValue( mutableCollection, newSchema );
            IndexField.SetValue( 1, newSchema );

            Router.TransmitAllSchema( newSchema );
        }

        private VisualPayload ParseData()
        {
            //var randomGen = new Random(1337);

            var mutablesList = new List<MutableObject>();

            foreach (var team in ScoreFromJsonFilter.LoadedTeams)
            {
                //var teamColor = ColorUtility.HsvtoRgb((float)randomGen.NextDouble(), .8f, 1f);

                var teamColor = TeamColorPalette.ColorFromIndex( team.TeamID );

                foreach (var entry in team.SubmissionScores)
                {
                    var newMutable = TurnSubmissionIntoMutable( entry, team, teamColor );

                    mutablesList.Add(newMutable);
                }
            }

            ScoreTarget.SetValue( mutablesList, Payload.Data );

            var groupId = GroupId.GetFirstValue(Payload.Data);

            var index = (DataShare.ContainsKey(groupId)
                ? DataShare[groupId]
                : 0);

            if (groupId != "")
                DataShare[groupId] = index + 1;

            IndexField.SetValue(index, Payload.Data);

            return Payload;
        }

        private static MutableObject TurnSubmissionIntoMutable( SubmissionScore entry, TeamScores teamScores, Color teamColor)
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
                {"Team Name", teamScores.TeamName},
                {"Team Color", teamColor},
                {"Team ID", teamScores.TeamID},
                {"Challenge Set", entry.ChallengeSet},
                {"Challenge Set Id", entry.ChallengeSetID}
            };
            return newMutable;
        }
    }
}
