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

namespace Visualizers.ScatterPlot
{
    public class ScatterPlotController : VisualizerController
    {
        private MutableField<IEnumerable<MutableObject>> m_EntryField = new MutableField<IEnumerable<MutableObject>> { AbsoluteKey = "Entries" };
        [Controllable(LabelText = "Bar Entries")]
        public MutableField<IEnumerable<MutableObject>> EntryField { get { return m_EntryField; } }


        private MutableField<float> m_XAxis = new MutableField<float> { LiteralValue = 0f};
        [Controllable(LabelText = "X Axis")]
        public MutableField<float> XAxis { get { return m_XAxis; } }

        private MutableField<float> m_YAxis = new MutableField<float> { LiteralValue = 0f};
        [Controllable(LabelText = "Y Axis")]
        public MutableField<float> YAxis { get { return m_YAxis; } }

        private MutableField<float> m_ZAxis = new MutableField<float> { LiteralValue = 0f };
        [Controllable(LabelText = "Z Axis")]
        public MutableField<float> ZAxis { get { return m_ZAxis; } }

        #region Selection Graph States
        public SelectionState NormalState { get { return Router["Normal"]; } }
        public SelectionState SelectedState { get { return Router["Selected"]; } }
        public SelectionState NormalStateMulti { get { return Router["Normal (Group)"]; } }
        public SelectionState SelectedStateMulti { get { return Router["Selected (Group)"]; } }

        #endregion
        

        public ScatterPlotController() : base()
        {
            XAxis.SchemaParent = EntryField;
            YAxis.SchemaParent = EntryField;
            ZAxis.SchemaParent = EntryField;

            Router.AddSelectionState("Normal");
            Router.AddSelectionState("Selected");
            Router.AddSelectionState("Normal (Group)");
            Router.AddSelectionState("Selected (Group)");
        }

        private void AssignStates(ScatterPlotVisualizer visualizer)
        {
            visualizer.NormalState = NormalState;
            visualizer.SelectedState = SelectedState;
            visualizer.NormalStateMulti = NormalStateMulti;
            visualizer.SelectedStateMulti = SelectedStateMulti;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            var elementList = EntryField.GetLastKeyValue(newSchema) as IEnumerable<MutableObject>;

            if (elementList == null)
                Debug.LogError("This isn't a valid schema!  The entries field is empty!");

            var singleSchema = elementList.First();
            var multiSchema = new MutableObject();
            multiSchema.Add("Entries", elementList);

            NormalState.TransmitSchema(singleSchema);
            SelectedState.TransmitSchema(singleSchema);

            NormalStateMulti.TransmitSchema(multiSchema);
            SelectedStateMulti.TransmitSchema(multiSchema);
        }

        protected override IEnumerator ProcessPayload(VisualPayload payload)
        {
            payload.VisualData.Bound.name = "Scatter Plot Bound";

            var scatterPlot = VisualizerFactory.InstantiateScatterPlot();

            scatterPlot.Initialize(this, payload);

            AssignStates(scatterPlot);

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

                scatterPlot.DrawPoint(entryPosition, entry);
            }

            scatterPlot.ApplyPoints();
        }
    }
}
