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
    public class AddNumericEntriesMutator : DataMutator
    {
        private MutableField<float> m_NumberToAdd = new MutableField<float>() 
        { LiteralValue = 1f };
        [Controllable(LabelText = "Number To Add")]
        public MutableField<float> NumberToAdd { get { return m_NumberToAdd; } }

        private MutableTarget m_NumericTarget = new MutableTarget() 
        { AbsoluteKey = "Number" };
        [Controllable(LabelText = "Numeric Target")]
        public MutableTarget NumericTarget { get { return m_NumericTarget; } }

        public AddNumericEntriesMutator()
        {
            NumericTarget.SchemaParent = NumberToAdd;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            var entries = NumberToAdd.GetEntries(mutable);

            foreach (var entry in entries)
            {
                NumericTarget.SetValue( NumberToAdd.GetValue( entry ), entry );
            }

            return mutable;
        }

    }
}
