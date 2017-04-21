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
    public class StringMatchSplitter : GroupSplitter
    {
        private MutableField<string> m_StringProperty = new MutableField<string>() 
        { AbsoluteKey = "Team Name" };
        [Controllable(LabelText = "String from each entry")]
        public MutableField<string> StringProperty { get { return m_StringProperty; } }

        private MutableField<string> m_StringArgument = new MutableField<string>() { LiteralValue = "Deep_Red" };
        [Controllable(LabelText = "String Parameter to match")]
        public MutableField<string> StringArgument { get { return m_StringArgument; } }

        public StringMatchSplitter()
        {
            StringArgument.SchemaPattern = EntryField;
            StringProperty.SchemaParent = EntryField;
        }

        protected override void SelectGroups( List<MutableObject> entry )
        {
            SelectedList = new List<MutableObject>();
            UnSelectedList = new List<MutableObject>();

            foreach ( var subEntry in EntryField.GetEntries( entry ) )
            {
                if (IsSelected(subEntry))
                    SelectedList.Add(subEntry.Last());
                else
                    UnSelectedList.Add(subEntry.Last());
            }
        }

        private bool IsSelected(List<MutableObject> entry)
        {
            if ( !StringArgument.CouldResolve( entry ) )
                return false;

            return (StringArgument.GetValue(entry)
                == StringProperty.GetValue(entry));
        }
    }


}
