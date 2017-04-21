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

using System.Collections.Generic;
using System.Linq;
using Choreography.Views;
using JetBrains.Annotations;
using Scripts.Utility.Misc;
using UnityEngine;

namespace Choreography.CameraControl
{
    public class SplineCameraControlLord : MonoBehaviour
    {
        [SerializeField]
        private static SplineCameraControlLord s_Instance;
        public static SplineCameraControlLord Instance { get { return s_Instance; } set { s_Instance = value; } }
        
        [Header("Scene References")]
        [SerializeField]
        private Camera m_MainCamera = null;
        public static Camera MainCamera { get { return Instance.m_MainCamera; } }
        public static GameObject CameraParent { get { return MainCamera.transform.parent.gameObject; } }

        public static Transform PriorPoint { get; set; }
        public static Transform CurrentPoint { get; set; }
        public static Transform NextPoint { get; set; }
        
        public static Transform PriorLookPoint { get; set; }
        public static Transform CurrentLookPoint { get; set; }
        public static Transform NextLookPoint { get; set; }

        private static SplineMovement CurrentMovement { get; set; }
        private static SplineMovement CurrentLookMovement { get; set; }

        private static bool SwitchingMovements { get; set; }
        private static bool SwitchingLookMovements { get; set; }

        private static bool AwaitingStartPosition { get; set; }
        private static bool AwaitingStartLookPosition { get; set; }

        private static Queue< SplineMovement > s_WaitingMovements;
        private static Queue<SplineMovement> WaitingMovements { get
        {
            return s_WaitingMovements ?? ( s_WaitingMovements = new Queue< SplineMovement >() );
        } }

        private static Queue<SplineMovement> s_WaitingLookMovements;
        private static Queue<SplineMovement> WaitingLookMovements
        {
            get
            {
                return s_WaitingLookMovements ?? (s_WaitingLookMovements = new Queue<SplineMovement>());
            }
        }

        [SerializeField]
        private GameObject m_CameraLookPoint = null;
        private GameObject CameraLookPoint { get
        {
                if (m_CameraLookPoint==null)
                    m_CameraLookPoint = new GameObject( "Camera Facing Target" );
                return m_CameraLookPoint;
        } }

        public void Awake()
        {
            Instance = this;
        }

        private GameObject DefaultPoint { get; set; }
        private GameObject DefaultLookPoint { get; set; }

        public void StartPlayback()
        {

            if (DefaultPoint == null) 
                DefaultPoint= new GameObject("Camera Start Point");
            DefaultPoint.transform.parent = transform;
            DefaultPoint.transform.position = Vector3.zero;
            PriorPoint = DefaultPoint.transform;
            CurrentPoint = DefaultPoint.transform;

            if (DefaultLookPoint==null)
                DefaultLookPoint = new GameObject("Camera Look Start Point");
            DefaultLookPoint.transform.parent = transform;
            DefaultLookPoint.transform.position = Vector3.forward;
            PriorLookPoint = DefaultLookPoint.transform;
            CurrentLookPoint = DefaultLookPoint.transform;
            CameraLookPoint.transform.position = DefaultLookPoint.transform.position;

            AwaitingStartPosition = true;
            AwaitingStartLookPosition = true;
        }

        public static void ClearCameraMovements()
        {
            WaitingMovements.Clear();
            CurrentMovement = null;

            WaitingLookMovements.Clear();
            CurrentLookMovement = null;
        }

        public static void EnqueueMovement( SplineMovement nextMovement )
        {
            if ( CurrentMovement == null )
                AssignNextMovement(nextMovement);
            else 
                WaitingMovements.Enqueue( nextMovement );
        }

        public static void EnqueueLookMovement( SplineMovement nextLookMovement )
        {
            if ( CurrentLookMovement == null )
                AssignNextLookMovement( nextLookMovement);
            else
                WaitingLookMovements.Enqueue( nextLookMovement );
        }

        private static void ConcludeMovement( SplineMovement currentMovement )
        {
            PriorPoint = CurrentPoint;
            CurrentPoint = currentMovement.TargetTransform;
            
            if (WaitingMovements.Any())
                AssignNextMovement( WaitingMovements.Dequeue() );
            else
                CurrentMovement = null;
        }

        private static void ConcludeLookMovement( SplineMovement currentLookMovement )
        {
            PriorLookPoint = CurrentLookPoint;
            CurrentLookPoint = currentLookMovement.TargetTransform;

            if ( WaitingLookMovements.Any() )
                AssignNextLookMovement( WaitingLookMovements.Dequeue() );
            else
                CurrentLookMovement = null;
        }

        private static void AssignNextMovement( SplineMovement nextMovement )
        {

            CurrentMovement = nextMovement;

            if (AwaitingStartPosition)
            {
                PriorPoint = CurrentMovement.TargetTransform;
                CurrentPoint = CurrentMovement.TargetTransform;
                AwaitingStartPosition = false;
            }

            NextPoint = CurrentMovement.TargetTransform;
            CurrentMovement.OnMovementCompleted += HandleMovementCompleted;

            SwitchingMovements = true;
        }

        private static void HandleMovementCompleted(float finalProportion)
        {
            ConcludeMovement(CurrentMovement);
        }

        private static void AssignNextLookMovement(SplineMovement nextLookMovement)
        {
            CurrentLookMovement = nextLookMovement;

           // if (AwaitingStartLookPosition)
           // {
           //     PriorLookPoint = CurrentLookMovement.TargetTransform;
           //     CurrentLookPoint = CurrentLookMovement.TargetTransform;
           //     AwaitingStartLookPosition = false;
           // }

            NextLookPoint = CurrentLookMovement.TargetTransform;
            CurrentLookMovement.OnMovementCompleted += HandleLookMovementCompleted;

            SwitchingLookMovements = true;
        }

        private static void HandleLookMovementCompleted(float finalProportion)
        {
            ConcludeLookMovement(CurrentLookMovement);
        }

        [UsedImplicitly]
        private void Update()
        {
            bool isPlayingChoreography = TimelineViewBehaviour.Instance != null && TimelineViewBehaviour.Instance.IsPlayingState;

            if ( !isPlayingChoreography )
            {
                // Convenience hot key for pointing camera to origin, for better Haxxis usability while editing (but not during a camera sequence as in choreography)
                if (Input.GetButtonDown("Point camera to origin") && HaxxisGlobalSettings.Instance.IsVgsJob == false)
                {
                    MainCamera.transform.LookAt(Vector3.zero);
                }
            }
            else
            {
                if (CurrentMovement != null)
                    do
                    {
                        SwitchingMovements = false;
                        CameraParent.transform.position = CurrentMovement.MovementUpdate(PriorPoint, CurrentPoint, NextPoint, Time.time);
                    } while (SwitchingMovements);


                if (CurrentLookMovement != null)
                    do
                    {
                        SwitchingLookMovements = false;
                        CameraLookPoint.transform.position = CurrentLookMovement.MovementUpdate(PriorLookPoint,
                            CurrentLookPoint, NextLookPoint, Time.time);
                    } while (SwitchingLookMovements);
                UpdateCameraLook();
            }
        }
        
        private void UpdateCameraLook()
        {
            MainCamera.transform.LookAt(CameraLookPoint.transform.position, Vector3.up);
        }
    }
}
