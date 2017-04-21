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
using Mutation.Mutators.VisualModifiers;
using UnityEngine;
using Utility;

namespace Mutation.Mutators.TeamSpecific
{
    public class TeamIdToTeamLogoMaterialCorrelation : MonoBehaviour
    {
        private static Dictionary<int, Material> TeamIdToLogoMaster { get; set; }

        [SerializeField]
        [Tooltip("Can be overridden by command line...")]
        private bool m_Anonymize = false;
        private bool Anonymize { get { return m_Anonymize; } set { m_Anonymize = value; } }

        [SerializeField]
        private List<Material> m_LogoMaterials = new List<Material>();
        private List<Material> LogoMaterials { get { return m_LogoMaterials; } }
        
        public void Start()
        {
            if (CommandLineArgs.IsPresent("Anonymize"))
                Anonymize = true;

            if (TeamIdToLogoMaster == null)
            {
                TeamIdToLogoMaster = new Dictionary<int, Material>();

                var i = 0;
                foreach (var material in LogoMaterials)
                {
                    TeamIdToLogoMaster.Add(i, material);
                    i++;
                }
            }
        }
        
        public static Material TeamIdToLogo(int teamId)
        {

            if (TeamIdToLogoMaster.ContainsKey(teamId))
                return TeamIdToLogoMaster[teamId];

            return MaterialFactory.GetDefaultMaterial();
        }
    }
}
