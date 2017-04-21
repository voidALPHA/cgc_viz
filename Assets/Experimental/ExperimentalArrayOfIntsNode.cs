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
using System.Collections.Generic;
using Mutation;
using Mutation.Mutators;
using Visualizers;

namespace Experimental
{
    public class ExperimentalArrayOfIntsNode : DataMutator
    {
        private MutableField<int> m_NumberOfInts = new MutableField<int>() 
        { LiteralValue = 4 };
        [Controllable(LabelText = "Number Of Ints")]
        public MutableField<int> NumberOfInts { get { return m_NumberOfInts; } }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            newSchema[ "IntsList" ] = new List< MutableObject >
            {
                new MutableObject()
                {
                    { "Int Value", 1 }
                }
            };

            Router.TransmitAllSchema( newSchema );
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            var intsList = new List<MutableObject>();
            for ( int i = 0; i < NumberOfInts.GetFirstValue( mutable ); i++ )
            {
                intsList.Add( new MutableObject(){new KeyValuePair< string, object >("Int Value", i+1)});
            }

            mutable[ "IntsList" ] = intsList;

            return mutable;
        }
    }
}
