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

using CameraScripting;
using UnityEngine;
using Utility.InputManagement;

namespace Utility
{
    public class CameraManagerBehaviour : MonoBehaviour
    {
        [SerializeField]
        private FreeCameraMode m_FreeCamera;

        private FreeCameraMode FreeCamera { get { return m_FreeCamera; } set { m_FreeCamera = value; } }

        private ICameraMode m_CurrentMode;
        private ICameraMode CurrentMode
        {
            get { return m_CurrentMode; }
            set
            {
                if (m_CurrentMode != null)
                {
                    m_CurrentMode.enabled = false;
                }
                m_CurrentMode = value;
                if (m_CurrentMode != null)
                {
                    m_CurrentMode.enabled = true;
                }
            }
        }

        private void Start()
        {
            CurrentMode = FreeCamera;
        }

        public void Update()
        {
            if (Input.GetButtonDown("Free-fly Camera") && !InputFocusManager.Instance.IsAnyInputFieldInFocus()) 
            {
                if (CurrentMode != null)
                {
                    CurrentMode = null;
                }
                else
                {
                    CurrentMode = FreeCamera;
                }

                //var currentView = FindObjectsOfType<UnityEngine.Camera>().OrderByDescending(c => c.depth).First().transform;
            }
        }

    }
}
