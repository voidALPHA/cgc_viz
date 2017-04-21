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
using Mutation;
using Visualizers;

namespace GroupSplitters
{
    public class AxialRangeSplitter : GroupSplitter
    {
        private MutableField<float> m_AxisField = new MutableField<float> { AbsoluteKey = "Axis" };
        [Controllable(LabelText = "Axis")]
        public MutableField<float> AxisField { get { return m_AxisField; } }

        private MutableField<float> m_SplittingPoint = new MutableField<float>(){LiteralValue = 4f};
        [Controllable(LabelText = "Splitting Point")]
        public MutableField<float> SplittingPoint { get { return m_SplittingPoint; } }

        public AxialRangeSplitter() : base()
        {
            AxisField.SchemaParent = EntryField;
        }

        protected override void SelectGroups(List<MutableObject> entry)
        {
            SelectedList = new List<MutableObject>();
            UnSelectedList = new List<MutableObject>();

            foreach (var subEntry in EntryField.GetEntries(entry))
            {
                if (IsSelected(subEntry))
                    SelectedList.Add(subEntry.Last());
                else
                    UnSelectedList.Add(subEntry.Last());
            }
        }

        protected bool IsSelected(List<MutableObject> mutable)
        {
            return (AxisField.GetValue(mutable)>SplittingPoint.GetValue(mutable));
        }
    }
}
