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
using Visualizers;

namespace Mutation.Mutators.SortMutators
{
    public abstract class SortMutator : DataMutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Scope", Order=-2)]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<IEnumerable<MutableObject>> m_EntriesToSort = new MutableField<IEnumerable<MutableObject>>() 
        { AbsoluteKey = "Entries" };
        [Controllable(LabelText = "Entries To Sort", Order = -2)]
        public MutableField<IEnumerable<MutableObject>> EntriesToSort { get { return m_EntriesToSort; } }

        private MutableTarget m_SortedTarget = new MutableTarget() 
        { AbsoluteKey = "SortedEntries" };
        [Controllable(LabelText = "Sorted Target")]
        public MutableTarget SortedTarget { get { return m_SortedTarget; } }

        protected SortMutator()
        {
            EntriesToSort.SchemaParent = Scope;
            SortedTarget.SchemaParent = Scope;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {
                var entriesToSort = EntriesToSort.GetValue( entry );

                if ( entriesToSort == null || !entriesToSort.Any() )
                {
                    SortedTarget.SetValue( new List< MutableObject >(), entry );
                    continue;
                }

                var sortedEntries = SortEntries( entry, entriesToSort.ToList() );

                SortedTarget.SetValue( sortedEntries, entry );
            }

            return mutable;
        }

        protected abstract List< MutableObject > SortEntries( List<MutableObject> mutableEntryList, List< MutableObject > sortableList );
    }
}
