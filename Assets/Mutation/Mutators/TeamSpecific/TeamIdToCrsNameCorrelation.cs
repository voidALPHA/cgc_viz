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
    public class TeamIdToCrsNameCorrelation : MonoBehaviour
    {
        private static Dictionary<int, string> TeamIdToCrsNameMaster { get; set; }

        [SerializeField]
        [Tooltip("Can be overridden by command line...")]
        private bool m_Anonymize = false;
        private bool Anonymize { get { return m_Anonymize; } set { m_Anonymize = value; } }

        [SerializeField]
        private List<string> m_CrsNames = new List<string>();
        private List<string> CrsNames { get { return m_CrsNames; } }


        public void Start()
        {
            if (CommandLineArgs.IsPresent("Anonymize"))
                Anonymize = true;

            if (TeamIdToCrsNameMaster == null)
            {
                TeamIdToCrsNameMaster = new Dictionary<int, string>();

                var i = 0;
                foreach (var crsName in CrsNames)
                {
                    TeamIdToCrsNameMaster.Add(i, Anonymize ? "CRS_" + i : crsName);
                    i++;
                }
            }
        }

        public static int CrsNameToId(string crsName)
        {
            if (TeamIdToCrsNameMaster.ContainsValue(crsName))
                return
                    TeamIdToCrsNameMaster.First(kvp => string.Compare(kvp.Value, crsName, StringComparison.InvariantCultureIgnoreCase) == 0).Key;
            return -1;
        }

        public static string TeamIdToName(int teamId)
        {
            if (TeamIdToCrsNameMaster.ContainsKey(teamId))
                return TeamIdToCrsNameMaster[teamId];
            return "Unknown";
        }
    }
}
