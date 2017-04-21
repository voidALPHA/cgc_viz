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

using System.Collections;
using System.Collections.Generic;
using Visualizers;

namespace Mutation.Mutators.DefaultValue
{
    public abstract class DefaultValueFromBoundsListMutator<T> : Mutator where T : struct
    {
        private MutableField<T> m_DefaultValue = new MutableField<T>() 
        { LiteralValue = default(T) };
        [Controllable(LabelText = "Default Value")]
        public MutableField<T> DefaultValue { get { return m_DefaultValue; } }
        
        private MutableField<IEnumerable<BoundingBox>> m_BoundsList = new MutableField<IEnumerable<BoundingBox>>() 
        { AbsoluteKey = "Bounds List"};
        [Controllable(LabelText = "Bounds List")]
        public MutableField<IEnumerable<BoundingBox>> BoundsList { get { return m_BoundsList; } }

        private MutableField<string> m_PerElementAbsoluteKey = new MutableField<string>() 
        { LiteralValue = "Variable Key" };
        [Controllable(LabelText = "Per Element Key")]
        public MutableField<string> PerElementAbsoluteKey { get { return m_PerElementAbsoluteKey; } }

        private MutableTarget m_DefaultableField = new MutableTarget() 
        { AbsoluteKey = "Output Value" };
        [Controllable(LabelText = "Defaultable Target")]
        public MutableTarget DefaultableField { get { return m_DefaultableField; } }

        public DefaultValueFromBoundsListMutator()
        {
            DefaultValue.SchemaPattern = DefaultableField;
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var extantDataField = new MutableField<T> { AbsoluteKey = PerElementAbsoluteKey.GetFirstValue( payload.Data ) };

            bool valueAssigned = false;

            foreach ( var entries in BoundsList.GetEntries( payload.Data ) )
            {
                var boundsList = BoundsList.GetValue( entries );

                foreach ( var bound in boundsList )
                {
                    var useExtantValue = extantDataField.ValidateKey( bound.Data );

                    if ( useExtantValue )
                    {
                        DefaultableField.SetValue(
                                extantDataField.GetLastKeyValue( bound.Data ), payload.Data);
                        valueAssigned = true;
                    }
                    if (valueAssigned)
                        break;
                }
                if (valueAssigned)
                    break;
            }

            if (!valueAssigned)
                DefaultableField.SetValue( DefaultValue.GetFirstValue( payload.Data ), payload.Data );

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            DefaultableField.SetValue( default(T), newSchema );

            Router.TransmitAllSchema( newSchema );
        }
    }
}
