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
using System.Linq;
using UnityEngine;

namespace FocusCamera
{
    public class SingleTargetCamera : MonoBehaviour, IFocusingCamera
    {
        public IFocusableTarget Target { get; set; }
        
        [SerializeField]
        private GameObject m_TargetObject;
        private GameObject TargetObject { get { return m_TargetObject; } set { m_TargetObject = value; } }

        private void Start()
        {
            if (Target == null)
            {
                Target = TargetObject.GetComponentInChildren(typeof (IFocusableTarget)) as IFocusableTarget;
            }

            Target.FocusTarget();
        }

        public void Update()
        {
            InterpretTargets();
        }

        public virtual void InterpretTargets()
        {
            Target.UpdateInput();

            transform.position = Target.CameraLocation();

            var firstTarget = Target.CameraTargets().FirstOrDefault();

            if (firstTarget==null)
                throw new Exception("No targets found!");

            transform.LookAt(firstTarget.Position);
        }
    }
}
