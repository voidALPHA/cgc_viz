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
using System.Collections.Generic;
using System.Linq;
using Assets.Visualizers;
using FocusCamera;
using Mutation;
using UnityEngine;
using Utility;
using Visualizers.IsoGrid;
using Random = System.Random;

namespace Visualizers
{
    public class BoundingBox : MonoBehaviour
    {
        public Vector3 Size { get { return transform.lossyScale; } set { transform.SetWorldScale(Size); } }

        private MutableObject m_Data;

        private static Vector3 _HighlightPadding = Vector3.one * .1f;

        public MutableObject Data
        {
            get
            {
                if (m_Data != null) return m_Data;

                m_Data = new MutableObject();
                return m_Data;
            }

            set { m_Data = value; }
        }

        private Dictionary<VisualizerController, Visualizer> m_BoundVisualizers =
            new Dictionary<VisualizerController, Visualizer>();
        private Dictionary<VisualizerController, Visualizer> BoundVisualizers
        {
            get { return m_BoundVisualizers; }
            set { m_BoundVisualizers = value; }
        }

        private BoundsHighlightSatellite m_HighlightBox;

        private BoundsHighlightSatellite HighlightBox
        {
            get { return m_HighlightBox ?? (m_HighlightBox = VisualizerFactory.InstantiateBoundHighlightPrefab(this)); }
        }

        public bool HighlightChildren
        {
            set
            {
                Highlight = value;
                foreach (var bound in GetChildBoundingBoxes())
                {
                    bound.HighlightChildren = value;
                }
            }
        }

        public bool Highlight { set
        {
            PositionHighlight();
            HighlightBox.gameObject.SetActive(value); } }

        private void PositionHighlight()
        {
            var center = Vector3.zero;
            var totalCount = 0f;
            foreach (Renderer child in GetComponentsInChildren<Renderer>())
            {
                if ( child.GetComponent< BoundsHighlightSatellite >() == null )
                {
                    center += child.bounds.center;
                    totalCount++;
                }
            }
            center /= Mathf.Max(totalCount,1f);

            var extents = new UnityEngine.Bounds();
            extents.center = center;

            foreach (Renderer child in GetComponentsInChildren<Renderer>())
            {
                if (child.GetComponent<BoundsHighlightSatellite>() == null)
                    extents.Encapsulate( child.bounds );
            }


            HighlightBox.transform.rotation = Quaternion.identity;

            HighlightBox.transform.position = extents.center;
            HighlightBox.transform.SetWorldScale(extents.size + 2f * _HighlightPadding);
        }

        public static UnityEngine.Bounds SharedVisualSpace( List< BoundingBox > bounds )
        {
            var center = Vector3.zero;
            var totalCount = 0f;
            foreach (var bound in bounds)
            foreach (Renderer child in bound.GetComponentsInChildren<Renderer>())
            {
                if (child.GetComponent<BoundsHighlightSatellite>() == null)
                {
                    center += child.bounds.center;
                    totalCount++;
                }
            }
            center /= Mathf.Max(totalCount, 1f);

            var extents = new UnityEngine.Bounds { center = center };

            foreach (var bound in bounds)
                foreach (Renderer child in bound.GetComponentsInChildren<Renderer>())
            {
                if (child.GetComponent<BoundsHighlightSatellite>() == null)
                    extents.Encapsulate(child.bounds);
            }

            return extents;
        }

        public Transform VisualSpace
        {
            get
            {
                PositionHighlight();
                return HighlightBox.transform;
            }
        }

        public IEnumerable<Vector3> GetCorners()
        {
            var currentCorner = transform.position;
            yield return currentCorner;
            yield return currentCorner += transform.right * Size.x;
            yield return currentCorner += transform.up * Size.y;
            yield return currentCorner -= transform.right * Size.x;
            yield return currentCorner += transform.forward * Size.z - transform.up * Size.y;
            yield return currentCorner += transform.right * Size.x;
            yield return currentCorner += transform.up * Size.y;
            yield return currentCorner -= transform.right * Size.x;
        }
        
        public static BoundingBox ConstructBoundingBox( string name )
        {
            var box = ConstructBoundingBox( name, Vector3.zero, Quaternion.identity, Vector3.one );

            return box;
        }

        public static BoundingBox ConstructBoundingBox( Vector3 position, string name = "Bounding Box" )
        {
            var box = ConstructBoundingBox(name, position, Quaternion.identity, Vector3.one);

            return box;
        }

