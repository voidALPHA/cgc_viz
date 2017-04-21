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
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.ColorMutators
{
    public class ColorFromSetByMutableIndex : DataMutator
    {

        private MutableField<int> m_ColorIndex = new MutableField<int>() 
        { LiteralValue = 0 };
        [Controllable(LabelText = "ColorIndex")]
        public MutableField<int> ColorIndex { get { return m_ColorIndex; } }

        private MutableTarget m_ColorTarget = new MutableTarget() { AbsoluteKey = "NewColor" };
        [Controllable(LabelText = "New Color Field")]
        public MutableTarget ColorTarget { get { return m_ColorTarget; } }

        public void Start()
        {
            ColorTarget.SchemaParent = ColorIndex;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            foreach (var entry in ColorIndex.GetEntries( mutable ))
            ColorTarget.SetValue( 
                ColorPalette.ColorFromIndex(ColorIndex.GetValue(entry)),
                entry);

            return mutable;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            foreach (var entry in ColorIndex.GetEntries(newSchema))
                ColorTarget.SetValue(
                    Color.magenta,
                    entry);

            Router.TransmitAllSchema(newSchema);
        }
    }
}
