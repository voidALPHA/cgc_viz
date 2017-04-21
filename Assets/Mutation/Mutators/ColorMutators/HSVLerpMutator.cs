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

using Assets.Utility;
using UnityEngine;
using Visualizers;
using ColorUtility = Assets.Utility.ColorUtility;

namespace Mutation.Mutators.ColorMutators
{
    public class HSVLerpMutator : ColorLerpMutator
    {
        private MutableField< bool > m_ApplyHLerpField = new MutableField<bool> { LiteralValue = false };
        [Controllable(LabelText = "Lerp H Value")]
        private MutableField< bool > ApplyHLerpField { get { return m_ApplyHLerpField; } }

        private MutableField< bool > m_ApplySLerpField = new MutableField<bool> { LiteralValue = false };
        [Controllable(LabelText = "Lerp S Value")]
        private MutableField< bool > ApplySLerpField { get { return m_ApplySLerpField; } }

        private MutableField< bool > m_ApplyVLerpField = new MutableField<bool> { LiteralValue = false };
        [Controllable(LabelText = "Lerp V Value")]
        private MutableField< bool > ApplyVLerpField { get { return m_ApplyVLerpField; } }


        protected override Color ColorLerp(MutableObject mutable, Color a, Color b, float proportion)
        {
            float H1, S1, V1;
            float H2, S2, V2;

            ColorUtility.RGBToHSV(a, out H1, out S1, out V1);
            ColorUtility.RGBToHSV(b, out H2, out S2, out V2);

            float endH = ApplyHLerpField.GetFirstValue( mutable ) ? H1 + (H2 - H1)*proportion : H2;
            float endS = ApplySLerpField.GetFirstValue( mutable ) ? S1 + ( S2 - S1 ) * proportion : S2;
            float endV = ApplyVLerpField.GetFirstValue( mutable ) ? V1 + ( V2 - V1 ) * proportion : V2;

            Color endColor = ColorUtility.HsvtoRgb(endH, endS, endV);

            endColor.a = DoApplyAlphaField.GetFirstValue( mutable )?a.a+(b.a-a.a)*proportion:b.a;

            return endColor;
        }
    }
}
