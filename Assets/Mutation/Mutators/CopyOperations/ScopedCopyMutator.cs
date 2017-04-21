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
using Visualizers;

namespace Mutation.Mutators.CopyOperations
{
    public class ScopedCopyMutator : DataMutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<Object> m_CopyValue = new MutableField<Object>() 
        { LiteralValue = "Value" };
        [Controllable(LabelText = "CopyValue")]
        public MutableField<Object> CopyValue { get { return m_CopyValue; } }

        private MutableTarget m_CopyTarget = new MutableTarget() 
        { AbsoluteKey = "Entries.Target" };
        [Controllable(LabelText = "Copy Target")]
        public MutableTarget CopyTarget { get { return m_CopyTarget; } }

        public void Start()
        {
            CopyValue.SchemaParent = Scope;
            CopyTarget.SchemaParent = Scope;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {
                var foundValue = CopyValue.GetFirstValueBelowArrity( entry );

                CopyTarget.SetValue( foundValue, entry );
            }

            return mutable;
        }
    }
}
