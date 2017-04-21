using System.Collections.Generic;
using System.Linq;
using ChainViews;
using Ui;
using UnityEngine;

namespace Utility.Undo
{
    public class ChainNodeMoved : UndoItem // TODO: Split into ChainNodeMoved, ChainGroupMoved, ChoreographyMoved
    {
        private int d_id;
        private bool recursive;
        private Vector3 difference;

        private List<Transform> targets
        {
            get
            {
                if(!recursive)
                    return new List<Transform>(new[] { Draggable.Draggables[d_id].transform });

                var list =
                    ChainView.GetDescendentNodeViews(Draggable.Draggables[d_id].GetComponent<ChainNodeView>()).Select(cnv => cnv.transform).ToList();
                list.Add(Draggable.Draggables[d_id].transform);
                return list;
            }
        }

        public ChainNodeMoved(Draggable d, Vector3 diff)
        {
            d_id = d.DragID;
            recursive = d.Targets.Count > 1;
            difference = diff;
        }

        public override void DoUndo()
        {
            foreach(var i in targets)
                i.localPosition -= difference;
            Draggable.Draggables[d_id].FireDragEnded();
            ChainView.Instance.TargetsDirty = true;
        }

        public override void DoRedo()
        {
            foreach(var i in targets)
                i.localPosition += difference;
            Draggable.Draggables[d_id].FireDragEnded();
            ChainView.Instance.TargetsDirty = true;
        }
    }

    public class ChainGroupMoved : UndoItem
    {
        private int d_id;
        private Vector3 difference;

        private List<Transform> targets
        {
            get { return Draggable.Draggables[d_id].Targets; }
        } 

        public ChainGroupMoved(Draggable d, Vector3 diff)
        {
            d_id = d.DragID;
            difference = diff;
        }

        public override void DoUndo()
        {
            foreach(var i in targets)
                i.localPosition -= difference;
            Draggable.Draggables[d_id].FireDragEnded();
            ChainView.Instance.TargetsDirty = true;
        }

        public override void DoRedo()
        {
            foreach(var i in targets)
                i.localPosition += difference;
            Draggable.Draggables[d_id].FireDragEnded();
            ChainView.Instance.TargetsDirty = true;
        }
    }

    // TODO: Add ChoreographyStepMoved
}