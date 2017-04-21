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
using UnityEngine;
using Utility;

namespace Visualizers.FilledGraph
{
    public class FilledGraphVisualizer : Visualizer
    {
        public override event Action OnVisualizerDestroyed = delegate { };

        private List<Vector2> m_LinePoints = new List<Vector2>();
        private List<Vector2> LinePoints { get { return m_LinePoints; } set { m_LinePoints = value; } }

        private List<Renderer> Renderers { get; set; }
        private List<MeshFilter> MeshFilters { get; set; }
        private List<Transform> RendererTransforms { get; set; }

        private const int MaxPointsPerMesh = 1000;

        [SerializeField]
        private Material m_LineMaterial;
        private Material LineMaterial { get { return m_LineMaterial; } set { m_LineMaterial = value; } }

        private Material LocalMaterial { get; set; }

        private Color MainColor { get; set; }

        public void Destroy()
        {
            Destroy(gameObject);

            DestroyImmediate(LocalMaterial);

            OnVisualizerDestroyed();
        }

        public void SetGraphData( Color mainColor )
        {
            MainColor = mainColor;
        }

        public void AddPoint( float xPosition, float yPosition)
        {
            LinePoints.Add( new Vector2(xPosition, yPosition) );
        }

        public void ApplyPoints()
        {
            if (LinePoints.Count < 2)
                return;
            
            Renderers = new List<Renderer>();
            MeshFilters = new List<MeshFilter>();
            RendererTransforms = new List<Transform>();

            int counter = 0;

            List<Vector2>.Enumerator pointEnumerator = LinePoints.GetEnumerator();
            bool morePoints = pointEnumerator.MoveNext();

            while ( morePoints )
            {
                var rendererObject = new GameObject("Renderer Object " + counter++);

                rendererObject.transform.parent = transform;
                rendererObject.transform.localScale = Vector3.one;
                rendererObject.transform.localPosition = Vector3.zero;
                rendererObject.transform.localRotation = Quaternion.identity;


                var vertList = new List< Vector2 >();
                while ( morePoints && vertList.Count < MaxPointsPerMesh )
                {
                    var thisVert = pointEnumerator.Current;

                    vertList.Add( thisVert );

                    morePoints = pointEnumerator.MoveNext();
                }

                vertList.Add( pointEnumerator.Current );
                

                var mesh = new Mesh();
                mesh.vertices = vertList.SelectMany( vertex =>
                    new List< Vector3 >()
                    {
                        /*transform.position + transform.PiecewiseMultiply( */new Vector3( 0, vertex.y, vertex.x ) ,
                        /*transform.position + transform.PiecewiseMultiply( */new Vector3( 0, 0, vertex.x ) 
                    } ).ToArray();

                var indices = Enumerable.Range( 0, vertList.Count-1 ).SelectMany( i=> new List<int>(){2*i, 2*i+1, 2*i+2, 2*i+1, 2*i+2, 2*i+3} ).ToArray();

                mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

                mesh.UploadMeshData( false );

                var meshFilter = rendererObject.AddComponent<MeshFilter>();
                MeshFilters.Add(meshFilter);
                meshFilter.mesh = mesh;

                var localMaterial = Instantiate(LineMaterial);
                localMaterial.SetColor("_Color", MainColor);

                var myRenderer = rendererObject.AddComponent<MeshRenderer>();
                Renderers.Add(myRenderer);
                myRenderer.material = localMaterial;
            }

        }
    }
}
