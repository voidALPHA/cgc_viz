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

#region Unity Technologies License

// The majority of code came from Unity-Technologies UI Repository, specifically
// https://bitbucket.org/Unity-Technologies/ui/src/f0c70f707cf09f959ad417049cb070f8e296ffe2/UnityEngine.UI/UI/Core/GraphicRaycaster.cs?at=5.5
// Any code from Unity Technologies is released by them via the MIT license:

// Copyright (c) 2014-2015, Unity Technologies

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#endregion

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChainViews;
using Ui;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


namespace UnityEngine.UI
{
    [AddComponentMenu("Event/Graphic Raycaster - voidALPHA")]
    [RequireComponent(typeof(Canvas))]
    public class GraphicRaycasterVA : BaseRaycaster
    {
        public static GraphicRaycasterVA Instance;

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        protected const int kNoEventMaskSet = -1;

        public enum BlockingObjects
        {
            None = 0,
            TwoD = 1,
            ThreeD = 2,
            All = 3,
        }

        [FormerlySerializedAs("ignoreReversedGraphics")] [SerializeField] private bool m_IgnoreReversedGraphics = true;

        [FormerlySerializedAs("blockingObjects")] [SerializeField] private BlockingObjects m_BlockingObjects =
            BlockingObjects.None;

        public bool ignoreReversedGraphics
        {
            get { return m_IgnoreReversedGraphics; }
            set { m_IgnoreReversedGraphics = value; }
        }

        public BlockingObjects blockingObjects
        {
            get { return m_BlockingObjects; }
            set { m_BlockingObjects = value; }
        }

        [SerializeField] protected LayerMask m_BlockingMask = kNoEventMaskSet;

        private List<Canvas> m_Canvases = new List<Canvas>();
        public Canvas ChoreoCanvas { get; set; }

        public void AddCanvas(Canvas c)
        {
            m_Canvases.Add(c);
        }

        public void RemoveCanvas(Canvas c)
        {
            m_Canvases.Remove(c);
        }

        [NonSerialized] private List<Graphic> m_RaycastResults = new List<Graphic>();
        [NonSerialized] public readonly List<RaycastResult> LastResultList = new List<RaycastResult>(); 

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            m_Canvases.RemoveAll(c => c == null);

            if(m_Canvases.Count == 0)
                return;

            var eventPosition = Display.RelativeMouseAt(eventData.position);
            int displayIndex = m_Canvases[0].targetDisplay;

            // Discard events that are not part of this display so the user does not interact with multiple displays at once.
            if(eventPosition.z != displayIndex)
                return;

            // The multiple display system is not supported on all platforms, when it is not supported the returned position
            // will be all zeros so when the returned index is 0 we will default to the event data to be safe.
            if(eventPosition.z == 0)
                eventPosition = eventData.position;

            // Convert to view space
            Vector2 pos;
            if(eventCamera == null)
            {
                // Multiple display support only when not the main display. For display 0 the reported
                // resolution is always the desktops resolution since its part of the display API,
                // so we use the standard none multiple display method. (case 741751)
                float w = Screen.width;
                float h = Screen.height;
                if(displayIndex > 0 && displayIndex < Display.displays.Length)
                {
                    w = Display.displays[displayIndex].systemWidth;
                    h = Display.displays[displayIndex].systemHeight;
                }
                pos = new Vector2(eventPosition.x / w, eventPosition.y / h);
            }
            else
                pos = eventCamera.ScreenToViewportPoint(eventPosition);

            // If it's outside the camera's viewport, do nothing
            if(pos.x < 0f || pos.x > 1f || pos.y < 0f || pos.y > 1f)
                return;

            float hitDistance = float.MaxValue;

            Ray ray = new Ray();

            if(eventCamera != null)
                ray = eventCamera.ScreenPointToRay(eventPosition);
            
            float dist = 100.0f;

            if(eventCamera != null)
                dist = eventCamera.farClipPlane - eventCamera.nearClipPlane;

