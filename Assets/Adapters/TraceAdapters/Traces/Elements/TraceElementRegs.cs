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

using System.IO;
using System.Linq;

namespace Adapters.TraceAdapters.Traces.Elements
{
    public class TraceElementRegs
    {
        //eip, eax, ecx, edx, ebx, esp, ebp, esi, edi
        public uint Eip { get; private set; }

        public uint Eax { get; private set; }

        public uint Ecx { get; private set; }

        public uint Edx { get; private set; }

        public uint Ebx { get; private set; }

        public uint Esp { get; private set; }

        public uint Ebp { get; private set; }

        public uint Esi { get; private set; }

        public uint Edi { get; private set; }

        public static TraceElementRegs Generate( BinaryReader reader )
        {
            var traceElementRegs = new TraceElementRegs
            {
                Eip = reader.ReadUInt32(),
                Eax = reader.ReadUInt32(),
                Ecx = reader.ReadUInt32(),
                Edx = reader.ReadUInt32(),
                Ebx = reader.ReadUInt32(),
                Esp = reader.ReadUInt32(),
                Ebp = reader.ReadUInt32(),
                Esi = reader.ReadUInt32(),
                Edi = reader.ReadUInt32()
            };

            return traceElementRegs;
        }

        public static uint SizeInBytes { get { return sizeof(uint) * 9; } }

        public override string ToString()
        {
            var str = "\tTrace Register Values:";

            var count = 0;
            foreach ( var p in GetType().GetProperties().Where( p => p.Name != "SizeInBytes") )
            {
                if ( count%3 == 0 )
                    str += "\n";

                str += string.Format( "\t\t{0}:{1}", p.Name, p.GetValue( this, null ) );
                
                count++;
            }

            return str;
        }
    }
}
