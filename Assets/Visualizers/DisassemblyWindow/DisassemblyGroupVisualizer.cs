// Copyright 2017 voidALPHA, Inc.
// This file is part of the Haxxis video generation system and is provided
// by voidALPHA in support of the Cyber Grand Challenge.
// Haxxis is free software: you can redistribute it and/or modify it under the terms
// of the GNU General Public License as published by the Free Software Foundation.
// Haxxis is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
// PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with
// Haxxis. If not, see <http://www.gnu.org/licenses/>.

using UnityEngine;
using UnityEngine.UI;

namespace Visualizers.DisassemblyWindow
{
    public class DisassemblyGroupVisualizer : MonoBehaviour
    {
        [Header("Component References")]

        [SerializeField]
        private RectTransform m_ChildAttachmentTransform = null;
        public RectTransform ChildAttachmentTransform { get { return m_ChildAttachmentTransform; } }


        private bool m_Enlarge = false;
        public bool Enlarge
        {
            private get { return m_Enlarge; }
            set
            {
                // One-time setter, cannot revert...

                m_Enlarge = value;

                if ( value )
                    Embiggen();
            }
        }

        private void Embiggen()
        {
            ChildAttachmentTransform.anchorMin = Vector2.zero;
            ChildAttachmentTransform.anchorMax = Vector2.one;

            ChildAttachmentTransform.offsetMin = Vector2.zero;
            ChildAttachmentTransform.offsetMax = Vector2.zero;

            ChildAttachmentTransform.GetComponent< HorizontalLayoutGroup >().childForceExpandWidth = true;

            foreach ( Transform child in ChildAttachmentTransform )
            {
                child.GetComponent< DisassemblyWindowVisualizer >().Embiggen();
            }
        }

        public void Destroy()
        {
            Destroy( gameObject );
        }

        public void Attach( DisassemblyWindowVisualizer windowVisualizer )
        {
            windowVisualizer.transform.SetParent( ChildAttachmentTransform, false );

            if ( Enlarge )
                windowVisualizer.Embiggen();
        }
    }
}