            if(blockingObjects == BlockingObjects.ThreeD || blockingObjects == BlockingObjects.All)  // voidALPHA - removed dependence on inaccessible ReflectionMethodsCache
            {
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit, dist, m_BlockingMask))
                    hitDistance = hit.distance;
            }

            if(blockingObjects == BlockingObjects.TwoD || blockingObjects == BlockingObjects.All)
            {
                var hit = Physics2D.Raycast((Vector2)ray.origin, (Vector2)ray.direction,
                    dist, (int)m_BlockingMask);
                if(hit.collider)
                    hitDistance = hit.fraction * dist;
            }

            m_RaycastResults.Clear();
            Raycast(m_Canvases, eventCamera, eventPosition, m_RaycastResults);

            for(var index = 0; index < m_RaycastResults.Count; index++)
            {
                var go = m_RaycastResults[index].gameObject;
                bool appendGraphic = true;

                if(ignoreReversedGraphics)
                {
                    if(eventCamera == null)
                    {
                        // If we dont have a camera we know that we should always be facing forward
                        var dir = go.transform.rotation * Vector3.forward;
                        appendGraphic = Vector3.Dot(Vector3.forward, dir) > 0;
                    }
                    else
                    {
                        // If we have a camera compare the direction against the cameras forward.
                        var cameraFoward = eventCamera.transform.rotation * Vector3.forward;
                        var dir = go.transform.rotation * Vector3.forward;
                        appendGraphic = Vector3.Dot(cameraFoward, dir) > 0;
                    }
                }

                if(appendGraphic)
                {
                    float distance = 0;

                    if(eventCamera == null)
                        distance = 0;
                    else
                    {
                        Transform trans = go.transform;
                        Vector3 transForward = trans.forward;
                        // http://geomalgorithms.com/a06-_intersect-2.html
                        distance = (Vector3.Dot(transForward, trans.position - ray.origin) /
                                    Vector3.Dot(transForward, ray.direction));

                        // Check to see if the go is behind the camera.
                        if(distance < 0)
                            continue;
                    }

                    if(distance >= hitDistance)
                        continue;

                    var castResult = new RaycastResult
                    {
                        gameObject = go,
                        module = this,
                        distance = distance,
                        screenPosition = eventPosition,
                        index = resultAppendList.Count,
                        depth = m_RaycastResults[index].depth,
                        sortingLayer = m_Canvases[0].sortingLayerID,
                        sortingOrder = m_Canvases[0].sortingOrder
                    };
                    resultAppendList.Add(castResult);
                }
            }

            LastResultList.Clear();
            LastResultList.AddRange(resultAppendList);
        }

        public override Camera eventCamera
        {
            get
            {
                return m_Canvases[0].worldCamera ?? Camera.main;
            }
        }

        /// <summary>
        /// Perform a raycast into the screen and collect all graphics underneath it.
        /// </summary>
        [NonSerialized] static readonly HashSet<Graphic> SortedGraphics = new HashSet<Graphic>();
        [NonSerialized] static readonly List<Graphic> AllWorkspaceGraphics = new List<Graphic>(), AllChoreoGraphics = new List<Graphic>();

        private int LastKnownGraphicCount = 0;
        private readonly Dictionary<Graphic, List<Graphic>> HierarchyCacheDictionary = new Dictionary<Graphic, List<Graphic>>(); 
        private readonly List<Graphic> RootWorkspaceGraphics = new List<Graphic>(); 
        private readonly List<Graphic> RootChoreoGraphics = new List<Graphic>();
        private readonly Queue<Graphic> GraphicsToTest = new Queue<Graphic>();
        private TransformTestDictionary transformTester = new TransformTestDictionary();

        private void Raycast(List<Canvas> canvases, Camera eventCamera, Vector2 pointerPosition, List<Graphic> results)
        {
            var workspaceTest = new Predicate<Graphic>( // Unity's original test, with modifications from voidALPHA to perform less intensive tests first
            g => !g.raycastTarget ||
                 !RectTransformUtility.RectangleContainsScreenPoint(g.rectTransform, pointerPosition, eventCamera) ||
                 g.depth == -1 ||
                 !g.Raycast(pointerPosition, eventCamera)
            );
            var choreoTest = new Predicate<Graphic>( // Unity's original test, with modifications from voidALPHA to perform less intensive tests first
            g => !g.raycastTarget ||
                 !RectTransformUtility.RectangleContainsScreenPoint(g.rectTransform, pointerPosition, null) ||
                 g.depth == -1 ||
                 !g.Raycast(pointerPosition, null)
            );
            
            foreach(var canvas in canvases)
            {
                AllWorkspaceGraphics.AddRange(GraphicRegistry.GetGraphicsForCanvas(canvas)); // Stick all graphics for canvas into list
            }
            if(ChoreoCanvas)
            {
                AllChoreoGraphics.AddRange(GraphicRegistry.GetGraphicsForCanvas(ChoreoCanvas));
            }
            
            if(AllWorkspaceGraphics.Count + AllChoreoGraphics.Count != LastKnownGraphicCount) // Graphics count changed
            {
                HierarchyCacheDictionary.Clear(); // Clear out cached dictionary and root graphic list
                RootWorkspaceGraphics.Clear();
                RootChoreoGraphics.Clear();
                if(SatelliteUpdateLord.Instance.NodesProcessing) // We're still processing nodes; perform (slower) non-cached tests while nodes come up
                {
                    if(Choreography.Views.TimelineViewBehaviour.StepPicker.gameObject.activeInHierarchy ||
                       pointerPosition.y < Choreography.Views.TimelineViewBehaviour.Instance.CurrentHeight)
                    {
                        AllChoreoGraphics.RemoveAll(choreoTest);
                        foreach(var graphic in AllChoreoGraphics)
                        {
                            SortedGraphics.Add(graphic);
                        }
                    }
                    else
                    {
                        AllWorkspaceGraphics.RemoveAll(workspaceTest);
                        foreach(var graphic in AllWorkspaceGraphics)
                        {
                            SortedGraphics.Add(graphic);
                        }
                    }
                    LastKnownGraphicCount = 0;
                }
                else
                {
                    transformTester.Refresh(AllWorkspaceGraphics, transform);
                    foreach(var g in AllWorkspaceGraphics) // Not processing nodes, do a lot of work up front to improve later speed of calculations
                    {
                        HierarchyCacheDictionary[g] = new List<Graphic>();
                        g.transform.GetComponentsInChildren(HierarchyCacheDictionary[g]);
                        var p = g.transform.parent;
                        if(transformTester.TestTransform(p))
                            RootWorkspaceGraphics.Add(g);
                    }
                    transformTester.Refresh(AllChoreoGraphics, ChoreoCanvas.transform);
                    foreach(var g in AllChoreoGraphics) // Not processing nodes, do a lot of work up front to improve later speed of calculations
                    {
                        HierarchyCacheDictionary[g] = new List<Graphic>();
                        g.transform.GetComponentsInChildren(HierarchyCacheDictionary[g]);
                        var p = g.transform.parent;
                        if(transformTester.TestTransform(p))
                            RootChoreoGraphics.Add(g);
                    }
                    LastKnownGraphicCount = AllWorkspaceGraphics.Count + AllChoreoGraphics.Count;
                    //Debug.Log(RootWorkspaceGraphics.Count + " root graphics to test against");
                }
            }

            var test = workspaceTest;
            if(Choreography.Views.TimelineViewBehaviour.StepPicker.gameObject.activeInHierarchy || pointerPosition.y < Choreography.Views.TimelineViewBehaviour.Instance.CurrentHeight)
            {
                test = choreoTest;
                foreach(var g in RootChoreoGraphics)
                {
                    GraphicsToTest.Enqueue(g);
                }
            }
            else if(ChainView.Instance.Visible)
            {
                foreach(var g in RootWorkspaceGraphics)
                {
                    GraphicsToTest.Enqueue(g);
                }
            }

            while(GraphicsToTest.Count > 0)
            {
                var g = GraphicsToTest.Dequeue();
                if(SortedGraphics.Contains(g)) continue;
                if(!test(g))
                {
                    foreach(var gr in HierarchyCacheDictionary[g])
                    {
                        if(gr == g) continue;
                        GraphicsToTest.Enqueue(gr);
                    }
                    SortedGraphics.Add(g);
                }
            }

            var sgraphics = new List<Graphic>(SortedGraphics);
            sgraphics.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
            //		StringBuilder cast = new StringBuilder();
            for(int i = 0; i < sgraphics.Count; ++i)
                results.Add(sgraphics[i]);
            //		Debug.Log (cast.ToString());

            AllWorkspaceGraphics.Clear();
            AllChoreoGraphics.Clear();
            SortedGraphics.Clear();
        }

        //private void OnDrawGizmos()
        //{
        //    Gizmos.color = new Color(0f, 0.25f, 0, 0.15f);

        //    foreach(Graphic g in RootWorkspaceGraphics)
        //    {
        //        if(!g) continue;
        //        var b = RectTransformUtility.CalculateRelativeRectTransformBounds(g.transform);
        //        var r = new Rect(b.center + g.transform.position - (b.size * 0.5f), b.size);
        //        Gizmos.DrawCube(r.center, r.size);
        //    }

        //    Gizmos.color = Color.white;
        //}

        private class TransformTestDictionary
        {
            private readonly Dictionary<Transform, bool> dict = new Dictionary<Transform, bool>();

            public void Refresh(IEnumerable<Graphic> gfx, Transform root)
            {
                dict.Clear();
                foreach(var graphic in gfx)
                {
                    dict[graphic.transform] = false;
                }
                dict[root] = true;
            }

            public bool TestTransform(Transform t)
            {
                try
                {
                    return dict[t];
                }
                catch(KeyNotFoundException)
                {
                    return dict[t] = TestTransform(t.parent);
                }
                catch(ArgumentNullException)
                {
                    return false;
                }
            }
        }
    }
}