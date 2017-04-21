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
using System.Linq;

namespace Utility
{
    public static class TypeExtensions
    {
        public static string GetGenericName( this Type type )
        {
            var friendlyName = type.Name;
            if ( !type.IsGenericType )
            {
                return type == typeof( Single ) ? "Float" : friendlyName;
            }

            var iBacktick = friendlyName.IndexOf( '`' );
            if ( iBacktick > 0 ) friendlyName = friendlyName.Remove( iBacktick );

            var genericParameters = type.GetGenericArguments().Select( x => x.GetGenericName() ).ToArray();
            friendlyName += "<" + string.Join( ", ", genericParameters ) + ">";

            return friendlyName;
        }

        public static string ShortAssemblyQualifiedName( this Type type )
        {
            return string.Format( "{0}, {1}", type.FullName, type.Assembly.GetName().Name );
        }
    }
}
