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
using UnityEngine;
using UnityEngine.UI;

namespace Utility.SystemStats
{
    public class SystemStatsTime : ISystemStats
    {
        public GameObject UnityObject { get; set; }

        public Text TextComponent { get; set; }

        public string StatString { get; set; }

        public void Update()
        {
            StatString = String.Format("TimeScale:{0,6:N3}  CaptureFramerate:{1,3}  TargetFrameRate:{2,3}  FrameCount:{3,7:N0}  RenderedFrameCount:{4,7:N0}",
                Time.timeScale, Time.captureFramerate, Application.targetFrameRate, Time.frameCount, Time.renderedFrameCount);
        }
    }
}