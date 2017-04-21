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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Mutation;
using Visualizers.IsoGrid;

namespace Visualizers.DisassemblyWindow
{
    [UsedImplicitly]
    public class DisassemblyGroupController : VisualizerController
    {
        private MutableField<string> m_WindowGroupKeyField = new MutableField<string>() { LiteralValue = "" };
        [Controllable( LabelText = "Window Group Key" )]
        private MutableField<string> WindowGroupKeyField { get { return m_WindowGroupKeyField; } }

        private MutableField<bool> m_EnlargeViewField = new MutableField<bool> { LiteralValue = false };
        [Controllable( LabelText = "Enlarge View" )]
        public MutableField<bool> EnlargeViewField
        {
            get { return m_EnlargeViewField; }
        }
        
        // Don't access this outside of GetGroupVisualizer! Must be lazy-init'd there.
        private static readonly Dictionary< string, DisassemblyGroupVisualizer > s_GroupVisualizers = new Dictionary< string, DisassemblyGroupVisualizer >();
        private static Dictionary< string, DisassemblyGroupVisualizer > GroupVisualizers { get { return s_GroupVisualizers; } }

        // Don't access this outside of GetWindowVisualizer! Must be lazy-init'd there.
        private static readonly Dictionary< string, Dictionary< string, DisassemblyWindowVisualizer > > s_WindowVisualizers = new Dictionary< string, Dictionary< string, DisassemblyWindowVisualizer > >();
        private static Dictionary< string, Dictionary< string, DisassemblyWindowVisualizer > > WindowVisualizers { get { return s_WindowVisualizers; } }


        private static DisassemblyGroupVisualizer GetGroupVisualizer( string groupKey )
        {
            // Return if already made
            if ( GroupVisualizers.ContainsKey( groupKey ) )
                return GroupVisualizers[ groupKey ];

            // Create
            var groupVisualizerGo = VisualizerFactory.InstantiateDisassemblyGroup();
            var groupVisualizer = groupVisualizerGo.GetComponent< DisassemblyGroupVisualizer >();

            // Register
            GroupVisualizers.Add( groupKey, groupVisualizer );
            WindowVisualizers.Add( groupKey, new Dictionary< string, DisassemblyWindowVisualizer >() );

            // Return
            return groupVisualizer;
        }


        public static DisassemblyWindowVisualizer GetWindowVisualizer( string groupKey, string windowKey )
        {
            // Return if already made
            if ( WindowVisualizers.ContainsKey( groupKey ) )
                if ( WindowVisualizers[ groupKey ].ContainsKey( windowKey ) )
                    return WindowVisualizers[ groupKey ][ windowKey ];

            // Create or get the group visualizer
            var groupVisualizer = GetGroupVisualizer( groupKey );


            // Create the window
            var windowVisualizerGo = VisualizerFactory.InstantiateDisassemblyWindow();
            var windowVisualizer = windowVisualizerGo.GetComponent<DisassemblyWindowVisualizer>();

            // Register
            WindowVisualizers[ groupKey ].Add( windowKey, windowVisualizer );
            

            // Parent
            groupVisualizer.Attach( windowVisualizer );
            

            // Return
            return windowVisualizer;

        }

        protected override IEnumerator ProcessPayload( VisualPayload payload )
        {
            var groupKey = WindowGroupKeyField.GetFirstValue( payload.Data );



            var groupVisualizer = GetGroupVisualizer( groupKey );

            //groupVisualizer.transform.SetParent( payload.VisualData.Bound.transform, false );


            MyGroupKeys.Add( groupKey );


            groupVisualizer.Enlarge = EnlargeViewField.GetFirstValue( payload.Data );


            //If there were states, here is where to call Router.TransmitAll( payload );


            yield return null;
        }

        private readonly List< string > m_MyGroupKeys = new List< string >();
        private List< string > MyGroupKeys { get { return m_MyGroupKeys; } }

        public override void Unload()
        {
            if ( !MyGroupKeys.Any() )
                return;

            foreach ( var groupKey in MyGroupKeys )
            {
                // Destroy windows within this group
                foreach ( var windowVisualizer in WindowVisualizers[ groupKey ].Values )
                {
                    windowVisualizer.Destroy();
                }

                // Destroy this group
                GroupVisualizers[ groupKey ].Destroy();

                // Clean up registry to match
                GroupVisualizers.Remove( groupKey );
                WindowVisualizers.Remove( groupKey );
            }

            MyGroupKeys.Clear();

            base.Unload();
        }
    }
}
