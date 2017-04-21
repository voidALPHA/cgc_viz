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
using System.Text;
using Mutation.Mutators;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators
{
    public class GridPositionMutator : DataMutator
    {
        private MutableField<int> m_SortCriterion = new MutableField<int>()
        {LiteralValue = 0};
        [Controllable(LabelText = "Sort Criterion")]
        public MutableField<int> SortCriterion
        {
            get { return m_SortCriterion; }
        }

        private MutableScope m_Entries = new MutableScope(){AbsoluteKey = "Entries"};
        [Controllable(LabelText = "Entries")]
        public MutableScope Entries { get { return m_Entries; } }

        private MutableTarget m_XAxisTarget = new MutableTarget() 
        { AbsoluteKey = "X Axis" };
        [Controllable(LabelText = "X Axis Field Target")]
        public MutableTarget XAxisTarget { get { return m_XAxisTarget; } }

        private MutableTarget m_ZAxisTarget = new MutableTarget() 
        { AbsoluteKey = "Z Axis" };
        [Controllable(LabelText = "Z Axis Field Target")]
        public MutableTarget ZAxisTarget { get { return m_ZAxisTarget; } }

        private MutableField<int> m_GridWidth = new MutableField<int>()
        {LiteralValue = 1};
        [Controllable(LabelText = "Grid Width")]
        public MutableField<int> GridWidth
        {
            get { return m_GridWidth; }
        }


        public GridPositionMutator()
        {
            SortCriterion.SchemaParent = Entries;

            XAxisTarget.SchemaParent = Entries;
            ZAxisTarget.SchemaParent = Entries;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            var metaEntries = Entries.GetEntries( mutable );

            //metaEntries = metaEntries.OrderBy( lMut => SortCriterion.GetValue( lMut ) ).ToList();

            int index = 0;

            foreach (var metaEntry in metaEntries)
            {
                XAxisTarget.SetValue( index % GridWidth.GetValue( metaEntry ), metaEntry );
                ZAxisTarget.SetValue( index / GridWidth.GetValue( metaEntry ), metaEntry );

                index++;
            }
            return mutable;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            var entries = Entries.GetEntries(newSchema);

            foreach (var entry in entries)
            {
                XAxisTarget.SetValue(0, entry);
                ZAxisTarget.SetValue(0, entry);
            }

            Router.TransmitAllSchema(newSchema);
        }
    }
}
