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
using System.Reflection;
using Assets.Utility;
using UnityEngine;

namespace Utility.DevCommand
{
    public class DevCommandManager : MonoBehaviour
    {
        public static DevCommandManager Instance { get; private set; }

        public class Argument
        {
            public Argument(string name, Type argType, bool required, string helpText = "")
            {
                Name = name;
                HelpText = helpText;
                ArgType = argType;
                IsRequired = required;
                Value = null;
                IsPresent = false;
            }

            public readonly string Name;      // (Optional...for documentation essentially)
            public readonly string HelpText;  // Optional help text for this argument
            public readonly Type ArgType;     // The intended type of the argument (string, int, etc.)
            public readonly bool IsRequired;  // Is this argument IsRequired

            public object Value;     // The parsed argument given at run time
            public bool IsPresent;
        }

        public class Option
        {
            public Option(string name, string helpText, List<Argument> argsForOption)
            {
                Name = name;
                HelpText = helpText;
                MutualGroupName = "";
                Arguments = argsForOption;
                IsPresent = false;
            }

            public Option(string name, string helpText = "", string mutualGroupName = "", List<Argument> argsForOption = null)
            {
                Name = name;
                HelpText = helpText;
                MutualGroupName = mutualGroupName;
                Arguments = argsForOption ?? new List<Argument>();
                IsPresent = false;
            }

            public readonly string Name;                 // Keyword to use to invoke this option
            public readonly string HelpText;             // Optional help text for this option
            public readonly string MutualGroupName;      // Group name for mutually exclusive options
            public readonly List<Argument> Arguments;    // Options can have arguments

            public bool IsPresent;
        }

        [SerializeField]
        private Dictionary<string, IDevCommand> m_DevCommands = new Dictionary<string, IDevCommand>();
        public Dictionary<string, IDevCommand> DevCommands { get { return m_DevCommands; } set { m_DevCommands = value; } }

        [SerializeField]
        private Queue<string> m_History = new Queue<string>();
        public Queue<string> History { get { return m_History; } set { m_History = value; } }

        [SerializeField]
        private int m_MaxHistoryItems = 500;
        private int MaxHistoryItems { get { return m_MaxHistoryItems; } set { m_MaxHistoryItems = value; } }

        [SerializeField]
        private Queue<string> m_QueuedDevCommands = new Queue<string>();
        private Queue<string> QueuedDevCommands { get { return m_QueuedDevCommands; } set { m_QueuedDevCommands = value; } }

        [SerializeField]
        private int m_MaxQueuedDevCommandsToRunPerFrame = 1;
        private int MaxQueuedDevCommandsToRunPerFrame { get { return m_MaxQueuedDevCommandsToRunPerFrame; } set { m_MaxQueuedDevCommandsToRunPerFrame = value; } }

        [SerializeField]
        private Queue<string> m_SavedQueuedDevCommands = new Queue<string>();
        private Queue<string> SavedQueuedDevCommands { get { return m_SavedQueuedDevCommands; } set { m_SavedQueuedDevCommands = value; } }

        [SerializeField]
        private float m_WaitTimeRemaining;
        public float WaitTimeRemaining { get { return m_WaitTimeRemaining; } set { m_WaitTimeRemaining = value; } }

        [SerializeField]
        private int m_WaitGateCount;
        private int WaitGateCount { get { return m_WaitGateCount; } set { m_WaitGateCount = value; } }

        private readonly Dictionary<string, Option> MutuallyExclusiveGroupsFound = new Dictionary<string, Option>();


        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (WaitTimeRemaining > 0.0f)
            {
                WaitTimeRemaining -= Time.deltaTime;
                if (WaitTimeRemaining > 0.0f)
                    return;

                WaitTimeRemaining = 0.0f;
            }

            var maxToRun = MaxQueuedDevCommandsToRunPerFrame;

            while (maxToRun > 0 && QueuedDevCommands.Count > 0 && WaitGateCount <= 0 && WaitTimeRemaining <= 0.0f)
            {
                var cmd = QueuedDevCommands.Dequeue( );
                RunDevCommand( cmd );

                maxToRun--;
            }
        }

