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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Ui;
using UnityEngine;

namespace Utility
{
    public static class TransformExtensions
    {
        public static Vector3 GetLocalX(this Transform trans)
        {
            return trans.right*trans.lossyScale.x;
        }
        public static Vector3 GetLocalY(this Transform trans)
        {
            return trans.up * trans.lossyScale.y;
        }
        public static Vector3 GetLocalZ(this Transform trans)
        {
            return trans.forward * trans.lossyScale.z;
        }

        public static Vector3[] GetLocalAxes(this Transform trans)
        {
            return new[]
            {
                trans.GetLocalX(), trans.GetLocalY(), trans.GetLocalZ()
            };
        }

        public static Vector3 LocalToWorldSpace( this Transform trans, Vector3 localAxes )
        {
            return trans.position + trans.PiecewiseMultiply( localAxes );
        }
        
        public static Vector3 PiecewiseMultiply(this Transform trans, Vector3 localAxes)
        {
            return trans.GetLocalX()*localAxes.x
                +trans.GetLocalY()*localAxes.y
                +trans.GetLocalZ()*localAxes.z;
        }

        public static void SetWorldScale(this Transform trans, Vector3 worldScale)
        {
            var parent = trans.parent;
            trans.parent = null;
            trans.localScale = worldScale;
            trans.parent = parent;
        }

        public static string GetHierarchyString( this Transform transform, string delimiter = "." )
        {
            return GetParentHierarchyString( transform, delimiter );
        }

        private static string GetParentHierarchyString( this Transform transform, string delimiter = "."  )
        {
            if ( transform.parent == null )
                return transform.name;

            return transform.parent.GetParentHierarchyString(delimiter) + delimiter + transform.name;
        }

        public static void ExpandToContain( this RectTransform thisTransform,
            IEnumerable< RectTransform > containedTransforms, float padding, Vector3 noChildrenSize )
        {
            ExpandToContain( thisTransform, containedTransforms, Padding.Uniform( padding ), noChildrenSize );
        }

        public static void ExpandToContain( this RectTransform thisTransform, IEnumerable< RectTransform > containedTransforms, Padding padding, Vector3 noChildrenSize )
        {
            // NOTE: 0, 0 is bottom left in global position space! This is critical info ;)

            //Debug.LogFormat( "{0} -================================================", ExpandCallCount++ );

            var scaleX = thisTransform.lossyScale.x;
            var scaleY = thisTransform.lossyScale.y;

            var top = 0.0f;
            var bottom = 0.0f;
            var left = 0.0f;
            var right = 0.0f;

            if ( containedTransforms.Any() )
            {
                top = float.MinValue;
                bottom = float.MaxValue;
                left = float.MaxValue;
                right = float.MinValue;

                foreach ( var containeeTransform in containedTransforms )
                {
                    var containeeTop = containeeTransform.position.y;
                    if ( containeeTop > top )
                        top = containeeTransform.position.y;

                    var containeeBottom = containeeTop - containeeTransform.sizeDelta.y * scaleX;
                    if ( containeeBottom < bottom )
                        bottom = containeeBottom;

                    var containeeLeft = containeeTransform.position.x;
                    if ( containeeLeft < left )
                        left = containeeLeft;
                    
                    var containeeRight = containeeLeft + containeeTransform.sizeDelta.x * scaleY;
                    if ( containeeRight > right )
                        right = containeeRight;
                }
            }

            // Beware this tosses out the calculated Z... Does this matter?
            padding *= scaleY;
            var pos = new Vector3( left - padding.Left, top + padding.Top, thisTransform.position.z );

            var size = ( new Vector3( right - left, top - bottom, 0.0f ) * 1.0f ) + new Vector3( padding.Left + padding.Right, padding.Top + padding.Bottom, 0.0f );

            if ( !containedTransforms.Any() )
                size = noChildrenSize;
            
            
            //Debug.LogFormat( "Size is {0}", size );
            //Debug.LogFormat( "Pos is {0}", pos );

            if ( containedTransforms.Any())
                thisTransform.position = pos;

            thisTransform.sizeDelta = new Vector2( size.x / scaleX, size.y / scaleY );
        }
    }
}
