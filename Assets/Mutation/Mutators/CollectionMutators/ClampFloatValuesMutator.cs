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

namespace Mutation.Mutators.CollectionMutators
{
    public class ClampFloatValuesMutator : DataMutator
    {
        private MutableField<float> m_ClampedValue = new MutableField<float>() 
        { AbsoluteKey = "Entries.Value" };
        [Controllable(LabelText = "Field to Clamp")]
        public MutableField<float> ClampedValue { get { return m_ClampedValue; } }

        private MutableField<float> m_MinValue = new MutableField<float>() 
        { LiteralValue = 0.0f };
        [Controllable(LabelText = "Min Value")]
        public MutableField<float> MinValue { get { return m_MinValue; } }

        private MutableField<float> m_MaxValue = new MutableField<float>() 
        { LiteralValue = float.MaxValue };
        [Controllable(LabelText = "Max Value")]
        public MutableField<float> MaxValue { get { return m_MaxValue; } }

        private MutableTarget m_TargetField = new MutableTarget() 
        { AbsoluteKey = "Entries.NewValue" };
        [Controllable(LabelText = "New Clamped Field")]
        public MutableTarget TargetField { get { return m_TargetField; } }

        public ClampFloatValuesMutator()
        {
            TargetField.SchemaParent = ClampedValue;
            MinValue.SchemaPattern = ClampedValue;
            MaxValue.SchemaPattern = ClampedValue;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            var entries = ClampedValue.GetEntries( mutable);

            foreach (var entry in entries)
                TargetField.SetValue(Mathf.Clamp(ClampedValue.GetValue(entry),
                    MinValue.GetValue(entry), MaxValue.GetValue(entry)),
                    entry);

            return mutable;
        }
    }
}