        public void StartWait()
        {
            ++WaitGateCount;
        }

        public void EndWait()
        {
            if (WaitGateCount > 0)
                --WaitGateCount;
            else
                throw new Exception("Unbalanced start/end wait calls");
        }

        private bool GenerateArgSpec(IDevCommand command)
        {
            command.HelpTextFull = command.Name + " - " + command.HelpTextBrief + "\n";

            command.Arguments.Clear();
            command.Options.Clear();

            // NOTE:  Unfortunately, Microsoft says the order of members returned by "GetMembers" is indeterminate:  https://msdn.microsoft.com/en-us/library/424c79hc(v=vs.110).aspx
            // Currently I depend on the order they appear in the source file for 'order of arguments', at least for making optional arguments appear after IsRequired arguments.
            // potential solution:  Sort by 'IsRequired'.
            var argMembers = command.GetType()
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(e => Attribute.IsDefined(e, typeof(DevCommandArgumentAttribute)));

            foreach (var m in argMembers)
            {
                if (m is PropertyInfo)
                {
                    var propertyInfo = m as PropertyInfo;
                    var argInstance = propertyInfo.GetValue( command, null ) as Argument;
                    command.Arguments.Add(argInstance);

                    command.HelpTextFull += "   " + ( argInstance.IsRequired ? "" : "[" ) + ArgumentTextDescription(argInstance);
                    command.HelpTextFull += ( argInstance.IsRequired ? "" : "]" ) + "\n";

                    if (argInstance.HelpText.Length > 0)
                        command.HelpTextFull += "    " + argInstance.HelpText + "\n";
                }
                else
                {
                    Debug.Log( "Member found, not Property but " + m.MemberType + ": " + m.Name );
                    return false;
                }
            }

            var optionMembers = command.GetType()
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(e => Attribute.IsDefined(e, typeof(DevCommandOptionAttribute)));

            foreach (var m in optionMembers)
            {
                if (m is PropertyInfo)
                {
                    var propertyInfo = m as PropertyInfo;
                    var optionInstance = propertyInfo.GetValue(command, null) as Option;
                    command.Options.Add(optionInstance);

                    command.HelpTextFull += "   -" + optionInstance.Name;

                    foreach ( var optionArg in optionInstance.Arguments )
                    {
                        command.HelpTextFull += " " + ( optionArg.IsRequired ? "" : "[" ) + ArgumentTextDescription(optionArg);
                        command.HelpTextFull += ( optionArg.IsRequired ? "" : "]" );
                    }
                    command.HelpTextFull += "\n";

                    if (optionInstance.HelpText.Length > 0)
                        command.HelpTextFull += "      " + optionInstance.HelpText + "\n";

                    foreach (var optionArg in optionInstance.Arguments)
                        if (optionArg.HelpText.Length > 0)
                            command.HelpTextFull += "      Note on " + optionArg.Name + ": " + optionArg.HelpText + "\n";
                }
                else
                {
                    Debug.Log("Member found, not Property but " + m.MemberType + ": " + m.Name);
                    return false;
                }
            }

            return true;
        }

        public void RegisterDevCommand(IDevCommand devCmd)
        {
            if (!GenerateArgSpec( devCmd ))
                throw new Exception("Problem in dev command argument spec generation");

            if (!ValidateArgumentSpec( devCmd ))
                throw new Exception("Problem in dev command argument spec validation");

            var lowerCaseKey = devCmd.Name.ToLower( );

            if (DevCommands.ContainsKey( lowerCaseKey ))
                throw new Exception( "RegisterDevCommand:  Dev command " + lowerCaseKey + " already registered" );

            DevCommands[lowerCaseKey] = devCmd;
        }

        public void UnregisterDevCommand(IDevCommand devCmd)
        {
            var lowerCaseKey = devCmd.Name.ToLower( );

            if (!DevCommands.ContainsKey( lowerCaseKey ))
                throw new Exception( "UnregisterDevCommand:  Dev command " + lowerCaseKey + " not registered" );

            DevCommands.Remove( lowerCaseKey );
        }

