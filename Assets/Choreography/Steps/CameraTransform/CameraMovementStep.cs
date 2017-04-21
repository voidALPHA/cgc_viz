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
using Choreography.CameraControl;
using JetBrains.Annotations;
using UnityEngine;
using Visualizers;

namespace Choreography.Steps.CameraTransform
{
    public abstract class CameraMovementStep : Step
    {
        private const string EndEventName = "EndMovement";

        public CameraMovementStep()
        {
            Router.AddEvent( EndEventName );
        }

        private ICameraMovement CameraMovement { get; set; }

        protected override IEnumerator ExecuteStep()
        {
            //Debug.Log( "Starting Camera Movement Step" );
            CameraMovement = CreateCameraMovement();
            CameraMovement.MovementComplete += HandleMovementCompleted;
            CameraMovement.Enqueue();

            while ( !CameraMovement.Done )
                yield return null;

            Router.FireEvent(EndEventName);
        }

        private void HandleMovementCompleted()
        {
            //EndExecution(false);
        }

        protected override void OnCancel()
        {
            if ( CameraMovement == null )
                return;

            CameraMovement.Cancel();
        }

        [Controllable, UsedImplicitly]
        protected void FromCamera()
        {
            OnSavePosition( CameraControlLord.MainCamera );
        }

        protected virtual void OnSavePosition( Camera camera )
        {
        }


        [Controllable, UsedImplicitly]
        protected void ToCamera()
        {
            OnLoadPosition( CameraControlLord.MainCamera );
        }

        protected virtual void OnLoadPosition( Camera camera )
        {
        }


        protected abstract ICameraMovement CreateCameraMovement();
    }
}
