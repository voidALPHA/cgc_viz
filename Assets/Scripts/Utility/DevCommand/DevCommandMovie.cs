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
using Choreography.Recording;
using Assets.Utility;
using UnityEngine;
using Utility.JobManagerSystem;

namespace Utility.DevCommand
{
    public class DevCommandMovie : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "movie";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "For movie recording";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Option m_StartOption = new DevCommandManager.Option("start", "Start recording movie, with optional specified filename", "mutuallyExclusive",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "fileName", typeof (string), false )
            });
        [DevCommandOption]
        private DevCommandManager.Option StartOption { get { return m_StartOption; } set { m_StartOption = value; } }

        private DevCommandManager.Option m_StopOption = new DevCommandManager.Option("stop", "Stop recording movie", "mutuallyExclusive");
        [DevCommandOption]
        private DevCommandManager.Option StopOption { get { return m_StopOption; } set { m_StopOption = value; } }

        private DevCommandManager.Option m_PauseOption = new DevCommandManager.Option("pause", "Pause recording", "mutuallyExclusive");
        [DevCommandOption]
        private DevCommandManager.Option PauseOption { get { return m_PauseOption; } set { m_PauseOption = value; } }

        private DevCommandManager.Option m_ResumeOption = new DevCommandManager.Option("resume", "Resume recording", "mutuallyExclusive");
        [DevCommandOption]
        private DevCommandManager.Option ResumeOption { get { return m_ResumeOption; } set { m_ResumeOption = value; } }

        private DevCommandManager.Option m_FrameRateOption = new DevCommandManager.Option("frameRate", "Set frame rate", "mutuallyExclusiveFR",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "frameRate", typeof (int), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option FrameRateOption { get { return m_FrameRateOption; } set { m_FrameRateOption = value; } }

        private DevCommandManager.Option m_FrameRateFromCmdLineArgOption = new DevCommandManager.Option("frameRateFromCmdLineArg", "Set frame rate, using the value specified by the named command line argument", "mutuallyExclusiveFR",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option FrameRateFromCmdLineArgOption { get { return m_FrameRateFromCmdLineArgOption; } set { m_FrameRateFromCmdLineArgOption = value; } }

        private DevCommandManager.Option m_JpegQualityOption = new DevCommandManager.Option("jpegQuality", "Set Exported JPEG Quality", "mutuallyExclusiveJQ",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "jpegQuality", typeof (int), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option JpegQualityOption { get { return m_JpegQualityOption; } set { m_JpegQualityOption = value; } }

        private DevCommandManager.Option m_JpegQualityFromCmdLineArgOption = new DevCommandManager.Option("jpegQualityFromCmdLineArg", "Set Exported JPEG Quality using the value specified by the named command line argument", "mutuallyExclusiveJQ",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option JpegQualityFromCmdLineArgOption { get { return m_JpegQualityFromCmdLineArgOption; } set { m_JpegQualityFromCmdLineArgOption = value; } }

        private DevCommandManager.Option m_SetVideoCodecOption = new DevCommandManager.Option("setVC", "Set FFMPEG-supported video codec", "mutuallyExclusiveVC",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "videoCodecName", typeof (string), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option SetVideoCodecOption { get { return m_SetVideoCodecOption; } set { m_SetVideoCodecOption = value; } }

        private DevCommandManager.Option m_SetVideoCodecFromCmdLineArgOption = new DevCommandManager.Option("setVCFromCmdLineArg", "Set FFMPEG-supported video codec using the value specified by the named command line argument", "mutuallyExclusiveVC",
            new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "cmdLineArg", typeof (string), true )
            });
        [DevCommandOption]
        private DevCommandManager.Option SetVideoCodecFromCmdLineArgOption { get { return m_SetVideoCodecFromCmdLineArgOption; } set { m_SetVideoCodecFromCmdLineArgOption = value; } }


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);
        }

        public bool Execute()
        {
            var videoCodecSet = false;
            //var videoCodecIndex = -1;

            if(SetVideoCodecOption.IsPresent)
            {
                var name = (string)SetVideoCodecOption.Arguments[0].Value;
                if(RecordingLord.CheckCodec(name))
                {
                    videoCodecSet = true;
                    RecordingLord.Vcodec = name;
                }
                    
                else
                {
                    Debug.Log("Error:  No match found for video codec \"" + name + "\"");
                    return false;
                }
            }

            if(SetVideoCodecFromCmdLineArgOption.IsPresent)
            {
                var videoCodecString = CommandLineArgs.GetArgumentValue((string)SetVideoCodecFromCmdLineArgOption.Arguments[0].Value);

                if(string.IsNullOrEmpty(videoCodecString))
                {
                    // Commenting this out to help avoid confusion
                    //Debug.Log("Error:  Cannot set video codec because that command line argument was not found or had no value");
                    return true;    // Don't report this as an error
                }
                
                if(RecordingLord.CheckCodec(videoCodecString))
                {
                    videoCodecSet = true;
                    RecordingLord.Vcodec = videoCodecString;
                }
                else
                {
                    Debug.Log("Error:  No match found for video codec \"" + videoCodecString + "\"");
                    return false;
                }
            }

            if(videoCodecSet)
            {
                Debug.Log("Video codec index set to " + RecordingLord.Vcodec);
            }

            var frameRateSet = false;
            var frameRate = 30;

            if ( FrameRateOption.IsPresent )
            {
                frameRate = (int)FrameRateOption.Arguments[0].Value;
                frameRateSet = true;
            }

            if ( FrameRateFromCmdLineArgOption.IsPresent )
            {
                var frameRateString = CommandLineArgs.GetArgumentValue((string)FrameRateFromCmdLineArgOption.Arguments[0].Value);

                if (string.IsNullOrEmpty(frameRateString))
                {
                    //Debug.Log("Error:  Cannot set frame rate because that command line argument was not found or had no value");
                    return false;
                }

                object value = null;
                if ( !frameRateString.StringToValueOfType( typeof( int ), ref value, true ) )
                    return false;

                frameRate = (int)value;
                frameRateSet = true;
            }

            if(frameRateSet)
            {
                RecordingLord.FrameRate = frameRate;
                Debug.Log("Movie frame rate set to " + RecordingLord.FrameRate);
            }

            var jqualitySet = false;
            var jquality = 100;

            if(JpegQualityOption.IsPresent)
            {
                jquality = (int)JpegQualityOption.Arguments[0].Value;
                jqualitySet = true;
            }

            if(JpegQualityFromCmdLineArgOption.IsPresent)
            {
                var jqualityString = CommandLineArgs.GetArgumentValue((string)JpegQualityFromCmdLineArgOption.Arguments[0].Value);

                if(string.IsNullOrEmpty(jqualityString))
                {
                    //Debug.Log("Error:  Cannot set frame rate because that command line argument was not found or had no value");
                    return false;
                }

                object value = null;
                if(!jqualityString.StringToValueOfType(typeof(int), ref value, true))
                    return false;

                jquality = (int)value;
                jqualitySet = true;
            }

            if(jqualitySet)
            {
                RecordingLord.JpegQuality = jquality;
                Debug.Log("Movie JPEG quality set to " + RecordingLord.JpegQuality);
            }

            if (StartOption.IsPresent)
            {
                if (!RecordingLord.IsRecording())
                {
                    // We start recording in a job, which happens as a coroutine.  This fixes (11/10/2015) an issue where videos
                    // were black for several seconds at the beginning, which I think was related to choreography-triggered
                    // video recording that were also being started in jobs.

                    var filename = (string)StartOption.Arguments[ 0 ].Value;
                    JobManager.Instance.StartJob(StartRecordingMovie(filename), jobName: "RecordMovieFromDevCmd");
                }
                else
                {
                    Debug.Log( "Error:  Movie already being captured" );
                    return false;
                }
            }

            if (StopOption.IsPresent)
            {
                if (RecordingLord.IsRecording())
                {
                    RecordingLord.StopRecording();
                    Debug.Log("Frames captured: " + RecordingLord.FrameTotal + "; total duration " + RecordingLord.Duration + " seconds");
                }          
                else
                {
                    Debug.Log( "Error:  No movie being captured" );
                    return false;
                }
            }

            if ( PauseOption.IsPresent )
            {
                if ( !RecordingLord.IsRecording() )
                {
                    Debug.Log("Error:  No movie being captured");
                    return false;
                }
                if ( RecordingLord.IsPaused() )
                {
                    Debug.Log("Warning:  Pause request made but movie recording is already paused");
                    return true;
                }
                RecordingLord.PauseRecording();
                Debug.Log("Movie recording paused; duration so far is " + RecordingLord.Duration + " seconds");
            }

            if ( ResumeOption.IsPresent )
            {
                if ( !RecordingLord.IsRecording() )
                {
                    Debug.Log("Error:  No movie being captured");
                    return false;
                }
                if (!RecordingLord.IsPaused())
                {
                    Debug.Log("Warning:  Resume request made but movie recording is not paused");
                    return true;
                }
                RecordingLord.ResumeRecording();
                Debug.Log("Movie recording resumed");
            }

            return true;
        }

        private IEnumerator StartRecordingMovie(string filename)
        {
            RecordingLord.StartRecording(filename);
            yield return null;
        }
    }
}
