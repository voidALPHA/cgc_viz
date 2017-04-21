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
using Visualizers;

namespace Mutation.Mutators.ArithmeticOperators
{
    public enum UnaryOperators
    {
        Sum,
        Max,
        Min,
        Abs,
        Sign,
        Average,
        Accumulate,
        Diff,
        Sin,
        Cos,
        Tan,
        Value,
    }

    public abstract class UnaryOperationsMutator<T> : DataMutator
    {

        private MutableScope m_Scope = new MutableScope() 
        { AbsoluteKey = "" };
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<T> m_Operand = new MutableField<T>() 
        { AbsoluteKey= "TotalScore" };
        [Controllable(LabelText = "Operand")]
        public MutableField<T> Operand { get { return m_Operand; } }


        private MutableField<UnaryOperators> m_Operation = new MutableField<UnaryOperators>() 
        { LiteralValue = UnaryOperators.Sum };
        [Controllable(LabelText = "Operation")]
        public MutableField<UnaryOperators> Operation { get { return m_Operation; } }

        private MutableTarget m_OutputValue = new MutableTarget() 
        { AbsoluteKey = "Output Target" };
        [Controllable(LabelText = "OutputValue")]
        public MutableTarget OutputValue { get { return m_OutputValue; } }

        protected UnaryOperationsMutator()
        {
            Operand.SchemaParent = Scope;
            OutputValue.SchemaParent = Scope;
        }


        protected override MutableObject Mutate(MutableObject mutable)
        {
            var operation = Operation.GetFirstValue( mutable );
            
            foreach (var entry in Scope.GetEntries(mutable))
            {
                MetaOperate( entry, operation );
            }

            return mutable;
        }

        protected abstract void MetaOperate( List< MutableObject > entryList, UnaryOperators operation);
    }
}
