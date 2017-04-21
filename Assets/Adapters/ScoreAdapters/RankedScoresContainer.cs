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
using Adapters.GameEvents.Model;
using Newtonsoft.Json;

namespace Adapters.ScoreAdapters
{
    public class RankedScore
    {
        public int Team { get; set; }

        public int Score { get; set; }

        public string Name { get; set; }
    }

    public class RankedScoresContainer
    {
        public bool Ok { get; set; }

        public List<RankedScore> Rank { get; set; }
        
        private static JsonSerializerSettings SerializerSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ContractResolver = new LowercaseUnderscoredToPascalCaseResolver()
                };
            }
        }

        public static RankedScoresContainer Deserialize(string json)
        {
            var scoresContainer = JsonConvert.DeserializeObject<RankedScoresContainer>(json, SerializerSettings);

            return scoresContainer;
        }

        public static RankedScoresContainer Deserialize(Stream stream)
        {
            var serializer = JsonSerializer.Create(SerializerSettings);

            using (var streamReader = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(streamReader))
            {
                var ret = serializer.Deserialize<RankedScoresContainer>(jsonTextReader);

                return ret;
            }
        }
    }

}
