using System.Linq;
using Chains;
using ChainViews;

namespace Utility.Undo
{
    public class NodeCommentChange : UndoItem
    {
        private int nodeID;
        private string commentBefore, commentAfter;

        public NodeCommentChange(ChainNode node, string commentAfter)
        {
            nodeID = node.JsonId;
            commentBefore = node.Comment;
            this.commentAfter = commentAfter;
        }

        private ChainNode Node
        {
            get
            {
                return
                    ChainView.Instance.RootGroupView.RecursiveNodeViewsEnumerable.First(
                        cnv => cnv.ChainNode.JsonId == nodeID).ChainNode;
            }
        }

        public override void DoUndo()
        {
            Node.Comment = commentBefore;
        }

        public override void DoRedo()
        {
            Node.Comment = commentAfter;
        }
    }
    public class GroupCommentChange : UndoItem
    {
        private int groupD_ID;
        private string commentBefore, commentAfter;

        public GroupCommentChange(ChainGroupView group, string commentAfter)
        {
            groupD_ID = group.BackgroundPanel.Draggable.DragID;
            commentBefore = group.Group.Comment;
            this.commentAfter = commentAfter;
        }

        private ChainGroup Group
        {
            get
            {
                return
                    ChainView.Instance.RootGroupView.RecursiveGroupViewsEnumerable.First(
                        cgv => cgv.BackgroundPanel.Draggable.DragID == groupD_ID).Group;
            }
        }

        public override void DoUndo()
        {
            Group.Comment = commentBefore;
        }

        public override void DoRedo()
        {
            Group.Comment = commentAfter;
        }
    }

}