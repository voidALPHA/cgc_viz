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
using System.Collections.Generic;
using Mutation;
using Mutation.Mutators;
using UnityEngine;
using Visualizers;

namespace Ui.CameraBoundsProvider
{
    public enum CameraCorner
    {
        TopRight,
        TopLeft,
        TopCenter,
        BottomRight,
        BottomLeft,
        BottomCenter
    }

    public class CreateCameraCornerMutator : Mutator
    {
        private MutableField<CameraCorner> m_CameraCorner = new MutableField<CameraCorner>() 
        { LiteralValue = CameraBoundsProvider.CameraCorner.BottomLeft };
        [Controllable(LabelText = "Camera Corner")]
        public MutableField<CameraCorner> CameraCorner { get { return m_CameraCorner; } }

        private List< BoundingBox > m_ConstructedBounds = new List< BoundingBox >();
        private List< BoundingBox > ConstructedBounds
        {
            get { return m_ConstructedBounds; }
            set { m_ConstructedBounds = value; }
        }

        public override void Unload()
        {
            for (int i=0; i<ConstructedBounds.Count; i++)
                GameObject.Destroy(ConstructedBounds[i].gameObject);

            ConstructedBounds.Clear();
        }

        public BoundingBox ConstructCornerChild( CameraCorner corner )
        {
            var newBound = BoundingBox.ConstructBoundingBox( Vector3.zero, corner.ToString() );
            newBound.transform.parent = CameraCornerProvider.GetCornerTransform( corner );
            newBound.transform.localScale = Vector3.one;
            newBound.transform.localRotation = Quaternion.identity;
            newBound.transform.localPosition = Vector3.zero;
            return newBound;
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var newBound = ConstructCornerChild( CameraCorner.GetFirstValue( payload.Data ) );

            ConstructedBounds.Add( newBound );

            var newPayload = new VisualPayload( payload.Data, new VisualDescription( newBound ) );

            var iterator = Router.TransmitAll( newPayload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
