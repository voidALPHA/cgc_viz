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

using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Scripts.Utility.Misc;
using UnityEngine;
using Utility;
using Utility.Misc;

// Not sure if this is used anymore...cuz the CameraControLordPrefab now uses "SplineCameraControlLord.cs"
namespace Choreography.CameraControl
{
    public class CameraControlLord : MonoBehaviour
    {
        [SerializeField]
        private static CameraControlLord s_Instance;
        public static CameraControlLord Instance { get { return s_Instance ?? (s_Instance = FindObjectOfType<CameraControlLord>()); } }

        [Header("Scene References")]
        [SerializeField]
        private Camera m_MainCamera = null;
        public static Camera MainCamera { get { return Instance.m_MainCamera; } }

        private static Queue<ICameraMovement> m_CameraMovements = new Queue<ICameraMovement>();
        private static Queue<ICameraMovement> CameraMovements
        {
            get { return m_CameraMovements; } 
            set { m_CameraMovements = value; } 
        }

        private IEnumerator ActiveMovementEnumeration { get; set; } 

        public void QueueCameraMovement(ICameraMovement movement)
        {
            CameraMovements.Enqueue(movement);
        }

        public void CancelCameraMovement( ICameraMovement movement )
        {
            ActiveMovementEnumeration = null;
        }

        [UsedImplicitly]
        private void Update()
        {
            if (ActiveMovementEnumeration == null)
            {
                var nextMovement = CameraMovements.DequeueToFirstOrDefault( movement => !movement.Cancelled );

                if ( nextMovement == null )
                {
                    // Convenience hot key for pointing camera to origin, for better Haxxis usability (but not during a camera sequence as in choreography)
                    if (Input.GetButtonDown("Point camera to origin") && HaxxisGlobalSettings.Instance.IsVgsJob == false)
                    {
                        MainCamera.transform.LookAt(Vector3.zero);
                    }

                    return;
                }

                nextMovement.MovementComplete += HandleMovementCompleted;
                ActiveMovementEnumeration = nextMovement.MovementEnumerator();
            }

            if (ActiveMovementEnumeration != null)
                ActiveMovementEnumeration.MoveNext();
        }

        public void HandleMovementCompleted()
        {
            ActiveMovementEnumeration = null;
        }
    }
}
