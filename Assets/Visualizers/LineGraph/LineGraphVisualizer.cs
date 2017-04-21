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
using System.Text;
using Assets.Utility;
using Chains;
using FocusCamera;
using Mutation;
using UnityEngine;
using Utility;
using Visualizers.MetaSelectors;

namespace Visualizers.LineGraph
{
    public class VertexDefinition
    {
        public VertexDefinition(Vector3 position, Color color)
        {
            Position = position;
            Color = color;
        }

        public VertexDefinition(Vector3 position, Color color, Vector3 lastPosition, Vector3 nextPosition)
        {
            Position = position;
            Color = color;
            LastPosition = lastPosition;
            NextPosition = nextPosition;
        }

        public Vector3 Position { get; set; }
        public Vector3 LastPosition { get; set; }
        public Vector3 NextPosition { get; set; }
        public Color Color { get; set; }
    }

    public class LineGraphVisualizer : Visualizer
    {

        public override event Action OnVisualizerDestroyed = delegate { };

        private List<VertexDefinition> m_LinePoints = new List<VertexDefinition>();
        private List<VertexDefinition> LinePoints { get { return m_LinePoints; } set { m_LinePoints = value; } }

        private List<Renderer> Renderers { get; set; }
        private List<MeshFilter> MeshFilters { get; set; }
        private List<Transform> RendererTransforms { get; set; }

        private const int MaxPointsPerMesh = 1000;

        [SerializeField]
        private Material m_LineMaterial;
        private Material LineMaterial { get { return m_LineMaterial; } set { m_LineMaterial = value; } }

        private Color MainColor { get; set; }
        private float LineWidth { get; set; }
        private Color EdgeColor { get; set; }
        private float EdgeWidth { get; set; }
        private float PointWidth { get; set; }
        private bool PulseLine { get; set; }
        private float PulseWidth { get; set; }
        private Material LocalMaterial { get; set; }
        private float ZDepthOffset { get; set; }
        private bool WipeLine { get; set; }
        private float StartTime { get; set; }
        private float WipeDuration { get; set; }

        private void Start()
        {

        }

        public void Destroy()
        {
            Destroy(gameObject);

            DestroyImmediate(LocalMaterial);

            OnVisualizerDestroyed();
        }

        public void AddPoint(Vector3 drawPoint, Color pointColor)
        {
            var localPoint = transform.localScale.PiecewiseMultiply(
                transform.localToWorldMatrix.MultiplyPoint(drawPoint));

            LinePoints.Add(new VertexDefinition(localPoint, pointColor));
        }

        public void SetLineData(Color mainColor, float lineWidth, float zDepthOffset, Color edgeColor, float edgeWidth, float pointWidth, bool pulseLine, float pulseWidth, bool wipeLine, float startTime, float wipeDuration)
        {
            MainColor = mainColor;
            LineWidth = lineWidth;
            ZDepthOffset = zDepthOffset;
            EdgeColor = edgeColor;
            EdgeWidth = edgeWidth;
            PointWidth = pointWidth;
            PulseWidth = pulseWidth;
            PulseLine = pulseLine;
            WipeLine = wipeLine;
            StartTime = startTime;
            WipeDuration = wipeDuration;
        }

