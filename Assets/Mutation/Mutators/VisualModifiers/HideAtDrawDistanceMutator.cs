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

using System.Collections;
using UnityEngine;
using Utility;
using Visualizers;

namespace Mutation.Mutators.VisualModifiers
{
    public class HideAtDrawDistanceMutator : Mutator
    {
        private MutableField<float> m_HideDistance = new MutableField<float>()
        { LiteralValue = 100f };
        [Controllable(LabelText = "Hide Distance")]
        public MutableField<float> HideDistance { get { return m_HideDistance; } }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var hideSatellite = payload.VisualData.Bound.gameObject.AddComponent<HideAtDrawDistanceSatellite>();

            hideSatellite.HideDistance = HideDistance.GetFirstValue( payload.Data );

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }

    public class HideAtDrawDistanceSatellite : MonoBehaviour
    {
        public float HideDistance { get; set; }

        private bool m_RenderersDisabled = false;

        private bool RenderersDisabled
        {
            get
            {
                return m_RenderersDisabled;
            }
            set
            {
                if ( value == m_RenderersDisabled )
                    return;

                m_RenderersDisabled = value;

                foreach ( var rend in transform.GetComponentsInChildren< Renderer >() )
                {
                    rend.enabled = !m_RenderersDisabled;
                }
            }
        }

        public void Update()
        {
            var drawDist = -1f*CameraLocaterSatellite.MasterCameraLocater.FoundCamera.worldToCameraMatrix.MultiplyPoint(
                transform.position ).z;

            RenderersDisabled = ( drawDist < 0 || drawDist > HideDistance );
        }
    }
}