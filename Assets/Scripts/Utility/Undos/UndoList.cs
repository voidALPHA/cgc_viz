using System;
using System.Collections.Generic;

namespace Utility.Undo
{
    public class UndoList : UndoItem
    {
        private List<UndoItem> UndoItems;

        public UndoList(IEnumerable<UndoItem> items)
        {
            UndoItems = new List<UndoItem>(items);
        }

        public override void DoRedo()
        {
            UndoItems.Reverse(); // Do Redos in reverse
            foreach(var i in UndoItems)
                i.DoRedo();
            UndoItems.Reverse();
        }

        public override void DoUndo()
        {
            foreach(var i in UndoItems)
                i.DoUndo();
        }
    }

}
