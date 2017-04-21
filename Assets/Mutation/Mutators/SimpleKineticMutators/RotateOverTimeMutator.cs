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
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.SimpleKineticMutators
{
    public class RotateOverTimeMutator : Mutator
    {
        private MutableField<Vector3> m_RotationAxis = new MutableField<Vector3>() 
        { LiteralValue = Vector3.up };
        [Controllable(LabelText = "Rotation Axis")]
        public MutableField<Vector3> RotationAxis { get { return m_RotationAxis; } }

        private MutableField<float> m_RotationSpeed = new MutableField<float>() 
        { LiteralValue = 1.0f };
        [Controllable(LabelText = "Rotation Speed")]
        public MutableField<float> RotationSpeed { get { return m_RotationSpeed; } }

        private MutableField<bool> m_AbsoluteRotation = new MutableField<bool>() 
        { LiteralValue = true };
        [Controllable(LabelText = "Absolute Rotation")]
        public MutableField<bool> AbsoluteRotation { get { return m_AbsoluteRotation; } }


        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            RotateOverTimeSatellite.CreateRotateOverTimeSatellite(
                payload.VisualData.Bound.gameObject,
                RotationAxis.GetFirstValue( payload.Data ),
                RotationSpeed.GetFirstValue( payload.Data ),
                AbsoluteRotation.GetFirstValue( payload.Data ));

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }

    public class RotateOverTimeSatellite : MonoBehaviour
    {
        public Vector3 RotationAxis { get; set; }
        public float RotationSpeed { get; set; }
        public bool RelativeAxis { get; set; }

        public static RotateOverTimeSatellite CreateRotateOverTimeSatellite(GameObject obj, Vector3 rotationAxis, float rotationSpeed, bool relativeAxis)
        {
            var newSatellite = new GameObject().AddComponent< RotateOverTimeSatellite >();

            newSatellite.transform.parent = obj.transform;
            newSatellite.transform.localScale = Vector3.one;
            newSatellite.transform.localRotation = Quaternion.identity;
            newSatellite.transform.localPosition = Vector3.zero;


            if ( obj.transform.parent != null )
                newSatellite.transform.SetParent( obj.transform.parent, true);
            else newSatellite.transform.SetParent( null, true);

            obj.transform.SetParent( newSatellite.transform, true);

            newSatellite.RotationAxis = rotationAxis;
            newSatellite.RotationSpeed = rotationSpeed;
            newSatellite.RelativeAxis = relativeAxis;

            return newSatellite;
        }

        void Update()
        {
            transform.Rotate(RotationAxis*RotationSpeed*Time.deltaTime, RelativeAxis?Space.Self:Space.World);
        }
    }
}
