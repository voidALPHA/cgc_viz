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

using Choreography.Views;
using Scripts.Utility.Misc;
using UnityEngine;
using Utility;
using Utility.InputManagement;
using Utility.Misc;

namespace CameraScripting
{
    public class FreeCameraMode : MonoBehaviour, ICameraMode
    {
        #region Properties

        private static bool m_FreeCameraDisabled = false;
        public static bool FreeCameraDisabled { get {return m_FreeCameraDisabled;} set { m_FreeCameraDisabled = value; } }

        [SerializeField]
        private bool m_IsPaused = false;
        public bool IsPaused
        {
            get { return m_IsPaused; }
            protected set { m_IsPaused = value; }
        }

        [SerializeField]
        private Camera m_LocalCamera;
        public Camera LocalCamera
        {
            get
            {

                var subCamera = 
                    m_LocalCamera ?? (m_LocalCamera = GetComponent<Camera>());

                return subCamera;
            }
        }

        public virtual bool CameraDisabled
        {
            get { return IsPaused; }
            set
            {
                IsPaused = value;
                LocalCamera.enabled = !IsPaused;
            }
        }

        [SerializeField]
        private float m_LateralControlSpeed = 10f;
        public float LateralControlSpeed
        {
            get { return m_LateralControlSpeed; }
            set { m_LateralControlSpeed = value; }
        }

        [SerializeField]
        private float m_ShiftMultiplier = 5f;
        public float ShiftMultiplier
        {
            get { return m_ShiftMultiplier; }
            set { m_ShiftMultiplier = value; }
        }

        [SerializeField]
        private float m_RotationalControlSpeed = 1f;
        public float RotationalControlSpeed
        {
            get { return m_RotationalControlSpeed; }
            set { m_RotationalControlSpeed = value; }
        }

        [SerializeField] private float m_PanControlSpeed = 1f;
        public float PanControlSpeed
        {
            get { return m_PanControlSpeed; }
            set { m_PanControlSpeed = value; }
        }
        
        private float m_SpeedMult = 1f;
        private float SpeedMult { get { return m_SpeedMult; } set { m_SpeedMult = value; } }

        [SerializeField]
        private float m_SpeedExponent = 2f;
        private float SpeedExponent { get { return m_SpeedExponent; } }

        [SerializeField]
        private float m_SpeedAdjust = 1f;
        private float SpeedAdjust { get { return m_SpeedAdjust; } }
        
        [SerializeField]
        private float m_MinSpeedMult = -10f;
        private float MinSpeedMult { get { return m_MinSpeedMult; } }

        [SerializeField]
        private float m_MaxSpeedMult = 10f;
        private float MaxSpeedMult { get { return m_MaxSpeedMult; } }

        #endregion

        private void Update()
        {
            if (IsPaused)
                return;

            if ( HaxxisGlobalSettings.Instance.IsVgsJob == true )
                return;

            //if ( HaxxisGlobalSettings.Instance.DisableEditor == true )
            //    return;

            if ( FreeCameraDisabled )
                return;
            
            if ( TimelineViewBehaviour.Instance != null )
                if ( TimelineViewBehaviour.Instance.Timeline.IsBusy )
                    return;

            InterpretControls(Time.deltaTime);
        }

        protected virtual void InterpretControls(float timeMult)
        {
            if (InputFocusManager.Instance.IsAnyInputFieldInFocus())
                return;

            if ( Input.GetKey( KeyCode.LeftAlt ) || Input.GetKey( KeyCode.RightAlt ) )  // This is kinda lame but to resolve conflict with Shift-Alt-S for system stats bar toggle (and later for more Shift-Alt combinations)
                return;

            Vector3 controlInput = Vector3.zero;
            controlInput.x += Input.GetAxis("Horizontal");
            controlInput.z += Input.GetAxis("Vertical");
            controlInput.y += Input.GetAxis("Depth/Height");

            float speedMod = Input.GetAxis( "Mouse ScrollWheel" );

            if ( Mathf.Abs( speedMod ) > .001f && Input.GetButton( "Accelerate Camera" ))
            {
                SpeedMult += Time.deltaTime * speedMod * SpeedAdjust;
                SpeedMult = Mathf.Clamp( SpeedMult, MinSpeedMult, MaxSpeedMult );
            }



            controlInput = transform.right * controlInput.x + transform.up * controlInput.y + transform.forward * controlInput.z;
            controlInput *= timeMult;
            if ( Input.GetButton( "Accelerate Camera" ))
                controlInput *= Mathf.Pow(SpeedExponent, SpeedMult);

            if (AllowMovement(controlInput))
                transform.parent.position += controlInput*LateralControlSpeed;

            if (Input.GetButton("Fire2"))
            {
                var lateralRotation = RotationalControlSpeed*Input.GetAxis("Mouse X");
                if (AllowRotation(Vector3.up, lateralRotation))
                    transform.Rotate(Vector3.up, lateralRotation, Space.World);

                var verticalRotation = -RotationalControlSpeed*Input.GetAxis("Mouse Y");
                if (AllowRotation(transform.right, verticalRotation))
                    transform.Rotate(Vector3.right, verticalRotation, Space.Self);
            }


         //   if (Input.GetButton("Fire3"))
         //   {
         //       var xMovement = -PanControlSpeed * Input.GetAxis("Mouse X") * transform.right;
         //
         //       if (AllowMovement(xMovement))
         //           transform.position += xMovement;
         //
         //       var yMovement = -PanControlSpeed * Input.GetAxis("Mouse Y") * transform.up;
         //       if (AllowMovement(yMovement))
         //           transform.position += yMovement;
         //   }

            PostMovement();
        }

        protected virtual bool AllowMovement(Vector3 offset)
        {
            return true;
        }

        protected virtual bool AllowRotation(Vector3 axis, float angle)
        {
            return true;
        }

        protected virtual void PostMovement()
        {
        }

    }
}
