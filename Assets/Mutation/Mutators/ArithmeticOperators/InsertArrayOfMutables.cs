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

namespace Mutation.Mutators.ArithmeticOperators
{
    public class InsertArrayOfMutables : Mutator
    {
        private MutableTarget m_ArrayTarget = new MutableTarget()
        { AbsoluteKey = "Entries" };
        [Controllable(LabelText = "Array Target")]
        public MutableTarget ArrayTarget { get { return m_ArrayTarget; } }

        private MutableField<int> m_NumberOfInts = new MutableField<int>() 
        { LiteralValue = 10 };
        [Controllable(LabelText = "Number Of Entries")]
        public MutableField<int> NumberOfInts { get { return m_NumberOfInts; } }

        public InsertArrayOfMutables()
        {
            NumberOfInts.SchemaPattern = ArrayTarget;
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach ( var entry in ArrayTarget.GetEntries( payload.Data ) )
            {
                var numberOfInts = NumberOfInts.GetValue( entry );

                var outMutable = new List<MutableObject>();

                for (int i=0; i<numberOfInts; i++)
                    outMutable.Add( new MutableObject() { { "Index", i}} );

                ArrayTarget.SetValue( outMutable, entry );
            }

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            foreach (var entry in ArrayTarget.GetEntries(newSchema))
            {
                var numberOfInts = 1;

                var outMutable = new List<MutableObject>();

                for (int i = 0; i < numberOfInts; i++)
                    outMutable.Add(new MutableObject() { { "Index", i } });

                ArrayTarget.SetValue(outMutable, entry);
            }
            
            base.OnProcessOutputSchema( newSchema );
        }
    }
}
