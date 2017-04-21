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

using Choreography.Recording;
using UnityEngine;
using Utility;
using Network = Utility.NetworkSystem.Network;

namespace Scripts.Utility.Misc
{
    public class HaxxisGlobalSettings
    {
        private static HaxxisGlobalSettings s_Instance;
        public static HaxxisGlobalSettings Instance
        {
            get
            {
                return s_Instance ?? (s_Instance = new HaxxisGlobalSettings());
            }
        }

        private bool? m_IsVgsJob = null;
        public bool? IsVgsJob
        {
            get { return m_IsVgsJob ?? ( m_IsVgsJob = CommandLineArgs.IsPresent( "isVGSJob" ) ); }
        }

        private bool? m_disableEditor = null;
        public bool? DisableEditor
        {
            get { return m_disableEditor ?? (m_disableEditor = CommandLineArgs.IsPresent("disableEditor")); }
        }

        private int m_InstructionLimit = -1;
        public int InstructionLimit
        {
            get
            {
                if ( m_InstructionLimit == -1 )
                {
                    int overriddenValue;
                    m_InstructionLimit =
                        CommandLineArgs.GetArgumentValue( "instructionLimit", out overriddenValue )
                        ? overriddenValue
                        : 500000;
                }

                return m_InstructionLimit;
            }
        }


        private bool HasReportedError { get; set; }

        public void ReportVgsError( int errorCode, string message = "" )
        {
            if ( HasReportedError )
                return;

            var logMessage = errorCode + ( string.IsNullOrEmpty( message ) ? "" : " (" + message + ")" );

            if ( Instance.IsVgsJob == true )
            {
                var vgsJobId = CommandLineArgs.GetArgumentValue( "jobID" );
                var masterServerSessionId = CommandLineArgs.GetArgumentValue( "MSSID" );
                if ( !string.IsNullOrEmpty( vgsJobId ) && !string.IsNullOrEmpty( masterServerSessionId ) )
                {
                    Network.Instance.SendRequest( "localhost:8004/JobResult/" + vgsJobId + "/" + masterServerSessionId + "/" + errorCode );
                }
                else
                    Debug.LogError( "Intended to report VGS error, but could not get job or session id: " + logMessage );

                HasReportedError = true;
            }
            else
            {
                Debug.LogError( "VGS Error Reported, but not in VGS job: " + logMessage );
            }
        }

        public void ReportVgsVideoDuration()
        {
            var vgsJobId = CommandLineArgs.GetArgumentValue("jobID");
            var masterServerSessionId = CommandLineArgs.GetArgumentValue("MSSID");
            if (!string.IsNullOrEmpty(vgsJobId) && !string.IsNullOrEmpty(masterServerSessionId))
            {
                Network.Instance.SendRequest("localhost:8004/VideoDuration/" + vgsJobId + "/" + masterServerSessionId + "/" + RecordingLord.FrameTotal + "/" + RecordingLord.Duration);
            }
            else
                Debug.LogError("Intended to report VGS video duration, but could not get job or session id");
        }
    }
}