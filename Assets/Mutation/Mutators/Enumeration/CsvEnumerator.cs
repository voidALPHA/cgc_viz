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
using Chains;
using Visualizers;

namespace Mutation.Mutators.Enumeration
{
    public class CsvEnumerator : ChainNode
    {
        private readonly MutableField<string> m_CommaSeparatedValues = new MutableField<string>() { LiteralValue = "Value1,Value2,Value3" };
        [Controllable(LabelText = "Comma-Separated Values")]
        public MutableField<string> CommaSeparatedValues { get { return m_CommaSeparatedValues; } }

        private readonly MutableTarget m_SingleEntryTarget = new MutableTarget() { AbsoluteKey = "Element" };
        [Controllable(LabelText = "Single Element Target")]
        public MutableTarget SingleEntryTarget { get { return m_SingleEntryTarget; } }

        private readonly MutableTarget m_NumberOfElementsTarget = new MutableTarget() { AbsoluteKey = "Number Of Elements" };
        [Controllable(LabelText = "Number Of Elements Target")]
        public MutableTarget NumberOfElementsTarget { get { return m_NumberOfElementsTarget; } }

        public SelectionState PerEntryState { get { return Router["Per Element"]; } }



        public CsvEnumerator()
        {
            SingleEntryTarget.SchemaParent = CommaSeparatedValues;

            Router.AddSelectionState("Per Element");
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            SingleEntryTarget.SetValue( "", newSchema );
            NumberOfElementsTarget.SetValue( 0, newSchema );
            
            Router.TransmitAllSchema(newSchema);
        }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            foreach ( var entry in CommaSeparatedValues.GetEntries( payload.Data ) )
            {
                string csvString = CommaSeparatedValues.GetValue( entry );

                if ( string.IsNullOrEmpty( csvString ) )
                    yield break;

                var values = csvString.Split( ',' );

                NumberOfElementsTarget.SetValue( values.Length, entry );

                foreach ( var value in values )
                {
                    SingleEntryTarget.SetValue( value, entry );

                    var iterator = PerEntryState.Transmit( payload );
                    while ( iterator.MoveNext() )
                        yield return null;
                }
            }
        }
    }
}
