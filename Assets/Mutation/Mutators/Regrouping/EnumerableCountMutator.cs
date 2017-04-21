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

namespace Mutation.Mutators.Regrouping
{
    public abstract class EnumerableCountMutator<T> : DataMutator
    {
        private MutableScope m_Scope = new MutableScope() {AbsoluteKey = ""};
        [Controllable(LabelText = "Meta Scope")]
        public MutableScope Scope { get { return m_Scope; } }
        
        private MutableField<IEnumerable<T> > m_EnumerableElements = new MutableField<IEnumerable<T> >() 
        { AbsoluteKey = "Entries" };
        [Controllable(LabelText = "EnumerableElements")]
        public MutableField<IEnumerable<T> > EnumerableElements { get { return m_EnumerableElements; } }

        
        private MutableTarget m_ElementCountTarget = new MutableTarget() 
        { AbsoluteKey = "Element Count" };
        [Controllable(LabelText = "Element Count Target")]
        public MutableTarget ElementCountTarget { get { return m_ElementCountTarget; } }

        public EnumerableCountMutator()
        {
            EnumerableElements.SchemaParent = Scope;
            ElementCountTarget.SchemaParent = Scope;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {
                int count = 0;
                foreach ( var subEntry in EnumerableElements.GetEntries( entry ) )
                    count += EnumerableElements.GetValue( subEntry ).Count();
                ElementCountTarget.SetValue( count, entry );
            }

            return mutable;
        }
    }
}
