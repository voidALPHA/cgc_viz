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
using System.Linq;
using System.Text;
using Choreography.Recording;
using UnityEngine;

namespace Utility
{
    public class CameraLocaterSatellite : MonoBehaviour
    {
        private static CameraLocaterSatellite m_MasterCameraLocater;

        public static CameraLocaterSatellite MasterCameraLocater
        {
            get
            {
                if (m_MasterCameraLocater == null)
                {
                    m_MasterCameraLocater = new GameObject().AddComponent<CameraLocaterSatellite>();
                    m_MasterCameraLocater.LocateCamera();
                }
                return m_MasterCameraLocater;
            }
        }

        public Camera FoundCamera { get; set; }

        private void LocateCamera()
        {
            if(RecordingLord.IsRecording()) return;
            //FoundCamera = Camera.allCameras.Where(c => c.enabled).OrderByDescending(c => c.depth).First();
            FoundCamera = Camera.allCameras.Where(c => c.enabled && ((c.cullingMask & LayerMask.NameToLayer("UI")) != 0)).OrderByDescending(c => c.depth).First();
        }

        private void Update()
        {
            LocateCamera();
        }

    }
}
