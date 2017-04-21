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

using JetBrains.Annotations;
using UnityEngine;

namespace Utility
{
    [ExecuteInEditMode]
    public class RotateContinuously : MonoBehaviour
    {
        [SerializeField]
        private float m_DegreesPerSecond = 100.0f;
        private float DegreesPerSecond { get { return m_DegreesPerSecond; } }

        [SerializeField]
        private Vector3 m_Vector = new Vector3( 0, 0, 1 );
        private Vector3 Vector { get { return m_Vector; } }

        [UsedImplicitly]
        private void Update()
        {
            if ( DoRotate )
                transform.Rotate( Vector * Time.deltaTime * DegreesPerSecond );
        }

        [SerializeField]
        private bool m_DoRotate = true;
        public bool DoRotate
        {
            get  { return m_DoRotate; }
            set
            {
                m_DoRotate = value;

                if (!m_DoRotate )
                    transform.Rotate( Vector3.zero );
            }
        }
    }
}
