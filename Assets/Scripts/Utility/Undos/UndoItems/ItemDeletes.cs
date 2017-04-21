using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chains;
using ChainViews;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Utility.JobManagerSystem;

namespace Utility.Undo
{
    public class NodeDelete : UndoItem
    {
        private int jsonid;
        private int nodeDragId;
        private int groupDragId;
        private Vector3 initialLocation;
        private string json;
        private Dictionary<string, int[]> router;
        private int parentJsonID;
        private string[] parentStates;

        public NodeDelete(ChainNode deletedNode)
        {
            var recievingGroup =
                ChainView.Instance.RootGroupView.RecursiveGroupViewsEnumerable.First(
                    cgv => cgv.Group.Nodes.Contains(deletedNode));
            var chainNodeView = recievingGroup.NodeViewsEnumerable.First(cnv => cnv.ChainNode == deletedNode);

            jsonid = deletedNode.JsonId;
            nodeDragId = chainNodeView.Draggable.DragID;
            groupDragId = recievingGroup.BackgroundPanel.Draggable.DragID;
            initialLocation = chainNodeView.transform.position;

            try
            {
                var parent = StateRouter.NodeParents[deletedNode];
                parentJsonID = parent.Node.JsonId;
                parentStates = parent.States.Select(s => s.Name).ToArray();
            }
            catch(KeyNotFoundException)
            {
                parentJsonID = -1;
                parentStates = new string[] { };
            }
            router = new Dictionary<string, int[]>();
            foreach(var state in deletedNode.Router.SelectionStatesEnumerable)
            {
                router[state.Name] = state.TargetsEnumerable.Select(cn => cn.JsonId).ToArray();
            }
            deletedNode.Router.UntargetAllTargets();
            deletedNode.RequestUntargeting();

            json = JsonConvert.SerializeObject(deletedNode, Formatting.Indented,
                HaxxisPackage.GetSerializationSettings(TypeNameHandling.All));
        }

        public override void DoUndo()
        {
            var respawn = JsonConvert.DeserializeObject<ChainNode>(json,
                HaxxisPackage.GetSerializationSettings(TypeNameHandling.All));

            if(respawn == null)
            {
                throw new InvalidOperationException("Deserializer could not respawn node!");
            }
            respawn.InitializeSchema();
            respawn.JsonId = jsonid;
            respawn.SuppressUndos = true;

            var groupView =
                ChainView.Instance.RootGroupView.RecursiveGroupViewsEnumerable.First(
                    cgv => cgv.BackgroundPanel.Draggable.DragID == groupDragId);
            groupView.Group.AddNode(respawn, false);
            var nodeView = groupView.NodeViewsEnumerable.First(cnv => cnv.ChainNode == respawn);
            nodeView.transform.position = initialLocation;
            nodeView.Draggable.DragID = nodeDragId;

            respawn.SuppressUndos = false;
            JobManager.Instance.StartJob(RelinkRouterJob(respawn), jobName: "Relink Routers", maxExecutionsPerFrame: 1);
        }

        private IEnumerator RelinkRouterJob(ChainNode respawn)
        {
            //yield return null;
            //yield return null; // Ensure that the nodes we're looking for exist

            try
            {
                foreach(var state in router.Keys)
                    foreach(var target in router[state])
                        respawn.Router[state].AddTarget(
                            ChainView.Instance.RootGroupView.RecursiveNodeViewsEnumerable.First(
                                cnv => cnv.ChainNode.JsonId == target).ChainNode, false);

                if(parentJsonID == -1) yield break;
                var parentRouter =
                    ChainView.Instance.RootGroupView.RecursiveNodeViewsEnumerable.First(
                        cnv => cnv.ChainNode.JsonId == parentJsonID).ChainNode.Router;
                foreach(var state in parentStates)
                {
                    parentRouter[state].AddTarget(respawn, false);
                }
            }
            catch(Exception)
            {
            }
        } 

        public override void DoRedo()
        {
            var node =
                ChainView.Instance.RootGroupView.RecursiveNodeViewsEnumerable.First(
                    cnv => cnv.ChainNode.JsonId == jsonid).ChainNode;
            node.SuppressUndos = true;
            node.Destroy(false);
        }
    }

    public class GroupDelete : UndoItem // TODO: DO
    {
        private int groupDragId;
        private string groupComment;
        private string groupOrigin;
        private int parentDragId;
        private Vector3 initialLocation;

        public GroupDelete(ChainGroup group)
        {
            var myGroup = ChainView.Instance.RootGroupView.RecursiveGroupViewsEnumerable.First(
                    cgv => cgv.Group == group);
            var parentGroup = ChainView.Instance.RootGroupView.RecursiveGroupViewsEnumerable.First(
                    cgv => cgv.Group.Groups.Contains(group));

            groupDragId = myGroup.BackgroundPanel.Draggable.DragID;
            parentDragId = parentGroup.BackgroundPanel.Draggable.DragID;
            initialLocation = myGroup.ChildAttachmentPoint.position;

            groupComment = group.Comment;
            groupOrigin = myGroup.LoadedPackagePath;
        }

        public override void DoUndo()
        {
            var group = new ChainGroup()
            {
                SuppressUndo = true,
                Comment = groupComment
            };
            var parentGroupView =
                ChainView.Instance.RootGroupView.RecursiveGroupViewsEnumerable.First(
                    cgv => cgv.BackgroundPanel.Draggable.DragID == parentDragId);
            var parentGroup = parentGroupView.Group;

            parentGroup.AddGroup(group);

            var groupView =
                ChainView.Instance.RootGroupView.RecursiveGroupViewsEnumerable.First(cgv => cgv.Group == group);
            groupView.transform.localPosition = Vector2.zero;
            groupView.SetVisiblePosition(initialLocation, false);
            groupView.BackgroundPanel.Draggable.DragID = groupDragId;
            groupView.LoadedPackagePath = groupOrigin;
            group.SuppressUndo = false;
        }

        public override void DoRedo()
        {
            var group =
                ChainView.Instance.RootGroupView.RecursiveGroupViewsEnumerable.First(
                    cgv => cgv.BackgroundPanel.Draggable.DragID == groupDragId).Group;
            group.SuppressUndo = true;
            group.RequestRemoval();
        }
    }
}
