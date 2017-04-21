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
using Mutation.Mutators.DefaultValue;
using UnityEngine;

namespace Visualizers.CsView
{
    public class CsVisContainer
    {
        public CsVisContainer()
        {
        }

        public CsVisContainer(ChallengeSetVisualizer visualizer)
        {
            Visualizer = visualizer;
        }

        public ChallengeSetVisualizer Visualizer { get; set; }
    }

    public class ChallengeSetVisualizer : Visualizer
    {
        public List<CsViewComponent> Components { get; set; }

        private Vector3 m_boundMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        public Vector3 ComponentBoundMax { get { return m_boundMax; } set { m_boundMax = value; } }

        private Vector3 m_boundMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        public Vector3 ComponentBoundMin { get { return m_boundMin; } set { m_boundMin = value; } }

        public void EncapsulatePoint(Vector3 point)
        {
            var newMax = ComponentBoundMax;
            newMax.x = Mathf.Max(point.x, ComponentBoundMax.x);
            newMax.y = Mathf.Max(point.y, ComponentBoundMax.y);
            newMax.z = Mathf.Max(point.z, ComponentBoundMax.z);

            var newMin = ComponentBoundMin;
            newMin.x = Mathf.Min(point.x, ComponentBoundMin.x);
            newMin.y = Mathf.Min(point.y, ComponentBoundMin.y);
            newMin.z = Mathf.Min(point.z, ComponentBoundMin.z);

            ComponentBoundMax = newMax;
            ComponentBoundMin = newMin;
        }

        [SerializeField]
        private Material m_DetailMaterial;
        public Material DetailMaterial { get { return m_DetailMaterial; } set { m_DetailMaterial = value; } }

        public void SetTexture(Texture texture)
        {
            DetailMaterial.SetTexture("_MainTex", texture);
            //DetailMaterial.SetTexture("_BumpMap", texture );
            //DetailMaterial.SetTexture("_EmissionMap", texture);
        }

        //public void OnDrawGizmos()
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawWireSphere( ComponentBoundMin+transform.position, .5f );
        //    Gizmos.DrawWireSphere(ComponentBoundMax + transform.position, .5f);
        //    Gizmos.DrawLine( ComponentBoundMax + transform.position, ComponentBoundMin + transform.position);
        //    Gizmos.color = Color.blue;
        //    Gizmos.DrawWireSphere((ComponentBoundMax + ComponentBoundMin)/2 + transform.position, .6f);
        //}
    }
}
