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
using Debug = UnityEngine.Debug;

namespace Utility
{
    // This utility provides convenience for command line argument passing (which is really meant for standalone builds, but still works in the Unity Editor).
    // You can pass an argument with a setting, as in "-MySetting=Blah", or with no setting, as in "-MyOption".  The argument must appear prefixed with a hypen (-).
    // From code, call "IsPresent(string argName)" to tell if an argument is present.  E.g. IsPresent("MySetting");
    // Call "GetArgumentValue(string argName)" to get the argument; if not present, it returns an empty string.

    // You can have hyphens within the value, as in: -MySetting=-3
    // You can have hyphens within the key name, as in: -My-New-Setting=3
    // To have a value that has spaces in it, enclose the value in quote marks (which will be removed), as in: -LabelText="This is Score"
    // No support for values that you want quotes to end up in

    // You can now pass a comma-separated list of values for a command line argument, as in: -MySettings=Red,Green,Blue
    // In that case you can call GetCommandLineArgument to get a single string value ("Red,Green,Blue"), or GetCommandLineArgumentAsList to get a list of string values ("Red", "Green", "Blue")

    public static class CommandLineArgs
    {
        //private const char ArgumentDelimiter = '-';
        private const char AssignmentSeparator = '=';
        private const char Quote = '"';
        private const char Space = ' ';
        private const string SpaceAndHyphen = " -";

        private static List<string> m_PassedCommandLineArgs = new List<string>();
        private static List<string> PassedCommandLineArgs { get { return m_PassedCommandLineArgs; } set { m_PassedCommandLineArgs = value; } }         

        private static List<string> m_AddedCommandLineArgs = new List<string>();
        public static List<string> AddedCommandLineArgs { get { return m_AddedCommandLineArgs; } set { m_AddedCommandLineArgs = value; } }

        public static List< string > GetCommandLineArguments()
        {
            var allArgs = GetPassedCommandLineArgs();

            allArgs.AddRange( AddedCommandLineArgs );

            return allArgs;
        }

        public static void AddCommandLineArgument(string cmdLineArg)
        {
            AddedCommandLineArgs.Add( cmdLineArg ); // We're not prepending the '-' because we'd have to remove it in FindDashedCommandLineArgument below
        }

        public static int DeleteCommandLineArgumentsContaining(string cmdLineArgPattern)
        {
            var removed = AddedCommandLineArgs.RemoveAll( elem => elem.ToLower().Contains( cmdLineArgPattern.ToLower() ) );
            return removed;
        }

        public static int DeleteAllCommandLineArguments()
        {
            var numDeleted = AddedCommandLineArgs.Count();
            AddedCommandLineArgs.Clear();
            return numDeleted;
        }

        public static bool IsPresent(string argName)
        {
            string value = null;
            return FindDashedCommandLineArgument(argName, ref value);
        }

        public static string GetArgumentValue(string argName)
        {
            var value = string.Empty;
            FindDashedCommandLineArgument(argName, ref value);
            return value;
        }

        public static bool GetArgumentValue(string argName, out int value)
        {
            var valueString = GetArgumentValue(argName);

            return int.TryParse(valueString, out value);
        }

        public static List<string> GetArgumentValueAsList(string argName)
        {
            var values = new List<string>();

            var value = string.Empty;
            FindDashedCommandLineArgument( argName, ref value );
            if ( !string.IsNullOrEmpty( value ) )
            {
                values = value.Split( ',' ).ToList();
            }

            return values;
        }

