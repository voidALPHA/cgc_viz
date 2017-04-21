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
using System.Text;
using UnityEngine;

namespace Mutation.Mutators.FilamentPositioning
{
    public class ValueToXYHilbertMutator : ValueToXYMutator
    {
        [SerializeField]
        private int m_HilbertDepth = 32;
        private int HilbertDepth { get { return m_HilbertDepth; } set { m_HilbertDepth = value; } }

        protected override Vector2 ComputeXYPosition(int index)
        {
            HilbertCurve.LongPoint hilbertPoint = HilbertCurve.DtoXY(HilbertDepth, index);
        
            return new Vector2(hilbertPoint.x, hilbertPoint.y);
        }
    }

    public class HilbertCurve
    {

        public class LongPoint
        {
            public long x;
            public long y;

            public LongPoint(long x, long y)
            {
                this.x = x;
                this.y = y;
            }
        }

        // reference: http://en.wikipedia.org/wiki/Hilbert_curve


        //convert (x,y) to d
        public static long XYtoD(long n, LongPoint point)
        {
            long rx, ry, s, d = 0;
            for (s = n / 2; s > 0; s /= 2)
            {
                rx = (point.x & s) > 0 ? 1 : 0;
                ry = (point.y & s) > 0 ? 1 : 0;
                d += s * s * ((3 * rx) ^ ry);
                Rot(s, point, rx, ry);
            }
            return d;
        }

        //convert d to (x,y)
        public static LongPoint DtoXY(long n, long d)
        {
            long rx, ry, s, t = d;

            var point = new LongPoint(0, 0);
            for (s = 1; s <= n; s *= 2)
            {
                rx = 1 & (t / 2);
                ry = 1 & (t ^ rx);
                Rot(s, point, rx, ry);
                point.x += s * rx;
                point.y += s * ry;
                t /= 4;
            }

            return point;
        }

        //rotate/flip a quadrant appropriately
        private static void Rot(long n, LongPoint point, long rx, long ry)
        {
            if (ry == 0)
            {
                if (rx == 1)
                {
                    point.x = n - 1 - point.x;
                    point.y = n - 1 - point.y;
                }

                //Swap x and y
                long t = point.x;
                point.x = point.y;
                point.y = t;
            }
        }
    }
}
