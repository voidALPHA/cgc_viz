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

namespace Ui
{
    public struct Padding
    {
        public float Left { get; set; }

        public float Top { get; set; }

        public float Right { get; set; }

        public float Bottom { get; set; }

        public static Padding Uniform( float padding )
        {
            return new Padding { Bottom = padding, Left = padding, Right = padding, Top = padding };
        }

        public static Padding operator *( Padding p, float s )
        {
            return new Padding
            {
                Bottom = p.Bottom * s,
                Left = p.Left * s,
                Right = p.Right * s,
                Top = p.Top * s
            };
        }
    }
}