        private void RunDevCommand(string cmdLine)
        {
            History.Enqueue( cmdLine );
            while (History.Count > MaxHistoryItems)
                History.Dequeue( );

            // Strip comments and whitespace at either end; convert tabs to spaces
            var stripped = cmdLine;

            var commentCharIndex = cmdLine.IndexOf( '#' );
            if (commentCharIndex >= 0)
                stripped = cmdLine.Remove( commentCharIndex );

            stripped = stripped.Trim( );
            stripped = stripped.Replace("\t", " ");

            // Recognize the command
            var spaceCharIndex = stripped.IndexOf( ' ' );
            var command = stripped;
            if (spaceCharIndex >= 0) // (No space char is fine because it might just be a command with no arguments or options)
                command = stripped.Substring( 0, spaceCharIndex );
            command = command.ToLower( );

            if (command.Length == 0)
                return; // For empty string, or all whitespace, with or without a comment, do nothing (and don't report as error)

            if (!DevCommands.ContainsKey( command ))
            {
                Debug.Log( "Dev command not recognized: \"" + command + "\"\n" );
                return;
            }

            // Strip the command away
            if (spaceCharIndex >= 0)
                stripped = stripped.Substring( spaceCharIndex ).Trim( );
            else
                stripped = string.Empty;

            // Use the dev command's specification of arguments and options to fill in its interpreted data, AND error-check
            var successForArgs = ParseDevCommandArguments( DevCommands[command], stripped );
            if (!successForArgs)
                return;

            // Execute the dev command
            var success = DevCommands[command].Execute( );
            if (!success)
                Debug.Log( "Error executing dev command \"" + cmdLine + "\"\n" );
        }

        public void QueueDevCommand(string cmdLine)
        {
            QueuedDevCommands.Enqueue( cmdLine );
        }

        public void SaveAndResetQueuedDevCommands()
        {
            SavedQueuedDevCommands = new Queue<string>( QueuedDevCommands );
            QueuedDevCommands.Clear( );
        }

        public void AppendSavedQueuedDevCommands()
        {
            foreach (var cmd in SavedQueuedDevCommands)
                QueueDevCommand( cmd );

            SavedQueuedDevCommands.Clear( );
        }


        private class Token
        {
            public Token( string tokenStr, bool wasInQuotes )
            {
                token = tokenStr;
                wasQuoted = wasInQuotes;
            }

            public readonly string token;
            public readonly bool wasQuoted;
        }

