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
    public class SelectedOnlyFirstNEntriesSplitter : SelectedOnlySplitter
    {
        private MutableField<int> m_NumberOfEntries = new MutableField<int>() 
        { LiteralValue = 10 };
        [Controllable(LabelText = "Number Of Entries")]
        public MutableField<int> NumberOfEntries { get { return m_NumberOfEntries; } }


        protected override void SelectGroups( MutableObject mutable )
        {
            var numberOfEntries = NumberOfEntries.GetFirstValue( mutable );

            SelectedList = new List< MutableObject >();
            
            int entryCount = 0;

            foreach ( var entry in EntryField.GetEntries( mutable ) )
            {
                if ( ++entryCount > numberOfEntries )
                    break;

                SelectedList.Add( entry.Last() );
            }
        }
    }
}
