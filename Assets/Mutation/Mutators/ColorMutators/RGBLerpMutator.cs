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
    public class RGBLerpMutator : ColorLerpMutator
    {
        private MutableField< bool > m_DoApplyColorField = new MutableField< bool > {LiteralValue = true};
        [Controllable( LabelText = "Lerp RGB Value" )]
        private MutableField< bool > DoApplyColorField
        {
            get { return m_DoApplyColorField; }
        }

        private MutableField<bool> m_UsePreciseLerp = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "Use Precise Color")]
        public MutableField<bool> UsePreciseLerp { get { return m_UsePreciseLerp; } }


        protected override Color ColorLerp(MutableObject mutable, Color a, Color b, float proportion)
        {
            var endColor = b;

            if ( DoApplyColorField.GetFirstValue( mutable ) )
                endColor = 
                    UsePreciseLerp.GetFirstValue( mutable )?
                    ColorUtility.SqrLerpColors( a,b,proportion ):
                    a + (b - a)*proportion;

            endColor.a = DoApplyAlphaField.GetFirstValue( mutable ) ? a.a + (b.a - a.a) * proportion : b.a;

            return endColor;
        }

    }
}
