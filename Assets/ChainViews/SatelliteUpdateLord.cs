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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ChainViews;
using ChainViews.Elements;
using Choreography.Recording;
using JetBrains.Annotations;

public class SatelliteUpdateLord : MonoBehaviour
{
    [SerializeField]
    private static SatelliteUpdateLord s_Instance;
    public static SatelliteUpdateLord Instance { get { return s_Instance ?? (s_Instance = FindObjectOfType<SatelliteUpdateLord>()); } }

    [SerializeField]
    private byte OnscreenNodesToGenerateConcurrently = 4;
    [SerializeField]
    private byte OffscreenNodesToGenerateConcurrently = 1;

    private Queue<ChainNodeView> nodesToUpdate;
    private Queue<ChainNodeView> nodesGenerating;

    public static event Action mbbUpdate = delegate { }; 

    public bool NodesProcessing
    {
        get { return nodesToUpdate.Count + nodesGenerating.Count > 0; }
    }

    public static void Enqueue(ChainNodeView cnv)
    {
        Instance.nodesToUpdate.Enqueue(cnv);
    }

    public static void Clear()
    {
        if(!Instance.NodesProcessing) return;
        Instance.nodesToUpdate.Clear();
        Instance.nodesGenerating.Clear();
    }

    // Use this for initialization
    [UsedImplicitly]
    void Start()
    {
        nodesToUpdate = new Queue<ChainNodeView>();
        nodesGenerating = new Queue<ChainNodeView>();
    }

    // Update is called once per frame
    [UsedImplicitly]
    void Update()
    {
        if(RecordingLord.IsRecording()) return;
        var deactivatedNodesDiscovered = 0;
        while(nodesToUpdate.Count > 0 && nodesGenerating.Count < OnscreenNodesToGenerateConcurrently)
        {
            var node = nodesToUpdate.Dequeue();
            if(!node.RectTransform) continue;
            if(!node.IsVisible)
            {
                nodesToUpdate.Enqueue(node);
                deactivatedNodesDiscovered++;
                if(deactivatedNodesDiscovered > nodesToUpdate.Count) break;
                continue;
            }
            node.UpdateQueuedControllableGeneration();
            nodesGenerating.Enqueue(node);
            deactivatedNodesDiscovered = 0;
        }
        while(nodesToUpdate.Count > 0 && nodesGenerating.Count < OffscreenNodesToGenerateConcurrently)
        {
            var node = nodesToUpdate.Dequeue();
            if(!node.RectTransform) continue;
            node.UpdateQueuedControllableGeneration();
            nodesGenerating.Enqueue(node);
        }

        if(ChainView.Instance.Dragging || ChainView.Instance.Zooming) return;
        mbbUpdate();
    }

    [UsedImplicitly]
    void LateUpdate()
    {
        while(nodesGenerating.Count > 0)
        {
            var node = nodesGenerating.Dequeue();
            node.LateUpdateQueuedControllableGeneration();
            ChainView.Instance.TargetsDirty = true;
        }
    }
}
