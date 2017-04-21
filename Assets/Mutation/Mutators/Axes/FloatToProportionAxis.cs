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

namespace Mutation.Mutators.Axes
{
    public class FloatToProportionAxis : Axis<float, float>{

        private MutableField<float> m_ValueField = new MutableField<float>()
        {AbsoluteKey = "Value"};
        [Controllable(LabelText = "Comparable Value")]
        public MutableField<float> ValueField
        {
            get { return m_ValueField; }
        }

        private MutableTarget m_Proportion = new MutableTarget() 
        { AbsoluteKey = "Proportion" };
        [Controllable(LabelText = "Proportion Target")]
        public MutableTarget Proportion { get { return m_Proportion; } }
        

        public FloatToProportionAxis() : base()
        {
            Proportion.SchemaParent = ValueField;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            var entries = ValueField.GetEntries(mutable);

            if ( entries.Count() < 1 )
                return mutable;

            float maxValue = ValueField.GetValue(entries.First());
            float minValue = maxValue;

            foreach (var entry in ValueField.GetEntries(mutable))
            {
                float foundValue = ValueField.GetValue(entry);

                if (maxValue < foundValue)
                    maxValue = foundValue;
                if (minValue > foundValue)
                    minValue = foundValue;
            }

            float range = Mathf.Abs(maxValue - minValue) < .00001f ? 1f : maxValue - minValue;

            foreach (var entry in ValueField.GetEntries(mutable))
            {
                float foundValue = ValueField.GetValue(entry);

                Proportion.SetValue((foundValue-minValue)/range,entry);
            }

            return mutable;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            foreach (var entry in ValueField.GetEntries(newSchema))
                Proportion.SetValue( .5f, entry );

            Router.TransmitAllSchema(newSchema);
        }
    }
}
