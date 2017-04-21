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
using System.Text;
using Mutation;
using Mutation.Mutators;
using Visualizers;

namespace Experimental
{
    public class ExperimentalRepeatingCounter : Mutator
    {
        private MutableField<int> m_Iterations = new MutableField<int>() 
        { LiteralValue = 10 };
        [Controllable(LabelText = "Number of Iterations")]
        public MutableField<int> Iterations { get { return m_Iterations; } }

        private MutableTarget m_Counter = new MutableTarget() 
        { AbsoluteKey = "Iteration Count" };
        [Controllable(LabelText = "Iteration Count")]
        public MutableTarget Counter { get { return m_Counter; } }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            Counter.SetValue(0, newSchema);

            Router.TransmitAllSchema(newSchema);
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            for ( int i = 0; i < Iterations.GetFirstValue( payload.Data ); i++ )
            {
                Counter.SetValue(i, payload.Data);

                var iterator = Router.TransmitAll(payload);
                while (iterator.MoveNext())
                    yield return null;
            }
        }
    }
}
