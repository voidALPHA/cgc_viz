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
using UnityEngine;

namespace Filters
{
    public class ScoreFromJSONFilter : IFilter
    {
        private ScoreDataProvider ScoreDataProvider { get; set; }

        public List<TeamScores> LoadedTeams { get; private set; }
        
        public event Action DataLoaded = delegate { };

        public void LoadData(string path = "ScoreData\\scores.json")
        {
            if (ScoreDataProvider == null)
                ScoreDataProvider = new ScoreDataProvider();

            ScoreDataProvider.ScoresLoaded += DataReady;

            ScoreDataProvider.LoadScores(path);
        }

        private void DataReady(List<TeamScores> foundTeams)
        {
            ScoreDataProvider.ScoresLoaded -= DataReady;

            LoadedTeams = foundTeams;

            DataLoaded();
        }

    }
}
