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
using UnityEngine;

public class RecordingSatellite : MonoBehaviour
{
    [NonSerialized]
    public RenderTexture renderTexture;

    private Camera m_cam;
    public Camera cam { get { return m_cam ?? GetComponent<Camera>(); } }

    //private void Start()
    //{
    //    renderTexture = new RenderTexture(1920, 1080, 24);
    //}

    private void OnPreRender()
    {
        if(RecordingLord.IsRecording())
        {
            cam.targetTexture = renderTexture;
        }
    }

    private void OnPostRender()
    {
        if(RecordingLord.IsRecording())
        {
            cam.targetTexture = null;

            if(RecordingLord.MainSatellite == this)
            {
                if(SystemInfo.graphicsDeviceID != 0)
                    Graphics.Blit(renderTexture, null as RenderTexture);
                RecordingLord.RenderedFrame(renderTexture);
            }
        }
    }
}
