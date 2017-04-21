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
using System.Linq;
using Bounds;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using Visualizers;

namespace Choreography.Steps.BoundsProviderSteps
{
    public enum BoundChildTypes
    {
        CenterOfFirst,
        OriginOfFirst,
        CenterOfPlusX,
        CenterOfMinusX,
        CenterOfPlusY,
        CenterOfMinusY,
        CenterOfPlusZ,
        CenterOfMinusZ,
        CenterOfAll,
        CenterOfAllPlusX,
        CenterOfAllMinusX,
        CenterOfAllPlusY,
        CenterOfAllMinusY,
        CenterOfAllPlusZ,
        CenterOfAllMinusZ,
    }

    public class BoundsToChildBoundStep : Step, IBoundsProvider
    {
        private const string CompleteEventName = "Complete";

        [Controllable, UsedImplicitly]
        public string BoundsProviderKey { get; set; }
        
        [Controllable, UsedImplicitly]
        public IBoundsProvider BoundsToFocus { get; set; }

        private BoundChildTypes m_BoundChildingType;
        [Controllable(LabelText = "Bound Child Method")]
        public BoundChildTypes BoundChildingType { get { return m_BoundChildingType; } set { m_BoundChildingType = value; } }

        #region Bounds
        private IEnumerable<BoundingBox> m_Bounds;
        public IEnumerable<BoundingBox> Bounds
        {
            get
            {
                if (m_Bounds == null)
                    throw new Exception("Bounds retrieved before evaluation!");
                return m_Bounds;
            }
            private set { m_Bounds = value; }
        }
        #endregion

        public BoundsToChildBoundStep()
        {
            BoundsProviderKey = "ChildFromBoundsStep";

            Bounds = new List< BoundingBox >();

            Router.AddEvent( CompleteEventName );
        }

        private void ClearBounds()
        {
            foreach (var bound in Bounds)
                GameObject.Destroy(bound.gameObject);

            Bounds = new List< BoundingBox >();
        }

        protected override IEnumerator ExecuteStep()
        {
            Bounds = new List< BoundingBox >
            {
                ConstructChildBound( BoundsToFocus.Bounds, BoundChildingType )
            };

            Router.FireEvent(CompleteEventName);

            yield return null;
        }

        private BoundingBox ConstructMinimalBound( Vector3 position, Quaternion rotation )
        {
            return BoundingBox.ConstructBoundingBox
                ("Child Bound", position, rotation, .01f * Vector3.one);
        }

        private BoundingBox ConstructChildBound( IEnumerable< BoundingBox > bounds, BoundChildTypes childMethod )
        {
            ClearBounds();

            if (!bounds.Any())
                return BoundingBox.ConstructBoundingBox( "Child Bound" );

            var firstBound = bounds.First();

            var sharedSpace = BoundingBox.SharedVisualSpace(bounds.ToList());

            switch ( childMethod )
            {
                case BoundChildTypes.CenterOfFirst:
                    return ConstructMinimalBound(
                        firstBound.VisualSpace.LocalToWorldSpace(Vector3.zero)
                        , firstBound.transform.rotation);
                case BoundChildTypes.OriginOfFirst:
                    return ConstructMinimalBound(
                        firstBound.VisualSpace.LocalToWorldSpace(Vector3.one*-.5f), 
                        firstBound.transform.rotation);
                case BoundChildTypes.CenterOfMinusX:
                    return ConstructMinimalBound(
                        firstBound.VisualSpace.LocalToWorldSpace(new Vector3(-.5f,0f,0f) ),
                        firstBound.transform.rotation);
                case BoundChildTypes.CenterOfPlusX:
                    return ConstructMinimalBound(
                        firstBound.VisualSpace.LocalToWorldSpace( new Vector3(+.5f, 0f, 0f)), 
                        firstBound.transform.rotation);
                case BoundChildTypes.CenterOfMinusY:
                    return ConstructMinimalBound(
                        firstBound.VisualSpace.LocalToWorldSpace(new Vector3(0f, -.5f, 0f)),
                        firstBound.transform.rotation);
                case BoundChildTypes.CenterOfPlusY:
                    return ConstructMinimalBound(
                        firstBound.VisualSpace.LocalToWorldSpace(new Vector3(0f, +.5f, 0f)),
                        firstBound.transform.rotation);
                case BoundChildTypes.CenterOfMinusZ:
                    return ConstructMinimalBound(
                        firstBound.VisualSpace.LocalToWorldSpace(new Vector3(0f, 0f, -.5f)),
                        firstBound.transform.rotation);
                case BoundChildTypes.CenterOfPlusZ:
                    return ConstructMinimalBound(
                        firstBound.VisualSpace.LocalToWorldSpace(new Vector3(0f, 0f, +.5f)),
                        firstBound.transform.rotation);
                case BoundChildTypes.CenterOfAll:

                    // note: this does not inherit rotation from any of the related bounds.  Use average?
                    var center = Vector3.zero;
                    var totalCount = 0f;
                    foreach (var bound in bounds)
                    {
                        center += bound.Centerpoint;
                        totalCount++;
                    }
                    center /= Mathf.Max(totalCount, 1f);

                    var extents = new UnityEngine.Bounds();
                    extents.center = center;

                    foreach (var bound in bounds)
                    {
                        extents.Encapsulate(bound.Centerpoint);
                    }
                    return BoundingBox.ConstructBoundingBox
                        ("Child Bound", extents.center, Quaternion.identity, .01f * Vector3.one);
                case BoundChildTypes.CenterOfAllPlusX:
                    return ConstructMinimalBound(
                            sharedSpace.center + new Vector3( .5f, 0, 0 ) * sharedSpace.size.x,
                            Quaternion.identity );
                case BoundChildTypes.CenterOfAllMinusX:
                    return ConstructMinimalBound(
                            sharedSpace.center + new Vector3(-.5f, 0, 0) * sharedSpace.size.x,
                            Quaternion.identity);
                case BoundChildTypes.CenterOfAllPlusY:
                    return ConstructMinimalBound(
                            sharedSpace.center + new Vector3(0, .5f, 0) * sharedSpace.size.y,
                            Quaternion.identity);
                case BoundChildTypes.CenterOfAllMinusY:
                    return ConstructMinimalBound(
                            sharedSpace.center + new Vector3(0, -.5f, 0) * sharedSpace.size.y,
                            Quaternion.identity);
                case BoundChildTypes.CenterOfAllPlusZ:
                    return ConstructMinimalBound(
                            sharedSpace.center + new Vector3(0, 0,.5f) * sharedSpace.size.z,
                            Quaternion.identity);
                case BoundChildTypes.CenterOfAllMinusZ:
                    return ConstructMinimalBound(
                            sharedSpace.center + new Vector3(0, 0,-.5f) * sharedSpace.size.z,
                            Quaternion.identity);
                default:
                    throw new Exception("Invalid child bounding operation!");
            }
        }

    }
}
