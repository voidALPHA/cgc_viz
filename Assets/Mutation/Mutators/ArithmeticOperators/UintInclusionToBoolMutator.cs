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
using Mutation.Mutators.Axes.ArrityTypeAxes;
using Mutation.Mutators.Regrouping;
using Visualizers;

namespace Mutation.Mutators.ArithmeticOperators
{
    public class UintInclusionToBoolMutator : TypeConversionAxis<uint, bool>
    {
        private MutableField<string> m_InclusionString = new MutableField<string>() { LiteralValue = "All" };
        [Controllable(LabelText = "InclusionString")]
        public MutableField<string> InclusionString { get { return m_InclusionString; } }

        protected override bool ConversionFunc(uint key, List<MutableObject> entry)
        {
            var inclusionString = InclusionString.GetValue(entry);

            return RegroupByUintInclusionMutator.InclusionFunc(inclusionString)(key);
        }
    }
}
