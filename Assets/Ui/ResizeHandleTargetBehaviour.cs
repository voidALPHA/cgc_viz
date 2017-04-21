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

using JetBrains.Annotations;
using UnityEngine;

namespace Ui
{
    [UsedImplicitly]
    public class ResizeHandleTargetBehaviour : MonoBehaviour, IResizeHandleTarget
    {
        // A standalone component with a default and inspectable implementation of IResizeHandleTarget

        public void OnPartialResize()
        {
        }

        public void OnFinalResize()
        {
        }

        [SerializeField]
        private bool m_SuppressResize = false;
        public bool SuppressResize { get { return m_SuppressResize; } }

        [SerializeField]
        private Vector2 m_MinResizeSize = Vector2.zero;
        public Vector2 MinResizeSize { get { return m_MinResizeSize; } }

        [SerializeField]
        private Vector2 m_MaxResizeSize = Vector2.zero;
        public Vector2 MaxResizeSize { get { return m_MaxResizeSize; } }
    }
}
