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

using Assets.Utility;
using JetBrains.Annotations;
using Utility;
using Visualizers;

namespace Mutation.Mutators
{
    [UsedImplicitly]
    public class TrimStringStartMutator : DataMutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable( LabelText = "Scope" )]
        public MutableScope Scope { get { return m_Scope; } }


        private MutableField<string> m_StringToRemoveField = new MutableField<string>() { LiteralValue = "Foo" };
        [Controllable( LabelText = "String To Remove" )]
        private MutableField<string> StringToRemoveField { get { return m_StringToRemoveField; } }

        private MutableField<string> m_OriginalString = new MutableField<string>() { LiteralValue = "FooBar" };
        [Controllable( LabelText = "Original String" )]
        private MutableField<string> OriginalString { get { return m_OriginalString; } }


        private MutableTarget m_TargetString = new MutableTarget() { AbsoluteKey = "Formatted String" };
        [Controllable( LabelText = "Formatted String Target" )]
        private MutableTarget TargetString { get { return m_TargetString; } }


        public TrimStringStartMutator()
        {
            StringToRemoveField.SchemaPattern = Scope;
            OriginalString.SchemaParent = Scope;
            TargetString.SchemaParent = Scope;
        }


        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {
                var stringToRemove = StringToRemoveField.GetValue( entry );
                var stringToTrim = OriginalString.GetValue( entry );

                var trimmedString = stringToTrim.TrimStart( stringToRemove );

                TargetString.SetValue( trimmedString, entry );
            }

            return mutable;
        }
    }
}
