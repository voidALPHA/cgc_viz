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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mutation.Mutators;
using Visualizers;
using Debug = UnityEngine.Debug;

namespace Mutation
{
    public class TestSelectScoringMutator : DataMutator
    {
        private MutableField<IEnumerable<MutableObject>> m_ScoreEntries = new MutableField<IEnumerable<MutableObject>> { AbsoluteKey = "Scores" };
        [Controllable(LabelText = "Scores")]
        private MutableField<IEnumerable<MutableObject>> ScoreEntries
        {
            get { return m_ScoreEntries; }
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            var sw = new Stopwatch();
            sw.Start();

            // This mutator calculates and injects 'total score for this team' and 'total score for this challenge set' into the payload

            Dictionary< string, float > totalScoresByTeam = new Dictionary<string, float>();
            Dictionary<string, float> totalScoresByChallengeSet = new Dictionary<string, float>();

            var scores = ScoreEntries.GetLastKeyValue(mutable);
            if (scores == null)
                throw new Exception(ScoreEntries.AbsoluteKey + " is not a subcollection of mutable objects!");

            foreach (var element in scores)
            {
                var teamKey = element["Team Name"].ToString( );
                if (!totalScoresByTeam.ContainsKey( teamKey ))
                    totalScoresByTeam.Add(teamKey, 0.0f);

                totalScoresByTeam[teamKey] += (float)element["Total Score"];

                var csKey = element["Challenge Set"].ToString();
                if (!totalScoresByChallengeSet.ContainsKey(csKey))
                    totalScoresByChallengeSet.Add(csKey, 0.0f);

                totalScoresByChallengeSet[csKey] += (float)element["Total Score"];
            }

            foreach (var element in scores)
            {
                element.Add( "Total Score For Team", totalScoresByTeam[element["Team Name"].ToString()] );
                element.Add("Total Score For Challenge Set", totalScoresByChallengeSet[element["Challenge Set"].ToString()]);
            }

            sw.Stop();
            Debug.Log("It took " + sw.ElapsedMilliseconds / 1000f + " seconds for TestSelectorScoringMutator.");

            // Send data out to any registered receivers
            return mutable;
        }
    }
}
