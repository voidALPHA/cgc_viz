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
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

namespace Choreography.Views.StepEditor.ControllableViews
{
    public class EditorControllableButtonBehaviour : MonoBehaviour
    {

        [Header("Component References")]

        [SerializeField]
        private Text m_MethodNameTextComponent = null;
        private Text MethodNameTextComponent { get { return m_MethodNameTextComponent; } }



        private Object MethodResolutionObject { get; set; }

        private MethodInfo Method { get; set; }



        public void SetMethod( Object methodResolutionObject, MethodInfo methodInfo )
        {
            if ( Method != null )
                throw new InvalidOperationException( "Cannot reuse this view." );

            MethodResolutionObject = methodResolutionObject;

            Method = methodInfo;


            MethodNameTextComponent.text = Method.Name;
        }



        [UsedImplicitly]
        public void HandleButtonPressed()
        {
            Method.Invoke( MethodResolutionObject, null );
        }
    }
}
