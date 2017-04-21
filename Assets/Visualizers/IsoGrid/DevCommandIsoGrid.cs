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
using Chains;
using ChainViews;
using UnityEngine;
using Visualizers.IsoGrid;

namespace Utility.DevCommand
{
    public class DevCommandIsoGrid : MonoBehaviour, IDevCommand
    {
        private Chain m_Chain = null;
        public Chain Chain
        {
            get { return m_Chain; }
            set
            {
                if ( m_Chain != null )
                {
                    // Obsolete with groups
                    //m_Chain.NodeAdded -= HandleChainNodesChanged;
                    //m_Chain.NodeRemoved -= HandleChainNodesChanged;
                }

                m_Chain = value;

                RegisterControllersFromChain( Chain );

                if ( m_Chain != null )
                {
                    // Obsolete with groups
                    //m_Chain.NodeAdded += HadleChainNodesChanged;
                    //m_Chain.NodeRemoved += HandleChainNodesChanged;
                }
            }
        }

        private void HandleChainNodesChanged( ChainNode obj )
        {
            RegisterControllersFromChain( Chain );
        }

        private readonly string m_Name = "isogrid";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private const string m_HelpTextBrief = "Isogrid visualizer control";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Option m_IsoGridIndexOption = new DevCommandManager.Option("isoGridIndex", "Set index of which isoGrid to apply this and subsequent commands to",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "isoGridIndex", typeof (int), true )
            });
        [DevCommandOptionAttribute]
        public DevCommandManager.Option IsoGridIndexOption { get { return m_IsoGridIndexOption; } set { m_IsoGridIndexOption = value; } }

        private DevCommandManager.Option m_XAxisOption = new DevCommandManager.Option("XAxis", "Define mutable key for the X axis",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "mutableKey", typeof (string), true )
            });
        [DevCommandOptionAttribute]
        public DevCommandManager.Option XAxisOption { get { return m_XAxisOption; } set { m_XAxisOption = value; } }

        private DevCommandManager.Option m_ZAxisOption = new DevCommandManager.Option("ZAxis", "Define mutable key for the Z axis",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "mutableKey", typeof (string), true )
            });
        [DevCommandOptionAttribute]
        public DevCommandManager.Option ZAxisOption { get { return m_ZAxisOption; } set { m_ZAxisOption = value; } }

        private DevCommandManager.Option m_YAxisOption = new DevCommandManager.Option("yAxis", "Define mutable key for the Y axis",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "mutableKey", typeof (string), true )
            });
        [DevCommandOptionAttribute]
        public DevCommandManager.Option YAxisOption { get { return m_YAxisOption; } set { m_YAxisOption = value; } }

        private DevCommandManager.Option m_ColorOption = new DevCommandManager.Option("color", "Define mutable key for color",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "mutableKey", typeof (string), true )
            });
        [DevCommandOptionAttribute]
        public DevCommandManager.Option ColorOption { get { return m_ColorOption; } set { m_ColorOption = value; } }


        private List<IsoGridController> m_Controllers = new List<IsoGridController>();
        public List<IsoGridController> Controllers { get { return m_Controllers; } set { m_Controllers = value; } }

        private int m_CurrentControllerIndex = 0;
        private int CurrentControllerIndex { get { return m_CurrentControllerIndex; } set { m_CurrentControllerIndex = value; } }


        private void Awake()
        {
            if ( ChainView.Instance != null )
                ChainView.Instance.ChainLoaded += HandleChainViewChainLoaded;
        }

        private void HandleChainViewChainLoaded( Chain chain )
        {
            Chain = chain;
        }

        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand( this );
        }

        public bool Execute()
        {
            if (Controllers.Count == 0)
            {
                Debug.Log("No isogrid controllers");
                return false;
            }

            if (CurrentControllerIndex >= Controllers.Count)
                CurrentControllerIndex = 0;

            // Set isogrid index (which isogrid do we want to access)
            if (IsoGridIndexOption.IsPresent)
            {
                var index = (int)IsoGridIndexOption.Arguments[0].Value;
                if (index < 0 || index >= Controllers.Count)
                {
                    Debug.Log("IsoGrid index out of range (currently must be in range 0 to " + (Controllers.Count - 1).ToString() + ")");
                    return false;
                }
                CurrentControllerIndex = index;
            }

            // Set various options

            // Set mutable keys
            if (XAxisOption.IsPresent)
                Controllers[CurrentControllerIndex].XAxis.AbsoluteKey = XAxisOption.Arguments[0].Value.ToString();
            if (ZAxisOption.IsPresent)
                Controllers[CurrentControllerIndex].ZAxis.AbsoluteKey = ZAxisOption.Arguments[0].Value.ToString();
            if (YAxisOption.IsPresent)
                Controllers[CurrentControllerIndex].YAxis.AbsoluteKey = YAxisOption.Arguments[0].Value.ToString();

            return true;
        }

        private void RegisterControllersFromChain(Chain chain)
        {
            // Obsolete with groups

            //Controllers.Clear();

            //foreach (var node in chain.NodeEnumerable)
            //{
            //    if (node is IsoGridController)
            //    {
            //        var controller = node as IsoGridController;
            //        Controllers.Add(controller);
            //    }
            //}
        }
    }
}
