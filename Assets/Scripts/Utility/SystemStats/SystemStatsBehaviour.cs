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
using Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Utility.SystemStats
{
    public class SystemStatsBehaviour : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField]
        private List<ISystemStats> m_StatsLines = new List<ISystemStats>( );
        public List<ISystemStats> StatsLines { get { return m_StatsLines; } set { m_StatsLines = value; } }

        [SerializeField]
        private Transform m_BackgroundPanelRoot = null;
        private Transform BackgroundPanelRoot { get { return m_BackgroundPanelRoot; } }

        [Header("Project References")]
        [SerializeField]
        private GameObject m_ItemPrefab = null;
        private GameObject ItemPrefab { get { return m_ItemPrefab; } }

        private bool m_ShowStats;
        public bool ShowStats
        {
            get { return m_ShowStats; }
            set
            {
                m_ShowStats = value;
                var canvas = GetComponent<Canvas>();
                canvas.enabled = m_ShowStats;
            }
        }

        
        private void Start()
        {
            ShowStats = false;

            AddStatsLine(new SystemStatsMemory());
            AddStatsLine(new SystemStatsPerf());
            AddStatsLine(new SystemStatsJobManager());
            AddStatsLine(new SystemStatsMouse());
            AddStatsLine(new SystemStatsTime());
        }


        private void Update()
        {
            // NOTE:  Using Shift-Alt- here.  However, there is some overload of Shift-Alt- with Unity Editor commands
            if (Input.GetButtonDown( "Toggle System Stats Display" ))
            {
                if (( Input.GetKey(KeyCode.LeftAlt  ) || Input.GetKey(KeyCode.RightAlt) ) &&
                    ( Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ))
                {
                    ShowStats = !ShowStats;
                }
            }

            foreach (var stat in StatsLines)
            {
                stat.Update();

                if (ShowStats)
                    stat.TextComponent.text = stat.StatString;
            }
        }

        public void AddStatsLine(ISystemStats statsLine)  // Can be called externally, that is, from app-specific code
        {
            StatsLines.Add(statsLine);

            var itemGo = Instantiate(ItemPrefab);
            itemGo.transform.SetParent(BackgroundPanelRoot.transform);

            statsLine.UnityObject = itemGo;
            statsLine.TextComponent = itemGo.GetComponentInChildren<Text>();
        }

        private void RemoveAllStatsLines()
        {
            foreach (var line in StatsLines)
                Destroy(line.UnityObject);

            StatsLines.Clear();
        }
    }
}
