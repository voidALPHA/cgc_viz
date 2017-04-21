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
    public class RoundFloatToNearestMultiple : DataMutator
    {
        public enum RoundTypeEnum
        {
            Nearest,
            Up,
            Down,
            Truncate
        }

        private MutableField<float> m_Operand = new MutableField<float>() 
        { AbsoluteKey = "Operand"};
        [Controllable(LabelText = "Operand")]
        public MutableField<float> Operand { get { return m_Operand; } }

        private MutableField<float> m_Denominator = new MutableField<float>() 
        { LiteralValue = 10f };
        [Controllable(LabelText = "Denominator")]
        public MutableField<float> Denominator { get { return m_Denominator; } }

        private MutableField<RoundTypeEnum> m_RoundingType = new MutableField<RoundTypeEnum>()
        { LiteralValue = RoundTypeEnum.Down };
        [Controllable(LabelText = "RoundingType")]
        public MutableField<RoundTypeEnum> RoundingType { get { return m_RoundingType; } }

        private MutableTarget m_NearestTarget = new MutableTarget() 
        { AbsoluteKey = "Nearest Multiple" };
        [Controllable(LabelText = "NearestTarget")]
        public MutableTarget NearestTarget { get { return m_NearestTarget; } }

        
        public RoundFloatToNearestMultiple()
        {
            Denominator.SchemaParent = Operand;
            NearestTarget.SchemaParent = Operand;
            RoundingType.SchemaPattern = Operand;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach (var entry in Operand.GetEntries( mutable ))
            {
                var roundType = RoundingType.GetValue( entry );

                var operand = Operand.GetValue( entry );

                var denominator = Denominator.GetValue( entry );

                var outputValue = operand / denominator;

                if (roundType == RoundTypeEnum.Nearest)
                    outputValue = Mathf.Round(outputValue);
                else if (roundType == RoundTypeEnum.Down)
                    outputValue = Mathf.Floor(outputValue);
                else if (roundType == RoundTypeEnum.Up)
                    outputValue = Mathf.Ceil(outputValue);
                else if (roundType == RoundTypeEnum.Truncate)
                    outputValue = (float)Math.Truncate(outputValue);

                outputValue *= denominator;

                NearestTarget.SetValue( outputValue, entry );
            }

            return mutable;
        }
    }
}
