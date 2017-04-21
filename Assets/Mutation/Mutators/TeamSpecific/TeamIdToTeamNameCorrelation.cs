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
using System.Linq;
using UnityEngine;
using Utility;

namespace Mutation.Mutators.TeamSpecific
{
    public class TeamIdToTeamNameCorrelation : MonoBehaviour
    {
        private static Dictionary<int, string> TeamIdToNameMaster { get; set; }

        [SerializeField]
        [Tooltip("Can be overridden by command line...")]
        private bool m_Anonymize = false;
        private bool Anonymize { get { return m_Anonymize; } set { m_Anonymize = value; } }

        [SerializeField]
        private List<string> m_TeamNames = new List< string >();
        private List<string> TeamNames { get { return m_TeamNames; } }


        public void Start()
        {
            if ( CommandLineArgs.IsPresent( "Anonymize" ) )
                Anonymize = true;

            if ( TeamIdToNameMaster == null )
            {
                TeamIdToNameMaster = new Dictionary< int, string >();

                var i = 0;
                foreach ( var teamName in TeamNames )
                {
                    TeamIdToNameMaster.Add( i, Anonymize ? "TEAM_" + i : teamName );
                    i++;
                }
            }
        }

        public static int TeamNameToId( string teamName )
        {
            if ( TeamIdToNameMaster.ContainsValue( teamName ) )
                return
                    TeamIdToNameMaster.First(kvp => string.Compare( kvp.Value, teamName, StringComparison.InvariantCultureIgnoreCase ) == 0).Key;
            return -1;
        }

        public static string TeamIdToName( int teamId )
        {
            if ( TeamIdToNameMaster.ContainsKey( teamId ) )
                return TeamIdToNameMaster[ teamId ];
            return "Unknown";
        }
    }
}
