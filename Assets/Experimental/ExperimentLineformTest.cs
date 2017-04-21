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
using System.Linq;
using Assets.Utility;
using UnityEngine;
using Utility;
using Visualizers.LineGraph;
using ColorUtility = Assets.Utility.ColorUtility;

namespace Experimental
{
    public class ExperimentLineformTest : MonoBehaviour
    {
        [SerializeField]
        private List<Transform> m_Points = new List<Transform>();
        private List<Transform> Points { get { return m_Points; } set { m_Points = value; } }

        private GameObject RendererObject { get; set; }

        [SerializeField]
        private Material m_LineMaterial;
        private Material LineMaterial { get { return m_LineMaterial; } set { m_LineMaterial = value; } }

        [SerializeField]
        private MeshFilter m_meshFilter;
        private MeshFilter MeshFilter { get { return m_meshFilter; } set { m_meshFilter = value; } }

        private const float COLOR_MOD = .29f;


        private List<VertexDefinition> VertList { get; set; }

        public void OnDrawGizmos()
        {
            if (MeshFilter.mesh == null)
                MeshFilter.mesh = new Mesh();

            if (Points == null || Points.Count<3)
                return;

            VertList = new List<VertexDefinition>();

            float colorNum = 0f;

            var lastVert = new VertexDefinition(Points.First().position, ColorUtility.HsvtoRgb(0, .8f, 1f));
            
            foreach (var point in Points)
            {
                var thisVert = new VertexDefinition(point.position, 
                    ColorUtility.HsvtoRgb(colorNum, .8f, 1f),
                    lastVert.Position, 
                    point.position);

                colorNum += COLOR_MOD;
                VertList.Add(thisVert);
                lastVert.NextPosition = thisVert.Position;
                lastVert = thisVert;
            }

            // last point required here to complete triangle

            MeshFilter.mesh.Clear();

            MeshFilter.mesh.vertices = VertList.Select(vertex => vertex.Position).ToArray();
            MeshFilter.mesh.colors = VertList.Select(vertex => vertex.Color).ToArray();
            MeshFilter.mesh.normals = VertList.Select(vertex => vertex.NextPosition-vertex.Position).ToArray();
            MeshFilter.mesh.tangents = VertList.Select(vertex => (vertex.LastPosition-vertex.Position).ConvertToVector4()).ToArray();


            var indices = new List<int>();
            for (var i = 0; i < VertList.Count; i++)
            {
                indices.Add(i);
            }

            MeshFilter.mesh.SetIndices(indices.ToArray(), MeshTopology.LineStrip, 0);

            MeshFilter.mesh.UploadMeshData(false);

        }
    }
}
