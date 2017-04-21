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
    public class TeamIdToStageOrderCorrelation : MonoBehaviour
    {
        private static Dictionary<int, int> TeamIdToStageOrderMaster { get; set; }

        [SerializeField]
        [Tooltip("Can be overridden by command line...")]
        private bool m_Anonymize = false;
        private bool Anonymize { get { return m_Anonymize; } set { m_Anonymize = value; } }

        [SerializeField]
        private List<int> m_TeamStageOrders = new List<int>();
        private List<int> TeamStageOrders { get { return m_TeamStageOrders; } }


        public void Start()
        {
            if (CommandLineArgs.IsPresent("Anonymize"))
                Anonymize = true;

            if (TeamIdToStageOrderMaster == null)
            {
                TeamIdToStageOrderMaster = new Dictionary<int, int>();

                var i = 0;
                foreach (var stageOrder in TeamStageOrders)
                {
                    TeamIdToStageOrderMaster.Add(i, Anonymize ? i : stageOrder);
                    i++;
                }
            }
        }

        public static int StageOrderToId(int stageOrder)
        {
            if (TeamIdToStageOrderMaster.ContainsValue(8-stageOrder))
                return
                    TeamIdToStageOrderMaster.First(kvp => kvp.Value == 8-stageOrder).Key;
            return -1;
        }

        public static int TeamIdToStageOrder(int teamId)
        {
            if (TeamIdToStageOrderMaster.ContainsKey(teamId))
                return 8-TeamIdToStageOrderMaster[teamId];
            return -1;
        }
    }
}
