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

namespace Ui.CameraBoundsProvider
{

    public class CameraCornerProvider : MonoBehaviour
    {
        [SerializeField]
        private Transform m_TopLeft;
        public Transform TopLeft { get { return m_TopLeft; } }

        [SerializeField]
        private Transform m_TopRight;
        public Transform TopRight { get { return m_TopRight; } }


        [SerializeField]
        private Transform m_TopCenter;
        public Transform TopCenter { get { return m_TopCenter; } }

        [SerializeField]
        private Transform m_BottomLeft;
        public Transform BottomLeft { get { return m_BottomLeft; } }

        [SerializeField]
        private Transform m_BottomRight;
        public Transform BottomRight { get { return m_BottomRight; } }

        [SerializeField]
        private Transform m_BottomCenter;
        public Transform BottomCenter { get { return m_BottomCenter; } }


        public static CameraCornerProvider Instance { get; set; }

        private void Start()
        {
            if (Instance == null)
                Instance = this;
        }

        public static Transform GetCornerTransform(CameraCorner corner)
        {
            switch (corner)
            {
                case CameraCorner.TopCenter:
                    return Instance.TopCenter;
                case CameraCorner.BottomCenter:
                    return Instance.BottomCenter;
                
                case CameraCorner.TopRight:
                    return Instance.TopRight;
                case CameraCorner.TopLeft:
                    return Instance.TopLeft;
                case CameraCorner.BottomLeft:
                    return Instance.BottomLeft;
                case CameraCorner.BottomRight:
                    return Instance.BottomRight;
            }
            throw new Exception("Cannot find camera corner " + corner + "!");
        }
    }

}
