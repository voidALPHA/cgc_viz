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
    public abstract class InsertSortIndexMutator : DataMutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Elements To Sort", Order = -2)]
        public MutableScope Scope { get { return m_Scope; } }
        
        private MutableTarget m_SortIndexTarget = new MutableTarget() 
        { AbsoluteKey = "Entries.Sort Index" };
        [Controllable(LabelText = "Sort Index Target")]
        public MutableTarget SortIndexTarget { get { return m_SortIndexTarget; } }

        protected InsertSortIndexMutator()
        {
            SortIndexTarget.SchemaParent = Scope;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            IndexEntries( mutable );

            return mutable;
        }

        protected abstract void IndexEntries(MutableObject mutable);

    }
}
