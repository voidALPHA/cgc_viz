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
using System.Globalization;
using System.Text.RegularExpressions;
using Assets.Utility;
using UnityEngine;
using ColorUtility = Assets.Utility.ColorUtility;

namespace Utility
{
    public static class StringExtensions
    {
        public static bool Contains( this String self, string contains, StringComparison comparison )
        {
            return self.IndexOf( contains, comparison ) != -1;
        }

        public static string TrimStart( this String self, string toTrim )
        {
            if ( toTrim == null )
                return self;

            if ( !self.StartsWith( toTrim ) )
                return self;

            return self.Substring( toTrim.Length );
        }

        public static bool StringToValueOfType(this string text, Type t, ref object result, bool errorMessages = false)
        {
            if ( t == typeof( string ) )
            {
                result = text;
                return true;
            }

            if ( t == typeof(bool) )
            {
                bool v;
                if ( bool.TryParse( text, out v ) )
                {
                    result = v;
                    return true;
                }
                else
                {
                    if (errorMessages)
                        Debug.Log("Error:  Cannot convert argument \"" + text + "\" to bool\n");
                    return false;
                }
            }

            if (t == typeof(int))
            {
                int v;
                if ( Int32.TryParse( text, NumberStyles.Float, CultureInfo.InvariantCulture, out v ) )
                {
                    result = v;
                    return true;                    
                }
                else
                {
                    if (errorMessages)
                        Debug.Log("Error:  Cannot convert argument \"" + text + "\" to integer\n");
                    return false;
                }
            }

            if (t == typeof(uint))
            {
                uint v;
                if ( UInt32.TryParse( text, out v ) )
                {
                    result = v;
                    return true;
                }
                else
                {
                    if (errorMessages)
                        Debug.Log("Error:  Cannot convert argument \"" + text + "\" to unsigned integer\n");
                    return false;
                }
            }

            if (t == typeof(long))
            {
                long v;
                if ( long.TryParse( text, out v ) )
                {
                    result = v;
                    return true;
                }
                else
                {
                    if (errorMessages)
                        Debug.Log("Error:  Cannot convert argument \"" + text + "\" to long integer\n");
                    return false;
                }
            }

            if (t == typeof(float))
            {
                float v;
                if ( float.TryParse( text, NumberStyles.Float, CultureInfo.InvariantCulture, out v ) )
                {
                    result = v;
                    return true;
                }
                else
                {
                    if (errorMessages)
                        Debug.Log("Error:  Cannot convert argument \"" + text + "\" to float\n");
                    return false;
                }
            }

            if ( t == typeof( Color ) )
            {
                var colorValue = Color.magenta;
                if ( ColorUtility.TryParse( text, ref colorValue ) )
                {
                    result = colorValue;
                    return true;
                }
                else
                {
                    if (errorMessages)
                        Debug.Log("Error:  Cannot convert argument \"" + text + "\" to color\n");
                    return false;
                }
            }

            if ( t == typeof( Vector3 ) )
            {
                var vector3Value = new Vector3();
                if (VectorUtility.TryParseVector(text, ref vector3Value))
                {
                    result = vector3Value;
                    return true;
                }
                else
                {
                    if (errorMessages)
                        Debug.Log("Error:  Cannot convert argument \"" + text + "\" to vector3\n");
                    return false;
                }
            }

            if ( t == typeof( Quaternion ) )
            {
                var quaternionValue = new Quaternion();
                if ( QuaternionUtility.TryParseQuaternion( text, ref quaternionValue ) )
                {
                    result = quaternionValue;
                    return true;
                }
                else
                {
                    if (errorMessages)
                        Debug.Log("Error:  Cannot convert argument \"" + text + "\" to quaternion\n");
                    return false;
                }
            }

            if (t.IsEnum)
            {
                // No TryParse until .NET 4...so we have to handle exceptions
                object value = null;
                var valid = true;
                int temp;
                if (Int32.TryParse(text, out temp))    // Check to see if it's just a number...then fail it.  (Enum.Parse allows it!)
                    valid = false;
                else
                {
                    try
                    {
                        value = Enum.Parse(t, text, true);
                    }
                    catch (Exception)
                    {
                        valid = false;
                    }
                }

                if ( valid )
                {
                    result = value;
                    return true;
                }
                else
                {
                    if ( errorMessages )
                    {
                        Debug.Log("Error:  Cannot convert argument \"" + text + "\" to the required enum.  Valid enum values are:\n");
                        foreach (var val in Enum.GetValues(t))
                            Debug.Log("   " + val);
                    }
                    return false;
                }
            }

            Debug.LogError("Error:  Argument type not yet supported.  Assigning \"\"");

            result = "";
            return true;
        }

        //useful function from stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
        public static string ByteArrayToString( byte[] bytes )
        {
            string hex = BitConverter.ToString( bytes );
            return hex.Replace( "-", "" );
        }

        public static string ToCleanFilename( this string filename )
        {
            return Regex.Replace( filename, "[:*? \"<>|]", "_" );
        }
    }
}
