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
using UnityEngine;

namespace PayloadSelection.CriterionStatements
{
    public abstract class EqualToCriterionStatement<T> : BinaryPredicateStatement<T> where T : IComparable
    {
        protected override string Symbol { get { return "=="; } }
    }

    public class FloatEqualToCriterionStatement : EqualToCriterionStatement< float >
    {
        private const float Epsilon = .0001f;

        public override string Name
        {
            get { return "Float (approximately) Equal To"; }
        }

        protected override bool CompareValues(float value1, float value2)
        {
            return Mathf.Abs(value1 - value2) < Epsilon;
        }
    }

    public class IntEqualToCriterionStatement : EqualToCriterionStatement<int>
    {
        public override string Name
        {
            get { return "Int Equal To"; }
        }

        protected override bool CompareValues(int value1, int value2)
        {
            return value1 - value2==0;
        }
    }
}
