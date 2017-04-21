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
using System.Globalization;

namespace Adapters.TraceAdapters.Instructions
{
    [Serializable]
    public class InstInstruction : Instruction
    {
        public InstInstructionNature Nature { get; private set; }

        private string m_Instruction;
        public string Instruction
        {
            get { return m_Instruction; }
            set
            {
                m_Instruction = value;

                if ( m_Instruction.StartsWith( "e8" ) ) // This won't work with all versions of call;  see http://x86.renejeschke.de/html/file_module_x86_id_26.html
                    Nature = InstInstructionNature.FunctionCall;
                else if ( m_Instruction.StartsWith( "c3" ) )
                    Nature = InstInstructionNature.FunctionReturn;
                else
                    Nature = InstInstructionNature.Other;
            }
        }

        public uint GetCalledAddress()
        {
            if ( Nature != InstInstructionNature.FunctionCall )
                throw new InvalidOperationException("Cannot get called address if instruction is not of nature 'call'.");

            var startIndex = Instruction.LastIndexOf( 'x' ) + 1;

            var stringAddress = Instruction.Substring( startIndex );

            uint uintAddress;

            if ( !uint.TryParse( stringAddress, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uintAddress ) )
                return 0;

            return uintAddress;
        }
    }
}
