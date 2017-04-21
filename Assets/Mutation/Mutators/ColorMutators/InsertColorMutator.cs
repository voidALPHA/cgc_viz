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
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.ColorMutators
{
    public class InsertColorMutator : DataMutator
    {
        private MutableField<Color> m_InsertColor = new MutableField<Color>() 
        { LiteralValue = Color.magenta };
        [Controllable(LabelText = "Color To Insert")]
        public MutableField<Color> InsertColor { get { return m_InsertColor; } }

        private MutableTarget m_ColorTarget = new MutableTarget() 
        { AbsoluteKey = "Entries.NewColor" };
        [Controllable(LabelText = "Color Target Field")]
        public MutableTarget ColorTarget { get { return m_ColorTarget; } }

        public void Start()
        {
            ColorTarget.SchemaParent = InsertColor;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            var entries = InsertColor.GetEntries(mutable);

            foreach (var entry in entries)
                ColorTarget.SetValue( InsertColor.GetValue( entry ), entry );

            return mutable;
        }

    }
}
