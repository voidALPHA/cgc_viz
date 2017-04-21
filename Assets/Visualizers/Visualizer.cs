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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mutation;
using UnityEngine;
using Visualizers.IsoGrid;

namespace Visualizers
{
    public abstract class Visualizer : MonoBehaviour
    {

        private bool m_Visible = true;
        public bool Visible
        {
            get { return m_Visible; }
            set
            {
                if (value == m_Visible)
                    return;
                m_Visible = value;
                SetVisible(m_Visible);
            }
        }

        protected virtual void SetVisible(bool state)
        {
            foreach (var rend in GetComponentsInChildren<Renderer>())
            {
                rend.enabled = state;
            }
        }

        public void Initialize(VisualizerController controller, VisualPayload payload)
        {
            payload.VisualData.Bound.ChildVisualizer(controller, this);

            //Bound.Data = payload.Data;  // discriminate this bound

            Payload = payload;
        }

        public VisualPayload Payload { get; set; }

        private BoundingBox m_Bound;
        public BoundingBox Bound
        {
            get
            {
                return m_Bound ??
                       ( m_Bound = gameObject.GetComponent< BoundingBox >() ?? gameObject.AddComponent< BoundingBox >() );
            }
            protected set { m_Bound = value; }
        }

        public virtual void DestroyVisualizer()
        {
            DestroyImmediate(this.gameObject);
        }

        public void OnDestroy()
        {
            OnVisualizerDestroyed();
        }

        public virtual event Action OnVisualizerDestroyed = delegate { };
    }
}
