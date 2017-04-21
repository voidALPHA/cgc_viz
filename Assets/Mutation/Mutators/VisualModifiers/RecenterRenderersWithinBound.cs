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

namespace Mutation.Mutators.VisualModifiers
{
    public class RecenterRenderersWithinBound : Mutator
    {
        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            UnityEngine.Bounds metaBound = new UnityEngine.Bounds();

            var foundRenderer = false;

            foreach ( var rend in payload.VisualData.Bound.GetComponentsInChildren< Renderer >() )
            {
                if ( !foundRenderer )
                {
                    metaBound = new UnityEngine.Bounds( rend.bounds.center, rend.bounds.size );
                    foundRenderer = true;
                }
                else
                    metaBound.Encapsulate( rend.bounds );
            }

            var worldToLocal = payload.VisualData.Bound.transform.worldToLocalMatrix;
            //var localToWorld = payload.VisualData.Bound.transform.localToWorldMatrix;


            var localizedXScale = worldToLocal.MultiplyVector( metaBound.size.x * Vector3.right );
            var localizedYScale = worldToLocal.MultiplyVector( metaBound.size.y * Vector3.up);
            var localizedZScale = worldToLocal.MultiplyVector( metaBound.size.z * Vector3.forward);

            var maximumExtentX =
                Mathf.Abs( localizedXScale.x )
                + Mathf.Abs( localizedYScale.x )
                + Mathf.Abs( localizedZScale.x );

            var maximumExtentY =
                Mathf.Abs(localizedXScale.y)
                + Mathf.Abs(localizedYScale.y)
                + Mathf.Abs(localizedZScale.y);
            
            var maximumExtentZ =
                Mathf.Abs(localizedXScale.z)
                + Mathf.Abs(localizedYScale.z)
                + Mathf.Abs(localizedZScale.z);

            var maximumOffendingScale = Mathf.Max( maximumExtentX, maximumExtentY, maximumExtentZ );

            var positionOffset = worldToLocal.MultiplyPoint( metaBound.center );


            var newBound = payload.VisualData.Bound.CreateDependingBound( "Recentering Bound" );

            newBound.transform.parent = payload.VisualData.Bound.transform.parent;

            payload.VisualData.Bound.transform.parent = newBound.transform;

            newBound.transform.localScale = Vector3.one / maximumOffendingScale;

            newBound.transform.position -= payload.VisualData.Bound.transform.PiecewiseMultiply( positionOffset );
                //localToWorld.MultiplyPoint( positionOffset );

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
