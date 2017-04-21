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
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Utility.JobManagerSystem
{
    public class JobManager : MonoBehaviour
    {
        public static JobManager Instance { get; private set; }

        public readonly long TicksPerMS = Stopwatch.Frequency / 1000L;

        private long m_FrameBudgetTicks;
        private float m_FrameBudgetMS;
        public float FrameBudgetMS
            { get { return m_FrameBudgetMS; }
              private set
              {
                m_FrameBudgetMS = value;
                m_FrameBudgetTicks = (long)(value * (float)TicksPerMS);
            } }

        public long JobBudget { get; private set; }

        private long LastJobOverTicks { get; set; }

        private long m_TimeSpentInJobsTicks;

        public long TimeSpentInJobsTicks
        {
            get
            {
                var time = m_TimeSpentInJobsTicks;
                m_TimeSpentInJobsTicks = 0;
                return time;
            }
            private set
            {
                m_TimeSpentInJobsTicks = value;
            }
        }

        private uint m_NextUid = 1000;
        private uint NextUid { get { return m_NextUid; } set { m_NextUid = value; } }

        private Dictionary<uint, Job> m_RegisteredJobs = new Dictionary<uint, Job>();
        public Dictionary<uint, Job> RegisteredJobs { get { return m_RegisteredJobs; } private set { m_RegisteredJobs = value; } }

        // These are statistical and set once per frame
        public int NumJobsRegistered { get; set; }
        public int NumJobsRunning { get; set; }
        public int NumJobsPaused { get; set; }

        private uint m_CurrentlyRunningJobId = UInt32.MaxValue;
        public uint CurrentlyRunningJobId { get { return m_CurrentlyRunningJobId; } private set { m_CurrentlyRunningJobId = value; } }

        private int FramesBeforeResetOverTicksBudget { get; set; }

        public delegate void OnJobCompletionHandler(uint id);

        public delegate void OnJobCancellationHandler(uint id, bool timedOut, Exception exception);

        public enum JobState
        {
            Waiting,
            Running,
            Paused
        }

        public class Job
        {
            public Coroutine Coroutine;
            public OnJobCompletionHandler JobCompletionHandler;
            public OnJobCancellationHandler JobCancellationHandler;
            public string Name;
            public List<uint> DependentJobs;    // These are jobs that must complete before this job starts
            public long TimeoutTimeInTicks;
            public int MaxExecutionsPerFrame;
            public bool CanSkipFrames;
            public uint JobIdToUnPauseWhenDone;

            public JobState State;
            public JobState StateBeforePaused;

            public bool FinishJobNow;           // Dangerous
        }

        private int m_ExceptionVerbosityLevel = 3;
        public int ExceptionVerbosityLevel { get { return m_ExceptionVerbosityLevel; } set { m_ExceptionVerbosityLevel = value; } }

        private Canvas m_BusyIndicator;
        private Canvas BusyIndicator { get { return m_BusyIndicator; } set { m_BusyIndicator = value; } }

        private bool m_BusyIndicatorDisplayOverride = true;
        public bool BusyIndicatorDisplayOverride { get { return m_BusyIndicatorDisplayOverride; } set { m_BusyIndicatorDisplayOverride = value; } }

        private uint m_MaxJobsInStatsDisplay = 50;
        public uint MaxJobsInStatsDisplay { get { return m_MaxJobsInStatsDisplay; } set { m_MaxJobsInStatsDisplay = value; } }


        private void Awake()
        {
            Instance = this;
            BusyIndicator = GetComponentInChildren<Canvas>();
            BusyIndicator.enabled = false;
        }

        private void Start()
        {
            FrameBudgetMS = 200.00f;
            LastJobOverTicks = 0L;
        }

        private void Update()
        {
            if (CurrentlyRunningJobId != UInt32.MaxValue)
                throw new Exception("Job manager Update call: 'currently running job id' must be 'no job' at this point");

            if (LastJobOverTicks > m_FrameBudgetTicks)
                LastJobOverTicks = m_FrameBudgetTicks;  // Prevent us from getting more than a frame 'behind' under heavy load
            else if (LastJobOverTicks < -m_FrameBudgetTicks)
                LastJobOverTicks = -m_FrameBudgetTicks; // Also prevent us from getting too far 'ahead' under light load (e.g. a single task with max executions set to 1)

            NumJobsRegistered = RegisteredJobs.Count;
            NumJobsRunning = RegisteredJobs.Values.Count(job => job.State == JobState.Running);  // How many jobs are actually running
            NumJobsPaused = RegisteredJobs.Values.Count( job => job.State == JobState.Paused );

            if (NumJobsRunning > 0)
            {
                JobBudget = m_FrameBudgetTicks / (long) NumJobsRunning;
                FramesBeforeResetOverTicksBudget = 2;

                BusyIndicator.enabled = BusyIndicatorDisplayOverride;
            }
            else
            {
                if (FramesBeforeResetOverTicksBudget > 0)
                {
                    if (--FramesBeforeResetOverTicksBudget == 0)
                    {
                        LastJobOverTicks = 0L;
                        BusyIndicator.enabled = false;
                    }
                }
            }

            //Debug.Log("JobManagerUpdate:  LastJobOverTicks " + LastJobOverTicks);
        }


        // For convenience, this call has all of the parameters as the main call, except the jobIdToUnPauseWhenDone parameter, which is set automatically
        // This method is really "Start a new job, and if it does not complete immediately, pause the calling job, and register that it will be un-paused when the new job completes"
        public uint StartJobAndPause
        (
            IEnumerator job,
            OnJobCompletionHandler completionHandler = null,
            OnJobCancellationHandler cancellationHandler = null,
            string jobName = "",
            List<uint> dependentJobs = null,
            float timeoutTime = Single.MaxValue,
            int maxExecutionsPerFrame = Int32.MaxValue,
            bool canSkipFrames = false,
            bool startImmediately = false
        )
        {
            if (CurrentlyRunningJobId == UInt32.MaxValue)
                throw new Exception("JobManager:  Call to StartJobAndPause received from code that is not a job");

            var id = StartJob( job, completionHandler, cancellationHandler, jobName, dependentJobs,
                timeoutTime, maxExecutionsPerFrame, canSkipFrames, startImmediately, CurrentlyRunningJobId);

            if (RegisteredJobs.ContainsKey(id)) // If the job has not already finished
                PauseJob(CurrentlyRunningJobId);

            return id;
        }

        public uint StartJob
        (
            IEnumerator job,
            OnJobCompletionHandler completionHandler = null,
            OnJobCancellationHandler cancellationHandler = null,
            string jobName = "",
            List<uint> dependentJobs = null,
            float timeoutTime = Single.MaxValue,
            int maxExecutionsPerFrame = Int32.MaxValue,
            bool canSkipFrames = false,
            bool startImmediately = false,  // Note that if startImmediately is set to true, the time budgeting for the current frame might be thrown off
            uint jobIdToUnPauseWhenDone = UInt32.MaxValue
        )
        {
            uint id = NextUid++;

            if (jobName.Equals( "" ))
                jobName = "(unnamed)";

            RegisteredJobs[id] = new Job();  // Add to the list of registered jobs PRIOR to launching the coroutine, since the job might finish immediately

            var jobEntry = RegisteredJobs[id];
            jobEntry.Coroutine = null;
            jobEntry.JobCompletionHandler = completionHandler;
            jobEntry.JobCancellationHandler = cancellationHandler;
            jobEntry.Name = jobName;
            if (dependentJobs == null)
                jobEntry.DependentJobs = new List<uint>( ); // This is because we can't pass a non-null default item as a function parameter above if the item is a reference
            else
            {
                // This rule (dependent jobs must have been registered prior to the dependee) prevents circularity
                if (dependentJobs.Any( jobId => jobId >= id ))
                    throw new Exception("JobManager:  Error starting job \"" + jobName + "\"; invalid dependent job ID (must be a job that has already been started)");

                jobEntry.DependentJobs = dependentJobs;

                // For all the dependent jobs, ensure they can't skip frames (that could cause a lockup)
                foreach (var jobId in dependentJobs)
                {
                    if (RegisteredJobs.ContainsKey(jobId))
                        RegisteredJobs[jobId].CanSkipFrames = false;
                }
            }
            jobEntry.TimeoutTimeInTicks = (timeoutTime > 1e20f ? Int64.MaxValue : Stopwatch.Frequency * (long)timeoutTime);
            jobEntry.MaxExecutionsPerFrame = maxExecutionsPerFrame;
            jobEntry.CanSkipFrames = canSkipFrames;
            jobEntry.JobIdToUnPauseWhenDone = jobIdToUnPauseWhenDone;

            jobEntry.State = JobState.Running;
            jobEntry.FinishJobNow = false;

            var coroutine = StartCoroutine( RunJob(id, job, startImmediately) );

            // Now that we have a reference to the coroutine, we can save it in the dictionary.  However the job may have ran
            // and immediately finished by the time we get here, so in that case the registered item has already been removed.
            if (RegisteredJobs.ContainsKey( id ))
                RegisteredJobs[id].Coroutine = coroutine;

            return id;
        }

        public bool CancelJob(uint id, bool cancelSpawnedJobs = false, Exception exception = null)
        {
            if (!RegisteredJobs.ContainsKey( id ))
                return false;

            if (cancelSpawnedJobs)
            {
                // There will be only 0 or 1 of these:
                var otherJob = RegisteredJobs.FirstOrDefault(e => e.Value.JobIdToUnPauseWhenDone.Equals(id));

                if (otherJob.Value != null)
                    CancelJob(otherJob.Key, true);
            }

            if (id != CurrentlyRunningJobId)
                StopCoroutine( RegisteredJobs[id].Coroutine );

            OnCancelInternal( id, false, exception );

            return true;
        }

        public bool PauseJob(uint id)
        {
            if (!RegisteredJobs.ContainsKey( id ))
                return false;

            if (RegisteredJobs[id].State != JobState.Paused)
            {
                RegisteredJobs[id].StateBeforePaused = RegisteredJobs[id].State;
                RegisteredJobs[id].State = JobState.Paused;
            }
            return true;
        }

        public bool UnPauseJob(uint id)
        {
            if (!RegisteredJobs.ContainsKey(id))
                return false;

            if (RegisteredJobs[id].State == JobState.Paused)
                RegisteredJobs[id].State = RegisteredJobs[id].StateBeforePaused;

            return true;
        }

        public bool IsJobRegistered(uint id)
        {
            return RegisteredJobs.ContainsKey(id);
        }

        public bool IsJobRunning(uint id)
        {
            return RegisteredJobs.ContainsKey( id ) && RegisteredJobs[id].State == JobState.Running;
        }

        public bool IsJobWaiting(uint id)
        {
            return RegisteredJobs.ContainsKey(id) && RegisteredJobs[id].State == JobState.Waiting;
        }

        public bool IsJobpaused(uint id)
        {
            return RegisteredJobs.ContainsKey( id ) && RegisteredJobs[id].State == JobState.Paused;
        }

        public bool FinishJobThisFrame(uint id)
        {
            if (!RegisteredJobs.ContainsKey( id ))
                return false;

            RegisteredJobs[id].FinishJobNow = true;
            return true;
        }

        public bool SetFrameBudgetMS(float ms)
        {
            if (ms < 0.0f)
                return false;

            FrameBudgetMS = ms;
            return true;
        }

        public bool SetBusyIndicatorText(string text)
        {
            BusyIndicator.GetComponentInChildren<Text>().text = text;
            return true;
        }


        // This is the WRAPPER.  This is what is actually executing as a Unity coroutine.
        // It is structured to use a given timeslice share (expressed as number of stopwatch ticks)

        private IEnumerator RunJob(uint id, IEnumerator job, bool startImmediately)
        {
            bool jobDone = false;

            var jobName = RegisteredJobs[id].Name;

            var timeoutStopwatch = new Stopwatch();
            timeoutStopwatch.Start();

            var wasWaitingForDependentJobs = RegisteredJobs[id].DependentJobs.Count > 0;
            if (wasWaitingForDependentJobs)
                RegisteredJobs[id].State = JobState.Waiting;

            while (RegisteredJobs[id].DependentJobs.Count > 0)
            {
                RegisteredJobs[id].DependentJobs.RemoveAll(item => !RegisteredJobs.ContainsKey(item));

                if (RegisteredJobs[id].DependentJobs.Count > 0)
                {
                    yield return null; // Yield, as we are still waiting for at least one dependent job to complete

                    if (timeoutStopwatch.ElapsedTicks >= RegisteredJobs[id].TimeoutTimeInTicks)
                    {
                        Debug.Log("JobManager:  \"" + jobName + "\" has timed out (before it even started) and is therefore being cancelled");
                        OnCancelInternal(id, true);
                        yield break;    // And we're completely done
                    }
                }
            }

            // Note:  We put this here because if the job has been waiting for dependent jobs, and the last dependent job has finished, and in the same frame,
            // we're now starting this job, the budgeting can effectively give more time than it should to this job execution, because the basic per-job
            // budget is calculated once per frame and is based on the number of active jobs.
            if (!startImmediately || wasWaitingForDependentJobs)
                yield return null;

            if (RegisteredJobs[id].State != JobState.Paused)
                RegisteredJobs[id].State = JobState.Running;

            //Debug.Log("JobManager:  Started \"" + jobName + "\"" + (RegisteredJobs[id].State == JobState.Paused ? " but in paused state" : ""));

            var sw = new Stopwatch();

            var timedOut = false;
            var exceptionOccurred = false;
            Exception exception = null;

            var savedCurrentlyRunningJobId = CurrentlyRunningJobId;
            CurrentlyRunningJobId = id;

            do
            {
                if (RegisteredJobs[id].FinishJobNow)
                {
                    Debug.Log("JobManager:  \"" + jobName + "\" request to complete entire job now...");

                    try
                    {
                        while (job.MoveNext())
                            ;
                    }
                    catch ( Exception e )
                    {
                        DumpJobExceptionMessage(id, e);
                        exceptionOccurred = true;
                        exception = e;
                    }

                    break;  // Break out of the outer do loop
                }

                var budgetedTimeInTicks = JobBudget - LastJobOverTicks;
                // Note that at this point, budgetedTimeInTicks can be negative.
                // In that case the job will execute exactly one iteration below, EXCEPT if CanSkipFrames is set to true, in which case we don't execute it at all

                //Debug.Log("budgetedTimeInTicks:" + budgetedTimeInTicks + " for job " + jobName);

                if (budgetedTimeInTicks < 0 && RegisteredJobs[id].CanSkipFrames)
                {
                    //Debug.Log("JobManager:  \"" + RegisteredJobs[id].Name + "\" is being skipped this frame");
                    LastJobOverTicks = 0 - budgetedTimeInTicks;
                    CurrentlyRunningJobId = savedCurrentlyRunningJobId;

                    yield return null;

                    savedCurrentlyRunningJobId = CurrentlyRunningJobId;
                    CurrentlyRunningJobId = id;
                }
                else
                {
                    long elapsedTicks = 0;

                    if (RegisteredJobs[id].State != JobState.Paused)
                    {
                        sw.Start();
                        var executionCount = 0;
                        do
                        {
                            try
                            {
                                if (!job.MoveNext()) // Call the actual code to be executed; returns false if entire job is done
                                {
                                    jobDone = true;
                                    elapsedTicks = sw.ElapsedTicks;
                                    break;
                                }
                            }
                            catch ( Exception e )
                            {
                                DumpJobExceptionMessage(id, e);
                                exceptionOccurred = true;
                                exception = e;
                                jobDone = true;
                                elapsedTicks = sw.ElapsedTicks;
                                break;
                            }

                            elapsedTicks = sw.ElapsedTicks;

                            if (++executionCount >= RegisteredJobs[id].MaxExecutionsPerFrame)
                                break;

                            if (RegisteredJobs[id].State == JobState.Paused)
                                break;

                        } while (elapsedTicks < budgetedTimeInTicks);

                        sw.Reset();
                    }

                    LastJobOverTicks = elapsedTicks - budgetedTimeInTicks; // Note that this can be negative
                    TimeSpentInJobsTicks += elapsedTicks;
                    //if (elapsedTicks < budgetedTimeInTicks)
                    //    Debug.Log( "JobManager:  Job slice finished earlier than budget!" );
                }

                if (!jobDone)
                {
                    if (timeoutStopwatch.ElapsedTicks < RegisteredJobs[id].TimeoutTimeInTicks)
                    {
                        CurrentlyRunningJobId = savedCurrentlyRunningJobId;

                        yield return null;

                        savedCurrentlyRunningJobId = CurrentlyRunningJobId;
                        CurrentlyRunningJobId = id;
                    }
                    else
                    {
                        Debug.Log( "JobManager:  \"" + jobName + "\" has timed out and is therefore being cancelled" );
                        OnCancelInternal( id, true );
                        jobDone = true;
                        timedOut = true;
                    }
                }
            } while (!jobDone);


            if ( exceptionOccurred )
            {
                var spawningJobId = RegisteredJobs[ id ].JobIdToUnPauseWhenDone;

                // Cancel this job, and all jobs spawned by this job
                CancelJob( id, true, exception );

                // Cancel all job(s) that spawned this job
                while (spawningJobId != UInt32.MaxValue)
                {
                    var nextSpawningJobId = RegisteredJobs[ spawningJobId ].JobIdToUnPauseWhenDone;

                    CancelJob( spawningJobId );

                    if ( savedCurrentlyRunningJobId == spawningJobId )
                        savedCurrentlyRunningJobId = UInt32.MaxValue;

                    spawningJobId = nextSpawningJobId;
                }
            }
            else if (!timedOut)
            {
                var onCompletion = RegisteredJobs[id].JobCompletionHandler;
                var jobIdToUnPauseWhenDone = RegisteredJobs[id].JobIdToUnPauseWhenDone;

                RegisteredJobs.Remove(id);

                //Debug.Log("JobManager:  \"" + jobName + "\" completed");

                if (jobIdToUnPauseWhenDone != UInt32.MaxValue)
                    if (!UnPauseJob(jobIdToUnPauseWhenDone))
                        Debug.Log("JobManager:  Warning: \"" + jobName + "\" tried to unpause job id " + jobIdToUnPauseWhenDone + " but wasn't able to");

                if (onCompletion != null)
                    onCompletion(id); // Call the caller's 'on completion' code if it was provided
            }

            CurrentlyRunningJobId = savedCurrentlyRunningJobId;
        }

        private void DumpJobExceptionMessage( uint jobId, Exception exception )
        {
            if ( ExceptionVerbosityLevel > 0 )
            {
                string message = "JobManager:  Exception in job \"" + RegisteredJobs[jobId].Name + "\"";
                if ( ExceptionVerbosityLevel > 1 )
                {
                    message += ": " + exception.Message;
                    if ( ExceptionVerbosityLevel > 2 )
                        message += "\n" + exception.StackTrace;
                }
                Debug.LogError(message);
            }
        }

        private void OnCancelInternal(uint id, bool timedOut, Exception exception = null)
        {
            var onCancel = RegisteredJobs[id].JobCancellationHandler;
            RegisteredJobs.Remove(id);

            if (onCancel != null)
                onCancel(id, timedOut, exception); // Call the caller's 'cleanup' code if it was provided
        }
    }
}

