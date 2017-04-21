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
using Choreography.Recording;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Choreography.Views
{
    public class TimelineCameraControlsViewBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Button m_PerspectiveButtonComponent = null;
        private Button PerspectiveButtonComponent { get { return m_PerspectiveButtonComponent; } }

        [SerializeField]
        private Button m_OrthoButtonComponent = null;
        private Button OrthoButtonComponent { get { return m_OrthoButtonComponent; } }

        [SerializeField]
        private InputField m_InputFieldComponent = null;
        private InputField InputFieldComponent { get { return m_InputFieldComponent; } }



        private Color DefaultTextColor { get; set; }


        [UsedImplicitly]
        private void Start()
        {
            DefaultTextColor = InputFieldComponent.textComponent.color;
        }


        [UsedImplicitly]
        private void Update()
        {
            if ( InputFieldComponent.isFocused )
                return;
            if(RecordingLord.IsRecording()) return;

            InputFieldComponent.text = Camera.main.orthographic ? Camera.main.orthographicSize.ToString() : Camera.main.fieldOfView.ToString();

            OrthoButtonComponent.interactable = !Camera.main.orthographic;
            PerspectiveButtonComponent.interactable = Camera.main.orthographic;
        }


        [UsedImplicitly]
        public void HandlePerspectiveButtonClicked()
        {
            Camera.main.orthographic = false;
        }

        [UsedImplicitly]
        public void HandleOrthoButtonClicked()
        {
            Camera.main.orthographic = true;
        }

        private bool ValueIsValid { get; set; }
        private float LastFloatValue { get; set; }

        [UsedImplicitly]
        public void HandleInputFieldChanged()
        {
            var floatValue = 0.0f;
            ValueIsValid = Single.TryParse( InputFieldComponent.text, out floatValue );
            
            LastFloatValue = floatValue;

            InputFieldComponent.textComponent.color = !ValueIsValid ? Color.red : DefaultTextColor;
        }

        [UsedImplicitly]
        public void HandleInputFieldSubmitted()
        {
            if ( !ValueIsValid )
                return;

            if ( Camera.main.orthographic )
                Camera.main.orthographicSize = LastFloatValue;
            else
                Camera.main.fieldOfView = LastFloatValue;
        }
    }
}
