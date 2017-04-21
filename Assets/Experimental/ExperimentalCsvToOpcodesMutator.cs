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
using System.Linq;
using Mutation;
using Mutation.Mutators;
using UnityEngine;
using Visualizers;
using Visualizers.CsView.Texturing;

namespace Experimental
{
    public class ExperimentalCsvToOpcodesMutator : Mutator
    {
        private MutableField<string> m_OpcodeCsv = new MutableField<string>() 
        { LiteralValue = "op1, op2, op3, op4" };
        [Controllable(LabelText = "Opcode Csv")]
        public MutableField<string> OpcodeCsv { get { return m_OpcodeCsv; } }

        private MutableField<string> m_FrequencyCsv = new MutableField<string>() 
        { LiteralValue = "41,31,63,82" };
        [Controllable(LabelText = "Frequency Csv")]
        public MutableField<string> FrequencyCsv { get { return m_FrequencyCsv; } }

        private MutableTarget m_OpcodeTarget = new MutableTarget() 
        { AbsoluteKey = "Opcode Histogram" };
        [Controllable(LabelText = "Opcode Target")]
        public MutableTarget OpcodeTarget { get { return m_OpcodeTarget; } }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            OpcodeTarget.SetValue( new OpcodeHistogram(), newSchema );
            
            Router.TransmitAllSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var opcodes = new OpcodeHistogram();

            var opcodeStrings = OpcodeCsv.GetFirstValue( payload.Data ).Split( ',' );

            var opcodeFrequencies = (from f in FrequencyCsv.GetFirstValue( payload.Data ).Split( ',' ) select int.Parse(f.Trim())).ToArray();

            for (int i=0; i<Mathf.Min(opcodeStrings.Length, opcodeFrequencies.Count()); i++)
                opcodes.Add(opcodeStrings[i], new OpcodePair( opcodeFrequencies[i], opcodeStrings[i] ) );
            
            OpcodeTarget.SetValue( opcodes, payload.Data );

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
