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
using UnityEngine;

namespace Filters
{
    public class ScoreDataProvider
    {
        public Action< List< TeamScores > > ScoresLoaded = delegate { };

        public void LoadScores(string path = "ScoreData\\scores.json")
        {
            //var path = Path.Combine( "ScoreData", "scores.json" );

            if ( !File.Exists( path ) )
            {
                Debug.Log( "Path does not exist: " + Path.GetFullPath( path ) );
                return;
            }
            var contents = File.ReadAllText( path );

            var scores = JsonConvert.DeserializeObject< List< TeamScores > >( contents );

            ScoresLoaded( scores );
        }
    }
}