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
using Choreography.CameraControl.Modules;
using UnityEngine;

namespace Choreography.CameraControl
{
    public class CameraMovement : ICameraMovement
    {
        public event Action MovementComplete = delegate { };

        private List<ICameraMovementModule> m_MovementModules = new List<ICameraMovementModule>();
        public List<ICameraMovementModule> MovementModules { get { return m_MovementModules; } set
        {
            m_MovementModules = value;
        } }

        public CameraMovement()
        {
        }

        public CameraMovement( List< ICameraMovementModule > movementModules )
        {
            MovementModules = movementModules;
        }



        public IEnumerator MovementEnumerator()
        {
            PreMove();

            while (!DoneMovingTowardTarget())
            {
                yield return null;
            }

            PostMove();

            Done = true;

            MovementComplete();
        }

        protected Vector3 Position
        {
            get { return CameraControlLord.MainCamera.transform.position; }
            set { CameraControlLord.MainCamera.transform.position = value; }
        }

        protected void PreMove()
        {
            foreach (var module in MovementModules)
                module.PreMove();
        }

        protected void PostMove()
        {
            foreach (var module in MovementModules)
                module.PostMove();
        }

        protected bool DoneMovingTowardTarget()
        {
            bool allDone = true;

            foreach ( var module in MovementModules )
            {
                var moduleFinished = module.IsDoneMovingTowardTarget;
                allDone = allDone && moduleFinished;
            }

            return allDone;
        }

        public void Enqueue()
        {
            CameraControlLord.Instance.QueueCameraMovement( this );
        }

        public void Cancel()
        {
            CameraControlLord.Instance.CancelCameraMovement( this );

            Cancelled = true;

            Done = true;
        }

        public bool Cancelled { get; private set; }

        public bool Done { get; private set; }
    }
}