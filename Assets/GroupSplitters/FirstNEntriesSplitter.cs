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
    public class FirstNEntriesSplitter : GroupSplitter
    {
        private MutableField<int> m_NumberOfEntries = new MutableField<int>() 
        { LiteralValue = 10 };
        [Controllable(LabelText = "Number Of Entries")]
        public MutableField<int> NumberOfEntries { get { return m_NumberOfEntries; } }
        
        private MutableField<bool> m_SelectedListOnly = new MutableField<bool>()
        { LiteralValue = false };
        [Controllable(LabelText = "Selected List Only")]
        public MutableField<bool> SelectedListOnly { get { return m_SelectedListOnly; } }

        public FirstNEntriesSplitter() : base()
        {
            NumberOfEntries.SchemaPattern = Scope;
        }

        protected override void SelectGroups( List<MutableObject> entry )
        {
            SelectedList = new List<MutableObject>();
            UnSelectedList = new List<MutableObject>();

            var selectedListOnly = SelectedListOnly.GetFirstValue( entry.First() );

            int entryCount = 0;
            var maxEntries = NumberOfEntries.GetValue( entry );

            foreach ( var subEntry in EntryField.GetEntries( entry ) )
            {
                if ( entryCount++ < maxEntries )
                    SelectedList.Add( subEntry.Last() );
                else
                {
                    if ( selectedListOnly )
                        break;
                    UnSelectedList.Add( subEntry.Last() );
                }
            }
        }

        protected override void DenoteEmptyUnSelectedList( MutableObject mutable, MutableObject defaultObject )
        {
            if (SelectedListOnly.GetFirstValue(mutable))
                UnSelectedList = new List< MutableObject >();
            else 
                base.DenoteEmptyUnSelectedList( mutable, defaultObject );
        }
    }
}
