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

using System;
using UnityEngine;

namespace Visualizers.AnnotationIcon
{
    public abstract class AnnotationIcon : Visualizer
    {
        public String AnnotationType { get; set; }

        public virtual String AnnotationText { get; set; }

        [SerializeField]
        private Transform m_RecolorParent = null;
        private Transform RecolorParent { get { return m_RecolorParent; } }

        // TODO: Material leak/waste?
        public Color Color
        {
            set
            {
                foreach (var childRenderer in (RecolorParent==null?
                GetComponentsInChildren< Renderer >():
                RecolorParent.GetComponentsInChildren<Renderer>()))
                    childRenderer.material.color = value;
            }
        }
    }
}
