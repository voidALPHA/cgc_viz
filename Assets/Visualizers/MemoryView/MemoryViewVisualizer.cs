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

namespace Visualizers.MemoryView
{
    public class DashDefinition
    {
        public DashDefinition( Vector3 position, Vector3 sizes, Color color )
        {
            Position = position;
            Sizes = sizes;
            Color = color;
        }

        public Vector3 Position { get; set; }
        public Vector3 Sizes { get; set; }
        public Color Color { get; set; }
    }

    public class MemoryViewVisualizer : Visualizer
    {
        public override event Action OnVisualizerDestroyed = delegate { };

        private List<DashDefinition> m_DashPoints = new List< DashDefinition >();
        private List<DashDefinition> DashPoints { get { return m_DashPoints;} set { m_DashPoints = value; } }

        private List<Renderer> Renderers { get; set; }
        private List<MeshFilter> MeshFilters { get; set; }
        private List<Transform> RendererTransforms { get; set; }

        private const int MaxPointsPerMesh = 10000;

        [SerializeField] private Material m_DashMaterial;
        private Material DashMaterial { get { return m_DashMaterial;} set { m_DashMaterial = value; } }
        
        private Material LocalMaterial { get; set; }


        private void Start()
        {
            
        }

        public void Destroy()
        {
            Destroy(gameObject);

            DestroyImmediate(LocalMaterial);

            OnVisualizerDestroyed();
        }

        public void AddDash( Vector3 drawPoint, Vector3 drawSizes, Color pointColor )
        {
            var localPoint = transform.localScale.PiecewiseMultiply(
                transform.localToWorldMatrix.MultiplyPoint( drawPoint ) );

            //var localPoint = drawPoint;
            DashPoints.Add( new DashDefinition(
                localPoint, drawSizes, pointColor
                ) );
        }

        public void SetLineData()
        {
            
        }

        public void ApplyPoints()
        {

            Renderers = new List<Renderer>();
            MeshFilters = new List<MeshFilter>();
            RendererTransforms = new List<Transform>();

            int counter = 0;

            List< DashDefinition >.Enumerator pointEnumerator = DashPoints.GetEnumerator();
            bool morePoints = pointEnumerator.MoveNext();

            while ( morePoints )
            {
                var rendererObject = new GameObject("Renderer Object " + (counter++));

                rendererObject.transform.parent = transform;

                RendererTransforms.Add( rendererObject.transform );

                var mesh = new Mesh();

                var vertList = new List< DashDefinition >();

                while ( morePoints && vertList.Count < MaxPointsPerMesh )
                {
                    var thisVert = pointEnumerator.Current;

                    vertList.Add( thisVert );
                    morePoints = pointEnumerator.MoveNext();
                }

                mesh.vertices = vertList.Select( vertex => vertex.Position ).ToArray();
                mesh.normals = vertList.Select( vertex => transform.PiecewiseMultiply(vertex.Sizes) ).ToArray();
                mesh.colors = vertList.Select( vertex => vertex.Color ).ToArray();

                var indices = new List< int >();
                for ( var i = 0; i < vertList.Count; i++ )
                {
                    indices.Add( i );
                }

                mesh.SetIndices( indices.ToArray(), MeshTopology.Points, 0 );

                mesh.UploadMeshData( false );

                var meshFilter = rendererObject.AddComponent<MeshFilter>();
                MeshFilters.Add(meshFilter);
                meshFilter.mesh = mesh;

                var localMaterial = Instantiate(DashMaterial);

                localMaterial.SetVector( "_DepthAxis", transform.forward );
                localMaterial.SetVector( "_ElevationAxis", transform.up );
                localMaterial.SetVector( "_WidthAxis", transform.right );

                var myRenderer = rendererObject.AddComponent<MeshRenderer>();
                Renderers.Add(myRenderer);
                myRenderer.material = localMaterial;
            }

            foreach (Renderer rend in Renderers)
                rend.transform.SetWorldScale(Vector3.one);
        }


    }
}
