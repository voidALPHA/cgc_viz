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
using ChainViews;
using Mutation;
using Mutation.Mutators;
using Scripts.Utility.Misc;
using UnityEngine;
using Utility;
using Utility.JobManagerSystem;
using Visualizers;

namespace Ui.FilamentControl
{
    public class MouseDragReactor : Mutator
    {
        private MutableTarget m_WorldPosition = new MutableTarget() 
        { AbsoluteKey = "World Position" };
        [Controllable(LabelText = "World Position")]
        public MutableTarget WorldPosition { get { return m_WorldPosition; } }

        private MutableTarget m_MousePosition = new MutableTarget() 
        { AbsoluteKey = "Mouse Position" };
        [Controllable(LabelText = "Mouse Position")]
        public MutableTarget MousePosition { get { return m_MousePosition; } }

        private MutableTarget m_DragActive = new MutableTarget() 
        { AbsoluteKey = "Drag Active" };
        [Controllable(LabelText = "Is Drag Active?")]
        public MutableTarget DragActive { get { return m_DragActive; } }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            WorldPosition.SetValue( Vector3.one, newSchema );
            MousePosition.SetValue( Vector3.one, newSchema );
            DragActive.SetValue( true, newSchema );

            Router.TransmitAllSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var mouseSatellite = payload.VisualData.Bound.gameObject.AddComponent< MouseDragSatellite >();

            mouseSatellite.MouseDrag += ( worldPos, mousePos, dragActive ) =>
            {
                WorldPosition.SetValue( worldPos, payload.Data );
                MousePosition.SetValue( mousePos, payload.Data );
                DragActive.SetValue( dragActive, payload.Data );

                JobManager.Instance.StartJob(
                    Router.TransmitAll(payload), 
                    jobName: "Mouse Drag", 
                    startImmediately: true, 
                    maxExecutionsPerFrame: 1);
            };

            yield break;
        }
    }

    public class MouseDragSatellite : MonoBehaviour
    {
        private bool IsNotVgs
        {
            get
            {
                return HaxxisGlobalSettings.Instance.IsVgsJob == false;
            }
        }

        private bool m_MouseDragging = false;
        private bool MouseDragging { get {return m_MouseDragging;}
            set
            {
                if ( m_MouseDragging == value )
                    return;
                m_MouseDragging = value;
                if ( !m_MouseDragging )
                    MouseDrag(PriorWorldPosition, PriorMousePosition, false);
            }
        }

        // events carry world location of ray intersection, then mouse position, then whether or not the drag is active
        public event Action< Vector3, Vector3, bool > MouseDrag = delegate { };

        private Vector3 PriorWorldPosition { get; set; }
        private Vector3 PriorMousePosition { get; set; }

        //private void OnMouseDown()
        //{
        //}

        private void SendDragActive() { MouseDrag(PriorWorldPosition, PriorMousePosition, true); }

        private void OnMouseUp()
        {
            if ( !IsNotVgs || HaxxisGlobalSettings.Instance.DisableEditor == true || ChainView.Instance.Visible)
                return;

            MouseDragging = false;
        }

        private void OnMouseDrag()
        {
            if (!IsNotVgs || HaxxisGlobalSettings.Instance.DisableEditor == true || ChainView.Instance.Visible)
                return;
            
            RaycastHit hit;
            var ray = CameraLocaterSatellite.MasterCameraLocater.FoundCamera.ScreenPointToRay(Input.mousePosition);

            if ( Physics.Raycast( ray, out hit ) )
            {
                MouseDragging = true;

                PriorWorldPosition = hit.point;
                PriorMousePosition = Input.mousePosition;

                SendDragActive();
            }
            else
            {
                MouseDragging = false;
            }
        }
    }
}
