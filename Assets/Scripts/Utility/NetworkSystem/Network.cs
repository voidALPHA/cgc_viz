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

#if UNITY_EDITOR
    using UnityEditor;
#endif


namespace Utility.NetworkSystem
{
    public class Network : MonoBehaviour
    {
        public static Network Instance { get; private set; }

        public class PendingRequest
        {
            public PendingRequest( int id, WWW request, bool silent, float timeoutSecs, bool lifelineRequest )
            {
                Id = id;
                Request = request;
                Silent = silent;
                TimeoutSecs = timeoutSecs;
                LifelineRequest = lifelineRequest;

                RequestStartTime = Time.unscaledTime;
                TimedOut = false;
            }

            public int Id;
            public WWW Request;
            public bool Silent;
            public float TimeoutSecs;
            public bool LifelineRequest;

            public float RequestStartTime;
            public bool TimedOut;
        }

        private List<PendingRequest> m_PendingRequests = new List<PendingRequest>();
        public List<PendingRequest> PendingRequests { get { return m_PendingRequests; } set { m_PendingRequests = value; } }

        private int m_NextRequestId = 1000;
        private int NextRequestId { get { return m_NextRequestId; } set { m_NextRequestId = value; } }

        private bool m_IsWaiting;
        private bool IsWaiting
        {
            get { return m_IsWaiting; }
            set
            {
                WaitingIndicator.enabled = WaitingIndicatorDisplayOverride ? value : false;
                m_IsWaiting = value;
            }
        }

        private Canvas m_WaitingIndicator;
        private Canvas WaitingIndicator { get { return m_WaitingIndicator; } set { m_WaitingIndicator = value; } }

        private bool m_WaitingIndicatorDisplayOverride = true;
        public bool WaitingIndicatorDisplayOverride
        {
            get
            {
                return m_WaitingIndicatorDisplayOverride;
            }
            set
            {
                if (value && IsWaiting)
                    WaitingIndicator.enabled = true;
                else if (!value)
                    WaitingIndicator.enabled = false;

                m_WaitingIndicatorDisplayOverride = value;
            }
        }



        private void Awake()
        {
            Instance = this;

            WaitingIndicator = GetComponentInChildren<Canvas>();
            WaitingIndicator.enabled = false;
        }

        private void Update()
        {
            if ( IsWaiting )
            {
                var curTime = Time.unscaledTime;

                foreach ( var request in PendingRequests )
                {
                    if ( request.Request.isDone )
                    {
                        if (!request.Silent)
                            Debug.Log("Request DONE: " + request.Request.url + " and the error return text is: " + request.Request.error + "; the returned text is: "+ request.Request.text);

                        if (request.LifelineRequest)
                        {
                            if ( request.Request.text == "Kill" )
                            {
                                Debug.Log("Network system lifeline request response received; triggering application to kill itself");
                            #if UNITY_EDITOR
                                if (EditorApplication.isPlaying)
                                    EditorApplication.isPlaying = false;
                                else
                            #endif
                                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                            }
                            else
                            {
                                // This was a lifeline request whose >Unity WWW object has timed out.<  We have no control to set timeout on a WWW object
                                Debug.Log("Network system lifeline request's WWW object has timed out; creating a new request");
                                ResendRequest(request);
                            }
                        }
                    }
                    else
                    {
                        if ( curTime - request.RequestStartTime > request.TimeoutSecs )
                        {
                            request.TimedOut = true;
                            if ( !request.Silent )
                                Debug.Log( "Request TIMED OUT after " + request.TimeoutSecs + " seconds: " + request.Request.url );
                        }
                    }
                }

                PendingRequests.RemoveAll(x => x.Request.isDone || x.TimedOut);

                if ( PendingRequests.Count == 0 )
                    IsWaiting = false;
            }
        }

        // Could be expanded to handle POST commands in addition to GET (which is implemented here)
        public int SendRequest(string url, bool silent = false, float timeoutSecs = Single.MaxValue, bool lifelineRequest = false)
        {
            if (!silent)
                Debug.Log("Sending request: " + url );

            var id = NextRequestId++;
            var request = new PendingRequest(id, new WWW( url ), silent, timeoutSecs, lifelineRequest);
            PendingRequests.Add(request);

            IsWaiting = true;

            return id;
        }

        public void ResendRequest(PendingRequest request)
        {
            request.Request = new WWW( request.Request.url );
        }

        // IDEA:  Could add a '-kill <id>' option in the dev command; ultimately it would call www.Dispose to cancel the operation, per Unity docs
        public void CancelRequest(int id) // For future, possibly
        {
            throw new NotImplementedException( "CancelRequest not implemented" );
        }
    }
}

