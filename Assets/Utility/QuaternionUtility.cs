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

using System.Linq;
using UnityEngine;

namespace Assets.Utility
{
    public static class QuaternionUtility
    {
        public static bool TryParseQuaternion( string text, ref Quaternion outputQuaternion )
        {
            // TODO: This could be more sophisticated...

            //Formatted thusly: (0.0,0.0,0.0,1.0)

            var stripped = text.Trim( '(', ')' );

            var tokens = stripped.Split( ',' );

            if ( tokens.Count() != 4 )
                return false;

            var values = new float[4];
            for ( int i = 0; i < 4; i++ )
            {
                if ( !float.TryParse( tokens[i], out values[i] ) )
                    return false;
            }

            outputQuaternion = new Quaternion( values[0], values[1], values[2], values[3] );

            return true;
        }
    }
}
