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
using Utility;

namespace Mutation.Mutators.ArithmeticOperators
{
    public class Vector3BinaryOperationsMutator : BinaryOperationsMutator<Vector3>
    {
        protected override Vector3 Operate(Vector3 arg1, Vector3 arg2, BinaryOperators operation)
        {
            switch (operation)
            {
                case BinaryOperators.Add:
                    return arg1 + arg2;
                case BinaryOperators.Subtract:
                    return arg1 - arg2;
                case BinaryOperators.Multiply:
                    return arg1.PiecewiseMultiply( arg2 );
                case BinaryOperators.Divide:
                    throw new InvalidOperationException("One does not simply divide two vectors!");
                case BinaryOperators.Exponent:
                    throw new InvalidOperationException("One does not simply exponentiate two vectors!");
                case BinaryOperators.Mod:
                    throw new InvalidOperationException("One does not simply mod two vectors!");
                case BinaryOperators.DivisibleBy:
                    throw new InvalidOperationException("One does not simply check if two vectors are divisible!");
                case BinaryOperators.Max:
                    throw new InvalidOperationException("One does not simply max two vectors!");
                case BinaryOperators.Min:
                    throw new InvalidOperationException("One does not simply min two vectors!");
                default:
                    throw new Exception("Unknown operation type!");
            }
        }
    }
}
