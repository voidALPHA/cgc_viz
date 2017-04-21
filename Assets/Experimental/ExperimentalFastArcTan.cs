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
using UnityEngine;

namespace Experimental
{
    [ExecuteInEditMode]
    public class ExperimentalFastArcTan : MonoBehaviour
    {
        [SerializeField]
        private bool m_Exec = false;
        private bool Exec { get { return m_Exec; } set { m_Exec = value; } }

        [SerializeField]
        private float m_StepInterval = .01f;
        private float StepInterval { get { return m_StepInterval; } }

        [SerializeField]
        private float m_Radius = 1f;
        private float Radius { get { return m_Radius; } }
        
        [SerializeField]
        private float m_DebugPointSize = .01f;
        private float DebugPointSize { get { return m_DebugPointSize; } }

        private List<Vector3> m_DebugPoints = new List< Vector3 >();
        public List<Vector3> DebugPoints { get { return m_DebugPoints; } set { m_DebugPoints=value; } }

        private List<Vector3> m_ComputedPoints = new List< Vector3 >();
        public List<Vector3> ComputedPoints { get { return m_ComputedPoints; } set { m_ComputedPoints=value; } }

        public void Update()
        {
            if ( !Exec )
                return;
            Exec = false;

            DebugPoints.Clear();
            ComputedPoints.Clear();

            for ( float i = 0; i < 2 * Mathf.PI; i += StepInterval )
            {
                var x = Mathf.Cos( i );// + transform.position.x;
                var y = Mathf.Sin( i );// + transform.position.y;

                var inverse = y / (Mathf.Abs(x)<0.0001f?0.0001f:x);

                var computedAngle = FastATan( inverse );

                if ( x < 0 )
                    computedAngle += Mathf.PI;

                var compX = Mathf.Cos( computedAngle );
                var compY = Mathf.Sin( computedAngle );


                var actualPoint = transform.position + new Vector3( x, y, 0 ) * Radius;
                var computedPoint = transform.position + new Vector3( compX, compY, 0 ) * Radius;

                DebugPoints.Add( actualPoint );
                ComputedPoints.Add( computedPoint );
            }
        }

        private float FastATan( float divis )
        {

            //return Mathf.Atan( divis );


            return (float)( Mathf.PI / 4f * divis -
                   divis * ( Math.Abs( divis ) - 1 ) * ( .2447 + .0663 * ( Math.Abs( divis ) ) ));

            //double d = divis;
            //return (float)( Mathf.PI / 4f * d -
            //       d * ( Math.Abs( d ) - 1 ) * ( .2447 + .0663 * ( Math.Abs( d ) ) ));
        }

        private void OnDrawGizmos()
        {
            var dbSize = DebugPointSize / 2f;

            for ( int i = 0; i < Mathf.Min( DebugPoints.Count, ComputedPoints.Count ); i++ )
            {
                var debugPoint = DebugPoints[ i ];
                var computedPoint = ComputedPoints[ i ];

                Gizmos.color = Color.red;
                Gizmos.DrawLine( computedPoint, debugPoint );

                Gizmos.color = Color.green;
                Gizmos.DrawLine( debugPoint-Vector3.up* dbSize, debugPoint+Vector3.up* dbSize);
                Gizmos.DrawLine(debugPoint - Vector3.forward * dbSize, debugPoint + Vector3.forward * dbSize);
                Gizmos.DrawLine(debugPoint - Vector3.right * dbSize, debugPoint + Vector3.right * dbSize);

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(computedPoint - Vector3.up * dbSize, computedPoint + Vector3.up * dbSize);
                Gizmos.DrawLine(computedPoint - Vector3.forward * dbSize, computedPoint + Vector3.forward * dbSize);
                Gizmos.DrawLine(computedPoint - Vector3.right * dbSize, computedPoint + Vector3.right * dbSize);
            }
        }

    }
}
