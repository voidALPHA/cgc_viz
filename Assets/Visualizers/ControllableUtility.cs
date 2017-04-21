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
using System.Reflection;

namespace Visualizers
{
    public static class ControllableUtility
    {
        public static string GetControllableLabel( MemberInfo memberInfo )
        {
            var first = memberInfo.GetCustomAttributes( true ).FirstOrDefault( a => a is ControllableAttribute );

            if ( first == null )
                return memberInfo.Name;

            var controllable = first as ControllableAttribute;

            if ( controllable == null )
                return memberInfo.Name;

            if ( string.IsNullOrEmpty( controllable.LabelText ) )
                return memberInfo.Name;

            return controllable.LabelText;
        }
    }
}