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

using System.Collections;
using Mutation;
using Mutation.Mutators;
using UnityEngine;
using Utility;

namespace Visualizers.CsView.Texturing
{
    public class OpcodeToTextureBandingMutator : Mutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<OpcodeHistogram> m_OpcodeHistogram = new MutableField<OpcodeHistogram>() 
        { AbsoluteKey = "Opcode Histogram"};
        [Controllable(LabelText = "OpcodeHistogram")]
        public MutableField<OpcodeHistogram> OpcodeHistogram { get { return m_OpcodeHistogram; } }

        private MutableField<Color> m_PrimaryColor = new MutableField<Color>() 
        { LiteralValue = Color.green };
        [Controllable(LabelText = "Primary Color")]
        public MutableField<Color> PrimaryColor { get { return m_PrimaryColor; } }

        private MutableField<float> m_PrimaryImpactOnBands = new MutableField<float>() 
        { LiteralValue = .2f };
        [Controllable(LabelText = "Hue Range From Primary")]
        public MutableField<float> PrimaryImpactOnBands { get { return m_PrimaryImpactOnBands; } }

        private MutableField<int> m_NumberOfBands = new MutableField<int>() 
        { LiteralValue = 4 };
        [Controllable(LabelText = "Number Of Bands")]
        public MutableField<int> NumberOfBands { get { return m_NumberOfBands; } }

        private MutableField<float> m_BandWidth = new MutableField<float>() 
        { LiteralValue = .03f };
        [Controllable(LabelText = "Band Width")]
        public MutableField<float> BandWidth { get { return m_BandWidth; } }

        private MutableField<float> m_BandEdgeWidth = new MutableField<float>() 
        { LiteralValue = .01f };
        [Controllable(LabelText = "Band Edge Width")]
        public MutableField<float> BandEdgeWidth { get { return m_BandEdgeWidth; } }

        private MutableField<float> m_BandSaturation = new MutableField<float>() 
        { LiteralValue = 1f };
        [Controllable(LabelText = "Band Saturation")]
        public MutableField<float> BandSaturation { get { return m_BandSaturation; } }

        private MutableField<float> m_BandValue = new MutableField<float>() 
        { LiteralValue = 1f };
        [Controllable(LabelText = "Band Value")]
        public MutableField<float> BandValue { get { return m_BandValue; } }



        private MutableTarget m_BandingTarget = new MutableTarget() 
        { AbsoluteKey = "Banding Gradient" };
        [Controllable(LabelText = "Banding Target")]
        public MutableTarget BandingTarget { get { return m_BandingTarget; } }

        public OpcodeToTextureBandingMutator()
        {
            OpcodeHistogram.SchemaParent = Scope;
            PrimaryColor.SchemaParent = Scope;
            PrimaryImpactOnBands.SchemaParent = Scope;
            NumberOfBands.SchemaParent = Scope;
            BandWidth.SchemaParent = Scope;
            BandEdgeWidth.SchemaParent = Scope;
            BandSaturation.SchemaParent = Scope;
            BandValue.SchemaParent = Scope;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            BandingTarget.SetValue( new ColorGradient(4), newSchema);

            Router.TransmitAllSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach ( var entry in Scope.GetEntries( payload.Data ) )
            {
                var generatedGradient = OpcodeToTextureBanding.GenerateTextureBanding(
                    OpcodeHistogram.GetValue( entry ),
                    PrimaryColor.GetValue( entry ),
                    PrimaryImpactOnBands.GetValue( entry ),
                    NumberOfBands.GetValue( entry ),
                    BandWidth.GetValue( entry ),
                    BandSaturation.GetValue( entry ),
                    BandValue.GetValue( entry ),
                    BandEdgeWidth.GetValue( entry ) );

                BandingTarget.SetValue( generatedGradient, entry );
            }

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
