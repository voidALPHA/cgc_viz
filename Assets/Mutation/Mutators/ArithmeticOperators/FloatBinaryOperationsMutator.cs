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

namespace Mutation.Mutators.ArithmeticOperators
{
    public class FloatBinaryOperationsMutator : BinaryOperationsMutator<float>
    {
        protected override float Operate(float arg1, float arg2, BinaryOperators operation)
        {
            switch (operation)
            {
                case BinaryOperators.Add:
                    return arg1 + arg2;
                case BinaryOperators.Subtract:
                    return arg1 - arg2;
                case BinaryOperators.Multiply:
                    return arg1 * arg2;
                case BinaryOperators.Divide:
                    return arg1 / arg2;
                case BinaryOperators.Exponent:
                    return Mathf.Pow(arg1, arg2);
                case BinaryOperators.Mod:
                    return arg1 % arg2;
                case BinaryOperators.DivisibleBy:
                    return arg1 % arg2 < .0001f ? 1 : 0;
                case BinaryOperators.Max:
                    return Mathf.Max( arg1, arg2 );
                case BinaryOperators.Min:
                    return Mathf.Min(arg1, arg2);
                default:
                    throw new Exception("Unknown operation type!");
            }
        }
    }
}