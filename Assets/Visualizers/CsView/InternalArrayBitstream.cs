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

namespace Visualizers.CsView
{
    public class InternalArrayBitstream : ICsBitStream
    {
        public List<bool> InternalData { get; set; }

        private bool m_AdvanceByBytes = false;
        public bool AdvanceByBytes
        {
            get { return m_AdvanceByBytes; }
            set { m_AdvanceByBytes = value; }
        }

        private int BitPointer { get; set; }

        public InternalArrayBitstream(IEnumerable<bool> data)
        {
            InternalData = data.ToList();
            BitPointer = 0;
        }

        public InternalArrayBitstream(IEnumerable<bool> data, bool advanceByBytes)
        {
            InternalData = data.ToList();
            AdvanceByBytes = advanceByBytes;
            BitPointer = 0;
        }


        public static InternalArrayBitstream GenerateBitStreamFromLetterNumbers(string input)
        {
            var bits = new List<bool>();
            foreach (var chr in input.ToCharArray())
            {
                var number = char.ToUpper(chr) - 64;

                var testNum = 1;

                for (int i = 0; i < 8; i++)
                {
                    bits.Add((number & testNum) > 0);
                    testNum = testNum << 1;
                }
            }

            return new InternalArrayBitstream(bits);
        }
        
        public static InternalArrayBitstream GenerateBitStreamFromInt(int number, int bitCount=32)
        {
            List<bool> bits = new List<bool>();

            var testNum = 1;

            for (int i = 0; i < bitCount; i++)
            {
                bits.Add((number & testNum) > 0);
                testNum = testNum << 1;
            }

            return new InternalArrayBitstream(bits);
        }

        public static InternalArrayBitstream GenerateBitStreamFromString(string input)
        {
            List<bool> bits = new List<bool>();

            foreach (var chr in input.ToCharArray())
            {
                var testNum = 1;

                for (int i = 0; i < 16; i++)
                {
                    bits.Add((chr & testNum) > 0);
                    testNum = testNum << 1;
                }
            }

            return new InternalArrayBitstream(bits);
        }

        public static InternalArrayBitstream GenerateBitStreamFromFloat( float input )
        {
            List<bool> bits = new List<bool>();

            float[] floatSource = {input};

            byte[] byteData = new byte[4];

            Buffer.BlockCopy( floatSource, 0, byteData, 0, 4 );

            for ( int i = 3; i >= 0; i--)
            {
                int byteValue = byteData[i];
                for ( int b = 7; b >= 0; b-- )
                {
                    bits.Add( ( byteValue & 1 ) == 1 );
                    byteValue = byteValue >> 1;
                }
            }

            return new InternalArrayBitstream( bits );
        }

        public IEnumerable<bool> ReadBits(int nBits)
        {
            for (int i = 0; i < nBits; i++)
            {
                var data = InternalData[BitPointer];

                AdvanceBitPointer(1);

                yield return data;
            }
            ToNextByte();
        }

        public bool ReadSequentialBit()
        {
            var data = InternalData[ BitPointer ];
            AdvanceBitPointer( 1 );
            return data;
        }

        public void AdvanceBitPointer(int positions)
        {
            BitPointer += positions;
            BitPointer = BitPointer % InternalData.Count;
        }

        public void ToNextByte()
        {
            if (!AdvanceByBytes)
                return;

            if (BitPointer % 8 != 0)
                BitPointer = ((BitPointer / 8 + 1) * 8) % InternalData.Count;
        }
        
    }
}
