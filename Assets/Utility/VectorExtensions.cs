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

using UnityEngine;

namespace Utility
{
    public static class VectorExtensions
    {
        private const float Epsilon = .0000001f;

        public static float[] GetComponents(this Vector3 vect)
        {
            return new[] {vect.x, vect.y, vect.z};
        }

        public static Vector3 PiecewiseMultiply(this Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x*v2.x, v1.y*v2.y, v1.z*v2.z);
        }

        public static Vector4 ConvertToVector4(this Vector3 vector)
        {
            return new Vector4(vector.x, vector.y, vector.z, 0);
        }

        public static Vector3 ConvertToVector3(this Vector4 vector)
        {
            return new Vector3(vector.x, vector.y, vector.z);
        }

        public static Vector3 MinAtEpsilon( this Vector3 vector )
        {
            return new Vector3(
                Mathf.Abs(vector.x) < Epsilon ? Epsilon : vector.x,
                Mathf.Abs(vector.y) < Epsilon ? Epsilon : vector.y,
                Mathf.Abs(vector.z) < Epsilon ? Epsilon : vector.z
            );
        }
    }
}