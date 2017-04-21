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
using Mutation;
using Visualizers;

namespace GroupSplitters
{
    public class BoolSplitter : GroupSplitter
    {
        private MutableField<bool> m_SelectedBool = new MutableField<bool>() 
        { AbsoluteKey = "Scores.hasAnnotation"};
        [Controllable(LabelText = "Select bool")]
        public MutableField<bool> SelectedBool { get { return m_SelectedBool; } }

        public BoolSplitter() : base()
        {
            SelectedBool.SchemaPattern = EntryField;
        }

        protected override void SelectGroups( List<MutableObject> entry )
        {
            SelectedList = new List<MutableObject>();
            UnSelectedList = new List<MutableObject>();
            
            foreach (var subEntry in EntryField.GetEntries(entry))
            {
                if (SelectedBool.GetValue( subEntry ))
                    SelectedList.Add(subEntry.Last());
                else
                    UnSelectedList.Add(subEntry.Last());
            }
        }
    }
}
