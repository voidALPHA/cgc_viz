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

    public class CopyValueIntoEntries : DataMutator
    {
        private MutableField<Object> m_CopyValue = new MutableField<Object>() 
        { LiteralValue = "Value" };
        [Controllable(LabelText = "Copy Value")]
        public MutableField<Object> CopyValue { get { return m_CopyValue; } }

        private MutableTarget m_CopyTarget = new MutableTarget() 
        { AbsoluteKey = "Entries.Target" };
        [Controllable(LabelText = "Copy Target Field")]
        public MutableTarget CopyTarget { get { return m_CopyTarget; } }

        public void Start()
        {
            CopyTarget.SchemaParent = CopyValue;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            var entries = CopyValue.GetEntries(mutable);

            foreach (var entry in entries)
                CopyTarget.SetValue( CopyValue.GetValue( entry ), entry );

            return mutable;
        }
    }
}