        public static BoundingBox ConstructBoundingBox( string name, Vector3 position, Quaternion rotation, Vector3 size)
        {

            var newObject = new GameObject();
            newObject.transform.position = position;
            newObject.transform.rotation = rotation;
            newObject.transform.localScale = size;

            var newBound = newObject.AddComponent<BoundingBox>();

            newBound.name = "[B] " + name;

            //Debug.LogWarning( "[BOUND DEBUG] Created bound " + name, newBound );

            return newBound;
        }

        public static BoundingBox ConstructBoundingBox(string name, Vector3 position, Quaternion rotation, Vector3 size, Transform parent)
        {
            var boundingBox = ConstructBoundingBox( name, position, rotation, size );

            boundingBox.transform.parent = parent;

            return boundingBox;

        }

        public BoundingBox CreateDependingBound( string name )
        {
            var newObject = new GameObject();

            var newBound = newObject.AddComponent<BoundingBox>();

            newObject.transform.parent = transform;
            newObject.transform.localPosition = Vector3.zero;
            newObject.transform.localRotation = Quaternion.identity;
            newObject.transform.localScale = Vector3.one;

            newBound.name = "[B] " + name;

            return newBound;
        }

        public IEnumerable< BoundingBox > GetChildBoundingBoxes()
        {
            foreach ( Transform childTrans in transform )
            {
                var childBound = childTrans.GetComponent< BoundingBox >();

                if ( childBound != null )
                    yield return childBound;
            }
        }

        public void ChildWithinBound(Transform childTransform)
        {
            childTransform.SetParent(transform);
            childTransform.localRotation = Quaternion.identity;
            childTransform.localScale = Vector3.one;
            childTransform.transform.localPosition = Vector3.zero;
        }

        public static BoundingBox ConstructAlignedEnclosingBox( string name, Quaternion rotation, params Vector3[] positions)
        {
            var newBox = ConstructBoundingBox(name, Vector3.zero, rotation, Vector3.one);

            newBox.EnclosePoints(positions);

            return newBox;
        }

        private bool TestForOverlapAlongAxis(Vector3 axis, BoundingBox other)
        {
            float minValue = float.MaxValue;
            float maxValue = float.MinValue;
            foreach (var point in GetCorners())
            {
                var axialPoint = Vector3.Dot(axis, point);
                maxValue = Mathf.Max(maxValue, axialPoint);
                minValue = Mathf.Min(minValue, axialPoint);
            }

            float otherMinValue = float.MaxValue;
            float otherMaxValue = float.MinValue;
            foreach (var point in other.GetCorners())
            {
                var axialPoint = Vector3.Dot(axis, point);
                otherMaxValue = Mathf.Max(otherMaxValue, axialPoint);
                otherMinValue = Mathf.Min(otherMinValue, axialPoint);
            }

            var minDir = (int)Mathf.Sign(otherMinValue - minValue)
                + (int)Mathf.Sign(otherMaxValue - minValue);
            if (minDir == 0)
                return true;

            var maxDir = (int)Mathf.Sign(otherMinValue - maxValue)
                + (int)Mathf.Sign(otherMaxValue - maxValue);
            if (maxDir == 0)
                return true;

            return maxDir + minDir == 0;
        }

        #region Visualizer binding

        public void ClearBounds()
        {
            var rand = new Random();

            foreach (Transform child in transform)
            {
                child.gameObject.name = GetAncestry(child) + "Not destroyed " + rand.Next();
                
                Destroy(child.gameObject);

                child.parent = null;
            }
            BoundVisualizers.Clear();
        }

        private string GetAncestry(Transform trans)
        {
            return trans.parent==null?"":GetAncestry(trans.parent)+trans.name;
        }

        // TODO: THis can all go away!

        public void ChildVisualizer(VisualizerController controller, Visualizer visualizer)
        {
            //ClearBoundVisualizer(controller);
            SetBoundVisualizer(controller, visualizer);
            ChildWithinBound(visualizer.transform);
        }

        public void SetBoundVisualizer(VisualizerController controller, Visualizer visualizer)
        {
            BoundVisualizers[controller] = visualizer;
        }

        public void ClearBoundVisualizer(VisualizerController controller)
        {
            if (BoundVisualizers.ContainsKey(controller))
            {
                BoundVisualizers[controller].DestroyVisualizer();
                BoundVisualizers.Remove(controller);
            }
        }

