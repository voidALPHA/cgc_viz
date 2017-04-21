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

namespace Mutation.Mutators.SpatialMutators
{
    public class PositionByIndexAroundXYCircle : DataMutator
    {
        
        private MutableField<int> m_CircleIndex = new MutableField<int>() 
        { LiteralValue = 0 };
        [Controllable(LabelText = "Index around Circle")]
        public MutableField<int> CircleIndex { get { return m_CircleIndex; } }

        private MutableTarget m_XAxis = new MutableTarget() 
        { AbsoluteKey = "X Axis" };
        [Controllable(LabelText = "X Axis Target")]
        public MutableTarget XAxis { get { return m_XAxis; } }

        private MutableTarget m_YAxis = new MutableTarget() 
        { AbsoluteKey = "Y Axis" };
        [Controllable(LabelText = "Y Axis Target")]
        public MutableTarget YAxis { get { return m_YAxis; } }

        public PositionByIndexAroundXYCircle()
            : base()
        {
            XAxis.SchemaParent = CircleIndex;
            YAxis.SchemaParent = CircleIndex;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            var max = int.MinValue;
            var min = int.MaxValue;

            foreach ( var entry in CircleIndex.GetEntries( mutable ) )
            {
                var foundValue = CircleIndex.GetValue( entry );

                if ( foundValue > max )
                    max = foundValue;

                if ( foundValue < min )
                    min = foundValue;
            }

            var inverseRange = 1f / ( ( max - min + 1 ) );

            foreach ( var entry in CircleIndex.GetEntries( mutable ) )
            {
                var proportion = ((CircleIndex.GetValue(entry) - min) + 1) * inverseRange;

                XAxis.SetValue(Mathf.Sin(proportion * Mathf.PI * 2f), entry);
                YAxis.SetValue(Mathf.Cos(proportion * Mathf.PI * 2f), entry);
            }

            return mutable;
        }

    }
}
