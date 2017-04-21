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
using LabelSystem;
using Mutation;
using UnityEngine;

namespace Visualizers.IsoGrid
{
    public class IsoGridController : VisualizerController
    {
        public IsoGridBehaviour IsoGrid { get; set; }

        private MutableField<IEnumerable<MutableObject>> m_EntryField = new MutableField<IEnumerable<MutableObject>> { AbsoluteKey = "Scores" };
        [Controllable(LabelText = "Bar Entries")]
        public MutableField<IEnumerable<MutableObject>> EntryField { get { return m_EntryField; } }

        // The following two mutable fields have unfortunate names.  XAxis is really "This is the variable to use for the X Axis Coordinate of the isoGrid".
        // Unfortunately renaming these breaks HPs.
        private MutableField<int> m_XAxis = new MutableField<int> { AbsoluteKey = "Scores.XAxisIndex" };
        [Controllable(LabelText = "X Axis Index")]
        public MutableField<int> XAxis { get { return m_XAxis; } }

        private MutableField<int> m_ZAxis = new MutableField<int> { AbsoluteKey = "Scores.ZAxisIndex" };
        [Controllable(LabelText = "Z Axis Index")]
        public MutableField<int> ZAxis { get { return m_ZAxis; } }

        private MutableField<float> m_YAxis = new MutableField<float> { AbsoluteKey = "Scores.Total Score" };
        [Controllable(LabelText = "Y Axis Range Variable")]
        public MutableField<float> YAxis { get { return m_YAxis; } }

        private MutableField<bool> m_ShowBackgrounds = new MutableField<bool>() { LiteralValue = true };
        [Controllable(LabelText = "Show Backgrounds")]
        public MutableField<bool> ShowBackgrounds { get { return m_ShowBackgrounds; } }

        private MutableField< bool > m_AllowSlicerField = new MutableField<bool> { LiteralValue = false };
        [Controllable(LabelText = "Allow Slicer")]
        private MutableField< bool > AllowSlicerField { get { return m_AllowSlicerField; } }

        private LabelSystem.LabelSystem m_LabelSystem = new AxialLabelSystem();
        [Controllable]
        private LabelSystem.LabelSystem LabelSystem { get { return m_LabelSystem; } set { m_LabelSystem = value; } }
        

        [SerializeField]
        private int m_VerticalLabelCount = 11;
        private int VerticalLabelCount { get { return m_VerticalLabelCount; } set { m_VerticalLabelCount = value; } }


        #region Selection Graph States

        public SelectionState NormalState { get { return Router[ "Normal" ]; } }
        public SelectionState SelectedState { get { return Router["Selected"]; } }
        public SelectionState NoneSelectedState { get { return Router["None Selected"]; } }
        public SelectionState NormalStateMulti { get { return Router["Normal (Group)"]; } }
        public SelectionState SelectedStateMulti { get { return Router["Selected (Group)"]; } }
        public SelectionState NoneSelectedStateMulti { get { return Router["None Selected (Group)"]; } }

        #endregion

        public IsoGridController() : base()
        {
            XAxis.SchemaParent = EntryField;
            ZAxis.SchemaParent = EntryField;
            YAxis.SchemaParent = EntryField;

            Router.AddSelectionState("Normal", "Single" );
            Router.AddSelectionState("Selected", "Single");
            Router.AddSelectionState("None Selected", "Single");

            Router.AddSelectionState("Normal (Group)", "Multi");
            Router.AddSelectionState("Selected (Group)", "Multi");
            Router.AddSelectionState("None Selected (Group)", "Multi");

            LabelSystem.ChainNode = this;
        }

        protected override IEnumerator ProcessPayload(VisualPayload payload)
        {
            Debug.LogWarning( "[BOUND DEBUG] IsoGrid rendering to bound.", payload.VisualData.Bound );

            IsoGrid = VisualizerFactory.InstantiateIsoGrid();

            IsoGrid.Initialize( this, payload );
            //payload.VisualData.Bound.ChildVisualizer(this, IsoGrid);

            IsoGrid.DrawBackgrounds = ShowBackgrounds.GetLastKeyValue(payload.Data);
            
            IsoGrid.InitializeIsoGrid(payload.VisualData.Bound);

            IsoGrid.AllowSlicer = AllowSlicerField.GetLastKeyValue( payload.Data );

            AssignStates(IsoGrid);

            var entries = EntryField.GetFirstValue(payload.Data);
            if (entries == null)
                throw new Exception("Illegal mutable field here!  " + EntryField.AbsoluteKey + " is not an enumerable of mutables!");


            // Determine the full size of the entire isogrid
            int numCellsXAxis = 0;
            int numCellsZAxis = 0;

            if ( entries.Any() )
            {
                numCellsXAxis = XAxis.GetEntries(payload.Data).Max(e => XAxis.GetValue(e)) + 1;
                numCellsZAxis = ZAxis.GetEntries(payload.Data).Max(e => ZAxis.GetValue(e)) + 1;
            }

            IsoGrid.UpdateHorizontalScale(numCellsXAxis);
            IsoGrid.UpdateDepthScale(numCellsZAxis);

            AssignElements(entries);

            var iterator = IsoGrid.Apply();
            while (iterator.MoveNext( ))
                yield return null;

            // PROCESS the label stuff, that was specified by the author:
            LabelSystem.Render(payload, IsoGrid.transform, IsoGrid.SelectionManager);
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            var elementList = EntryField.GetFirstValue(newSchema) as IEnumerable<MutableObject>;

            if (elementList == null)
                Debug.LogError("This isn't a valid schema!  The entries field is empty!");

            var singleSchema = elementList.First();
            var multiSchema = new MutableObject();
            multiSchema.Add("Entries", elementList);

            NormalState.TransmitSchema(singleSchema);
            SelectedState.TransmitSchema(singleSchema);
            NoneSelectedState.TransmitSchema(singleSchema);

            NormalStateMulti.TransmitSchema(multiSchema);
            SelectedStateMulti.TransmitSchema(multiSchema);
            NoneSelectedStateMulti.TransmitSchema(multiSchema);
        }

        private void AssignStates(IsoGridBehaviour isoGrid)
        {
            isoGrid.NormalState = NormalState;
            isoGrid.SelectedState = SelectedState;
            isoGrid.NoneSelectedState = NoneSelectedState;
            isoGrid.NormalStateMulti = NormalStateMulti;
            isoGrid.SelectedStateMulti = SelectedStateMulti;
            isoGrid.NoneSelectedStateMulti = NoneSelectedStateMulti;
        }

        // This supports non-rectangular data, because the X and Z "grid coordinates" are stored in the payload itself
        private void AssignElements(IEnumerable<MutableObject> mutables)
        {
            foreach (var element in mutables)
                IsoGrid.AddEntry(XAxis.GetLastKeyValue(element), ZAxis.GetLastKeyValue(element), element);
        }
    }
}