        public void ClearAllBoundVisualizers()
        {
            foreach (var visualizer in BoundVisualizers.Values)
                visualizer.DestroyVisualizer();

            BoundVisualizers.Clear();
        }
        #endregion

        #region Bounds Descent

        public static List<BoundingBox> DescendThroughNondiscriminatedBounds(IEnumerable<BoundingBox> bounds)
        {
            var doneList = new List<BoundingBox>();

            var progressStack = new Stack<Transform>();

            foreach (var bound in bounds)
            {
                //if (bound.Data.Count != 0)
                //    doneList.Add(bound);
                //else
                    progressStack.Push(bound.transform);
            }

            while (progressStack.Any())
            {
                var obj = progressStack.Pop();

                foreach (Transform child in obj.transform)
                {
                    var foundBound = child.GetComponent<BoundingBox>();

                    if (foundBound != null)
                    {
                        if (foundBound.Data.Count != 0)
                        {
                            doneList.Add(foundBound);
                            continue;
                        }
                    }

                    progressStack.Push(child);
                }
            }

            return doneList;
        }

        #endregion

        public void EnclosePoints(params Vector3[] positions)
        {
            EnclosePoints(positions.Select(p=>new CameraTarget(p,0)).ToArray());
        }

        public Vector3 Centerpoint
        {
            get
            {
                return transform.position
                       + (transform.right*Size.x
                       + transform.up*Size.y
                       + transform.forward*Size.z) * .5f;
            }
        }

        public bool IsWithinBound(Vector3 point)
        {
            var localPoint = transform.worldToLocalMatrix.MultiplyPoint(point);

            if (localPoint.x < 0 || localPoint.x > Size.x)
                return false;
            if (localPoint.y < 0 || localPoint.y > Size.y)
                return false;
            if (localPoint.z < 0 || localPoint.z > Size.z)
                return false;

            return true;
        }

        public bool OverlapsBound(BoundingBox other)
        {
            List<Vector3> ownAxes = new List<Vector3>()
            {
                transform.right,
                transform.up,
                transform.forward
            };
            List<Vector3> otherAxes = new List<Vector3>()
            {
                other.transform.right,
                other.transform.up,
                other.transform.forward
            };

            List<Vector3> compareAxes = new List<Vector3>();
            compareAxes.AddRange(ownAxes);
            compareAxes.AddRange(otherAxes);

            compareAxes.AddRange(from axis in ownAxes 
                                 from otherAxis in otherAxes 
                                 where Mathf.Abs(Vector3.Dot(axis, otherAxis)) > .00001f 
                                 select Vector3.Cross(axis, otherAxis));

            foreach (var axis in compareAxes)
                if (!TestForOverlapAlongAxis(axis, other))
                    return false;
            return true;
        }

        public void EnclosePoints(params CameraTarget[] objBounds)
        {
            if (objBounds.Length == 0)
                throw new InvalidOperationException("Can't enclose zero points with a bounding box");

            Vector3 minPosition = Vector3.one * float.MaxValue; //Vector3.zero;
            Vector3 maxPosition = Vector3.one * float.MinValue; //Vector3.one; 

            foreach (var point in objBounds)
            {
                var localPoint = transform.worldToLocalMatrix.MultiplyPoint(point.Position);
                
                minPosition.x = Math.Min(minPosition.x, localPoint.x - point.Size);
                minPosition.y = Math.Min(minPosition.y, localPoint.y - point.Size);
                minPosition.z = Math.Min(minPosition.z, localPoint.z - point.Size);

                maxPosition.x = Math.Max(maxPosition.x, localPoint.x + point.Size);
                maxPosition.y = Math.Max(maxPosition.y, localPoint.y + point.Size);
                maxPosition.z = Math.Max(maxPosition.z, localPoint.z + point.Size);
            }

            var matr = transform.localToWorldMatrix;

            transform.localScale = transform.localScale.PiecewiseMultiply(maxPosition-minPosition);
            
            transform.position = matr.MultiplyPoint(minPosition);
        }

        #region Renderer settings

        public void ToggleRenderers( bool renderState )
        {
            foreach ( var rend in gameObject.GetComponentsInChildren< Renderer >() )
                rend.enabled = renderState;
        }

        public IEnumerable< Renderer > GetRenderers()
        {
            return gameObject.GetComponentsInChildren< Renderer >();
        }

        #endregion

    }
}
