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
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators
{
    public class MaxValueMutator : DataMutator
    {
        private MutableField<float> m_FieldToCompare = new MutableField<float> { AbsoluteKey = "Entries.Score" };
        [Controllable(LabelText = "Property to compare")]
        public MutableField<float> FieldToCompare
        {
            get { return m_FieldToCompare; }
            set { m_FieldToCompare = value; }
        }

        private MutableTarget m_MaxValue = new MutableTarget() 
        { AbsoluteKey = "Max Value" };
        [Controllable(LabelText = "Max Value Target")]
        public MutableTarget MaxValue { get { return m_MaxValue; } }

        public MaxValueMutator() : base()
        {
            MaxValue.SchemaParent = FieldToCompare;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            var maximumValue = float.MinValue;

            foreach (var entry in FieldToCompare.GetEntries(mutable))
            {
                //if (!FieldToCompare.Resolvable(entry))
                //    throw new Exception("Entry not found");

                if (FieldToCompare.GetValue(entry).CompareTo(maximumValue) > 0)
                    maximumValue = FieldToCompare.GetValue(entry);
            }

            foreach ( var entry in FieldToCompare.GetEntries(mutable) )
            {
                MaxValue.SetValue( maximumValue, entry );
            }
            
            return mutable;
        }
    }
}
