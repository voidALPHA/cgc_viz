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
using System.Linq;
using JetBrains.Annotations;
using Mutation;
using Visualizers;

namespace GroupSplitters
{
    [UsedImplicitly]
    public class HasAnnotationsGroupSplitter : GroupSplitter
    {
        private MutableField<bool> m_ContainsAnnotationField = new MutableField<bool> { AbsoluteKey = "Instructions.Annotated" };
        [Controllable(LabelText = "Is Instruction Annotated")]
        public MutableField<bool> ContainsAnnotationField { get { return m_ContainsAnnotationField; } }

        protected override string SelectedName { get { return "Annotated"; } }
        protected override string UnSelectedName { get { return "Not Annotated"; } }

        public HasAnnotationsGroupSplitter()
        {
            ContainsAnnotationField.SchemaParent = EntryField;
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
            return ContainsAnnotationField.GetValue(mutable);
        }
    }
}
