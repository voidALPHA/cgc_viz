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
using Mutation.Mutators;
using Newtonsoft.Json;
using UnityEngine;
using Utility;
using Visualizers;

namespace Mutation.Mutators.ColorMutators
{
    public abstract class ColorLerpMutator : DataMutator
    {
        private MutableScope m_ColorScope = new MutableScope();
        [Controllable(LabelText = "Scope", Order = -2)]
        public MutableScope ColorScope { get { return m_ColorScope; } }

        private MutableField<Color> m_FromColor = new MutableField<Color>() 
        { LiteralValue = Color.red };
        [Controllable(LabelText = "From Color")]
        public MutableField<Color> FromColor { get { return m_FromColor; } }

        private MutableField<Color> m_ToColor = new MutableField<Color>() 
        { LiteralValue = Color.green };
        [Controllable(LabelText = "To Color (Target)")]
        public MutableField<Color> ToColor { get { return m_ToColor; } }

        private MutableField<float> m_Proportion = new MutableField<float>() 
        { LiteralValue = .5f };
        [Controllable(LabelText = "Lerp Proportion")]
        public MutableField<float> Proportion { get { return m_Proportion; } }

        private MutableField< bool > m_DoApplyAlphaField = new MutableField<bool> { LiteralValue = false };
        [Controllable(LabelText = "Apply Alpha")]
        protected MutableField< bool > DoApplyAlphaField { get { return m_DoApplyAlphaField; } }

        private MutableTarget m_ColorTarget = new MutableTarget() { AbsoluteKey = "NewColor" };
        [Controllable(LabelText = "New Color Field")]
        public MutableTarget ColorTarget { get { return m_ColorTarget; } }


        protected ColorLerpMutator()
        {
            FromColor.SchemaPattern = ColorScope;
            ToColor.SchemaPattern = ColorScope;
            Proportion.SchemaPattern = ColorScope;
            ColorTarget.SchemaParent = ColorScope;
        }

        protected abstract Color ColorLerp(MutableObject mutable, Color a, Color b, float proportion);

        protected override MutableObject Mutate(MutableObject mutable)
        {
            foreach (var entry in ColorScope.GetEntries(mutable))
                ColorTarget.SetValue( ColorLerp( mutable, FromColor.GetValue(entry), ToColor.GetValue(entry),
                    Proportion.GetValue(entry)),entry);

            return mutable;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            foreach (var entry in ColorScope.GetEntries(newSchema))
                ColorTarget.SetValue( Color.magenta, entry );

            Router.TransmitAllSchema( newSchema );
        }
    }
}
