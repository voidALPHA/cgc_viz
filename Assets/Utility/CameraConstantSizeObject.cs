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
    [ExecuteInEditMode]
    public class CameraConstantSizeObject : MonoBehaviour
    {
        [SerializeField]
        private float m_Size = 100.0f;
        public float Size { get { return m_Size; } set { m_Size = value; } }

        [UsedImplicitly]
        private void Update()
        {
            var cam = CameraLocaterSatellite.MasterCameraLocater.FoundCamera;
            var camtr = cam.transform;
            var distance = Vector3.Dot( transform.position - camtr.position, camtr.forward );
            var width = cam.pixelWidth;
            var height = cam.pixelHeight;
            var denom = Mathf.Sqrt( width * width + height * height ) * Mathf.Tan( cam.fieldOfView * Mathf.Deg2Rad );
            var scale = Mathf.Max( 0.0001f, distance / denom * Size );

            transform.localScale = new Vector3( scale, scale, scale );
        }
    }
}