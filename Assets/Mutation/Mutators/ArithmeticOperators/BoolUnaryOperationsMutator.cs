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
    public enum BoolOperation
    {
        Or,
        And
    }

    public class BoolUnaryOperationsMutator : DataMutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "List Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<bool> m_Operand = new MutableField<bool>() 
        { AbsoluteKey = "Entries.Bool Value" };
        [Controllable(LabelText = "Operand")]
        public MutableField<bool> Operand { get { return m_Operand; } }
        
        private MutableField<BoolOperation> m_Operation = new MutableField<BoolOperation>() 
        { LiteralValue = BoolOperation.Or };
        [Controllable(LabelText = "Operation")]
        public MutableField<BoolOperation> Operation { get { return m_Operation; } }

        private MutableTarget m_OutputTarget = new MutableTarget() 
        { AbsoluteKey = "Entries.Output Value" };
        [Controllable(LabelText = "OutputTarget")]
        public MutableTarget OutputTarget { get { return m_OutputTarget; } }

        public BoolUnaryOperationsMutator ()
        {
            Operand.SchemaParent = Scope;

            Operation.SchemaPattern = Scope;
            OutputTarget.SchemaParent = Scope;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {
                var operation = Operation.GetValue( entry );

                MetaOperate(entry, operation);
            }

            return mutable;
        }

        private void MetaOperate( List< MutableObject > entry, BoolOperation operation )
        {
            switch ( operation )
            {
                case BoolOperation.And:
                    var total = true;
                    foreach ( var subEntry in Operand.GetEntries( entry ) )
                    {
                        total = total && Operand.GetValue( subEntry );
                    }
                    foreach ( var target in OutputTarget.GetEntries( entry ) )
                    {
                        OutputTarget.SetValue( total, target );
                    }
                    break;
                case BoolOperation.Or:
                    total = false;
                    foreach (var subEntry in Operand.GetEntries(entry))
                    {
                        total = total || Operand.GetValue(subEntry);
                    }
                    foreach (var target in OutputTarget.GetEntries(entry))
                    {
                        OutputTarget.SetValue(total, target);
                    }
                    break;
                default:
                    throw new Exception("Unknown operation type " + operation + "!");

            }
        }
    }
}
