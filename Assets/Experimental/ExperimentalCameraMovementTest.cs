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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Choreography.CameraControl;
using Choreography.CameraControl.Modules;
using UnityEngine;

namespace Experimental
{
    public class ExperimentalCameraMovementTest : MonoBehaviour
    {
        public void Start()
        {
            var goToOriginMovement = new CameraMovement();

            goToOriginMovement.MovementModules.Add( 
                new MoveToPositionInTimeModule( Vector3.zero, 3f) );
            goToOriginMovement.MovementModules.Add( 
                new RotateToFacePointInTimeModule( new Vector3(0,0,100f), 3f ));

            CameraControlLord.Instance.QueueCameraMovement( goToOriginMovement );

            var moveUpMovement = new CameraMovement();
            moveUpMovement.MovementModules.Add( 
                new MoveToPositionInTimeModule( new Vector3(-1,15,-1), 5 ));
            moveUpMovement.MovementModules.Add(
                new RotateToFacePointInTimeModule(new Vector3(0, 0, 30f), 3f));

            CameraControlLord.Instance.QueueCameraMovement( moveUpMovement );


        }
    }
}
