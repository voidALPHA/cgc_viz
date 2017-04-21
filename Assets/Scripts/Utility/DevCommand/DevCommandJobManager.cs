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
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Utility.JobManagerSystem;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace Utility.DevCommand
{
    public class DevCommandJobManager : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "jm";   // For now for quick testing; later make it 'jobs' or 'jobmanager' or 'job'
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Job Manager";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Option m_ListOption = new DevCommandManager.Option
            ("list", "Lists active jobs or a specified job", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "jobID", typeof (uint), false )
            });
        [DevCommandOptionAttribute]
        private DevCommandManager.Option ListOption { get { return m_ListOption; } set { m_ListOption = value; } }

        private DevCommandManager.Option m_KillJobOption = new DevCommandManager.Option
            ("kill", "Kills a specified job", "mutuallyExclusiveKill", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "jobID", typeof (uint), true )
            });
        [DevCommandOptionAttribute]
        private DevCommandManager.Option KillJobOption { get { return m_KillJobOption; } set { m_KillJobOption = value; } }

        private DevCommandManager.Option m_KillAllJobsOption = new DevCommandManager.Option("killAll", "Kills all jobs", "mutuallyExclusiveKill");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option KillAllJobsOption { get { return m_KillAllJobsOption; } set { m_KillAllJobsOption = value; } }

        private DevCommandManager.Option m_ChangeFrameBudgetOption = new DevCommandManager.Option
            ("setFrameBudget", "Sets a new specified total frame budget, in milliseconds", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "frameBudget", typeof (float), true ),
            });
        [DevCommandOptionAttribute]
        private DevCommandManager.Option ChangeFrameBudgetOption { get { return m_ChangeFrameBudgetOption; } set { m_ChangeFrameBudgetOption = value; } }

        private DevCommandManager.Option m_ChangeBusyTextOption = new DevCommandManager.Option
            ("setBusyText", "Sets the text message to be shown in the busy indicator", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "message", typeof (string), true ),
            });
        [DevCommandOptionAttribute]
        private DevCommandManager.Option ChangeBusyTextOption { get { return m_ChangeBusyTextOption; } set { m_ChangeBusyTextOption = value; } }

        private DevCommandManager.Option m_PauseJobOption = new DevCommandManager.Option
            ("pause", "Pauses a specified job", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "jobID", typeof (uint), true )
            });
        [DevCommandOptionAttribute]
        private DevCommandManager.Option PauseJobOption { get { return m_PauseJobOption; } set { m_PauseJobOption = value; } }

        private DevCommandManager.Option m_UnPauseJobOption = new DevCommandManager.Option
            ("unpause", "Un-pauses a specified job", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "jobID", typeof (uint), true )
            });
        [DevCommandOptionAttribute]
        private DevCommandManager.Option UnPauseJobOption { get { return m_UnPauseJobOption; } set { m_UnPauseJobOption = value; } }

        private DevCommandManager.Option m_FinishJobNowOption = new DevCommandManager.Option
            ("finishNow", "Completes a specified job immediately, no matter how long it takes (dangerous)", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "jobID", typeof (uint), true )
            });
        [DevCommandOptionAttribute]
        private DevCommandManager.Option FinishJobNowOption { get { return m_FinishJobNowOption; } set { m_FinishJobNowOption = value; } }

        private DevCommandManager.Option m_TestOption = new DevCommandManager.Option
            ("test", "For testing: Starts a job...", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "jobTestType", typeof (int), true )
            });
        [DevCommandOptionAttribute]
        private DevCommandManager.Option TestOption { get { return m_TestOption; } set { m_TestOption = value; } }

        private DevCommandManager.Option m_BusyIndicatorOnOption = new DevCommandManager.Option("busyIndicatorOn", "Turns on busy indicator display", "mutuallyExclusiveBI");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option BusyIndicatorOnOption { get { return m_BusyIndicatorOnOption; } set { m_BusyIndicatorOnOption = value; } }

        private DevCommandManager.Option m_BusyIndicatorOffOption = new DevCommandManager.Option("busyIndicatorOff", "Turns off busy indicator display", "mutuallyExclusiveBI");
        [DevCommandOptionAttribute]
        private DevCommandManager.Option BusyIndicatorOffOption { get { return m_BusyIndicatorOffOption; } set { m_BusyIndicatorOffOption = value; } }

        private DevCommandManager.Option m_SetExceptionVerbosityOption = new DevCommandManager.Option
            ("setExceptionVerbosity", "Sets exception verbosity from 0 (silent) to 3 (full including call stack)", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "verbosity", typeof (int), true ),
            });
        [DevCommandOptionAttribute]
        private DevCommandManager.Option SetExceptionVerbosityOption { get { return m_SetExceptionVerbosityOption; } set { m_SetExceptionVerbosityOption = value; } }

        private DevCommandManager.Option m_SetMaxJobsShownInStatsLinesOption = new DevCommandManager.Option
            ("setMaxJobsInStatsLine", "Sets the maximum number of jobs shown in job manager stats line", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "maxJobs", typeof (uint), true ),
            });
        [DevCommandOptionAttribute]
        private DevCommandManager.Option SetMaxJobsShownInStatsLinesOption { get { return m_SetMaxJobsShownInStatsLinesOption; } set { m_SetMaxJobsShownInStatsLinesOption = value; } }


        [SerializeField]
        private JobManager m_JobManager;
        private JobManager JobManager { get { return m_JobManager; } set { m_JobManager = value; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);

            JobManager = FindObjectOfType<JobManager>();
        }

        public bool Execute()
        {
            var jm = JobManager.Instance;

            if (KillJobOption.IsPresent)
            {
                uint id = (uint) KillJobOption.Arguments[0].Value;
                if (!JobManager.CancelJob(id))
                    Debug.Log("No active job with ID " + id);
            }

            if (KillAllJobsOption.IsPresent)
            {
                int jobsKilled = 0;
                while (JobManager.RegisteredJobs.Count > 0)
                {
                    var item = JobManager.RegisteredJobs.First( );
                    JobManager.CancelJob(item.Key, true);
                    jobsKilled++;
                }
                if (jobsKilled > 0)
                    Debug.Log("All " + jobsKilled + " jobs killed");
                else
                    Debug.Log("No jobs to kill");
            }

            if (ChangeFrameBudgetOption.IsPresent)
            {
                if (JobManager.SetFrameBudgetMS((float)ChangeFrameBudgetOption.Arguments[0].Value))
                    Debug.Log("Total frame budget changed");
                else
                    Debug.Log("Error setting total frame budget");
            }

            if (ChangeBusyTextOption.IsPresent)
            {
                JobManager.SetBusyIndicatorText( (string) ChangeBusyTextOption.Arguments[0].Value );
            }

            if (PauseJobOption.IsPresent)
            {
                uint id = (uint)PauseJobOption.Arguments[0].Value;
                if (!JobManager.PauseJob(id))
                    Debug.Log("No active job with ID " + id);
            }

            if (UnPauseJobOption.IsPresent)
            {
                uint id = (uint)UnPauseJobOption.Arguments[0].Value;
                if (!JobManager.UnPauseJob(id))
                    Debug.Log("No active job with ID " + id);
            }

            if (FinishJobNowOption.IsPresent)
            {
                uint id = (uint)FinishJobNowOption.Arguments[0].Value;
                if (!JobManager.FinishJobThisFrame(id))
                    Debug.Log("No active job with ID " + id);
            }

            // For testing:
            if (TestOption.IsPresent)
            {
                switch ((int) TestOption.Arguments[0].Value)
                {
                    case 0:
                        var id = jm.StartJob(DoStuff(2000), OnDoStuffCompleted, OnDoStuffCancelled, String.Format("DoStuff"), maxExecutionsPerFrame: 1);
                        jm.StartJob(DoStuff(2000), OnDoStuffCompleted, OnDoStuffCancelled, String.Format("WaitingForDoStuff"), new List<uint>() { id }, timeoutTime: 10.0f );
                        break;

                    case 1:
                        // To test jobs starting other jobs:
                        JobManager.StartJob( TestJobSpawningJob( 0 ), jobName: "JobSpawningJob" );
                        break;

                    case 2:
                        // Test for a fine-grained job that only wants one execution per frame
                        jm.StartJob(DoStuff(200), OnDoStuffCompleted, OnDoStuffCancelled, String.Format("DoStuff"), timeoutTime: 120.0f, maxExecutionsPerFrame: 1);
                        jm.StartJob(DoStuff(2000), OnDoStuffCompleted, OnDoStuffCancelled, String.Format("DoStuff{0}", 999), canSkipFrames: true);
                        break;

                    case 3:
                        {
                            // ONE JOB DEPENDS ON several coarse-grained jobs
                            List<uint> dependentJobIds = new List<uint>( );
                            for (int i = 0; i < 5; i++)
                            {
                                uint id1 = jm.StartJob(DoStuff(( i + 1 ) * 400), OnDoStuffCompleted, OnDoStuffCancelled, String.Format("DoStuff{0}", i), canSkipFrames: true);
                                dependentJobIds.Add( id1 );
                            }
                            jm.StartJob(Test(), OnTestCompleted, OnTestCancelled, String.Format("Test{0}", 999), dependentJobIds);
                        }
                        break;

                    case 4:
                        {
                            // ONE JOB DEPENDS ON several fine-grained jobs
                            List<uint> dependentJobIds = new List<uint>();
                            for (int i = 0; i < 10; i++)
                            {
                                uint id2;
                                if (i == 2)
                                    id2 = jm.StartJob(Test(), OnTestCompleted, OnTestCancelled, String.Format("Test{0}", i), jobIdToUnPauseWhenDone: dependentJobIds[0]);
                                else
                                    id2 = jm.StartJob(Test(), OnTestCompleted, OnTestCancelled, String.Format("Test{0}", i));
                                dependentJobIds.Add(id2);
                            }
                            jm.StartJob(DoStuff(2000), OnDoStuffCompleted, OnDoStuffCancelled, "DoStuff", dependentJobIds, timeoutTime: 60.0f);
                        }
                        break;

                    case 5:
                        {
                            // ONE JOB IS THE DEPENDEE ON 20 OTHER JOBS, and each of those is dependent on the previous
                            uint id1 = jm.StartJob(DoStuff(2000), OnDoStuffCompleted, OnDoStuffCancelled, "DoStuff", timeoutTime: 60.0f);
                            List<uint> dependentJobIds = new List<uint>( );
                            dependentJobIds.Add( id1 );
                            for (int i = 0; i < 20; i++)
                            {
                                List<uint> dependentJobIds2 = new List<uint>( dependentJobIds );
                                uint id2 = jm.StartJob(Test(), OnTestCompleted, OnTestCancelled, "Test", dependentJobIds2);
                                dependentJobIds.Add( id2 );
                            }
                        }
                        break;

                    case 6:
                        jm.StartJob(VeryQuickJob(), OnVeryQuickJobCompletion, jobName: "VeryQuickJob", startImmediately: true);
                        break;

                    case 7:
                        jm.StartJob(NestedJob(), jobName: "NestedJob", maxExecutionsPerFrame: 1);
                        break;
                }
            }

            if (ListOption.IsPresent)
            {
                foreach (var jobEntry in JobManager.RegisteredJobs)
                {
                    var s = String.Format("ID:{0,6}  Timeout:", jobEntry.Key);
                    var job = jobEntry.Value;
                    s += job.TimeoutTimeInTicks == Int64.MaxValue ? String.Format("{0,8}", "None ") : String.Format("{0,7:F2}s", job.TimeoutTimeInTicks / Stopwatch.Frequency);
                    s += "  CanSkipFrames:" + (job.CanSkipFrames ? "Y" : "N");
                    s += "  Max EPF:";
                    s += job.MaxExecutionsPerFrame == Int32.MaxValue ? String.Format("{0,4}", "None") : String.Format("{0,4}", job.MaxExecutionsPerFrame);
                    s += String.Format("  {0,7}", (job.State == JobManager.JobState.Running ? "Running" : (job.State == JobManager.JobState.Paused ? "Paused" : "Waiting")));
                    s += String.Format("  \"{0}\"", job.Name);

                    Debug.Log(s);
                }
            }

            if ( BusyIndicatorOnOption.IsPresent )
                jm.BusyIndicatorDisplayOverride = true;

            if ( BusyIndicatorOffOption.IsPresent )
                jm.BusyIndicatorDisplayOverride = false;

            if ( SetExceptionVerbosityOption.IsPresent )
            {
                var level = (int) SetExceptionVerbosityOption.Arguments[ 0 ].Value;
                if ( level >= 0 && level <= 3 )
                {
                    jm.ExceptionVerbosityLevel = level;
                    Debug.Log("Exception verbosity level set to " + level);
                }
                else
                {
                    Debug.Log("Error:  Exception verbosity level must be 0 to 3 inclusive");
                    return false;
                }
            }

            if ( SetMaxJobsShownInStatsLinesOption.IsPresent )
            {
                jm.MaxJobsInStatsDisplay = (uint)SetMaxJobsShownInStatsLinesOption.Arguments[0].Value;
            }

            var status = String.Format("Number of jobs registered: {0}; Running: {1}; Paused: {2}; Waiting: {3}; Frame budget: {4:N2} ms",
                jm.NumJobsRegistered, jm.NumJobsRunning, jm.NumJobsPaused, jm.NumJobsRegistered - jm.NumJobsRunning - jm.NumJobsPaused, JobManager.FrameBudgetMS);
            Debug.Log(status);
            return true;
        }


        // TEST CODE:

        private float GrandTotal;
        private float TimeWastingTotal = 0.0f;

        private IEnumerator TestJobSpawningJob(int depth)
        {
            GrandTotal = 0.0f;
            for (int i = 0; i < 500; i++)
            {
                for (int j = 0; j < 100000; j++)
                    TimeWastingTotal += GrandTotal * 3.14f / ( j + 1 );
                GrandTotal += ( i );

                yield return null;
            }
            if (depth < 2)
            {
                JobManager.StartJob( TestJobSpawningJob( depth + 1 ), jobName: "JobSpawningJob" );

                for (int i = 0; i < 500; i++)
                {
                    for (int j = 0; j < 100000; j++)
                        TimeWastingTotal += GrandTotal * 3.14f / ( j + 1 );
                    GrandTotal += ( i );

                    yield return null;
                }
                JobManager.StartJob(TestJobSpawningJob(depth + 1), jobName: "JobSpawningJob");
                yield return null;
            }
            for (int i = 0; i < 100; i++)
                yield return null;
        }

        private Random rndGen = new Random(4726742);

        private IEnumerator DoStuff(int count)
        {
            //Debug.Log("Now in DoStuff");
            GrandTotal = 0.0f;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < 100000; j++)
                    TimeWastingTotal += GrandTotal * 3.14f / ( j + 1 );
                GrandTotal += ( i );
                //Debug.Log("DoStuff prints " + i + " and GrandTotal is " + GrandTotal);

                yield return null;
            }
        }

        private void OnDoStuffCompleted(uint id)
        {
            //Debug.Log("From the caller:  OnDoStuffCompleted");
        }

        private void OnDoStuffCancelled(uint id, bool timedOut, Exception exception)
        {
            Debug.Log("From the caller:  OnDoStuffCancelled; timedOut = " + timedOut);
            if (exception == null)
                Debug.Log("No exception occurred");
            else
                Debug.Log("Exception occurred");
        }

        private IEnumerator Test()
        {
            var count = rndGen.Next(1000000, 5000000);
            //Debug.Log("Counting to " + count);
            for (int i = 0; i < count; i++)
            {
                //Debug.Log("Test prints " + i);
                GrandTotal += (float)i;
                if ( i == 1000000 )
                    throw new Exception();

                yield return null;
            }
        }

        private IEnumerator Lightweight(int i)
        {
            yield return null;
            Debug.Log("Doing a lightweight task " + i);
        }

        private void OnTestCompleted(uint id)
        {
            //Debug.Log("From the caller:  OnTestCompleted");
        }

        private void OnTestCancelled(uint id, bool timedOut, Exception exception)
        {
            Debug.Log("From the caller:  OnTestCancelled; timedOut = " + timedOut);
            if (exception == null)
                Debug.Log("No exception occurred");
            else
                Debug.Log("Exception occurred");
        }

        private IEnumerator VeryQuickJob()
        {
            Debug.Log("We are in VeryQuickJob");
            yield return null;
            yield break;
        }

        private void OnVeryQuickJobCompletion(uint jobId)
        {
            Debug.Log("We are in OnVeryQuickJobCompletion");
        }

        private IEnumerator NestedJob()
        {
            Debug.Log("Frame Count:" + Time.frameCount + "  Inside NestedJob");

            for (int i = 0; i < 5; i++)
            {
                var id = JobManager.Instance.StartJobAndPause(NestedJobFoo(  ), startImmediately: false);
                if (JobManager.Instance.IsJobRegistered( id ))
                    yield return null;

                //IEnumerator foo = NestedJobFoo();
                //while (foo.MoveNext())
                //    yield return null;
            }
            Debug.Log("Frame Count:" + Time.frameCount + "  Finally");
        }

        private IEnumerator NestedJobFoo()
        {
            Debug.Log("Frame Count:" + Time.frameCount + "  Inside NestedJobFoo");

            for (int j = 0; j < 3; j++)
            {
                Debug.Log( "Frame Count:" + Time.frameCount + "  Foo " + j );
                yield return null;
            }
        }
    }
}
