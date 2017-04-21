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

using JetBrains.Annotations;
using UnityEngine;

namespace Utility
{
    [UsedImplicitly]
    public class CameraFacingObject : MonoBehaviour
    {
        public bool CameraFacing = false; // as opposed to aligning to the camera's view direction

        //[UsedImplicitly]
        //private void OnWillRenderObject()
        //{
        //    if(CameraFacing)
        //        transform.LookAt(Camera.current.transform, Vector3.up);
        //    else
        //        transform.rotation = Camera.current.transform.rotation;
        //}

        [UsedImplicitly]
        public void Update()
        {
            if(!CameraFacing)
                transform.rotation =
                    CameraLocaterSatellite.MasterCameraLocater
                        .FoundCamera.transform.rotation;
            else
            {
                transform.LookAt(CameraLocaterSatellite.MasterCameraLocater.FoundCamera.transform, Vector3.up);
            }
        }



    }
}