        public void ApplyPoints()
        {
            if (LinePoints.Count < 2)
                return;

            Renderers = new List<Renderer>();
            MeshFilters = new List<MeshFilter>();
            RendererTransforms = new List<Transform>();


            int counter = 0;

            List<VertexDefinition>.Enumerator pointEnumerator = LinePoints.GetEnumerator();
            bool morePoints = pointEnumerator.MoveNext();
            var firstVert = LinePoints.First();
            var secondVert = LinePoints.Skip(1).First();
            var lastVertex = new VertexDefinition(firstVert.Position - secondVert.Position, firstVert.Color);


            while (morePoints)
            {
                var rendererObject = new GameObject("Renderer Object " + (counter++));

                rendererObject.transform.parent = transform;

                //rendererObject.transform.localScale = Vector3.one;

                RendererTransforms.Add(rendererObject.transform);

                var mesh = new Mesh();

                var vertList = new List<VertexDefinition>();

                while (morePoints && vertList.Count < MaxPointsPerMesh)
                {
                    var thisVert = pointEnumerator.Current;

                    thisVert.LastPosition = lastVertex.Position;
                    lastVertex.NextPosition = thisVert.Position;

                    if ( ( thisVert.Position - lastVertex.Position ).sqrMagnitude > .0001f )
                    {
                        vertList.Add( thisVert );

                        lastVertex = thisVert;
                    }

                    morePoints = pointEnumerator.MoveNext();
                }

                lastVertex.NextPosition = !morePoints 
                    ? lastVertex.Position + Vector3.up * .2f 
                    : pointEnumerator.Current.Position;
                //lastVertex.LastPosition = lastVertex.Position - Vector3.up * .2f;

                if (morePoints)
                    pointEnumerator.Current.LastPosition = lastVertex.Position;

                vertList.Add(!morePoints ? lastVertex :pointEnumerator.Current);

                mesh.vertices = vertList.Select(vertex => vertex.Position).ToArray();
                mesh.colors = vertList.Select(vertex => vertex.Color).ToArray();
                mesh.normals = vertList.Select(vertex => vertex.NextPosition - vertex.Position).ToArray();
                mesh.tangents = vertList.Select(vertex => (vertex.LastPosition - vertex.Position).ConvertToVector4()).ToArray();

                mesh.colors[0] = Color.magenta;
                mesh.colors[mesh.colors.Count() - 1] = Color.magenta;

                // for ( int i=0; i<mesh.normals.Count(); i++ )
                // {
                //     var normal = mesh.normals[ i ];
                //     var tangent = mesh.tangents[ i ];
                //     var vertex = mesh.vertices[ i ];
                //
                //     if ( normal.sqrMagnitude < Mathf.Pow( .001f, 2f ) )
                //     {
                //
                //         //Debug.Log( "What?  This offset should be greater than zero..." );
                //     }
                // }

                var indices = new List<int>();
                //   indices.Add( 0 );
                for (var i = 0; i < vertList.Count; i++)
                {
                    indices.Add(i);
                }
                //   indices.Add( vertList.Count-1 );

                mesh.SetIndices(indices.ToArray(), MeshTopology.LineStrip, 0);

                mesh.UploadMeshData(false);

                var meshFilter = rendererObject.AddComponent<MeshFilter>();
                MeshFilters.Add(meshFilter);
                meshFilter.mesh = mesh;

                var localMaterial = Instantiate(LineMaterial);

                localMaterial.SetColor("_Color", MainColor);
                localMaterial.SetColor("EdgeColor", EdgeColor);
                localMaterial.SetFloat("_Size", LineWidth);
                localMaterial.SetFloat("_ZDepthOverride", ZDepthOffset);
                localMaterial.SetFloat("_Edge", EdgeWidth);
                localMaterial.SetFloat("_PointEdge", PointWidth);
                localMaterial.SetFloat("_PulseWidth", PulseWidth);
                localMaterial.SetFloat("_Pulse", PulseLine ? 1f : 0f);
                localMaterial.SetFloat("_Wipe", WipeLine?1f:0f );
                localMaterial.SetFloat("_StartTime", StartTime );
                localMaterial.SetFloat("_WipeDuration", WipeDuration);

                var myRenderer = rendererObject.AddComponent<MeshRenderer>();
                Renderers.Add(myRenderer);
                myRenderer.material = localMaterial;
            }

            foreach (Renderer rend in Renderers)
                rend.transform.SetWorldScale(Vector3.one);

        }

        public void OnDrawGizmosSelected()
        {
            if ( LinePoints == null )
                return;
            
            foreach ( var point in LinePoints )
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere( point.Position, .1f );

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine( point.Position+Vector3.up*.1f, point.LastPosition+Vector3.up*.1f );

                Gizmos.color = Color.green;
                Gizmos.DrawLine(point.Position - Vector3.up * .1f, point.NextPosition- Vector3.up * .1f);
            }

        }



        #region FocusableTarget implementation

        public void UpdateInput()
        {
        }

        public Vector3 CameraLocation()
        {
            throw new NotImplementedException();
        }

        public List<CameraTarget> CameraTargets()
        {
            throw new NotImplementedException();
        }

        public void FocusTarget()
        {
            throw new NotImplementedException();
        }

        public void UnFocusTarget()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
