using System;
using System.Linq;
using Chains;
using ChainViews;
using ChainViews.Elements;
using Mutation;
using UnityEngine;

namespace Utility.Undo
{
    public class MutableFieldChange : UndoItem
    {
        private int node_id;
        private string field;
        private bool literalBefore, literalAfter;
        private string valueBefore, valueAfter;

        public MutableFieldChange(MutableBoxBehaviour mbb, IMutableField valBefore, string valAfter)
        {
            field = mbb.name;
            node_id = mbb.transform.parent.parent.GetComponent<ChainNodeView>().ChainNode.JsonId;
            literalBefore = valBefore.SchemaSource == SchemaSource.Literal;
            literalAfter = valAfter.StartsWith("Literal.");
            valueBefore = GetUserFacingKeyFromMutableField(valBefore);
            valueAfter = literalAfter ? valAfter.Substring(8) : valAfter;
        }

        private static string GetUserFacingKeyFromMutableField(IMutableField field)
        {
            switch(field.SchemaSource)
            {
                default:
                    return field.GetLiteralValueAsString();
                case SchemaSource.Mutable:
                    return "Local Payload" + (field.AbsoluteKey != "" ? "." : "") + field.AbsoluteKey;
                case SchemaSource.Cached:
                    return "Cached Data" + (field.AbsoluteKey != "" ? "." : "") + field.AbsoluteKey;
                case SchemaSource.Global:
                    return "Global Data" + (field.AbsoluteKey != "" ? "." : "") + field.AbsoluteKey;
            }
        }

        private MutableBoxBehaviour Behaviour
        {
            get
            {
                var node =
                    ChainView.Instance.RootGroupView.RecursiveNodeViewsEnumerable.First(nv => nv.ChainNode.JsonId == node_id);
                return
                    (MutableBoxBehaviour)node.ControllableUiItems.First(mb => (mb is MutableBoxBehaviour) && mb.name == field);
            }
        }

        public override void DoUndo()
        {
            Behaviour.DoUndo(literalBefore, valueBefore);
        }

        public override void DoRedo()
        {
            Behaviour.DoUndo(literalAfter, valueAfter);
        }
    }
}
