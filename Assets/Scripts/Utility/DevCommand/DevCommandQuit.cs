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
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEngine;

// test comment for build server

namespace Utility.DevCommand
{
    public class DevCommandQuit : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "quit";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Quits the app (standalone builds) or exits play mode if running in the IDE";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }

        private DevCommandManager.Option m_KillOption = new DevCommandManager.Option("kill", "Kill self (instead of calling Unity's Application.Quit)", "MutuallyExclusive");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option KillOption { get { return m_KillOption; } set { m_KillOption = value; } }

        private DevCommandManager.Option m_CrashOption = new DevCommandManager.Option("crash", "Crash on shutdown", "MutuallyExclusive");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option CrashOption { get { return m_CrashOption; } set { m_CrashOption = value; } }


        private class IntentionalCrash
        {
            ~IntentionalCrash()
            {
                var x = new GameObject();   // Intentionally instantiate a Unity game object in a destructor, to cause a crash on shutdown
            }
        }
        private IntentionalCrash IntentionalCrashInstance;


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);
        }

        public bool Execute()
        {
        #if UNITY_EDITOR
            if ( EditorApplication.isPlaying )
            {
                Debug.Log( "Quitting Editor" );
                if (KillOption.IsPresent)
                    Debug.Log("Kill option was present with quit command but does not apply when in Unity Editor");

                EditorApplication.isPlaying = false;
            }
            else
        #endif
            {
                //DevCommandManager.Instance.StartWait();
                //StartCoroutine(QuitterCoroutine());

                if ( KillOption.IsPresent )
                {
                    Debug.Log( "Killing self" );
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
                else
                {
                    if ( CrashOption.IsPresent )
                    {
                        Debug.Log("Intentional crash on shutdown being caused");
                        IntentionalCrashInstance = new IntentionalCrash();
                    }

                    Application.Quit();
                }
            }

            return true;
        }

        // Experimental code below
        private IEnumerator QuitterCoroutine()
        {
            Debug.Log("Started QuitterCoroutine");

            yield return null;

            // Wait until we transition to a system time whose seconds are divisible by 10
            //while (true)
            //{
            //    var now = DateTime.Now;
            //    if ( now.Second % 10 == 9 )
            //        break;
            //    yield return null;
            //}
            //while (true)
            //{
            //    var now = DateTime.Now;
            //    if (now.Second % 10 == 0)
            //        break;
            //    yield return null;
            //}

        //
            //FileStream fs = null;  // We intentionally don't close this; quit will close it
            //var readyToQuit = false;
            //var framesWaited = 0;
            //do
            //{
            //    try
            //    {
            //        fs = File.Open( "README.txt", FileMode.Open, FileAccess.Write, FileShare.None );

            //        var buf = new byte[10];
            //        for (var i = 0; i < 10; i++)
            //            buf[i] = (byte)(0x40 + i);
            //        fs.Write(buf, 0, 10);

            //        readyToQuit = true;
            //    }
            //    catch ( Exception e )
            //    {
            //        Debug.Log( "Caught exception " + e.Message );
            //    }

            //    if ( !readyToQuit )
            //    {
            //        framesWaited++;
            //        yield return null;
            //    }
            //} while ( !readyToQuit );

            //Debug.Log("Quitting after waiting " + framesWaited + " frames");

            //Application.Quit();

            // TODO:  Who knows whether the file closing is done before or after the registry writing (to save player prefs).
            // So the next thing to try is a semaphore system in which Haxxis checks and then sets a file to a certain state,
            // and then the closing bash script is the thing that clears that file...because then we know for sure that
            // Haxxis has completely shut down
        }
    }
}
