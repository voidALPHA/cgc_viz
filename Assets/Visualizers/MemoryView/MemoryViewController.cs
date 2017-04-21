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
using Chains;
using Mutation;
using UnityEngine;
using Visualizers.IsoGrid;

namespace Visualizers.MemoryView
{
    public class MemoryViewController : VisualizerController
    {
        private MutableField<Vector3> m_DashStartPosition = new MutableField<Vector3>() 
        { AbsoluteKey = "Entries.Position" };
        [Controllable(LabelText = "Dash Start Position")]
        public MutableField<Vector3> DashStartPosition { get { return m_DashStartPosition; } }
        
        private MutableField<float> m_DashWidth = new MutableField<float>() 
        { LiteralValue = .1f };
        [Controllable(LabelText = "Dash Width")]
        public MutableField<float> DashWidth { get { return m_DashWidth; } }

        private MutableField<float> m_Elevation = new MutableField<float>() 
        { LiteralValue = 0f };
        [Controllable(LabelText = "Dash Elevation")]
        public MutableField<float> Elevation { get { return m_Elevation; } }

        private MutableField<float> m_DashDepth = new MutableField<float>() 
        { LiteralValue = .1f };
        [Controllable(LabelText = "Dash Depth")]
        public MutableField<float> DashDepth { get { return m_DashDepth; } }



        //private MutableField<Vector3> m_WidthAxis = new MutableField<Vector3>() 
        //{ LiteralValue = Vector3.right };
        //[Controllable(LabelText = "Global Width Axis")]
        //public MutableField<Vector3> WidthAxis { get { return m_WidthAxis; } }

        //private MutableField<Vector3> m_ElevationAxis = new MutableField<Vector3>() 
        //{ LiteralValue = Vector3.up };
        //[Controllable(LabelText = "Global Elevation Axis")]
        //public MutableField<Vector3> ElevationAxis { get { return m_ElevationAxis; } }

        private MutableField<Color> m_DashColor = new MutableField<Color>() 
        { LiteralValue = Color.red };
        [Controllable(LabelText = "Dash Color")]
        public MutableField<Color> DashColor { get { return m_DashColor; } }

        public SelectionState DefaultState { get { return Router["Default"]; } }
        

        public MemoryViewController()
        {
            DashColor.SchemaParent = DashStartPosition;

            DashWidth.SchemaParent = DashStartPosition;
            DashDepth.SchemaParent = DashStartPosition;
            Elevation.SchemaParent = DashStartPosition;

            Router.AddSelectionState( "Default" );
        }

        protected override IEnumerator ProcessPayload( VisualPayload payload )
        {
            var memoryGraph = VisualizerFactory.InstantiateMemoryGraph();

            memoryGraph.Initialize( this, payload );

            foreach ( var entry in DashStartPosition.GetEntries( payload.Data ) )
            {
                var startPosition = DashStartPosition.GetValue( entry );

                var dashWidth = DashWidth.GetValue( entry );
                var dashDepth = DashDepth.GetValue( entry );
                var elevation = Elevation.GetValue( entry );

                var dashColor = DashColor.GetValue( entry );

                memoryGraph.AddDash(
                    startPosition, 
                    new Vector3(dashWidth, elevation, dashDepth),
                    dashColor);

                yield return null;
            }

            yield return null;

            memoryGraph.ApplyPoints();
        }
    }
}
