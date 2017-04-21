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
using System.Runtime.InteropServices;
using Mutation;
using Mutation.Mutators.Axes.ArrityTypeAxes;

namespace Visualizers.CsView.Texturing
{
    public class OpcodeHistogramToMutable : TypeConversionAxis<OpcodeHistogram, List<MutableObject>>
    {
        protected override List< MutableObject > ConversionFunc( OpcodeHistogram key, List<MutableObject> entry)
        {
            var opcodeList = new List<MutableObject>();

            foreach ( var opcode in key )
            {
                opcodeList.Add( new MutableObject
                {
                    {"Opcode", opcode.Key },
                    {"Frequency", opcode.Value.Frequency }
                } );
            }

            return opcodeList;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            var opcodeList = new List<MutableObject>();
            
            opcodeList.Add(new MutableObject
            {
                {"Opcode", "xor" },
                {"Frequency", 12 }
            });
            opcodeList.Add(new MutableObject
            {
                {"Opcode", "mov" },
                {"Frequency", 17 }
            });

            TargetField.SetValue( opcodeList, newSchema );

            Router.TransmitAllSchema( newSchema );
        }
    }
}
