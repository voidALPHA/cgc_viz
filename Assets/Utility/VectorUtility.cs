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
    public static class VectorUtility
    {
        public static bool TryParseVector( string text, ref Vector3 outputVector )
        {
            // TODO: This could be more sophisticated...

            //Formatted thusly: (0.0,0.0,0.0)

            var stripped = text.Trim( '(', ')' );

            var tokens = stripped.Split( ',' );

            if ( tokens.Count() != 3 )
                return false;

            var values = new float[3];
            for ( int i = 0; i < 3; i++ )
            {
                if ( !float.TryParse( tokens[i], out values[i] ) )
                    return false;
            }

            outputVector = new Vector3( values[0], values[1], values[2] );

            return true;
        }
    }
}
