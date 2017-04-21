using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Utility.Undo
{
    public class UndoLord : MonoBehaviour
    {
        public class UndoStackClass
        {
            public int Capacity { get; set; }

            private List<UndoItem> undoItems;
            private List<UndoItem> redoItems;

            public UndoStackClass(int capacity)
            {
                Capacity = capacity;
                undoItems = new List<UndoItem>(capacity);
                redoItems = new List<UndoItem>(capacity);
            }

            public void DropStack()
            {
                undoItems.Clear();
                redoItems.Clear();
            }

            public void Push(UndoItem item)
            {
                redoItems.Clear();
                undoItems.Add(item);
                while(undoItems.Count > Capacity)
                {
                    undoItems.RemoveAt(0);
                }
            }

            public void Undo()
            {
                if(undoItems.Count == 0) return;

                var undoItem = undoItems[undoItems.Count - 1];
                undoItems.RemoveAt(undoItems.Count - 1);

                undoItem.DoUndo();

                redoItems.Add(undoItem);
            }

            public void Redo()
            {
                if(redoItems.Count == 0) return;

                var redoItem = redoItems[redoItems.Count - 1];
                redoItems.RemoveAt(redoItems.Count - 1);

                redoItem.DoRedo();

                undoItems.Add(redoItem);
            }
        }

        public int UndoCapacity
        {
            get { return UndoStack.Capacity; }
            set
            {
                UndoStack.Capacity = value;
                PlayerPrefs.SetInt("Haxxis.UndoCapacity", value);
            }
        }

        private int UndoTimer { get; set; }

        private UndoStackClass m_undoStack = null;

        private UndoStackClass UndoStack
        {
            get
            {
                return m_undoStack ?? (m_undoStack = new UndoStackClass(PlayerPrefs.GetInt("Haxxis.UndoCapacity", 100)));
            }
        }

        private static UndoLord m_instance = null;

        public static UndoLord Instance
        {
            get { return m_instance ?? (m_instance = FindObjectOfType<UndoLord>()); }
        }

        public void DropStack()
        {
            UndoStack.DropStack();
        }

        public void Undo()
        {
            if(UndoTimer > 0) return;
            UndoStack.Undo();
            UndoTimer = 4;
        }

        public void Redo()
        {
            if(UndoTimer > 0) return;
            UndoStack.Redo();
            UndoTimer = 4;
        }

        public void Push(UndoItem item)
        {
            UndoStack.Push(item);
        }

        void Update()
        {
            if(UndoTimer > 0)
                UndoTimer--;
        }
    }

}