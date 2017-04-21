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

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Utility.InputManagement
{
    [RequireComponent(typeof(InputField))]
    public class InputFocusManagerClientBehaviour : MonoBehaviour
    {
        private InputField m_InputField;
        private InputField InputField { get { return m_InputField ?? ( m_InputField = GetComponent< InputField >() ); } }
 
        [UsedImplicitly]
        private void Start()
        {
            InputFocusManager.Instance.AddInputField( InputField );
        }

        [UsedImplicitly]
        private void OnDestroy()
        {
            if ( InputFocusManager.Instance != null )
                InputFocusManager.Instance.RemoveInputField( InputField );
        }

        [UsedImplicitly]
        private void OnEnable()
        {
            InputFocusManager.Instance.AddInputField( InputField );
        }

        [UsedImplicitly]
        private void OnDisable()
        {
            if ( InputFocusManager.Instance != null )
                InputFocusManager.Instance.RemoveInputField( InputField );
        }
    }
}
