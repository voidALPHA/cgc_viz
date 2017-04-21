using System.Linq;
using Chains;
using ChainViews;

namespace Utility.Undo
{
    public class RouterConnection : UndoItem
    {
        private string state;
        private int idFrom, idTo;
        private bool wasAdd;

        public RouterConnection(ChainNode to, SelectionState state, bool add)
        {
            idTo = to.JsonId;
            this.state = state.Name;
            idFrom =
                ChainView.Instance.RootGroupView.RecursiveNodeViewsEnumerable.First(
                    cnv => cnv.ChainNode.Router[this.state] == state).ChainNode.JsonId;
            wasAdd = add;
        }

        private SelectionState State
        {
            get
            {
                return
                    ChainView.Instance.RootGroupView.RecursiveNodeViewsEnumerable.First(
                        cnv => cnv.ChainNode.JsonId == idFrom).ChainNode.Router[state];
            }
        }
        private ChainNode ToNode
        {
            get
            {
                return
                    ChainView.Instance.RootGroupView.RecursiveNodeViewsEnumerable.First(
                        cnv => cnv.ChainNode.JsonId == idTo).ChainNode;
            }
        }

        public override void DoUndo()
        {
            if(wasAdd)
                State.RemoveTarget(ToNode, false);
            else
                State.AddTarget(ToNode, false);
        }

        public override void DoRedo()
        {
            if(wasAdd)
                State.AddTarget(ToNode, false);
            else
                State.RemoveTarget(ToNode, false);
        }
    }

}