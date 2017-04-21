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
using System.IO.Pipes;
using System.Diagnostics;
using System.Threading;
using Environmental.RoundLabel;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace Choreography.Recording
{
    public class RecordingLord : MonoBehaviour
    {

        public static event Action RecordingStarted = delegate { };

        public static event Action RecordingStopped = delegate { };

        public static event Action RecordingPaused = delegate { };

        public static event Action RecordingResumed = delegate { };

        private static RenderTexture m_rt;

        private static RenderTexture RT
        {
            get
            {
                return m_rt ??
                       (m_rt = new RenderTexture(Screen.currentResolution.width, Screen.currentResolution.height, 24)
                       {
                           antiAliasing = QualitySettings.antiAliasing,
                           anisoLevel = 1,
                           autoGenerateMips = false
                       });
            }
            set { m_rt = value; }
        }

        private static RecordingLord s_Instance;

        private static RecordingLord Instance
        {
            get { return s_Instance ?? (s_Instance = FindObjectOfType<RecordingLord>()); }
        }

        [SerializeField] private Camera[] m_Cameras;

        private Camera[] Cameras
        {
            get { return m_Cameras; }
        }

        private static RecordingSatellite m_MainSatellite;

        public static RecordingSatellite MainSatellite
        {
            get
            {
                return m_MainSatellite ??
                       (m_MainSatellite =
                           Instance.Cameras[Instance.Cameras.Length - 1].GetComponent<RecordingSatellite>());
            }
        }

        private static bool Recording { get; set; }

        [SerializeField] private int m_FrameRate = 30;

        public static int FrameRate
        {
            get { return Instance.m_FrameRate; }
            set { Instance.m_FrameRate = value; }
        }

        private static Process FfmpegProcess { get; set; }
        private static Stream FfmpegStream { get; set; }

        public static int FrameTotal { get; set; }

        private static float StartTime { get; set; }
        public static float Duration { get; set; }

#if UNITY_EDITOR
        private const string FfmpegLocation = "WinIncludes/Utilities/ffmpeg.exe";
#elif UNITY_STANDALONE_WIN
        private const string FfmpegLocation = "Utilities/ffmpeg.exe";
#elif UNITY_STANDALONE_LINUX
        private const string FfmpegLocation = "Utilities/ffmpeg";
#elif UNITY_STANDALONE_OSX
        private const string FfmpegLocation = "Utilities/ffmpeg";
#endif


        public static string Vcodec = "libx264";
        public static int JpegQuality = 100;

        private static readonly HashSet<string> ValidCodecs = new HashSet<string>(new string[]
        {
            "a64multi", "a64multi5", "alias_pix", "amv", "apng", "asv1", "asv2", "avrp", "avui", "ayuv", "bmp",
            "libxavs", "cinepak", "cljr", "vc2", "dnxhd", "dpx", "dvvideo", "ffv1", "ffvhuff", "flashsv", "flashsv2",
            "flv", "gif", "h261", "h263", "h263p", "libx264", "libx264rgb", "libopenh264", "h264_nvenc", "h264_qsv",
            "nvenc", "nvenc_h264", "hap", "libx265", "nvenc_hevc", "hevc_evenc", "hevc_qsv", "huffyuv", "jpeg2000",
            "libopenjpeg", "jpegls", "ljpeg", "mjpeg", "mpeg1video", "mpeg2video", "mpeg2_qsv", "mpeg4", "libxvid",
            "msmpeg4v2", "msmpeg4", "msvideo1", "pam", "pbm", "pcx", "pgm", "pgmyuv", "png", "ppm", "prores",
            "prores_aw", "prores_ks", "qrtle", "r10k", "r210", "rawvideo", "roqvideo", "rv10", "rv20", "sgi", "snow",
            "sunrast", "targa", "libtheora", "tiff", "utvideo", "v210", "v308", "v408", "v410", "libvpx", "libvpx-vp9",
            "libwebp", "wmv1", "wmv2", "wrapped_avframe", "xbm", "xface", "xwd", "y41p", "yuv4", "zlib", "zmbv"
        });

        private void Start()
        {
            FrameRate = 30;
        }

        //private void LateUpdate()
        //{
        //    if(!Recording) return;

        //    //foreach(var cam in m_Cameras)
        //    //{
        //    //    cam.targetTexture = RT;
        //    //    cam.Render();
        //    //    cam.targetTexture = null;
        //    //}

        //    //MainSatellite.renderTexture = RT;

        //    //RenderedFrame(RT);

        //    //if(!FfmpegProcess.StandardOutput.EndOfStream)
        //    //{
        //    //    Debug.Log("[FFMPEG] " + FfmpegProcess.StandardOutput.ReadToEnd());
        //    //}
        //    //if(!FfmpegProcess.StandardError.EndOfStream)
        //    //{
        //    //    Debug.LogError("[!!FFMPEG!!] " + FfmpegProcess.StandardError.ReadToEnd());
        //    //}
        //}

        private static Texture2D img { get; set; }

        public static void RenderedFrame(RenderTexture rt)
        {
            if(!Recording) return;

            var prevRt = RenderTexture.active;
            RenderTexture.active = rt;

            if(img == null) img = new Texture2D(rt.width, rt.height);
            img.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            img.Apply();

            RenderTexture.active = prevRt;

            //Instance.StartCoroutine(SendToFfmpeg(img, FfmpegFrameRendered++));

            var bytes = img.EncodeToJPG(JpegQuality);
            FfmpegStream.Write(bytes, 0, bytes.Length);
            FfmpegStream.Flush();

            FrameTotal++;
        }

        public static void StartRecording(string filename)
        {
            if(Recording) return;

            RoundLabelBehaviour.UpdateAllRoundLabels();

            FfmpegProcess = new Process
            {
                StartInfo =
                {
                    FileName = FfmpegLocation,
                    Arguments =
                        "-y -f image2pipe -r " + FrameRate + " -i pipe:.jpg -vcodec " + Vcodec + " -an -r " + FrameRate +
                        " " + filename,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = false,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                }
            };

            Time.captureFramerate = FrameRate;

            FfmpegProcess.Start();

            FrameTotal = 0;
            StartTime = Time.time;
            Duration = 0.0f;
            Recording = true;
            FfmpegStream = FfmpegProcess.StandardInput.BaseStream;

            RT = null;

            foreach(var cam in Instance.m_Cameras)
            {
                cam.GetComponent<RecordingSatellite>().renderTexture = RT;
            }

            RecordingStarted();
        }

        public static void PauseRecording()
        {
            if(!Recording) return;

            Duration += (Time.time - StartTime);

            Recording = false;

            Time.captureFramerate = 0;

            RecordingPaused();
        }

        public static void ResumeRecording()
        {
            if(!IsPaused()) return;

            StartTime = Time.time;

            Recording = true;

            Time.captureFramerate = FrameRate;

            RecordingResumed();
        }

        public static void StopRecording()
        {
            if(!Recording) return;

            Duration += (Time.time - StartTime);

            if(!Recording) return;

            Recording = false;

            //while(FfmpegFrameSent < FfmpegFrameRendered)
            //{
            //    Thread.Sleep(50);
            //}

            FfmpegStream.Close();
            FfmpegStream = null;
            FfmpegProcess.Close();
            FfmpegProcess = null;

            Time.captureFramerate = 0;

            RecordingStopped();
        }

        public static bool IsRecording()
        {
            return Recording;
        }

        public static bool IsPaused()
        {
            return !Recording && FfmpegProcess != null;
        }

        //private static IEnumerator SendToFfmpeg(Texture2D tex, int frame)
        //{
        //    var tr = new ThreadedRender
        //    {
        //        frame = frame,
        //        tex = tex
        //    };

        //    Thread t = new Thread(tr.Run);
        //    t.Start();
        //    while(!tr.sent) yield return null;
        //}

        //private class ThreadedRender
        //{
        //    public Texture2D tex;
        //    public int frame;

        //    public bool sent = false;

        //    public void Run()
        //    {
        //        var bytes = tex.EncodeToPNG();

        //        while(FfmpegFrameSent < frame - 1)
        //        {
        //            Thread.Sleep(100);
        //        }

        //        lock(FfmpegStream)
        //        {
        //            FfmpegStream.Write(bytes, 0, bytes.Length);
        //            FfmpegStream.Flush();
        //        }

        //        sent = true;
        //    }
        //}

        public static bool CheckCodec(string codec)
        {
            return ValidCodecs.Contains(codec);
        }
    }
}
