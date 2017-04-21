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
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.ArithmeticOperators
{
    
    public class RoundFloatMutator : DataMutator
    {
        public enum RoundTypeEnum
        {
            Nearest,
            Up,
            Down,
            Truncate
        }
        private MutableScope m_Scope = new MutableScope() { AbsoluteKey = "" };
        [Controllable( LabelText = "Scope" )]
        public MutableScope Scope { get { return m_Scope; } }


        private MutableField<RoundTypeEnum> m_RoundType = new MutableField<RoundTypeEnum>() { LiteralValue = RoundTypeEnum.Nearest };
        [Controllable( LabelText = "Round Type" )]
        public MutableField<RoundTypeEnum> RoundType { get { return m_RoundType; } }


        private MutableField<float> m_Operand = new MutableField<float>() { AbsoluteKey = "TotalScore" };
        [Controllable( LabelText = "Operand" )]
        public MutableField<float> Operand { get { return m_Operand; } }

        private MutableTarget m_OutputValue = new MutableTarget() { AbsoluteKey = "Output Target" };
        [Controllable( LabelText = "Output Value" )]
        public MutableTarget OutputValue { get { return m_OutputValue; } }

        public RoundFloatMutator()
        {
            Operand.SchemaParent = Scope;
            OutputValue.SchemaParent = Scope;
        }


        protected override MutableObject Mutate(MutableObject mutable)
        {
            var roundType = RoundType.GetFirstValue( mutable );

            foreach (var entry in Scope.GetEntries(mutable))
            {
                foreach ( var subEntry in Operand.GetEntries( entry ) )
                {
                    float inputValue = Operand.GetValue( subEntry );
                    float outputValue;

                    if ( roundType == RoundTypeEnum.Nearest )
                        outputValue = Mathf.Round( inputValue );
                    else if ( roundType == RoundTypeEnum.Down )
                        outputValue = Mathf.Floor( inputValue );
                    else if ( roundType == RoundTypeEnum.Up )
                        outputValue = Mathf.Ceil( inputValue );
                    else if ( roundType == RoundTypeEnum.Truncate )
                        outputValue = (float)Math.Truncate( inputValue );
                    else
                        throw new NotImplementedException("No handler set for RoundType of " + roundType + ".");

                    WriteValue( outputValue, subEntry );
                }                
            }

            return mutable;
        }

        protected virtual void WriteValue( float outputValue, List< MutableObject > subEntry )
        {
            OutputValue.SetValue( outputValue, subEntry );
        }
    }
}