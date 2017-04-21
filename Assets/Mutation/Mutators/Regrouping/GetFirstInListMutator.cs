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
using GroupSplitters;
using Visualizers;

namespace Mutation.Mutators.Regrouping
{
    public class GetFirstInListMutator : DataMutator
    {
        private MutableScope m_Scope = new MutableScope() { AbsoluteKey = "" };
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<IEnumerable<MutableObject>> m_ListToSplit = new MutableField<IEnumerable<MutableObject>>() 
        { AbsoluteKey = "Entries" };
        [Controllable(LabelText = "List To Split")]
        public MutableField<IEnumerable<MutableObject>> ListToSplit { get { return m_ListToSplit; } }

        private MutableTarget m_FirstElement = new MutableTarget() 
        { AbsoluteKey = "First Element" };
        [Controllable(LabelText = "First Element")]
        public MutableTarget FirstElement { get { return m_FirstElement; } }

        public GetFirstInListMutator()
        {
            ListToSplit.SchemaParent = Scope;
            FirstElement.SchemaParent = Scope;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {
                var list = ListToSplit.GetValue( entry).ToList();

                //if (list.Count==0)
                //    FirstElement.SetValue( new MutableObject() { { "**Implicit Schema**", new GroupSplitter.ImplicitSchemaIndicator(this)}}, mutable );
                //else
                FirstElement.SetValue( list.First(), entry );

            }
            return mutable;
        }
    }
}
