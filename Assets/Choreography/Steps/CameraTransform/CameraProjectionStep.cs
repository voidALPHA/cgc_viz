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
using JetBrains.Annotations;
using UnityEngine;
using Visualizers;

namespace Choreography.Steps.CameraTransform
{
    [UsedImplicitly]
    public class CameraProjectionStep : Step
    {
        public event Action Dirty = delegate { };
        
        private const string EndEventName = "Applied";

        private ControllableCondition m_UseOrtho = new ControllableCondition( false );
        [Controllable( ObservableEventName = "Dirty" ), UsedImplicitly]
        private ControllableCondition UseOrtho
        {
            get { return m_UseOrtho; }
            set { m_UseOrtho = value; }
        }

        private float m_Size = 10;
        [Controllable(ConditionalPropertyName = "UseOrtho", InvertConditional = false, ObservableEventName = "Dirty"), UsedImplicitly]
        private float Size
        {
            get { return m_Size; }
            set 
            {
                m_Size = value;
                Dirty();
            }
        }

        private float m_FieldOfView = 60;
        [Controllable( ConditionalPropertyName = "UseOrtho", InvertConditional = true, ObservableEventName = "Dirty" ), UsedImplicitly]
        private float FieldOfView
        {
            get { return m_FieldOfView; }
            set
            {
                m_FieldOfView = value;
                Dirty();
            }
        }

        public CameraProjectionStep()
        {
            Router.AddEvent( EndEventName );
        }

        protected override IEnumerator ExecuteStep()
        {
            Camera.main.orthographic = UseOrtho;

            if ( UseOrtho )
                Camera.main.orthographicSize = Size;
            else
                Camera.main.fieldOfView = FieldOfView;

            Router.FireEvent(EndEventName);

            yield break;
        }

        [Controllable, UsedImplicitly]
        private void FromCamera()
        {
            UseOrtho.ConditionValid = Camera.main.orthographic;

            if ( UseOrtho )
                Size = Camera.main.orthographicSize;
            else
                FieldOfView = Camera.main.fieldOfView;
        }

        [Controllable, UsedImplicitly]
        private void ToCamera()
        {
            Camera.main.orthographic = UseOrtho;

            if ( UseOrtho )
                Camera.main.orthographicSize = Size;
            else
                Camera.main.fieldOfView = FieldOfView;
        }
    }
}