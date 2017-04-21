namespace Utility.Undo
{
    public abstract class UndoItem
    {
        public abstract void DoUndo();
        public abstract void DoRedo();
    }
}