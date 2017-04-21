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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utility.InputManagement
{
    public class InputFocusManager : MonoBehaviour
    {
        private static InputFocusManager s_Instance;
        public static InputFocusManager Instance
        {
            get
            {
                if ( s_Instance == null )
                {
                    s_Instance = FindObjectOfType< InputFocusManager >();
                }

                return s_Instance;
            }
        }

        //[SerializeField]
        private readonly HashSet< InputField > m_InputFields = new HashSet<InputField>();
        private HashSet< InputField > InputFields { get { return m_InputFields; } }


        public void AddInputField( InputField inputField )
        {
            //if ( InputFields.Contains( inputField ) ) // Unnecessary with the change from a List to a HashSet; extra Adds are ignored.
            //    return;
            
            InputFields.Add(inputField);
        }

        public void RemoveInputField( InputField inputField )
        {
            //if ( !InputFields.Contains( inputField ) ) // Unnecessary check
            //    return;

            InputFields.Remove( inputField );
        }

        public bool IsAnyInputFieldInFocus()
        {
            foreach(var inputField in InputFields)
            {
                if(inputField.isFocused) return true;
            }
            return false;
        }

        public bool IsInputFieldInFocus( InputField inputField )
        {
            // What's the point of this, other than checking that the inputField is contained in InputFields?
            //var item = InputFields.Find( x => x == inputField );
            //return item != null && item.isFocused;
            return InputFields.Contains(inputField) && inputField.isFocused;
        }
    }
}
