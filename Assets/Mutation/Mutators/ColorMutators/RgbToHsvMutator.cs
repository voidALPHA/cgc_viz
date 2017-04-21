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
using Visualizers;
using ColorUtility = Assets.Utility.ColorUtility;

namespace Mutation.Mutators.ColorMutators
{
    public class ColorToHsvMutator : DataMutator
    {
        private MutableField<Color> m_RGBColor = new MutableField<Color>() 
        { AbsoluteKey= "Color" };
        [Controllable(LabelText = "Color")]
        public MutableField<Color> RGBColor { get { return m_RGBColor; } }

        private MutableTarget m_HTarget = new MutableTarget() 
        { AbsoluteKey = "Color.H" };
        [Controllable(LabelText = "H Target")]
        public MutableTarget HTarget { get { return m_HTarget; } }

        private MutableTarget m_STarget = new MutableTarget() 
        { AbsoluteKey = "Color.S" };
        [Controllable(LabelText = "S Target")]
        public MutableTarget STarget { get { return m_STarget; } }
        
        private MutableTarget m_VTarget = new MutableTarget() 
        { AbsoluteKey = "Color.V" };
        [Controllable(LabelText = "V Target")]
        public MutableTarget VTarget { get { return m_VTarget; } }

        public ColorToHsvMutator()
        {
            HTarget.SchemaParent = RGBColor;
            STarget.SchemaParent = RGBColor;
            VTarget.SchemaParent = RGBColor;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in RGBColor.GetEntries( mutable ) )
            {
                float outH, outS, outV;
                ColorUtility.RGBToHSV( RGBColor.GetValue( entry ), out outH, out outS, out outV );

                HTarget.SetValue( outH, entry );
                STarget.SetValue(outS, entry);
                VTarget.SetValue(outV, entry);
            }

            return mutable;
        }
    }

    public class HsvToColorMutator : DataMutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<float> m_Hue = new MutableField<float>() 
        { LiteralValue = .5f };
        [Controllable(LabelText = "Hue")]
        public MutableField<float> Hue { get { return m_Hue; } }

        private MutableField<float> m_Saturation = new MutableField<float>() 
        { LiteralValue = .5f };
        [Controllable(LabelText = "Saturation")]
        public MutableField<float> Saturation { get { return m_Saturation; } }

        private MutableField<float> m_Value = new MutableField<float>() 
        { LiteralValue = .5f };
        [Controllable(LabelText = "Value")]
        public MutableField<float> Value { get { return m_Value; } }

        private MutableTarget m_ColorTarget = new MutableTarget() 
        { AbsoluteKey = "Color" };
        [Controllable(LabelText = "ColorTarget")]
        public MutableTarget ColorTarget { get { return m_ColorTarget; } }

        public HsvToColorMutator()
        {
            Hue.SchemaParent = Scope;
            Saturation.SchemaParent = Scope;
            Value.SchemaParent = Scope;
            ColorTarget.SchemaParent = Scope;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {
                var hue = Hue.GetValue( entry );
                var saturation = Saturation.GetValue( entry );
                var value = Value.GetValue( entry );

                ColorTarget.SetValue( ColorUtility.HsvtoRgb( hue, saturation, value ), entry );
            }
            return mutable;
        }
    }
}
