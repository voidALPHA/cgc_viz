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
using Chains;
using Mutation;
using UnityEngine;
using Visualizers.IsoGrid;
using Visualizers.LineGraph;

namespace Visualizers.FilledGraph
{
    public class FilledGraphController : VisualizerController
    {
        private MutableScope m_EntryField = new MutableScope();
        [Controllable(LabelText = "Line Entries Scope")]
        public MutableScope EntryField { get { return m_EntryField; } }

        private MutableField<float> m_YAxis = new MutableField<float> { LiteralValue = 0f };
        [Controllable(LabelText = "Y Axis")]
        public MutableField<float> YAxis { get { return m_YAxis; } }

        private MutableField<float> m_ZAxis = new MutableField<float> { LiteralValue = 0f };
        [Controllable(LabelText = "Z Axis")]
        public MutableField<float> ZAxis { get { return m_ZAxis; } }

        private MutableField<Color> m_MainColor = new MutableField<Color>()
        { LiteralValue = Color.red };
        [Controllable(LabelText = "MainColor")]
        public MutableField<Color> MainColor { get { return m_MainColor; } }

        private SelectionState Default { get { return Router[ "Default" ]; } }

        public FilledGraphController() : base()
        {
            YAxis.SchemaParent = EntryField;
            ZAxis.SchemaParent = EntryField;

            Router.AddSelectionState( "Default" );
        }
        
        private void AssignStates(FilledGraphVisualizer visualizer)
        {
        }


        protected override IEnumerator ProcessPayload( VisualPayload payload )
        {
            var filledGraph = VisualizerFactory.InstantiateFilledGraph();

            filledGraph.Initialize( this, payload );

            AssignStates( filledGraph );


            var mainColor = MainColor.GetFirstValue( payload.Data );

            
            foreach ( var entry in EntryField.GetEntries( payload.Data ) )
            {
                filledGraph.AddPoint( ZAxis.GetValue( entry ),
                    YAxis.GetValue( entry ) );
            }

            filledGraph.SetGraphData(
                mainColor);

            filledGraph.ApplyPoints();

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
