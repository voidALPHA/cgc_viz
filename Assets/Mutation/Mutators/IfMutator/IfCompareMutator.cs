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
using JetBrains.Annotations;
using Visualizers;

namespace Mutation.Mutators.IfMutator
{
    [UsedImplicitly]
    public class IfCompareMutator : IfMutator
    {
        #region Comparison Guts

        // This whole section can be turned into a common, static class...
        
        public enum ComparisonType
        {
            // Firsts
            Equal,
            NotEqual,
            GreaterThan,
            LessThan,
            GreaterThanEqual,
            LessThanEqual,
        }


        private static readonly Dictionary<ComparisonType, Func<IComparable, IComparable, bool>> m_ComparisonPredicates = new Dictionary<ComparisonType, Func<IComparable, IComparable, bool>>
        {
            { ComparisonType.Equal, (a, b) => a.CompareTo( b ) == 0 },
            { ComparisonType.NotEqual, (a, b) => a.CompareTo( b ) != 0 },
            { ComparisonType.GreaterThan, (a, b) => a.CompareTo( b ) > 0 },
            { ComparisonType.LessThan, (a, b) => a.CompareTo( b ) < 0 },
            { ComparisonType.GreaterThanEqual, (a, b) => a.CompareTo( b ) >= 0 },
            { ComparisonType.LessThanEqual, (a, b) => a.CompareTo( b ) <= 0 },
        };
        public static Dictionary<ComparisonType, Func<IComparable, IComparable, bool>> ComparisonPredicates { get { return m_ComparisonPredicates; } }

        #endregion


        private MutableField<IComparable> m_FirstOperand = new MutableField<IComparable>() { AbsoluteKey = "Comparison String" };
        [Controllable( LabelText = "First Operand" )]
        private MutableField<IComparable> FirstOperand { get { return m_FirstOperand; } }

        private MutableField<ComparisonType> m_ComparisonTypeField = new MutableField<ComparisonType> { LiteralValue = ComparisonType.Equal };
        [Controllable( LabelText = "Comparison Type" )]
        private MutableField<ComparisonType> ComparisonTypeField { get { return m_ComparisonTypeField; } }

        private MutableField<IComparable> m_SecondOperand = new MutableField<IComparable>() { AbsoluteKey = "Comparison String" };
        [Controllable( LabelText = "Second Operand" )]
        private MutableField<IComparable> SecondOperand { get { return m_SecondOperand; } }

        protected override bool MeetsCriterion( VisualPayload payload )
        {
            var comparisonType = ComparisonTypeField.GetFirstValue( payload.Data );

            var firstOperand = FirstOperand.GetFirstValue( payload.Data );
            var secondOperand = SecondOperand.GetFirstValue( payload.Data );

            return ComparisonPredicates[ comparisonType ]( firstOperand, secondOperand );
        }
    }
}