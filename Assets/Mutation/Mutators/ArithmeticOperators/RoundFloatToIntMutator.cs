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


using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.ArithmeticOperators
{
    public class RoundFloatToIntMutator : DataMutator
    {
        private MutableField<float> m_Operand = new MutableField<float>() { AbsoluteKey = "TotalScore" };
        [Controllable(LabelText = "Operand")]
        public MutableField<float> Operand { get { return m_Operand; } }

        private MutableTarget m_OutputValue = new MutableTarget() { AbsoluteKey = "Output Target" };
        [Controllable(LabelText = "Output Value")]
        public MutableTarget OutputValue { get { return m_OutputValue; } }

        public RoundFloatToIntMutator()
        {
            OutputValue.SchemaParent = Operand;
        }


        protected override MutableObject Mutate(MutableObject mutable)
        {
            foreach (var subEntry in Operand.GetEntries(mutable))
            {
                float inputValue = Operand.GetValue(subEntry);
                int outputValue;

                //    if (roundType == RoundTypeEnum.Nearest)
                        outputValue = Mathf.RoundToInt(inputValue);
                //else if (roundType == RoundTypeEnum.Down)
                //    outputValue = Mathf.Floor(inputValue);
                //else if (roundType == RoundTypeEnum.Up)
                //    outputValue = Mathf.Ceil(inputValue);
                //else if (roundType == RoundTypeEnum.Truncate)
                //    outputValue = (float)Math.Truncate(inputValue);
                //else
                //    throw new NotImplementedException("No handler set for RoundType of " + roundType + ".");

                OutputValue.SetValue(outputValue, subEntry);
            }

            return mutable;
        }

    }
}


//using System.Collections.Generic;
//namespace Mutation.Mutators.ArithmeticOperators
//{
//    public class RoundFloatToIntMutator : RoundFloatMutator
//    {
//        protected override void WriteValue( float outputValue, List<MutableObject> subEntry )
//        {
//            OutputValue.SetValue( (int)outputValue, subEntry );
//        }
//    }
//}