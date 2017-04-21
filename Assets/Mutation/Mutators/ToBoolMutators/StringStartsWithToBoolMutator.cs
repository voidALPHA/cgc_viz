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
using JetBrains.Annotations;
using Visualizers;

namespace Mutation.Mutators.ToBoolMutators
{
    [UsedImplicitly]
    public class StringStartsWithToBoolMutator : ToBoolMutator
    {
        private MutableField<string> m_PrefixField = new MutableField<string>() { LiteralValue = "Foo" };
        [Controllable( LabelText = "Prefix To Test For" )]
        private MutableField<string> PrefixField { get { return m_PrefixField; } }

        private MutableField<string> m_OriginalString = new MutableField<string>() { LiteralValue = "FooBar" };
        [Controllable( LabelText = "Original String" )]
        private MutableField<string> OriginalString { get { return m_OriginalString; } }

        public StringStartsWithToBoolMutator()
        {
            PrefixField.SchemaPattern = Scope;
            OriginalString.SchemaParent = Scope;
        }

        protected override bool GetBool( List< MutableObject > entry )
        {
            var prefix = PrefixField.GetValue( entry );
            var stringToTest = OriginalString.GetValue( entry );

            return stringToTest.StartsWith( prefix );
        }
    }
}