        private bool ParseDevCommandArguments(IDevCommand command, string rawArgsString)
        {
            foreach (var arg in command.Arguments)
            {
                arg.Value = null;
                arg.IsPresent = false;
            }

            foreach (var opt in command.Options)
            {
                opt.IsPresent = false;
                foreach (var optionArg in opt.Arguments)
                {
                    optionArg.Value = null;
                    optionArg.IsPresent = false;
                }
            }

            // Break the string into individual pieces (tokens), keeping quoted strings as one piece, and convert those preceded by "-" to lower case
            // Preserve spaces within quotes, when they appear any place within a space-delimited value, so:  ABC"Max Value"DEF   ...becomes...   ABCMax ValueDEF
            var tokens = new List<Token>( );
            var remainingString = rawArgsString;
            while (remainingString.Length > 0)
            {
                remainingString = remainingString.TrimStart( );
                if (remainingString.Length > 0)
                {
                    //    log ABC"This is good" -count 5
                    // String now starts with a non-space; Look for first quote (if any), and first space (if any)
                    var quoteIndex = remainingString.IndexOf( '"' );
                    var spaceIndex = remainingString.IndexOf( ' ' );
                    if ((quoteIndex >= 0) && 
                        (spaceIndex < 0 || quoteIndex < spaceIndex))
                    {
                        var endQuoteIndex = remainingString.IndexOf( '"', quoteIndex + 1 );
                        if (endQuoteIndex < 0)
                        {
                            Debug.Log("Error:  Mismatched quotes\n");
                            return false;
                        }
                        var sansQuotes = remainingString.Substring( 0, quoteIndex );    // Any characters before the first quote mark
                        sansQuotes += remainingString.Substring( quoteIndex + 1, ( endQuoteIndex - quoteIndex ) - 1 );  // Characters between the quotes (removing the quotes)
                        var endIndex = remainingString.IndexOf( ' ', endQuoteIndex );
                        if (endIndex < 0)
                            endIndex = remainingString.Length;
                        sansQuotes += remainingString.Substring(endQuoteIndex + 1, (endIndex - endQuoteIndex) - 1);   // Any characters after the second quote mark, up to a space or end of line
                        tokens.Add(new Token(sansQuotes, true));
                        if (endIndex >= (remainingString.Length - 1))
                            remainingString = string.Empty;
                        else
                            remainingString = remainingString.Substring(endIndex + 1, (remainingString.Length - endIndex) - 1);

                        continue;
                    }

                    if (spaceIndex < 0)
                        spaceIndex = remainingString.Length;
                    var nextWord = remainingString.Substring(0, spaceIndex);
                    if (nextWord.StartsWith( "-" ))
                        nextWord = nextWord.ToLower( );
                    tokens.Add( new Token(nextWord, false) );
                    remainingString = remainingString.Substring(spaceIndex, (remainingString.Length - spaceIndex));
                }
            }

            // Main arguments must be in spec order.  Options can be in any order...but options must appear after all the main arguments.
            //    Also, options can have their own arguments, which within themselves must appear in spec order.
            var mainArgsFound = 0;
            var optArgsFound = 0;
            Option curOption = null;
            string curOptionName = null;
            MutuallyExclusiveGroupsFound.Clear();

            foreach (var tokenItem in tokens)
            {
                var token = tokenItem.token;

                if (token.StartsWith( "-" ) && !tokenItem.wasQuoted) // Is it an option or an argument?
                {
                    // Option:
                    var optionText = token.Substring( 1 ); // Strip hypen
                    var matchList = command.Options.Where( e => e.Name.ToLower().Equals( optionText ) ).ToList();
                    if (matchList.Count > 0)
                    {
                        if (!AreAllRequiredOptionArgumentsPresent( curOption, curOptionName, optArgsFound ))    // Error-check for the PREVIOUS option, if any
                            return false;

                        curOption = matchList[0];
                        curOption.IsPresent = true;
                        curOptionName = token;

                        optArgsFound = 0;

                        if (!String.IsNullOrEmpty(curOption.MutualGroupName))
                        {
                            if (MutuallyExclusiveGroupsFound.ContainsKey(curOption.MutualGroupName))
                            {
                                Debug.Log("Error:  Option \"" + optionText + "\" cannot be used in the same command execution as option \"" + MutuallyExclusiveGroupsFound[curOption.MutualGroupName].Name + "\"; they are mutually exclusive");
                                return false;
                            }
                            MutuallyExclusiveGroupsFound[curOption.MutualGroupName] = curOption;
                        }
                    }
                    else
                    {
                        Debug.Log( "Error:  Option \"" + optionText + "\" not recognized\n" );
                        return false;
                    }
                }
                else
                {
                    if (curOption == null)
                    {
                        // Main argument:
                        if (!ParseArgument(token, command, command.Arguments, ref mainArgsFound))
                            return false;
                    }
                    else
                    {
                        // Option argument:
                        if (!ParseArgument(token, command, curOption.Arguments, ref optArgsFound))
                            return false;
                    }
                }
            }

            if (!AreAllRequiredOptionArgumentsPresent( curOption, curOptionName, optArgsFound ))
                return false;

            if (mainArgsFound < command.Arguments.Count)
                if (command.Arguments[mainArgsFound].IsRequired)
                {
                    Debug.Log( "Error:  Missing required main argument:  " + ArgumentTextDescription(command.Arguments[mainArgsFound]) + "\n" );
                    return false;
                }

            return true;
        }

        private bool ParseArgument(string token, IDevCommand command, List<Argument> args, ref int argsFound)
        {
            if (argsFound < args.Count)
            {
                args[argsFound].IsPresent = true;

                Type t = args[argsFound].ArgType;

                object value = null;
                if (token.StringToValueOfType( t, ref value, true ) == false)
                    return false;

                args[ argsFound ].Value = value;

                argsFound++;
            }
            else
            {
                Debug.Log( "Error:  Too many arguments (maximum " + args.Count + "), when \"" + token + "\" encountered\n" );
                return false;
            }
            return true;
        }

