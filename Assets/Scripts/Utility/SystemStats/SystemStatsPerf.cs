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
    public class SystemStatsPerf : ISystemStats
    {
        public GameObject UnityObject { get; set; }

        public Text TextComponent { get; set; }

        public string StatString { get; set; }

        public void Update()
        {
            float rawDt = Time.unscaledDeltaTime;
            float rawMspf = rawDt * 1000.0f;
            float rawFps = rawDt <= 0.0f ? 0.0f : 1.0f / rawDt;

            float unityDt = Time.deltaTime;
            float unityMspf = unityDt * 1000.0f;
            float unityFps = unityDt <= 0.0f ? 0.0f : 1.0f / unityDt;

            float smoothDt = Time.smoothDeltaTime;
            float smoothMspf = smoothDt * 1000.0f;
            float smoothFps = Time.timeScale <= 0.0f ? 0.0f : 1.0f / smoothDt;

            StatString = String.Format("Real FPS:{0,6:N2}  Real MSPF:{1,6:N2}  Unity FPS:{2,6:N2}  Unity MSPF:{3,6:N2}  Smoothed FPS:{4,6:N2}  Smoothed MSPF:{5,6:N2}",
                rawFps, rawMspf, unityFps, unityMspf, smoothFps, smoothMspf);
        }
    }
}
