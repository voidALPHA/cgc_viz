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
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Adapters.GameEvents.Model
{
    public class GameEventContainer
    {
        public List<Round> Rounds { get; set; }

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

        public static GameEventContainer Deserialize( string json )
        {
            var gameEventContainer = JsonConvert.DeserializeObject< GameEventContainer >( json, SerializerSettings );

            return gameEventContainer;
        }

        public static GameEventContainer Deserialize( Stream stream )
        {
            var serializer = JsonSerializer.Create( SerializerSettings );

            using ( var streamReader = new StreamReader( stream ) )
            using ( var jsonTextReader = new JsonTextReader( streamReader ) )
            {
                var ret = serializer.Deserialize<GameEventContainer>( jsonTextReader );

                return ret;
            }
        }
    }


    public class LowercaseUnderscoredToPascalCaseResolver : DefaultContractResolver
    {
        public LowercaseUnderscoredToPascalCaseResolver()
            : base( true )
        {
        }

        protected internal override string ResolvePropertyName( string propertyName )
        {
            var allIndices = new List< int >();
            int index = 0;
            foreach ( var ch in propertyName )
            {
                if ( index != 0 )
                    if ( Char.IsUpper( ch ))
                        allIndices.Add( index );

                index++;
            }

            allIndices.Reverse();

            var newName = propertyName;

            foreach ( var i in allIndices )
            {
                newName = newName.Insert( i, "_" );
            }

            newName = newName.ToLower();

            return newName;
        }
    }
}
