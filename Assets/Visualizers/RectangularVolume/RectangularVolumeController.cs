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

using Chains;
using Mutation;
using System.Collections;
using UnityEngine;
using Visualizers.IsoGrid;

namespace Visualizers.RectangularVolume
{
    public class RectangularVolumeController : VisualizerController
    {
        //private RectangularVolumeVisualizer Volume { get; set; }

        private MutableField<float> m_XAxis = new MutableField<float> { LiteralValue = 1f };
        [Controllable(LabelText = "X Axis Value")]
        public MutableField<float> XAxis { get { return m_XAxis; } set { m_XAxis = value; } }

        private MutableField<float> m_XMax = new MutableField<float> { LiteralValue = 1f};
        [Controllable(LabelText = "X Axis Maximum")]
        public MutableField<float> XMax { get { return m_XMax; } set { m_XAxis = value; } }


        private MutableField<float> m_YAxis = new MutableField<float> { LiteralValue = 1f };
        [Controllable(LabelText="Y Axis Value")]
        public MutableField<float> YAxis { get { return m_YAxis; } set { m_YAxis = value; } }

        private MutableField<float> m_YMax = new MutableField<float> { LiteralValue = 1f };
        [Controllable(LabelText = "Y Axis Maximum")]
        public MutableField<float> YMax { get { return m_YMax; } set { m_YAxis = value; } }


        private MutableField<float> m_ZAxis = new MutableField<float> { LiteralValue = 1f };
        [Controllable(LabelText = "Z Axis Value")]
        public MutableField<float> ZAxis { get { return m_ZAxis; } set { m_ZAxis = value; } }

        private MutableField<float> m_ZMax = new MutableField<float> { LiteralValue = 1f };
        [Controllable(LabelText = "Z Axis Maximum")]
        public MutableField<float> ZMax { get { return m_ZMax; } set { m_ZAxis = value; } }

        private MutableField<bool> m_UseOpaqueMaterial = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "Use Opaque Material")]
        public MutableField<bool> UseOpaqueMaterial { get { return m_UseOpaqueMaterial; } }

        private MutableField<Color> m_Color = new MutableField<Color>(){LiteralValue = UnityEngine.Color.magenta};
        [Controllable(LabelText = "Geometry Color")]
        public MutableField<Color> Color { get { return m_Color; } set { m_Color = value; } }
        
        public SelectionState DefaultState { get { return Router["Default"]; } }

        public RectangularVolumeController()
        {
            Router.AddSelectionState("Default");
        }

        protected override IEnumerator ProcessPayload(VisualPayload payload)
        {
            //payload.ClearBoundVisualizer();

            var volume = VisualizerFactory.InstantiateRectangularVolume();

            //payload.GetTargetBound().ChildWithinBound(volume.transform);
                //.BoundedVisual = volume;

            volume.Initialize(this, payload);

            volume.SetOpaqueMaterial( UseOpaqueMaterial.GetFirstValue( payload.Data ) );
                

            var xProportion = XAxis.GetFirstValue(payload.Data);
            if (XMax.CouldResolve(payload.Data) && XMax.GetFirstValue(payload.Data) > .001f)
                xProportion = xProportion / XMax.GetFirstValue(payload.Data);

            var yProportion = YAxis.GetFirstValue(payload.Data);
            if (YMax.CouldResolve(payload.Data) && YMax.GetFirstValue(payload.Data) > .001f)
                yProportion = yProportion / YMax.GetFirstValue(payload.Data);

            var zProportion = ZAxis.GetFirstValue(payload.Data);
            if (ZMax.CouldResolve(payload.Data) && ZMax.GetFirstValue(payload.Data) > .001f)
                zProportion = zProportion / ZMax.GetFirstValue(payload.Data);

            volume.Xscale = xProportion;
            volume.Yscale = yProportion;
            volume.Zscale = zProportion;

            volume.Color = Color.GetFirstValue(payload.Data);

            var newPayload = new VisualPayload( payload.Data, new VisualDescription(volume.Bound) );

            var iterator = Router.TransmitAll( newPayload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
