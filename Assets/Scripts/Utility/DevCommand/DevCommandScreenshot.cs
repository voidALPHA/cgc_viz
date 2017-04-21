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
using System.IO;
using UnityEngine;

namespace Utility.DevCommand
{
    public class DevCommandScreenshot : MonoBehaviour, IDevCommand
    {
        private readonly string m_Name = "screenshot";
        public string Name { get { return m_Name; } }

        private List<DevCommandManager.Argument> m_Arguments = new List<DevCommandManager.Argument>();
        public List<DevCommandManager.Argument> Arguments { get { return m_Arguments; } set { m_Arguments = value; } }

        private List<DevCommandManager.Option> m_Options = new List<DevCommandManager.Option>();
        public List<DevCommandManager.Option> Options { get { return m_Options; } set { m_Options = value; } }

        private readonly string m_HelpTextBrief = "Takes a screenshot and saves to disk as png";
        public string HelpTextBrief { get { return m_HelpTextBrief; } }

        public string HelpTextFull { get; set; }


        private DevCommandManager.Argument m_FileName = new DevCommandManager.Argument("fileName", typeof(string), false, "Optional filename");
        [DevCommandArgumentAttribute]
        private DevCommandManager.Argument FileName { get { return m_FileName; } set { m_FileName = value; } }

        // This is an option that has an argument:
        private DevCommandManager.Option m_SizeMultiplierOption = new DevCommandManager.Option
            ("superSize", "Factor to multiply resolution for larger images", new List<DevCommandManager.Argument>()
            {
                new DevCommandManager.Argument( "sizeMultiplier", typeof (int), true )
            });
        [DevCommandOptionAttribute]
        private DevCommandManager.Option SizeMultiplierOption { get { return m_SizeMultiplierOption; } set { m_SizeMultiplierOption = value; } }


        [SerializeField]
        private string m_CaptureFolder = "Captures/";
        private string CaptureFolder { get { return m_CaptureFolder; } set { m_CaptureFolder = value; } }

        private bool WaitingForScreenShot = false;
        private string ActualFileName = "";


        private void Start()
        {
            DevCommandManager.Instance.RegisterDevCommand(this);
        }

        public bool Execute()
        {
            if (!Directory.Exists( CaptureFolder ))
                Directory.CreateDirectory( CaptureFolder );

            var timeStamp = DateTime.Now.ToString( "_yyyyMMdd_HHmmss" );

            if (FileName.IsPresent)
                ActualFileName = FileName.Value + timeStamp;
            else
                ActualFileName = "Screenshot" + timeStamp;
            ActualFileName = Path.ChangeExtension(ActualFileName, "png");

            var superSizeFactor = SizeMultiplierOption.IsPresent ? (int) SizeMultiplierOption.Arguments[0].Value : 1;

            // Apparently CaptureScreenshot does not accept any path...just a filename.  If a path is also provided, no file gets written!  Unity 5.0.1f1  pterry 10/1/2015
            Application.CaptureScreenshot(ActualFileName, superSizeFactor);

            WaitingForScreenShot = true;

            DevCommandManager.Instance.StartWait(); // Prevent further dev commands from executing until after the screen shot is fully written out (useful e.g. in smoke test when we do a final screen shot and then quit)

            return true;
        }

        // We need to wait a frame after making the CaptureScreenshot call, because Unity makes the screenshot at the end of the frame!

        // NOTES:
        // In a standalone build, sometimes this takes multiple frames to execute...typically 6 to 8 frames at the moment.   pterry 10/1/2015
        // This would be an issue if multiple screenshots are requested soon after one another
        
        private void Update()
        {
            if (WaitingForScreenShot)
            {
                var sourceFile = ActualFileName;
                if (!Application.isEditor)
                    sourceFile = Application.dataPath + "/" + sourceFile;

                if ( File.Exists( sourceFile ) )
                {
                    if ( !IsFileLocked( sourceFile ) )
                    {
                        WaitingForScreenShot = false;

                        var destPathAndFilename = CaptureFolder + ActualFileName;
                        File.Move(sourceFile, destPathAndFilename);

                        Debug.Log("Screenshot saved at " + Path.GetFullPath(destPathAndFilename));

                        DevCommandManager.Instance.EndWait();
                    }
                }
            }
        }

        private static bool IsFileLocked(string filename)
        {
            var locked = false;
            try
            {
                FileStream fs = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                fs.Close();
            }
            catch (IOException)
            {
                locked = true;
            }
            return locked;
        }
    }
}
