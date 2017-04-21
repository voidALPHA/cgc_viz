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
using Mutation.Mutators.IfMutator;
using Visualizers;

namespace Mutation.Mutators.ArithmeticOperators
{
    // do not use literals!  because JSON can't know the difference between an int and a long

    public class ComparisonToBoolMutator : DataMutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        // Note that neither operand can be a literal, because the code would not know what type it is (e.g. int vs. string)

        private MutableField<IComparable> m_FirstOperand = new MutableField<IComparable>() 
        { AbsoluteKey= "Total Score" };
        [Controllable(LabelText = "First Operand")]
        public MutableField<IComparable> FirstOperand { get { return m_FirstOperand; } }

        private MutableField<IfCompareMutator.ComparisonType> m_Comparison = new MutableField<IfCompareMutator.ComparisonType>() { LiteralValue = IfCompareMutator.ComparisonType.Equal };
        [Controllable(LabelText = "Comparison")]
        public MutableField<IfCompareMutator.ComparisonType> Comparison { get { return m_Comparison; } }

        private MutableField<IComparable> m_SecondOperand = new MutableField<IComparable>() 
        { AbsoluteKey= "Other Score" };
        [Controllable(LabelText = "Second Operand")]
        public MutableField<IComparable> SecondOperand { get { return m_SecondOperand; } }

        private MutableTarget m_ResultTarget = new MutableTarget() 
        { AbsoluteKey = "Values Equal" };
        [Controllable(LabelText = "Result Target")]
        public MutableTarget ResultTarget { get { return m_ResultTarget; } }

        public ComparisonToBoolMutator()
        {
            FirstOperand.SchemaParent = Scope;
            SecondOperand.SchemaParent = Scope;
            Comparison.SchemaPattern = Scope;
            ResultTarget.SchemaParent = Scope;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {
                var comparison = Comparison.GetValue( entry );

                var firstOperand = FirstOperand.GetValue( entry );
                var secondOperand = SecondOperand.GetValue( entry );

                ResultTarget.SetValue( IfCompareMutator.ComparisonPredicates[comparison](firstOperand, secondOperand), entry );
            }

            return mutable;
        }
    }
}
