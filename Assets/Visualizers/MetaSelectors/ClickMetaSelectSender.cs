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

using Mutation;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Visualizers.MetaSelectors
{
    public struct InputModifiers
    {
        public bool Control { get; set; }
        public bool Shift { get; set; }
        
        public static InputModifiers CurrentInputModifiers()
        {
            InputModifiers currentModifiers = new InputModifiers();
            currentModifiers.Control = Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl);
            currentModifiers.Shift = Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift);
            return currentModifiers;
        }

        public static InputModifiers None()
        {
            return new InputModifiers();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is InputModifiers))
                return false;

            var otherModifiers = (InputModifiers) obj;

            return Control == otherModifiers.Control && Shift == otherModifiers.Shift;

            //return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Control.GetHashCode() << 1 + Shift.GetHashCode();
        }
    }

    public class ClickMetaSelectSender : MonoBehaviour
    {
        private void OnMouseUpAsButton()
        {
            if (!EventSystem.current.IsPointerOverGameObject()) // Prevent click when clicking on UI
                SendClick(transform, InputModifiers.CurrentInputModifiers());
        }


        private void SendClick(Transform target, InputModifiers inputModifiers, VisualPayload payload = null)
        {
            var newVisualizer = target.GetComponent<Visualizer>();
            var newPayload = newVisualizer == null ? payload : newVisualizer.Payload;

            foreach (var clickReceiver in target.GetComponents<ClickMetaSelectReceiver>())
                clickReceiver.SendClickEvent(payload, inputModifiers);

            payload = newPayload;

            if (target.parent!=null)
                SendClick(target.parent, inputModifiers, payload);
        }
    }
}
