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

/// Credit jack.sydorenko 
/// Sourced from - http://forum.unity3d.com/threads/new-ui-and-line-drawing.253772/

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    [AddComponentMenu( "UI/Extensions/Primitives/UILineRenderer" )]
    public class UiLineRenderer : MaskableGraphic
    {
        public float LineThickness = 2;
        public bool relativeSize;

        [SerializeField]
        private Vector2[] m_Points;
        public Vector2[] Points
        {
            get
            {
                return m_Points;
            }
            set
            {
                m_Points = value;

                if ( m_Points.Length < 0 )
                    return;

                SetVerticesDirty();
            }
        }

        public override Texture mainTexture
        {
            get { return s_WhiteTexture; }
        }

        protected override void OnPopulateMesh( VertexHelper vh )
        {
            // requires sets of quads
            if(Points == null || Points.Length < 2)
                return;


            var sizeX = rectTransform.rect.width;
            var sizeY = rectTransform.rect.height;
            var offsetX = -rectTransform.pivot.x * rectTransform.rect.width;
            var offsetY = -rectTransform.pivot.y * rectTransform.rect.height;

            // don't want to scale based on the size of the rect, so this is switchable now
            if(!relativeSize)
            {
                sizeX = 1;
                sizeY = 1;
            }

            vh.Clear();

            Vector2 uvTopLeft = Vector2.zero;
            Vector2 uvBottomLeft = new Vector2(0, 1);

            Vector2 uvTopRight = new Vector2(1, 0);
            Vector2 uvBottomRight = new Vector2(1, 1);

            var uvs = new[] { uvTopLeft, uvBottomLeft, uvBottomRight, uvTopRight };

            var halfLineThickness = LineThickness / 2;
            var oneeightyOverPi = 180f / Mathf.PI;

            for(int i = 0; i < Points.Length; i += 2)
            {
                var cur = Points[i];
                var next = Points[i + 1];
                cur = new Vector2(cur.x * sizeX + offsetX, cur.y * sizeY + offsetY);
                next = new Vector2(next.x * sizeX + offsetX, next.y * sizeY + offsetY);

                float angle = Mathf.Atan2(next.y - cur.y, next.x - cur.x) * oneeightyOverPi;

                var v1 = cur + new Vector2(0, -halfLineThickness);
                var v2 = cur + new Vector2(0, +halfLineThickness);
                var v3 = next + new Vector2(0, +halfLineThickness);
                var v4 = next + new Vector2(0, -halfLineThickness);

                var angleVector = new Vector3(0, 0, angle);

                v1 = RotatePointAroundPivot(v1, cur, angleVector);
                v2 = RotatePointAroundPivot(v2, cur, angleVector);
                v3 = RotatePointAroundPivot(v3, next, angleVector);
                v4 = RotatePointAroundPivot(v4, next, angleVector);

                vh.AddUIVertexQuad(SetVbo(new[] { v1, v2, v3, v4 }, uvs));
            }
        }

        protected UIVertex[] SetVbo( Vector2[] vertices, Vector2[] uvs )
        {
            UIVertex[] vbo = new UIVertex[4];
            for ( int i = 0; i < vertices.Length; i++ )
            {
                var vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = vertices[i];
                vert.uv0 = uvs[i];
                vbo[i] = vert;
            }
            return vbo;
        }

        public Vector3 RotatePointAroundPivot( Vector3 point, Vector3 pivot, Vector3 angles )
        {
            Vector3 dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler( angles ) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }

        [UsedImplicitly]
        protected override void Awake()
        {
            base.Awake();

            raycastTarget = false;
        }

        //public override bool Raycast( Vector2 sp, Camera eventCamera )
        //{
        //    // TODO: This could be implemented to allow mouse picking of lines...

        //    //return base.Raycast( sp, eventCamera );
        //    return false;
        //}
    }
}

//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//namespace Ui
//{
//    [ExecuteInEditMode]
//    public class UiLineRenderer : MaskableGraphic
//    {
//        private enum UiLineStyle { Lines, LineStrip };

//        public float LineThickness = 2;
//        public bool UseMargins;
//        public Vector2 Margin;

//        [SerializeField]
//        private UiLineStyle m_LineStyle = UiLineStyle.Lines;
//        private UiLineStyle LineStyle { get { return m_LineStyle; } }

//        

//        protected override void Start()
//        {
//            if ( LineThickness < 1.0f && QualitySettings.antiAliasing == 0 )
//            {
//                Debug.LogWarning( "UILineRenderer: Line thicknesses below 1.0 will not show without AntiAliasing.", this );
//            }

//            base.Start();
//        }


//        protected override void OnFillVBO( List< UIVertex > vbo )
//        {
//            if ( Points == null || Points.Length < 2 )
//                return;

