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
using System.Text;
using UnityEngine;

namespace Utility
{
    public static class ExceptionUtility
    {
        public static void LogException( string localMessage, Exception exception )
        {
            var builder = new StringBuilder( "<color=red>" + localMessage + "</color>" + "\n" );

            builder.Append( "<b>Exception message:</b>\n" );
            builder.Append( "<color=orange>" + exception.Message + "</color>\n" );

            builder.Append( "\n<b>Exception call stack:</b>\n" );
            var stripe = true;
            foreach ( var line in exception.StackTrace.Split( '\n' ) )
            {
                var colorString = stripe ? "<color=grey>" : "<color=white>";

                builder.Append( colorString + line + "</color>\n" );

                stripe = !stripe;
            }

            builder.Append( "\n<b>Call stack for this Debug.Log call:</b>\n" );

            Debug.Log( builder.ToString() );
        }
    }
}