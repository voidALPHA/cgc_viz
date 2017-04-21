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

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Filters
{
    public class RoundScores : List< TeamScores >
    {
        public static RoundScores FromStream( Stream stream )
        {
            var serializer = new JsonSerializer();

            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                return serializer.Deserialize<RoundScores>(jsonTextReader);
            }
        }
    }

    public class TeamScores
    {
        [JsonProperty( "name" )]
        public string TeamName { get; set; }

        [JsonProperty("id")]
        public int TeamID { get; set; }

        [JsonProperty("submissions")]
        public List< SubmissionScore > SubmissionScores { get; set; }

        public TeamScores()
        {
            TeamName = string.Empty;
        }
    }

    public class SubmissionScore
    {
        [JsonProperty( "cset" )]
        public string ChallengeSet { get; set; }

        private string m_ShortName="Shortname";
        [JsonProperty( "cset_display_name" )]
        public string ShortName
        {
            get { return m_ShortName; }
            set { m_ShortName = value; }
        }

        [JsonProperty("cset_id")]
        public int ChallengeSetID { get; set; }

        [JsonProperty( "security" )]
        public SecurityScore Security { get; set; }

        [JsonProperty( "evaluation" )]
        public EvaluationScore Evaluation { get; set; }

        [JsonProperty( "availability" )]
        public AvailabilityScore Availability { get; set; }

        public float Total { get; set; }

        private int m_NdsId=-1;
        [JsonProperty( "idsid" )]
        public int? NdsId
        {
            get { return m_NdsId; }
            set { m_NdsId = value??-1; }
        }

        public SubmissionScore()
        {
            ChallengeSet = string.Empty;

            Security = new SecurityScore();
            Evaluation = new EvaluationScore();
            Availability = new AvailabilityScore();
        }
    }

    public class AvailabilityScore
    {
        public class PerformanceScore
        {
            [JsonProperty( "file-size" )]
            public float FileSize { get; set; }

            [JsonProperty( "mem-use" )]
            public float MemoryUse { get; set; }

            [JsonProperty( "exec-time" )]
            public float ExecutionTime { get; set; }

            public float Total { get; set; }
        }

        public class FunctionalityScore
        {
            public float Total { get; set; }
        }

        [JsonProperty( "func" )]
        public FunctionalityScore Functionality { get; set; }

        [JsonProperty( "perf" )]
        public PerformanceScore Performance { get; set; }

        public float Total { get; set; }

        public AvailabilityScore()
        {
            Performance = new PerformanceScore();

            Functionality = new FunctionalityScore();
        }
    }

    public class EvaluationScore
    {
        public float Total { get; set; }
    }

    public class SecurityScore
    {
        public float Consensus { get; set; }

        public float Reference { get; set; }

        public float Total { get; set; }
    }

}