        private static bool FindDashedCommandLineArgument( string argumentName, ref string value )
        {
            argumentName = argumentName.ToLower();

            // First, build a list of all command line arguments, stripping off the 'dash'
            var allArgs = GetPassedCommandLineArgs();
            allArgs.AddRange(AddedCommandLineArgs);

            //for (var i = 0; i < allArgs.Count; i++)
            //{
            //    var arg = allArgs[ i ];
            //    // To allow values with spaces, the caller puts quotes around the entire CLA, e.g. "-Foo=Bar Two Three"
            //    if ( arg.Length >= 3 && arg[ 0 ] == Quote && arg[ 1 ] == ArgumentDelimiter && arg[ arg.Length - 1 ] == Quote )
            //        allArgs[ i ] = arg.Substring( 1, arg.Length - 3 );
            //    else if (arg[0] == ArgumentDelimiter)
            //        allArgs[i] = arg.Substring(1);
            //}


            // Now search for a match of argument key
            for (var i = 0; i < allArgs.Count; i++)
            {
                var kvp = new List<string>(allArgs[i].Split(AssignmentSeparator));
                if (kvp[0].Trim().ToLower() == argumentName)
                {
                    if(kvp.Count > 1)
                    {
                        kvp.RemoveAt(0);
                        value = string.Join(AssignmentSeparator.ToString(), kvp.ToArray()).Trim();
                    }
                    return true;
                }
            }
            return false;
        }

        private static List< string > GetPassedCommandLineArgs()
        {
            // could be optimized by lazy initialization

            PassedCommandLineArgs.Clear();

            var input = Environment.CommandLine;

            // testing.  Be sure to test all the cases in the comments at the top of this file.
            //input = "E:\\CGC\\HAXXIS.EXE -key=\" -3\" -logFile 10017_20160202_343_log.txt -test=\"\" -zAxisLabelFormat=\"XXX YYY\" -gpu 0 -My-New-Setting=-34 -My-Option-On -RunOXnStartup=none.txt -HPName=E:\\blah.json -VideoFilename=blah.mp4 -isVGSJob -jodID=10017 -MSSID=733 -foo=bar -foo2=bar2";
            //Debug.Log("Raw command line: >" + input + "<");

            while ( input.Length > 0 )
            {
                // Search for a space followed by a hyphen:
                var delimIndex = input.IndexOf( SpaceAndHyphen );
                if ( delimIndex < 0 )
                    break;

                input = input.Substring( delimIndex + SpaceAndHyphen.Length );
                if ( input.Length == 0 )
                    break;

                var spaceIndex = input.IndexOf( Space );
                var equalsIndex = input.IndexOf( AssignmentSeparator );
                // If no space or equals, register this as a dashed CLA with no value, and we're done
                if ( spaceIndex < 0 && equalsIndex < 0 )
                {
                    PassedCommandLineArgs.Add( input );
                    break;
                }

                // If there is a space, and either no equals, or space comes before equals, register this as a dashed CLA with no value, swallow it, and continue
                if ( (spaceIndex >= 0 && (equalsIndex < 0 || spaceIndex < equalsIndex)) )
                {
                    PassedCommandLineArgs.Add( input.Substring( 0, spaceIndex ) );
                    input = input.Substring( spaceIndex );
                    continue;
                }

                if ( equalsIndex < 0 )
                    break;

                // Now we're at the point where we have a key, and an equals sign.  Is there a quote immediately following the equals sign?
                var quoteIndex = input.IndexOf( Quote );
                if ( quoteIndex >= 0 && quoteIndex == equalsIndex + 1 )
                {
                    input = input.Remove( quoteIndex, 1 );  // Remove the first quote
                    var secondQuoteIndex = input.IndexOf( Quote );
                    if ( secondQuoteIndex < 0 )
                    {
                        Debug.Log("Error in parsing command line arguments: Missing closing quote mark in assigned value");
                        break;  // Should there be better handling than this than just aborting?
                    }
                    PassedCommandLineArgs.Add( input.Substring( 0, secondQuoteIndex ) );
                    input = input.Substring( secondQuoteIndex + 1 );
                    continue;
                }

                // Equals sign but no quote right after it.  So swallow up to the next space
                if ( spaceIndex < 0 )
                {
                    PassedCommandLineArgs.Add( input ); // If no space left, it's the rest of the input, and we're done
                    break;
                }
                PassedCommandLineArgs.Add( input.Substring( 0, spaceIndex ) );
                input = input.Substring( spaceIndex );
            }

            return PassedCommandLineArgs;
        }
    }
}