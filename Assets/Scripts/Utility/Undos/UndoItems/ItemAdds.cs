using System;
using System.Linq;
using Chains;
using ChainViews;
using UnityEngine;

namespace Utility.Undo
{
    public class NodeAdd : UndoItem
    {
        private int jsonid;
        private int nodeDragId;
        private int groupDragId;
        private Type NodeType;
        private Vector3 initialLocation;

        public NodeAdd(ChainNode addedNode, ChainGroupView recievingGroup, Vector3 spawnLocation)
        {
            jsonid = addedNode.JsonId;
            nodeDragId = recievingGroup.NodeViewsEnumerable.First(cnv => cnv.ChainNode == addedNode).Draggable.DragID;
            groupDragId = recievingGroup.BackgroundPanel.Draggable.DragID;
            NodeType = addedNode.GetType();
            initialLocation = spawnLocation;
        }

        public override void DoUndo()
        {
            var node = ChainView.Instance.RootGroupView.RecursiveNodeViewsEnumerable.First(cnv => cnv.ChainNode.JsonId == jsonid).ChainNode;
            node.SuppressUndos = true;
            node.Destroy(false);
        }

        public override void DoRedo()
        {
            var respawn = Activator.CreateInstance(NodeType) as ChainNode;
            if(respawn == null)
            {
                throw new InvalidOperationException("Activator could not respawn undone node!");
            }
            respawn.InitializeSchema();
            respawn.JsonId = jsonid;
            respawn.SuppressUndos = true;

            var groupView = ChainView.Instance.RootGroupView.RecursiveGroupViewsEnumerable.First(cgv => cgv.BackgroundPanel.Draggable.DragID == groupDragId);
            groupView.Group.AddNode(respawn, false);
            var nodeView = groupView.NodeViewsEnumerable.First(cnv => cnv.ChainNode == respawn);
            nodeView.transform.position = initialLocation;
            nodeView.Draggable.DragID = nodeDragId;

            respawn.SuppressUndos = false;
        }
    }

    public class GroupAdd : UndoItem
    {
        private int groupDragId;
        private int parentDragId;
        private Vector3 initialLocation;

        public GroupAdd(ChainGroupView newGroup, ChainGroupView recievingGroup, Vector3 spawnLocation)
        {
            groupDragId = newGroup.BackgroundPanel.Draggable.DragID;
            parentDragId = recievingGroup.BackgroundPanel.Draggable.DragID;
            initialLocation = spawnLocation;
        }

        public override void DoUndo()
        {
            var group =
                ChainView.Instance.RootGroupView.RecursiveGroupViewsEnumerable.First(
                    cgv => cgv.BackgroundPanel.Draggable.DragID == groupDragId).Group;
            group.SuppressUndo = true;
            group.RequestRemoval();
        }

        public override void DoRedo()
        {
            var group = new ChainGroup()
            {
                SuppressUndo = true
            };
            var parentGroupView =
                ChainView.Instance.RootGroupView.RecursiveGroupViewsEnumerable.First(
                    cgv => cgv.BackgroundPanel.Draggable.DragID == parentDragId);
            var parentGroup = parentGroupView.Group;

            parentGroup.AddGroup(group);

            var groupView = ChainView.Instance.RootGroupView.RecursiveGroupViewsEnumerable.First(cgv => cgv.Group == group);
            groupView.transform.localPosition = Vector2.zero;
            groupView.SetVisiblePosition(initialLocation, false);
            groupView.BackgroundPanel.Draggable.DragID = groupDragId;
            group.SuppressUndo = false;
        }
    }
}