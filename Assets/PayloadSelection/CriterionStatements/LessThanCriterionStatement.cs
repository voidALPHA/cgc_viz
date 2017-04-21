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

namespace PayloadSelection.CriterionStatements
{
    public abstract class LessThanCriterionStatement<T> : BinaryPredicateStatement<T> where T : IComparable
    {
        protected override string Symbol { get { return "<"; } }

        protected override bool CompareValues(T value1, T value2)
        {
            return value1.CompareTo(value2) < 0;
        }
    }

    public class FloatLessThanCriterionStatement : GreaterThanCriterionStatement<float>
    {
        public override string Name
        {
            get { return "Float Less Than"; }
        }
    }

    public class IntLessThanCriterionStatement : GreaterThanCriterionStatement<int>
    {
        public override string Name
        {
            get { return "Int Less Than"; }
        }
    }
}