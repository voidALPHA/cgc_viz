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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chains;
using Mutation;
using UnityEngine;
using Visualizers.IsoGrid;

namespace Visualizers.LineGraph
{
    public class LineGraphController : VisualizerController
    {
        private MutableField<IEnumerable<MutableObject>> m_EntryField = new MutableField<IEnumerable<MutableObject>> { AbsoluteKey = "Entries" };
        [Controllable(LabelText = "Line Entries")]
        public MutableField<IEnumerable<MutableObject>> EntryField { get { return m_EntryField; } }


        private MutableField<float> m_XAxis = new MutableField<float> { LiteralValue = 0f };
        [Controllable(LabelText = "X Axis")]
        public MutableField<float> XAxis { get { return m_XAxis; } }

        private MutableField<float> m_YAxis = new MutableField<float> { LiteralValue = 0f };
        [Controllable(LabelText = "Y Axis")]
        public MutableField<float> YAxis { get { return m_YAxis; } }

        private MutableField<float> m_ZAxis = new MutableField<float> { LiteralValue = 0f };
        [Controllable(LabelText = "Z Axis")]
        public MutableField<float> ZAxis { get { return m_ZAxis; } }

        private MutableField<float> m_LineWidth = new MutableField<float>() { LiteralValue = .085f };
        [Controllable(LabelText = "LineWidth [0-.1]")]
        public MutableField<float> LineWidth { get { return m_LineWidth; } }

        private MutableField<bool> m_PulseLine = new MutableField<bool>() { LiteralValue = false };
        [Controllable(LabelText = "Pulse Line?")]
        public MutableField<bool> PulseLine { get { return m_PulseLine; } }


        private MutableField<float> m_PulseWidth = new MutableField<float>() { LiteralValue = .085f };
        [Controllable(LabelText = "PulseWidth [0-.1]")]
        public MutableField<float> PulseWidth { get { return m_PulseWidth; } }


        private MutableField<Color> m_MainColor = new MutableField<Color>() 
        { LiteralValue = Color.red };
        [Controllable(LabelText = "MainColor")]
        public MutableField<Color> MainColor { get { return m_MainColor; } }



        private MutableField<float> m_EdgeWidth = new MutableField<float>() 
        { LiteralValue = .085f };
        [Controllable(LabelText = "EdgeWidth [0-.5]")]
        public MutableField<float> EdgeWidth { get { return m_EdgeWidth; } }

        private MutableField<float> m_ZDepthOffset = new MutableField<float>() 
        { LiteralValue = 0f };
        [Controllable(LabelText = "ZDepthOverride")]
        public MutableField<float> ZDepthOffset { get { return m_ZDepthOffset; } }



        private MutableField<Color> m_EdgeColor = new MutableField<Color>() { LiteralValue = Color.black };
        [Controllable(LabelText = "EdgeColor")]
        public MutableField<Color> EdgeColor { get { return m_EdgeColor; } }


        private MutableField<Color> m_PointColor = new MutableField<Color>() { LiteralValue = Color.white };
        [Controllable(LabelText = "PointColor")]
        public MutableField<Color> PointColor { get { return m_PointColor; } }

        private MutableField<float> m_PointWidth = new MutableField<float>() { LiteralValue = .01f };
        [Controllable(LabelText = "PointWidth [0-.5]")]
        public MutableField<float> PointWidth { get { return m_PointWidth; } }
        
        private MutableField<bool> m_Wipe = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "Wipe line along X?")]
        public MutableField<bool> Wipe { get { return m_Wipe; } }
        
        private MutableField<float> m_StartTime = new MutableField<float>() 
        { LiteralValue = 0f };
        [Controllable(LabelText = "StartTime")]
        public MutableField<float> StartTime { get { return m_StartTime; } }

        private MutableField<float> m_WipeDuration = new MutableField<float>() 
        { LiteralValue = 1 };
        [Controllable(LabelText = "WipeDuration")]
        public MutableField<float> WipeDuration { get { return m_WipeDuration; } }



        public LineGraphController()
            : base()
        {
            XAxis.SchemaParent = EntryField;
            YAxis.SchemaParent = EntryField;
            ZAxis.SchemaParent = EntryField;
            PointColor.SchemaParent = EntryField;

        }

        private void AssignStates(LineGraphVisualizer visualizer)
        {
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            var elementList = EntryField.GetLastKeyValue(newSchema) as IEnumerable<MutableObject>;

            if (elementList == null)
                Debug.LogError("This isn't a valid schema!  The entries field is empty!");

            //var singleSchema = elementList.First();
            var multiSchema = new MutableObject();
            multiSchema.Add("Entries", elementList);
        }

        protected override IEnumerator ProcessPayload(VisualPayload payload)
        {
            payload.VisualData.Bound.name = "Line Graph Bound";

            var lineGraph = VisualizerFactory.InstantiateLineGraph();

            lineGraph.Initialize(this, payload);

            AssignStates(lineGraph);

            var entries = EntryField.GetLastKeyValue(payload.Data) as IEnumerable<MutableObject>;
            if (entries == null)
                throw new Exception("Illegal mutable field here!  " + EntryField.AbsoluteKey + " is not an enumerable of mutables!");

            if (!entries.Any())
            {
                yield return null;
                yield break;
            }

            foreach (var entry in entries)
            {
                Vector3 entryPosition =
                    new Vector3(
                        XAxis.GetLastKeyValue(entry),
                        YAxis.GetLastKeyValue(entry),
                        ZAxis.GetLastKeyValue(entry)
                        );

                Color pointColor = PointColor.GetLastKeyValue(entry);

                lineGraph.AddPoint(entryPosition, pointColor);

                yield return null;
            }

            yield return null;

            lineGraph.SetLineData(MainColor.GetLastKeyValue(payload.Data),
                LineWidth.GetLastKeyValue(payload.Data),
                ZDepthOffset.GetLastKeyValue(payload.Data),
                EdgeColor.GetLastKeyValue(payload.Data),
                EdgeWidth.GetLastKeyValue(payload.Data),
                PointWidth.GetLastKeyValue(payload.Data),
                PulseLine.GetLastKeyValue(payload.Data),
                PulseWidth.GetLastKeyValue( payload.Data ),
                Wipe.GetLastKeyValue( payload.Data ),
                StartTime.GetLastKeyValue(payload.Data),
                WipeDuration.GetLastKeyValue(payload.Data)
                );

            lineGraph.ApplyPoints();

        }
    }
}
