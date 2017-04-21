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

using Visualizers;

namespace Mutation.Mutators.DefaultValue
{
    public abstract class DefaultValueMutator<T> : DataMutator where T : struct
    {
        private MutableField<T> m_DefaultValue = new MutableField<T>() 
        { LiteralValue = default(T) };
        [Controllable(LabelText = "Default Value")]
        public MutableField<T> DefaultValue { get { return m_DefaultValue; } }


        private MutableTarget m_DefaultableField = new MutableTarget() { AbsoluteKey = "Output Value" };
        [Controllable(LabelText = "Defaultable Target")]
        public MutableTarget DefaultableField { get { return m_DefaultableField; } }

        public DefaultValueMutator()
        {
            DefaultValue.SchemaPattern = DefaultableField;
        } 

        protected override MutableObject Mutate( MutableObject mutable )
        {

            var extantDataField = new MutableField<T> { AbsoluteKey = DefaultableField.AbsoluteKey };
            
            foreach ( var entry in DefaultableField.GetEntries( mutable ) )
            {
                var useExtantValue =
                    MutableField<MutableObject>.CouldFieldResolveOnEntry(DefaultableField.AbsoluteKey, entry);

                DefaultableField.SetValue( 
                    useExtantValue?
                    extantDataField.GetValue(entry)
                    :DefaultValue.GetValue(entry), entry);
            }

            return mutable;
        }
    }
}