        private bool AreAllRequiredOptionArgumentsPresent(Option curOption, string curOptionName, int optArgsFound)
        {
            if (curOption != null)
                if (optArgsFound < curOption.Arguments.Count)
                    if (curOption.Arguments[optArgsFound].IsRequired)
                    {
                        Debug.Log("Error:  Missing required option argument:  " + ArgumentTextDescription(curOption.Arguments[optArgsFound]) + " for " + curOptionName + "\n");
                        return false;
                    }

            return true;
        }

        private string ArgumentTextDescription(Argument argument)
        {
            return "<" + argument.Name + ( argument.Name.Equals(string.Empty) ? "" : ":" )
                + SimpleTypeDescription(argument.ArgType) + ">";
        }

        private string SimpleTypeDescription(Type type) // Because we don't really want the fully qualified string returned from type's 'to string'
        {
            if (type == typeof (string))
                return "string";
            if (type == typeof(int))
                return "int";
            if (type == typeof(uint))
                return "uint";
            if (type == typeof(long))
                return "long";
            if (type == typeof(float))
                return "float";
            if ( type.IsEnum )
            {
                string text = "";
                var first = false;
                foreach ( var val in Enum.GetValues( type ) )
                {
                    if ( first )
                        text += "|";
                    else
                        first = true;
                    text += val;
                }

                return text;                
            }

            throw new Exception("SimpleTypeDescription: Invalid type encountered; need to add a case here?");
        }

        private bool ValidateArgumentSpec(IDevCommand command)
        {
            bool optionalArgFound = false;
            foreach (var arg in command.Arguments)
            {
                if (!arg.IsRequired)
                    optionalArgFound = true;
                else if (optionalArgFound)
                {
                    Debug.Log( "Error in argument specification for command \"" + command.Name + "\":  Any optional arguments must come AFTER non-optional arguments\n" );
                    return false;
                }
            }

            foreach (var option in command.Options)
            {
                optionalArgFound = false;
                foreach (var arg in option.Arguments)
                {
                    if (!arg.IsRequired)
                        optionalArgFound = true;
                    else if (optionalArgFound)
                    {
                        Debug.Log( "Error in argument specification for command \"" + command.Name + "\":  Any optional arguments must come AFTER non-optional arguments\n" );
                        return false;
                    }
                }
            }

            return true;
        }
    }

    // Summary documentation
    // 
    // A dev command is an interpreted string that gets routed to a registered dev command handler.
    // A dev command consists of the command name, followed by zero or more >main arguments<, followed by one or more >options<, all separated by spaces.
    // Arguments can be required or optional, depending on the specification of the particular dev command being used.
    // Arguments must appear in the same order as the dev command specification.
    // The dev command specification must list any optional arguments >after< non-optional arguments.
    // Arguments can be strings with no spaces, strings enclosed in quotes, integers, unsigned integers, long integers, or floats.
    // Options are always optional, are always preceded by a "-", and must always match the dev command's option's specification.  Example:  "-render"
    // Options must appear after the main arguments, but otherwise can appear in any order.
    //   (Note that order of execution of the options, however, is dependent on the code of the particular dev command being execcuted).
    // Options can have their own argument(s), as in "-count 5", or "-print Error 33"
    //
    // An argument can consist of a quoted string (that may contain spaces); in that case the quotes are removed but the enclosed string is preserved.
    // Everything is case-insensitive, with the exception that the case of strings enclosed in quotes are preserved.
    //
    // "Help" is a dev command itself, and its full specification is:  "help [<devCommand:string>] [-full]"
    // In the help nomenclature, square brackets indicate that something is optional (although options are always optional).
    // Angle brackets enclose an argument specification, which consists of a text description (which is just documentation), followed by a colon (:) character,
    //     followed by the type of the argument (e.g. string, int, etc.)
}
