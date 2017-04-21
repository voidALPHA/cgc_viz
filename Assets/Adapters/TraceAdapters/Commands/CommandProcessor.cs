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
using System.Text;
using Choreography.Recording;
using Choreography.Views;
using Scripts.Utility.Misc;
using UnityEngine;
using Utility;
using Utility.JobManagerSystem;
using Timer = System.Timers.Timer;

namespace Adapters.TraceAdapters.Commands
{
    public class CommandProcessor
    {
        public static string TraceApiUrlPlayerPrefsKey { get { return "TraceApiUrl"; } }

        public static string Url
        {
            get
            {
                var commandLineUrl = CommandLineArgs.GetArgumentValue("traceapiurl");

                if ( !string.IsNullOrEmpty( commandLineUrl ) )
                    return commandLineUrl;

                return PlayerPrefs.GetString( TraceApiUrlPlayerPrefsKey, "http://localhost:8000" );
            }
            set
            {
                PlayerPrefs.SetString( TraceApiUrlPlayerPrefsKey, value );
            }
        }

        public static bool DoLog { get; set; }

        private static float m_TimeOut = 60f;   // Long because TraceAPI handles these one at a time, and VGS can request a LOT of them
        public static float TimeOut { get { return m_TimeOut; } set { m_TimeOut = value; } }

        public static IEnumerator Execute(Command command)
        {
            var finalUrl = string.Format( Url + command.RelativeUrl );

            WWW www = null;

            var jobId = JobManager.Instance.CurrentlyRunningJobId;
            var savedMaxExPerFrame = JobManager.Instance.RegisteredJobs[ jobId ].MaxExecutionsPerFrame;
            JobManager.Instance.RegisteredJobs[ jobId ].MaxExecutionsPerFrame = 1;

            Debug.Log("Command processor request: " + finalUrl);

            var timedOut = false;
            var timer = new Timer(TimeOut * 1000);
            timer.Elapsed += (s, e) => timedOut = true;
            timer.Start();

            var throwException = false;

            try
            {
                if ( command is PostCommand )
                {
                    var postCommand = command as PostCommand;

                    var postBytes = Encoding.ASCII.GetBytes( postCommand.PostString );

                    www = new WWW(finalUrl, postBytes, postCommand.Headers);
                }
                else
                {
                    www = new WWW(finalUrl);
                }

                if ( DoLog )
                    Debug.LogFormat( "<color=#ff600ff>API Query: {0}</color>", finalUrl );

                while (!www.isDone)
                {
                    if (timedOut)
                    {
                        Debug.LogErrorFormat("Error in www request: Timed out after {0} seconds, for request: {1}", TimeOut, finalUrl);
                        HaxxisGlobalSettings.Instance.ReportVgsError( 12, "Trace API request timed out" );
                        if (!StopChoreographyIfVgs())
                            throwException = true;
                        break;
                    }

                    yield return null;
                }
                timer.Stop();

                JobManager.Instance.RegisteredJobs[jobId].MaxExecutionsPerFrame = savedMaxExPerFrame;

                if (!timedOut)
                {
                    if (!string.IsNullOrEmpty(www.error))
                    {
                        Debug.LogErrorFormat("Error in www: {0}, for request: {1}", www.error, finalUrl);
                        HaxxisGlobalSettings.Instance.ReportVgsError(13, "Trace API request had error: " + www.error + "; URL was: " + finalUrl);
                        if (!StopChoreographyIfVgs())
                            throw new InvalidOperationException();
                        yield break;
                    }

                    if (DoLog)
                        Debug.LogFormat("<color=#ff600ff>API Result: {0}</color>", www.text);

                    yield return null;

                    command.HandleResponse(www.bytes);
                }
            }
            finally
            {
                if (www != null)
                    www.Dispose();
            }
            if (throwException)
            {
                if (!StopChoreographyIfVgs())
                    throw new InvalidOperationException();
            }
        }

        private static bool StopChoreographyIfVgs()
        {
            if ( HaxxisGlobalSettings.Instance.IsVgsJob == true && TimelineViewBehaviour.Instance.IsPlayingState )
            {
                TimelineViewBehaviour.Instance.Stop();
                return true;
            }
            return false;
        }
    }
}
