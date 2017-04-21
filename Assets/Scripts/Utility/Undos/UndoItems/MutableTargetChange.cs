using System.Linq;
using ChainViews;
using ChainViews.Elements;
using Mutation;
using UnityEngine;

namespace Utility.Undo
{
    public class MutableTargetChange : UndoItem
    {
        private int node_id;
        private string field;
        private string keyBefore, keyAfter;

        public MutableTargetChange(MutableTargetViewBehaviour targ, string before, string after)
        {
            field = targ.name;
            node_id = targ.transform.parent.parent.GetComponent<ChainNodeView>().ChainNode.JsonId;
            keyBefore = before;
            keyAfter = after;
        }

        private MutableTargetViewBehaviour GetBehaviour()
        {
            var node =
                ChainView.Instance.RootGroupView.RecursiveNodeViewsEnumerable.First(nv => nv.ChainNode.JsonId == node_id);
            return (MutableTargetViewBehaviour)node.ControllableUiItems.First(mb => (mb is MutableTargetViewBehaviour) && mb.name == field);
        }

        public override void DoUndo()
        {
            GetBehaviour().DoUndo(keyBefore);
        }

        public override void DoRedo()
        {
            GetBehaviour().DoUndo(keyAfter);
        }
    }
}