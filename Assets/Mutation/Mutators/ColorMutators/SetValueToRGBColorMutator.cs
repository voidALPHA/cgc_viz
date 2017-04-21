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
using Mutation.Mutators.Axes.ArrityTypeAxes;
using UnityEngine;
using Visualizers;
using ColorUtility = Assets.Utility.ColorUtility;

namespace Mutation.Mutators.ColorMutators
{
    public class SetValueToRGBColorMutator : TypeConversionAxis<Color, Color>
    {
        private MutableField<float> m_NewValue = new MutableField<float>() 
        { LiteralValue = 1.0f };
        [Controllable(LabelText = "New Value")]
        public MutableField<float> NewValue { get { return m_NewValue; } }
        

        protected override Color ConversionFunc( Color key, List<MutableObject> entry )
        {
            var foundValue = NewValue.GetValue( entry );

            float colorH, colorS, colorV;
            ColorUtility.RGBToHSV( key, out colorH, out colorS, out colorV );

            return ColorUtility.HsvtoRgb( colorH, colorS, foundValue );
        }
    }
}
