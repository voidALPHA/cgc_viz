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
using UnityEngine.EventSystems;

namespace Utility.Misc
{
    public class MouseMessageForwarder : MonoBehaviour
    {
        private void OnMouseDown()
        {
            if (!EventSystem.current.IsPointerOverGameObject()) // Prevent click when clicking on UI
                ForwardMessage("OnMouseDown");
        }

        private void OnMouseUp()
        {
            if (!EventSystem.current.IsPointerOverGameObject()) // Prevent click when clicking on UI
                ForwardMessage("OnMouseUp");
        }

        private void OnMouseUpAsButton()
        {
            if (!EventSystem.current.IsPointerOverGameObject()) // Prevent click when clicking on UI
                ForwardMessage("OnMouseUpAsButton");
        }

        private void ForwardMessage(string msg)
        {
            if (transform.parent == null)
                return;

            transform.parent.SendMessage( msg, false );
        }
    }
}