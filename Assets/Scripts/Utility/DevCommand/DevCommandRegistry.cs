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
using UnityEngine;
using Microsoft.Win32;

namespace Utility.DevCommand
{
    public class DevCommandRegistry : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "registry";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Windows registry access";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Option m_DumpValueOption = new DevCommandManager.Option("dumpValue", "Dump the value of a specified registry key",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "keyNamePrefix", typeof (string), true ),
                new DevCommandManager.Argument( "keyName", typeof (string), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option DumpValueOption { get { return m_DumpValueOption; } set { m_DumpValueOption = value; } }

        public enum ValueType
        {
            Int,
            String
            // TODO:  Add Long?
        }

        private DevCommandManager.Option m_SetValueOption = new DevCommandManager.Option("setValue", "Set the value of a specified registry key using a specified type (currently int or string)",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "keyNamePrefix", typeof (string), true ),
                new DevCommandManager.Argument( "keyName", typeof (string), true ),
                new DevCommandManager.Argument( "type", typeof (ValueType), true ),
                new DevCommandManager.Argument( "value", typeof (string), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option SetValueOption { get { return m_SetValueOption; } set { m_SetValueOption = value; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand( this );
        }

        public bool Execute()
        {
            var success = true;

            if ( DumpValueOption.IsPresent )
            {
                var keyNamePrefix = (string)DumpValueOption.Arguments[0].Value;
                var keyName = (string)DumpValueOption.Arguments[1].Value;

                try
                {
                    var regValue = Registry.GetValue(keyNamePrefix, keyName, "Return this default if NoSuchName does not exist.");
                    Debug.Log( "Registry entry " + keyNamePrefix + ", key " + keyName + " has this value: " + regValue + ", and its type is: " + regValue.GetType());
                }
                catch (Exception)
                {
                    Debug.Log("Error:  Keyname does not start with a valid registry root (e.g. HKEY_CURRENT_USER)");
                    success = false;
                }
            }
                
            if ( SetValueOption.IsPresent )
            {
                var keyNamePrefix = (string)SetValueOption.Arguments[0].Value;
                var keyName = (string)SetValueOption.Arguments[1].Value;

                if ((ValueType)SetValueOption.Arguments[2].Value == ValueType.Int)
                {
                    var strValue = (string)SetValueOption.Arguments[ 3 ].Value;
                    int intValue;
                    if ( !Int32.TryParse( strValue, out intValue ) )
                    {
                        Debug.Log("Error:  Expecting integer value but found " + strValue);
                        success = false;
                    }
                    else
                    {
                        try
                        {
                            Registry.SetValue(keyNamePrefix, keyName, intValue, RegistryValueKind.DWord);
                            Debug.Log("Registry entry " + keyNamePrefix + ", key " + keyName + " has been set to value " + intValue + ", with type: REG_DWORD");
                        }
                        catch (Exception)
                        {
                            Debug.Log("Error:  Keyname does not start with a valid registry root (e.g. HKEY_CURRENT_USER)");
                            success = false;
                        }
                    }
                }
                else
                {
                    var value = (string)SetValueOption.Arguments[3].Value;
                    Debug.Log("value is " + value);
                    try
                    {
                        Registry.SetValue( keyNamePrefix, keyName, value, RegistryValueKind.String );
                        Debug.Log( "Registry entry " + keyNamePrefix + ", key " + keyName + " has been set to value " + value + ", with type: REG_SZ" );
                    }
                    catch (Exception)
                    {
                        Debug.Log("Error:  Keyname does not start with a valid registry root (e.g. HKEY_CURRENT_USER)");
                        success = false;
                    }
                }
            }

            return success;
        }
    }
}
