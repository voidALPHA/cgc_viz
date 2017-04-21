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

using Mutation;
using UnityEngine;

namespace Visualizers.IsoGrid
{
    public class BarBehaviour : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField]
        private Transform m_Geometry = null;
        private Transform Geometry { get { return m_Geometry; } }


        public MutableObject Payload { get; set; }

        public float Value { get; set; }

        public float Scale
        {
            get { return transform.localScale.y; }
            set { transform.localScale = new Vector3( 1f, value, 1f ); }
        }

        public Color Color
        {
            get { return Geometry.GetComponent<Renderer>().material.color; }
            set { Geometry.GetComponent < Renderer >().material.color = value; }
        }

    }
}
