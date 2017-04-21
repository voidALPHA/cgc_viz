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

using UnityEngine;

namespace Ui
{
    public class DropDownCanvasHostBehaviour : MonoBehaviour
    {
        private static DropDownCanvasHostBehaviour s_Instance;
        private static DropDownCanvasHostBehaviour Instance
        {
            get
            {
                if ( s_Instance == null )
                    s_Instance = FindObjectOfType< DropDownCanvasHostBehaviour >();

                return s_Instance ;
            }
        }

        private static RectTransform s_RectTransform;
        private static RectTransform RectTransform
        {
            get
            {
                if ( s_RectTransform == null )
                    s_RectTransform = Instance.GetComponent< RectTransform >();

                return s_RectTransform;
            }
        }
    }
}