//            if ( LineStyle == UiLineStyle.LineStrip )
//                FillWithLineStrip( vbo );
//            else if ( LineStyle == UiLineStyle.Lines )
//                FillWithLines( vbo );
//            else
//                Debug.LogWarning( "Unhandled line style, aborting." );
//        }

//        private void FillWithLines( List<UIVertex> vbo )
//        {
//            var sizeX = rectTransform.rect.width;
//            var sizeY = rectTransform.rect.height;
//            var offsetX = -rectTransform.pivot.x * rectTransform.rect.width;
//            var offsetY = -rectTransform.pivot.y * rectTransform.rect.height;

//            if ( UseMargins )
//            {
//                sizeX -= Margin.x;
//                sizeY -= Margin.y;
//                offsetX += Margin.x / 2f;
//                offsetY += Margin.y / 2f;
//            }

//            vbo.Clear();

//            for ( int i = 0; i < Points.Length; i+=2 )
//            {
//                var cur = Points[i];
//                var next = Points[i + 1];
//                cur = new Vector2( cur.x * sizeX + offsetX, cur.y * sizeY + offsetY );
//                next = new Vector2( next.x * sizeX + offsetX, next.y * sizeY + offsetY );

//                float angle = Mathf.Atan2( next.y - cur.y, next.x - cur.x ) * 180f / Mathf.PI;

//                var v1 = cur + new Vector2( 0, +LineThickness / 2 );
//                var v2 = cur + new Vector2( 0, -LineThickness / 2 );
//                var v3 = next + new Vector2( 0, -LineThickness / 2 );
//                var v4 = next + new Vector2( 0, +LineThickness / 2 );

//                v1 = RotatePointAroundPivot( v1, cur, new Vector3( 0, 0, angle ) );
//                v2 = RotatePointAroundPivot( v2, cur, new Vector3( 0, 0, angle ) );
//                v3 = RotatePointAroundPivot( v3, next, new Vector3( 0, 0, angle ) );
//                v4 = RotatePointAroundPivot( v4, next, new Vector3( 0, 0, angle ) );

//                AddLineVertsToVbo( vbo, new[] { v1, v2, v3, v4 } );

//            }
//        }

//        private void FillWithLineStrip( List<UIVertex> vbo )
//        {

//            var sizeX = rectTransform.rect.width;
//            var sizeY = rectTransform.rect.height;
//            var offsetX = -rectTransform.pivot.x * rectTransform.rect.width;
//            var offsetY = -rectTransform.pivot.y * rectTransform.rect.height;

//            if ( UseMargins )
//            {
//                sizeX -= Margin.x;
//                sizeY -= Margin.y;
//                offsetX += Margin.x / 2f;
//                offsetY += Margin.y / 2f;
//            }

//            vbo.Clear();

//            Vector2 prevV1 = Vector2.zero;
//            Vector2 prevV2 = Vector2.zero;

//            for ( int i = 1; i < Points.Length; i++ )
//            {
//                var prev = Points[i - 1];
//                var cur = Points[i];
//                prev = new Vector2( prev.x * sizeX + offsetX, prev.y * sizeY + offsetY );
//                cur = new Vector2( cur.x * sizeX + offsetX, cur.y * sizeY + offsetY );

//                float angle = Mathf.Atan2( cur.y - prev.y, cur.x - prev.x ) * 180f / Mathf.PI;

//                var v1 = prev + new Vector2( 0, -LineThickness / 2 );
//                var v2 = prev + new Vector2( 0, +LineThickness / 2 );
//                var v3 = cur + new Vector2( 0, +LineThickness / 2 );
//                var v4 = cur + new Vector2( 0, -LineThickness / 2 );

//                v1 = RotatePointAroundPivot( v1, prev, new Vector3( 0, 0, angle ) );
//                v2 = RotatePointAroundPivot( v2, prev, new Vector3( 0, 0, angle ) );
//                v3 = RotatePointAroundPivot( v3, cur, new Vector3( 0, 0, angle ) );
//                v4 = RotatePointAroundPivot( v4, cur, new Vector3( 0, 0, angle ) );

//                if ( i > 1 )
//                    AddLineVertsToVbo( vbo, new[] { prevV1, prevV2, v1, v2 } );

//                AddLineVertsToVbo( vbo, new[] { v1, v2, v3, v4 } );

//                prevV1 = v3;
//                prevV2 = v4;
//            }
//        }

//        protected void AddLineVertsToVbo( List<UIVertex> vbo, Vector2[] vertices )
//        {
//            for ( int i = 0; i < vertices.Length; i++ )
//            {
//                var vert = UIVertex.simpleVert;
//                vert.color = color;
//                vert.position = vertices[i];
//                vbo.Add( vert );
//            }
//        }

//        

//        
//    }
//}
