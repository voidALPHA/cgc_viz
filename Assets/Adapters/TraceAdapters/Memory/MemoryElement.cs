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
using System.IO;
using Adapters.TraceAdapters.Traces.Elements;
using UnityEngine;

namespace Adapters.TraceAdapters.Memory
{
    public abstract class MemoryElement : TraceElement
    {
        protected MemoryElement( TraceElementHeader header ) : base(header)
        {
        }
        
        public override string ToString()
        {
            return string.Format( GetType().Name +
                                  "\n{0}", Header );
        }

        
    }

    public class MemoryElementRead : MemoryElement
    {
        public uint Addr { get; private set; }

        public byte[] BytesRead { get; private set; }

        public int NumberOfBytesRead { get { return BytesRead.Length; } }

        public MemoryElementRead(TraceElementHeader header) : base(header)
        {
        }

        public static MemoryElementRead Generate(TraceElementHeader header, BinaryReader reader)
        {
            var memRead = new MemoryElementRead(header)
            {
                Addr = reader.ReadUInt32()
            };
            
            int bytesRemaining = (int)header.Length
                //-(int)TraceElementHeader.SizeInBytes
                -sizeof(uint);

            if ( bytesRemaining <= 0 )
                memRead.BytesRead = new byte[] { };
            else
                memRead.BytesRead = reader.ReadBytes( bytesRemaining );

            return memRead;
        }

        public int SizeInBytes { get { return sizeof(uint) + NumberOfBytesRead; } }
    }

    public class MemoryElementWrite : MemoryElement
    {
        public uint Addr { get; private set; }

        public byte[] BytesWritten { get; private set; }

        public int NumberOfBytesWritten { get { return BytesWritten.Length; } }

        public MemoryElementWrite(TraceElementHeader header) : base(header)
        {
        }

        public static MemoryElementWrite Generate(TraceElementHeader header, BinaryReader reader)
        {
            var memWrite = new MemoryElementWrite(header)
            {
                Addr = reader.ReadUInt32()
            };

            int bytesRemaining = (int)header.Length 
                //- ( (int)TraceElementHeader.SizeInBytes
                - sizeof(uint);

            if (bytesRemaining <= 0)
                memWrite.BytesWritten = new byte[] { };
            else
                memWrite.BytesWritten = reader.ReadBytes(bytesRemaining);

            return memWrite;
        }

        public int SizeInBytes { get { return sizeof(uint) + NumberOfBytesWritten; } }
    }
}
