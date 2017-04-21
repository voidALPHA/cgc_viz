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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Windows.Forms;

// Test comment to test auto-build

namespace Utility.DevCommand
{
    public class DevCommandCmdLineArg : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "cla";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Command line argument control";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Option m_AddOption = new DevCommandManager.Option
            ("add", "Add one or more command line arguments (up to ten at a time)", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), true ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false )
            });
        [DevCommandOptionAttribute]
        public DevCommandManager.Option AddOption { get { return m_AddOption; } set { m_AddOption = value; } }

        private DevCommandManager.Option m_DeleteOption = new DevCommandManager.Option
            ("delete", "Delete all added command line arguments that contain the match string provided (cannot delete passed command line arguments)", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), true )
            });
        [DevCommandOptionAttribute]
        public DevCommandManager.Option DeleteOption { get { return m_DeleteOption; } set { m_DeleteOption = value; } }

        private DevCommandManager.Option m_ReplaceOption = new DevCommandManager.Option
            ("replace", "Replace all added command line arguments that contain the match string provided with the (single) command line argument provided (cannot replace passed command line arguments) (up to ten at a time)", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), true ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false ),
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), false )
            });
        [DevCommandOptionAttribute]
        public DevCommandManager.Option ReplaceOption { get { return m_ReplaceOption; } set { m_ReplaceOption = value; } }

        private DevCommandManager.Option m_DeleteAllOption = new DevCommandManager.Option("deleteAll", "Delete all of the added command line arguments");
        [DevCommandOptionAttribute]
        public DevCommandManager.Option DeleteAllOption { get { return m_DeleteAllOption; } set { m_DeleteAllOption = value; } }

        private DevCommandManager.Option m_DumpOption = new DevCommandManager.Option("dump", "Dump all of the command line arguments");
        [DevCommandOptionAttribute]
        public DevCommandManager.Option DumpOption { get { return m_DumpOption; } set { m_DumpOption = value; } }

        private DevCommandManager.Option m_DumpToClipboardOption = new DevCommandManager.Option("dumpToClipboard", "Dump all added command line arguments to clipboard, formatting with leading hyphens");
        [DevCommandOptionAttribute]
        public DevCommandManager.Option DumpToClipboardOption { get { return m_DumpToClipboardOption; } set { m_DumpToClipboardOption = value; } }

        private DevCommandManager.Option m_AddFromClipboardOption = new DevCommandManager.Option("addFromClipboard", "Add command line arguments from text on the clipboard");
        [DevCommandOptionAttribute]
        public DevCommandManager.Option AddFromClipboardOption { get { return m_AddFromClipboardOption; } set { m_AddFromClipboardOption = value; } }

        private DevCommandManager.Option m_CopyFromClipboardOption = new DevCommandManager.Option("copyFromClipboard", "Add command line arguments from text on the clipboard, replacing all previously-added command line arguments");
        [DevCommandOptionAttribute]
        public DevCommandManager.Option CopyFromClipboardOption { get { return m_CopyFromClipboardOption; } set { m_CopyFromClipboardOption = value; } }

        private DevCommandManager.Option m_TestOption = new DevCommandManager.Option
            ("test", "For testing", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), true )
            });
        [DevCommandOptionAttribute]
        public DevCommandManager.Option TestOption { get { return m_TestOption; } set { m_TestOption = value; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);
        }

        public bool Execute()
        {
            if ( AddOption.IsPresent )
            {
                foreach ( DevCommandManager.Argument arg in AddOption.Arguments.Where( arg => arg.IsPresent ) )
                    AddCLA((string)arg.Value);
            }

            if ( DeleteOption.IsPresent )
            {
                DeleteMatchingCLAs( (string)DeleteOption.Arguments[0].Value );
            }

            if ( DeleteAllOption.IsPresent )
            {
                var numDeleted = CommandLineArgs.DeleteAllCommandLineArguments();
                Debug.Log("Deleted all " + numDeleted + " added command line arguments" );
            }

            if ( ReplaceOption.IsPresent )
            {
                var newCla = (string)ReplaceOption.Arguments[ 0 ].Value;
                var equalSignIndex = newCla.IndexOf('=');
                var oldKey = equalSignIndex < 0 ? newCla : newCla.Substring(0, equalSignIndex);
                DeleteMatchingCLAs( oldKey );
                AddCLA(newCla);
            }

            if ( DumpOption.IsPresent )
            {
                var args = CommandLineArgs.GetCommandLineArguments();

                foreach ( var arg in args )
                    Debug.Log( arg );

                Debug.Log(args.Count + " command line arguments total");
            }

            // This is meant for copying from VGS web interface 'parameters' box
            if ( AddFromClipboardOption.IsPresent || CopyFromClipboardOption.IsPresent )
            {
                if ( CopyFromClipboardOption.IsPresent )
                    CommandLineArgs.DeleteAllCommandLineArguments();

                var allText = Clipboard.GetText();
                if ( !string.IsNullOrEmpty( allText ) )
                {
                    var text = new string(allText.TakeWhile( c => c != '\n').ToArray());   // Just take the first line of text
                    Debug.Log("Parsing this text from clipboard: " + text );
                    //       example:  -ShowPolls=false -title="SCORES BY ROUND" -isVGSJob -foo=Bar
                    // translates to:   ShowPolls=false  title=SCORES BY ROUND    isVGSJob  foo=Bar
                    while ( true )
                    {
                        text = text.Trim();
                        var hyphenIndex = text.IndexOf('-');
                        if ( hyphenIndex < 0 )
                        {
                            if ( !string.IsNullOrEmpty( text ) )
                            {
                                Debug.Log("Warning: Ignoring extra text: " + text );
                            }
                            break;
                        }
                        if ( hyphenIndex > 0 )
                        {
                            Debug.Log("Warning: Ignoring extra text: " + text.Substring( 0, hyphenIndex ));
                        }
                        text = text.Substring( hyphenIndex + 1);

                        // Hyphen found; now look for the next equals sign, OR space (for a CLA with no value)
                        var equalsIndex = text.IndexOf( '=' );
                        var spaceIndex = text.IndexOf( ' ' );
                        if ( equalsIndex < 0 && spaceIndex < 0 ) // Neither
                        {
                            AddCLA( text );
                            break;
                        }
                        if ( (equalsIndex < 0) ||       // Space only
                             (spaceIndex >= 0 && spaceIndex < equalsIndex))    // Both space and equals were found
                        {
                            // Found a CLA with no value (e.g. -isVGSJob)
                            AddCLA( text.Substring( 0, spaceIndex ) );
                            text = text.Substring( spaceIndex + 1 );
                            continue;
                        }
                        // There is an equals, and it came before any space.  Now look for quotes in the value
                        var quoteIndex = text.IndexOf( '"' );
                        var onePastValueIndex = spaceIndex;
                        if ( quoteIndex >= 0 && quoteIndex == equalsIndex + 1 )
                        {
                            var nextQuoteIndex = text.IndexOf( '"', quoteIndex + 1 );
                            if ( nextQuoteIndex < 0 )
                            {
                                Debug.Log("Error parsing CLAs from clipboard: missing closing quote after " + text );
                                return false;
                            }
                            // Remote the quotes
                            text = text.Substring( 0, quoteIndex ) + text.Substring( quoteIndex + 1, ( nextQuoteIndex - quoteIndex ) - 1 ) + text.Substring( nextQuoteIndex + 1 );
                            onePastValueIndex = nextQuoteIndex - 2 + 1;
                        }
                        if (onePastValueIndex < 0)
                        {
                            AddCLA( text );
                            break;
                        }
                        AddCLA(text.Substring(0, onePastValueIndex));
                        if (onePastValueIndex < text.Length - 1)
                            text = text.Substring(onePastValueIndex + 1);
                        else
                            break;
                    }
                }
                else
                {
                    Debug.Log("Error: Clipboard did not contain text");
                    return false;
                }
            }

            if ( DumpToClipboardOption.IsPresent )    // This is meant for copy TO VGS web interface 'parameters' box
            {
                var text = "";
                foreach ( var cla in CommandLineArgs.AddedCommandLineArgs )
                {
                    var formattedCla = cla;
                    var spaceIndex = cla.IndexOf( ' ' );
                    if (spaceIndex >= 0)
                    {
                        var equalsIndex = cla.IndexOf( '=' );
                        if ( equalsIndex >= 0 )
                        {
                            formattedCla = cla.Substring( 0, equalsIndex + 1 ) + '"' + cla.Substring( equalsIndex + 1, (cla.Length - equalsIndex) - 1 ) + '"';
                        }
                        else
                            Debug.Log("Error: Command line argument has a space but not an equals sign");   // This should not happen
                    }
                    text += " -" + formattedCla;
                }
                if ( string.IsNullOrEmpty( text ) )
                    text = "(No parameters)";

                Clipboard.SetText(text);
                Debug.Log("Copied this string to Windows clipboard: " + text);
            }

            if ( TestOption.IsPresent )
            {
                var value = CommandLineArgs.GetArgumentValue( (string)TestOption.Arguments[ 0 ].Value );
                Debug.Log("Value found: " + value );
            }

            return true;
        }

        private void AddCLA( string cla )
        {
            CommandLineArgs.AddCommandLineArgument(cla);
            Debug.Log("Added command line argument: \"" + cla + "\"");
        }

        private void DeleteMatchingCLAs( string match )
        {
            var numDeleted = CommandLineArgs.DeleteCommandLineArgumentsContaining(match);
            Debug.Log("Deleted " + numDeleted + " command line arguments that matched: \"" + match + "\"");
        }
    }
}
