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

namespace Adapters.TraceAdapters.Traces.Elements
{
    public class TraceElementHeader
    {
        public uint RawTypeIndex { get; private set; }

        public uint Length { get; private set; }

        public double WallTime { get; private set; }

        public int InstructionCount { get; private set; }
        
        public static TraceElementHeader Generate( BinaryReader reader )
        {
            var traceElementHeader = new TraceElementHeader
            {
                RawTypeIndex = reader.ReadUInt32(),
                Length = reader.ReadUInt32(),
                WallTime = reader.ReadDouble(),
                InstructionCount = (int)reader.ReadUInt64()
            };

            return traceElementHeader;
        }

        public static uint SizeInBytes { get { return sizeof(uint) + sizeof(uint) + sizeof(double) + sizeof(ulong); } }
        public TraceElementHeader()
        {
        }

        public TraceElementHeader( TraceElementHeader header )
        {
            header.RawTypeIndex = RawTypeIndex;
            header.Length = Length;
            header.WallTime = WallTime;
            header.InstructionCount = InstructionCount;
        }

        public TraceElementHeader( uint rawTypeIndex, uint length, double wallTime, int instructionCount )
        {
            RawTypeIndex = rawTypeIndex;
            Length = length;
            WallTime = wallTime;
            InstructionCount = instructionCount;
        }

        //public static uint SizeInBytes { get { return sizeof(uint) + sizeof(uint) + sizeof(double) + sizeof(ulong); } }

        public override string ToString()
        {
            return string.Format( "TraceElementHeader:" +
                                  "\n\tType:{0} Len:{1} Time:{2} InstCount:{3}", RawTypeIndex, Length, WallTime, InstructionCount );
        }
    }
}
