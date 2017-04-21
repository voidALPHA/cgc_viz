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

using System.Collections.Generic;
using Chains;
using Mutation;
using Newtonsoft.Json;
using UnityEngine;
using Utility;
using Visualizers;
using Visualizers.MetaSelectors;

namespace LabelSystem
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class LabelSystem
    {
        private MutableField<bool> m_ShowChartTitle = new MutableField<bool>() { LiteralValue = false };
        [Controllable(LabelText = "Show Chart Title")]
        public MutableField<bool> ShowChartTitle { get { return m_ShowChartTitle; } }

        private MutableField<string> m_ChartTitleText = new MutableField<string> { LiteralValue = "" };
        [Controllable(LabelText = "Chart Title Text")]
        public MutableField<string> ChartTitleText { get { return m_ChartTitleText; } }

        private MutableField<Vector3> m_ChartTitleOffset = new MutableField<Vector3> { LiteralValue = Vector3.zero };
        [Controllable(LabelText = "Chart Title Offset")]
        public MutableField<Vector3> ChartTitleOffset { get { return m_ChartTitleOffset; } }

        private MutableField<LabelOrientation> m_ChartTitleOrientation = new MutableField<LabelOrientation> { LiteralValue = LabelOrientation.Out };
        [Controllable(LabelText = "Chart Title Orientation")]
        public MutableField<LabelOrientation> ChartTitleOrientation { get { return m_ChartTitleOrientation; } }

        [JsonProperty]
        public ChainNode ChainNode { get; set; }

        protected Transform LabelRoot { get; set; }

        private List<LabelBehaviour> m_LabelViews = new List<LabelBehaviour>();
        private List<LabelBehaviour> LabelViews { get { return m_LabelViews; } }

        public abstract string LabelSystemUiHeader { get; }

        public abstract void Render(VisualPayload payload, Transform labelRoot, IMetaSelectable selectable);

        public void Initialize()    // Not sure this is needed because on eval, the entire root object is destroyed.  However if we ever do re-running on a node...
        {
            foreach (var label in LabelViews)
                Object.Destroy(label.gameObject);

            LabelViews.Clear();
        }

        protected void RenderChartTitle(VisualPayload payload)
        {
            if (ShowChartTitle.GetLastKeyValue( payload.Data ))
            {
                var labelComponent = CreateLabel(ChartTitleText.GetLastKeyValue(payload.Data), ChartTitleOffset.GetLastKeyValue(payload.Data), ChartTitleOrientation.GetLastKeyValue(payload.Data));

                var targetSize = GetDesiredLabelLength(labelComponent);

                SetLabelLength(labelComponent, targetSize);
            }
        }

        public LabelBehaviour CreateLabel(string text, Vector3 pos, LabelOrientation labelOrientation,
            LabelBehaviour labelComponent = null)
        {
            if (labelComponent == null)
            {
                var labelGo = LabelSystemFactory.InstantiateLabel();
                labelComponent = labelGo.GetComponent<LabelBehaviour>();
            }

            labelComponent.Text = text;

            labelComponent.Orientation = labelOrientation;

            labelComponent.transform.SetParent(LabelRoot);
            labelComponent.transform.localPosition = pos;
            labelComponent.transform.localRotation = Quaternion.identity;

            LabelViews.Add(labelComponent);

            return labelComponent;
        }

        public float GetDesiredLabelLength(LabelBehaviour labelComponent)
        {
            var renderer = labelComponent.TextComponent.GetComponent<Renderer>();
            var size = renderer.GetLocallyAlignedBounds();

            const float padding = 0.30f;
            var totalSize = ( size.x + ( padding * 2.0f ) );

            return totalSize;
        }

        public void SetLabelLength(LabelBehaviour labelComponent, float length)
        {
            var geomRenderer = labelComponent.Geometry.GetComponentInChildren<Renderer>();
            var sizeGeom = geomRenderer.GetLocallyAlignedBounds();

            var newScale = labelComponent.Geometry.localScale;
            newScale.x = length / sizeGeom.x;

            labelComponent.Geometry.localScale = newScale;
        }

        public virtual void SetInteractivity(IMetaSelectable selectable)
        {
        }
    }
}
