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

namespace Adapters.TraceAdapters.Traces
{
    public class TraceHeader
    {
        public uint Magic { get; private set; }

        public uint Version { get; private set; }

        public ushort BinaryId { get; private set; }

        public ushort Flags { get; private set; }

        #region Factory
        public static TraceHeader ConstructSchemaHeader()
        {
            return new TraceHeader();
        }
        #endregion

        private TraceHeader()
        {
        }

        public TraceHeader( BinaryReader reader )
        {
            Magic = reader.ReadUInt32();

            Version = reader.ReadUInt32();

            BinaryId = reader.ReadUInt16();

            Flags = reader.ReadUInt16();
        }

        public override string ToString()
        {
            return string.Format( "Trace Header:" +
                                  "\n\tMagic: {0}" +
                                  "\n\tVersion: {1}" +
                                  "\n\tBinaryId: {2}" +
                                  "\n\tFlags: {3}",
                                  Magic, Version, BinaryId, Flags );
        }
    }
}
