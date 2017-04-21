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


using Visualizers;

namespace Mutation.Mutators
{
    public class TotalValuesMutators : DataMutator
    {
        private MutableField<float> m_ValueToSum = new MutableField<float>() 
        { LiteralValue = 1f };
        [Controllable(LabelText = "Value To Total")]
        public MutableField<float> ValueToSum { get { return m_ValueToSum; } }

        private MutableTarget m_TotalField = new MutableTarget() 
        { AbsoluteKey = "Total" };
        [Controllable(LabelText = "Total Field")]
        public MutableTarget TotalField { get { return m_TotalField; } }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            float total = 0f;

            foreach ( var entry in ValueToSum.GetEntries( mutable ) )
                total += ValueToSum.GetValue( entry );

            TotalField.SetValue( total, mutable );

            return mutable;
        }
    }
}
