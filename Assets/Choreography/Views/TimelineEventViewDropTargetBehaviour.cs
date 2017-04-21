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
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Choreography.Views
{
    public class TimelineEventViewDropTargetBehaviour : MonoBehaviour
    {
        [SerializeField]
        private List< GameObject > m_EnableOnWouldAccept = new List< GameObject >();
        private List< GameObject > EnableOnWouldAccept { get { return m_EnableOnWouldAccept; } }

        public event Action< TimelineStepViewBehaviour > StepViewDropped = delegate { };

        // TODO: this could have preview event as well so responder can have say in acceptance indicator...

        [UsedImplicitly]
        private void OnDragEnter( Object draggedObject )
        {
            var stepView = draggedObject as TimelineStepViewBehaviour;
            if (stepView == null)
                return;

            IndicateWouldAccept( true );
        }

        [UsedImplicitly]
        private void OnDragExit( Object draggedObject )
        {
            var stepView = draggedObject as TimelineStepViewBehaviour;
            if (stepView == null)
                return;

            IndicateWouldAccept( false );
        }

        [UsedImplicitly]
        private void OnDrop( Object droppedObject )
        {
            var stepView = droppedObject as TimelineStepViewBehaviour;
            if ( stepView == null )
                return;
            
            StepViewDropped( stepView );
        }

        private void IndicateWouldAccept( bool wouldAccept )
        {
            EnableOnWouldAccept.ForEach( go => go.SetActive( wouldAccept ) );
        }
    }
}
