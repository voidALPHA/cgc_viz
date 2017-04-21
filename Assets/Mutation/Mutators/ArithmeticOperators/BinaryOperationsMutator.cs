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
using Visualizers;

namespace Mutation.Mutators.ArithmeticOperators
{
    public abstract class BinaryOperationsMutator<T> : DataMutator
    {
        public enum BinaryOperators
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Exponent,
            Mod,
            DivisibleBy,
            Max,
            Min,
        }

        private MutableField<T> m_PrimaryOperand = new MutableField<T>() { LiteralValue = default(T) };
        [Controllable(LabelText = "Primary Operand")]
        public MutableField<T> PrimaryOperand { get { return m_PrimaryOperand; } }

        private MutableField<BinaryOperators> m_Operation = new MutableField<BinaryOperators>() 
        { LiteralValue = BinaryOperators.Add };
        [Controllable(LabelText = "Operation")]
        public MutableField<BinaryOperators> Operation { get { return m_Operation; } }

        private MutableField<T> m_SecondaryOperand = new MutableField<T>() 
        { LiteralValue = default(T) };
        [Controllable(LabelText = "Secondary Operand")]
        public MutableField<T> SecondaryOperand { get { return m_SecondaryOperand; } }

        private MutableTarget m_OutputValue = new MutableTarget() 
        { AbsoluteKey = "Output Target" };
        [Controllable(LabelText = "Output Value")]
        public MutableTarget OutputValue { get { return m_OutputValue; } }

        public BinaryOperationsMutator()
        {
            //SecondaryOperand.SchemaPattern = PrimaryOperand;
            Operation.SchemaPattern = PrimaryOperand;
            OutputValue.SchemaParent = PrimaryOperand;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in PrimaryOperand.GetEntries( mutable ) )
            {
                var foundValue = Operate( PrimaryOperand.GetValue( entry ),
                    SecondaryOperand.GetFirstValueBelowArrity( entry ), Operation.GetFirstValueBelowArrity( entry ) );
                OutputValue.SetValue( foundValue, entry );
            }
            return mutable;
        }

        protected abstract T Operate( T arg1, T arg2, BinaryOperators operation );
    }

}
