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
using JetBrains.Annotations;
using Ui;
using UnityEngine;

namespace ChainViews
{
    [RequireComponent( typeof( UiLineRenderer ) )]
    public class LineManagerBehaviour : MonoBehaviour
    {
        public class RectTransformPair
        {
            public RectTransform A { get; private set; }
            public RectTransform B { get; private set; }

            public RectTransformPair( RectTransform a, RectTransform b )
            {
                A = a;
                B = b;
            }

            public override bool Equals( object obj )
            {
                var other = obj as RectTransformPair;
                if ( other == null )
                    return false;

                return  A == other.A && B == other.B;
            }

            public override int GetHashCode()
            {
                return A.GetHashCode() + B.GetHashCode();
            }
        }

        
        [Header( "Component References" )]
        
        [SerializeField]
        private readonly List<RectTransformPair> m_TransformPairs = new List<RectTransformPair>();
        private List<RectTransformPair> TransformPairs { get { return m_TransformPairs; } }


        
        private RectTransform m_RectTransform;
        public RectTransform RectTransform
        {
            get { return m_RectTransform ?? ( m_RectTransform = GetComponent< RectTransform >() ); }
        }



        private UiLineRenderer m_LineRenderer;
        private UiLineRenderer LineRenderer
        {
            get { return m_LineRenderer ?? ( m_LineRenderer = GetComponent< UiLineRenderer >() ); }
        }

        private List<Vector2> m_Points;
        private List<Vector2> Points
        {
            get { return m_Points ?? ( m_Points = new List<Vector2>() ); }
        }

        private bool draggingLastFrame = false;


        [UsedImplicitly]
        private void Start()
        {
        }


        //public void AddLine( RectTransform a, RectTransform b )
        //{
        //    Debug.Log( "Adding line..." );

        //    var transformPair = new RectTransformPair( a, b );

        //    if ( TransformPairs.Contains( transformPair ) )
        //        throw new InvalidOperationException( "Line already defined." );

        //    TransformPairs.Add( transformPair );

        //    UpdateVertices();
        //}

        //public void RemoveLine( RectTransform a, RectTransform b )
        //{
        //    var transformPair = new RectTransformPair( a, b );

        //    if ( !TransformPairs.Contains( transformPair ) )
        //        throw new InvalidOperationException( "Line not defined." );

        //    TransformPairs.Remove( transformPair );

        //    UpdateVertices();
        //}

        public void SetLines( IEnumerable< RectTransformPair > pairs )
        {
            TransformPairs.Clear();

            if ( pairs != null )
                TransformPairs.AddRange( pairs );

            UpdateVertices();
        }


        [UsedImplicitly]
        private void Update()
        {
            if(!ChainView.Instance.Dragging && draggingLastFrame)
            {
                UpdateVertices();
            }

            draggingLastFrame = ChainView.Instance.Dragging;
        }

        private void UpdateVertices()
        {
            Points.Clear();

            if ( !TransformPairs.Any() )
            {
                Points.Add( Vector2.zero );
                Points.Add( Vector2.zero );
            }
            else
            {
                var parentPos = RectTransform.position;
                var parentSize = RectTransform.sizeDelta;
                parentSize = new Vector2(1f / parentSize.x, 1f / parentSize.y);

                for ( int i = 0; i < TransformPairs.Count; ++i )
                {
                    var pair = TransformPairs[i];

                    var posA = pair.A.position - parentPos;
                    var start = new Vector2( posA.x * parentSize.x, posA.y * parentSize.y );
                    var posB = pair.B.position - parentPos;
                    var end = new Vector2( posB.x * parentSize.x, posB.y * parentSize.y );

                    Points.Add( start );
                    Points.Add( end );
                }
            }

            LineRenderer.Points = Points.ToArray();
        }
    }
}
