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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chains;
using Visualizers;

namespace Mutation.Mutators.Enumeration
{
    public class IntCsvOrRangeEnumerator : ChainNode
    {
        private readonly MutableField< string > m_CommaSeparatedValues = new MutableField< string >()
        {
            LiteralValue = "1..6"
        };

        [Controllable( LabelText = "Numeric Csv or ..Range" )]
        public MutableField< string > CommaSeparatedValues
        {
            get { return m_CommaSeparatedValues; }
        }

        private readonly MutableTarget m_SingleEntryTarget = new MutableTarget() { AbsoluteKey = "Element" };

        [Controllable( LabelText = "Single Element Target" )]
        public MutableTarget SingleEntryTarget
        {
            get { return m_SingleEntryTarget; }
        }

        private readonly MutableTarget m_NumberOfElementsTarget = new MutableTarget()
        {
            AbsoluteKey = "Number Of Elements"
        };

        [Controllable( LabelText = "Number Of Elements Target" )]
        public MutableTarget NumberOfElementsTarget
        {
            get { return m_NumberOfElementsTarget; }
        }

        public SelectionState PerEntryState
        {
            get { return Router[ "Per Element" ]; }
        }



        public IntCsvOrRangeEnumerator()
        {
            SingleEntryTarget.SchemaParent = CommaSeparatedValues;

            Router.AddSelectionState( "Per Element" );
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            var csv = CommaSeparatedValues.GetFirstValue( newSchema );
            int csvFirst=0;
            if (!int.TryParse(csv, out csvFirst) ) {
                csvFirst = csv.Contains( ',' )
                ? int.Parse( csv.Split( ',' ).First() )
                : int.Parse( csv.Split( '.' ).First() );
            }

            SingleEntryTarget.SetValue( csvFirst, newSchema );
            NumberOfElementsTarget.SetValue( 1, newSchema );

            Router.TransmitAllSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach ( var entry in CommaSeparatedValues.GetEntries( payload.Data ) )
            {
                string csvString = CommaSeparatedValues.GetValue( entry );

                if ( string.IsNullOrEmpty( csvString ) )
                    yield break;

                var values = SplitCsv( csvString );

                NumberOfElementsTarget.SetValue( values.Count, entry );

                foreach ( var value in values )
                {
                    SingleEntryTarget.SetValue( value, entry );

                    var iterator = PerEntryState.Transmit( payload );
                    while ( iterator.MoveNext() )
                        yield return null;
                }
            }
        }

        private List< int > SplitCsv( string csvString )
        {

            int csvFirst = 0;
            if ( int.TryParse( csvString, out csvFirst ) )
            {
                return new List< int >{ csvFirst};
            }

            if ( csvString.Contains( ',' ) )
            {
                return (from str in csvString.Split(',') select int.Parse( str )).ToList();
            }

            var splitPosition = csvString.IndexOf( "..", StringComparison.InvariantCultureIgnoreCase);

            var startIndex = int.Parse(csvString.Substring( 0, splitPosition ).Trim());
            var endIndex = int.Parse( csvString.Substring(splitPosition + 2 ).Trim() );

            return Enumerable.Range( startIndex, endIndex - startIndex + 1 ).ToList();
        }
    }
}
