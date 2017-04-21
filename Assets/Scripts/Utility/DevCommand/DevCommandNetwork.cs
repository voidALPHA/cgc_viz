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
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;


namespace Utility.DevCommand
{
    public class DevCommandNetwork : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "network";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Network system control";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Argument m_UrlText = new DevCommandManager.Argument("URLText", typeof(string), false, "The URL for the GET request, e.g. http://localhost:8000/Bananas/35");
        [DevCommandArgumentAttribute]
        public DevCommandManager.Argument UrlText { get { return m_UrlText; } set { m_UrlText = value; } }

        private DevCommandManager.Option m_TimeoutOption = new DevCommandManager.Option
            ("timeout", "Set a timeout for request", "", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "timeoutSeconds", typeof (float), true )
            });
        [DevCommandOptionAttribute]
        private DevCommandManager.Option TimeoutOption { get { return m_TimeoutOption; } set { m_TimeoutOption = value; } }

        private DevCommandManager.Option m_WaitOption = new DevCommandManager.Option("wait", "Wait until request completes before executing next dev command", "MutuallyExclusiveWait");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option WaitOption { get { return m_WaitOption; } set { m_WaitOption = value; } }

        private DevCommandManager.Option m_WaitAllOption = new DevCommandManager.Option("waitForAll", "Wait until all pending requests complete before executing next dev command", "MutuallyExclusiveWait");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option WaitAllOption { get { return m_WaitAllOption; } set { m_WaitAllOption = value; } }

        private DevCommandManager.Option m_ResolveCmdLineArgsOption = new DevCommandManager.Option("resolveCmdLineArgs", "Convert instances of {key} in url to command line argument values of that key");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option ResolveCmdLineArgsOption { get { return m_ResolveCmdLineArgsOption; } set { m_ResolveCmdLineArgsOption = value; } }

        private DevCommandManager.Option m_ResolveProcessIDOption = new DevCommandManager.Option("resolveProcessID", "Convert instances of PROCESS_ID in url to actual process ID");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option ResolveProcessIDOption { get { return m_ResolveProcessIDOption; } set { m_ResolveProcessIDOption = value; } }

        private DevCommandManager.Option m_SilentOption = new DevCommandManager.Option("silent", "Don't dump results");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option SilentOption { get { return m_SilentOption; } set { m_SilentOption = value; } }

        private DevCommandManager.Option m_LifelineOption = new DevCommandManager.Option("lifeline", "Mark this request such that when a response is received, Haxxis kills itself");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option LifelineOption { get { return m_LifelineOption; } set { m_LifelineOption = value; } }

        private DevCommandManager.Option m_CancelLifelineOption = new DevCommandManager.Option("cancelLifelineRequest", "Cancel all pending lifeline requests");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option CancelLifelineOption { get { return m_CancelLifelineOption; } set { m_CancelLifelineOption = value; } }

        private DevCommandManager.Option m_ListOption = new DevCommandManager.Option("list", "List all pending requests");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option ListOption { get { return m_ListOption; } set { m_ListOption = value; } }

        private DevCommandManager.Option m_WaitingIndicatorOnOption = new DevCommandManager.Option("waitingIndicatorOn", "Turns on waiting indicator display", "mutuallyExclusiveWI");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option WaitingIndicatorOnOption { get { return m_WaitingIndicatorOnOption; } set { m_WaitingIndicatorOnOption = value; } }

        private DevCommandManager.Option m_WaitingIndicatorOffOption = new DevCommandManager.Option("waitingIndicatorOff", "Turns off waiting indicator display", "mutuallyExclusiveWI");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option WaitingIndicatorOffOption { get { return m_WaitingIndicatorOffOption; } set { m_WaitingIndicatorOffOption = value; } }


        [SerializeField]
        private NetworkSystem.Network m_Network;
        private NetworkSystem.Network Network { get { return m_Network; } set { m_Network = value; } }

        private List<int> m_RequestsToWaitFor = new List< int >();
        private List<int> RequestsToWaitFor { get { return m_RequestsToWaitFor; } set { m_RequestsToWaitFor = value; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);

            Network = FindObjectOfType<NetworkSystem.Network>();
        }

        private void Update()
        {
            // If we're waiting for any requests, see if they are no longer pending.  If nothing more to wait for, lift the wait gate
            if ( RequestsToWaitFor.Any() )
            {
                var requests = NetworkSystem.Network.Instance.PendingRequests;

                RequestsToWaitFor.RemoveAll( id => !requests.Exists( x => x.Id == id ));

                if ( !RequestsToWaitFor.Any() )
                    DevCommandManager.Instance.EndWait();
            }
        }

        public bool Execute()
        {
            var mc = NetworkSystem.Network.Instance;

            if ( UrlText.IsPresent )
            {
                var urlText = (string)UrlText.Value;

                var singleReqTimeout = Single.MaxValue;   // Default is 'no timeout'
                if (TimeoutOption.IsPresent)
                {
                    singleReqTimeout = (float)TimeoutOption.Arguments[0].Value;
                }

                if (ResolveCmdLineArgsOption.IsPresent)
                {
                    int openBraceDelim;
                    while ((openBraceDelim = urlText.IndexOf('{')) >= 0)
                    {
                        var closeBraceDelim = urlText.IndexOf('}');
                        var oldString = urlText.Substring(openBraceDelim, (closeBraceDelim - openBraceDelim) + 1);
                        var keyName = oldString.Substring(1, oldString.Length - 2);
                        var newString = CommandLineArgs.GetArgumentValue(keyName);
                        if (string.IsNullOrEmpty(newString))
                        {
                            Debug.Log("Error:  Could not convert command line argument with key " + keyName);
                            return false;
                        }

                        urlText = urlText.Replace(oldString, newString);
                    }
                }

                if (ResolveProcessIDOption.IsPresent)
                {
                    const string processIdKeyword = "PROCESS_ID";

                    var processIdKeywordIndex = urlText.IndexOf( processIdKeyword );
                    if ( processIdKeywordIndex < 0 )
                    {
                        Debug.Log( "Error:  -resolveProcessID was used but no keyword PROCESS_ID was found in the given URL" );
                        return false;
                    }
                    else
                    {
                        var processIdValueString = Process.GetCurrentProcess().Id.ToString();
                        urlText = urlText.Replace(processIdKeyword, processIdValueString);
                    }
                }

                var id = mc.SendRequest(urlText, SilentOption.IsPresent, singleReqTimeout, LifelineOption.IsPresent);

                if (WaitOption.IsPresent)
                {
                    // Add the ID of the request we just made, to the 'waiting for' list, and set the wait gate
                    RequestsToWaitFor.Add( id );
                    DevCommandManager.Instance.StartWait();
                }
            }

            if ( WaitAllOption.IsPresent )
            {
                // Go through all pending requests, and add them to our 'waiting for' list.  If there are any, set the wait gate
                var requests = mc.PendingRequests;

                foreach ( var r in requests )
                    RequestsToWaitFor.Add( r.Id );

                if ( requests.Any() )
                    DevCommandManager.Instance.StartWait();
            }

            if ( CancelLifelineOption.IsPresent )
            {
                var requests = mc.PendingRequests;
                var countBefore = requests.Count;

                requests.RemoveAll( r => r.LifelineRequest );

                Debug.Log( "Cancelled " + (countBefore - requests.Count) + " pending lifeline request(s)");
            }

            if ( ListOption.IsPresent )
            {
                var requests = mc.PendingRequests;

                foreach (var r in requests)
                {
                    var s = String.Format( "ID:{0,4}  ", r.Id );
                    s += "Silent:" + ( r.Silent ? "Y" : "N" );
                    s += "  Timeout:" + ( r.TimeoutSecs >= Single.MaxValue ? String.Format("{0,8}", "None ") : String.Format("{0,7:F2}s", r.TimeoutSecs) );
                    s += "  URL:" + r.Request.url;

                    Debug.Log(s);
                }
                Debug.Log(requests.Count + " pending requests");
            }

            if (WaitingIndicatorOnOption.IsPresent)
                mc.WaitingIndicatorDisplayOverride = true;

            if (WaitingIndicatorOffOption.IsPresent)
                mc.WaitingIndicatorDisplayOverride = false;

            return true;
        }
    }
}
