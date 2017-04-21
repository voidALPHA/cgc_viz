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

namespace Utility
{
    // Reference: http://www.sanity-free.com/146/crc8_implementation_in_csharp.html
    // Reference: http://stackoverflow.com/questions/472906/converting-a-string-to-byte-array-without-using-an-encoding-byte-by-byte

    public static class CheckSumUtil
    {
        public static byte CheckSumFromStringAndType( string str, Type T )
        {
            return ComputeChecksum(BytesFromString (str + ": " + T.ToString()));
        }

        public static byte[] BytesFromString(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private static byte[] table = new byte[256];
        // x8 + x7 + x6 + x4 + x2 + 1
        private const byte poly = 0xd5;

        public static byte ComputeChecksum( byte[] bytes )
        {
            byte crc = 0;
            if ( bytes != null && bytes.Length > 0 )
            {
                foreach ( byte b in bytes )
                {
                    crc = table[ crc ^ b ];
                }
            }
            return crc;
        }

        static CheckSumUtil()
        {
            for ( int i = 0; i < 256; ++i )
            {
                int temp = i;
                for ( int j = 0; j < 8; ++j )
                {
                    if ( ( temp & 0x80 ) != 0 )
                    {
                        temp = ( temp << 1 ) ^ poly;
                    }
                    else
                    {
                        temp <<= 1;
                    }
                }
                table[ i ] = (byte) temp;
            }

        }
    }
}
