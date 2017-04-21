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
using System.Collections.Generic;
using System.Linq;
using Visualizers.CsView;

namespace Utility
{
    public static class CsBitstreamUtil
    {
        public static int BoolListToInt( IEnumerable< bool > data )
        {
            int i = 0;
            return data.Sum(bit => (1 << i++) * (bit ? 1 : 0));
        }

        public static int ReadInt( this ICsBitStream bitstream, int bitsToRead=32 )
        {
            return BoolListToInt( bitstream.ReadBits( bitsToRead ) );
        }

        public static float ReadFloat( this ICsBitStream bitstream )
        {
            float[] floatTarget=new float[1];

            var bitData = bitstream.ReadBits( 32 ).ToArray();

            var byteData = new byte[4];

            for ( int i = 0; i < 4; i++ )
            {
                int byteValue = 0;
                for ( int b = 0; b < 8; b++ )
                    byteValue=(byteValue << 1)+(bitData[i*8+b]?1:0);
                byteData[ i ] = (byte)byteValue;
            }

            Buffer.BlockCopy( byteData,0, floatTarget,0,4);

            return floatTarget[ 0 ];
        }
    }
}
