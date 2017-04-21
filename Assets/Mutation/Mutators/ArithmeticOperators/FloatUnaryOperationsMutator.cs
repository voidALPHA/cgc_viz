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

namespace Mutation.Mutators.ArithmeticOperators
{
    public class FloatUnaryOperationsMutator : UnaryOperationsMutator<float>
    {
        protected override void MetaOperate( List< MutableObject > entryList, UnaryOperators operation)
        {
            switch (operation)
            {
                case UnaryOperators.Value:
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        OutputValue.SetValue(Operand.GetValue(subEntry), subEntry);
                    }
                    break;

                case UnaryOperators.Abs:
                    foreach ( var subEntry in Operand.GetEntries( entryList ) )
                    {
                        OutputValue.SetValue( Mathf.Abs( Operand.GetValue( subEntry ) ), subEntry );
                    }
                    break;
                case UnaryOperators.Sum:
                    float total = 0;
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        total += Operand.GetValue( subEntry );
                    }
                    foreach (var subEntry in OutputValue.GetEntries(entryList))
                    {
                        OutputValue.SetValue(total, subEntry);
                    }
                    break;
                case UnaryOperators.Average:
                    total = 0;
                    int count = 0;
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        total += Operand.GetValue(subEntry);
                        count++;
                    }
                    total /= count;
                    foreach (var subEntry in OutputValue.GetEntries(entryList))
                    {
                        OutputValue.SetValue(total, subEntry);
                    }
                    break;
                case UnaryOperators.Max:
                    float max = float.MinValue;
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        var foundVal = Operand.GetValue(subEntry);
                        if ( max < foundVal )
                            max = foundVal;
                    }
                    foreach (var subEntry in OutputValue.GetEntries(entryList))
                    {
                        OutputValue.SetValue(max, subEntry);
                    }
                    break;
                case UnaryOperators.Min:
                    float min = float.MaxValue;
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        var foundVal = Operand.GetValue(subEntry);
                        if (min > foundVal)
                            min = foundVal;
                    }
                    foreach (var subEntry in OutputValue.GetEntries(entryList))
                    {
                        OutputValue.SetValue(min, subEntry);
                    }
                    break;
                case UnaryOperators.Accumulate:
                    total = 0f;
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        total += Operand.GetValue( subEntry );

                        OutputValue.SetValue(total, subEntry);
                    }
                    break;
                case UnaryOperators.Diff:
                    bool first = true;
                    var prior = 0f;
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        var foundEntry = Operand.GetValue(subEntry);
                        prior = foundEntry - prior;

                        if ( first )
                        {
                            first = false;
                            OutputValue.SetValue(0f, subEntry);
                            prior = foundEntry;
                            continue;
                        }

                        OutputValue.SetValue(prior, subEntry);
                        prior = foundEntry;
                    }
                    break;
                case UnaryOperators.Sign:
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        OutputValue.SetValue((Operand.GetValue(subEntry)>=0?1f:-1f), subEntry);
                    }
                    break;
                case UnaryOperators.Sin:
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        OutputValue.SetValue(Mathf.Sin(Operand.GetValue(subEntry)), subEntry);
                    }
                    break;
                case UnaryOperators.Cos:
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        OutputValue.SetValue(Mathf.Cos(Operand.GetValue(subEntry)), subEntry);
                    }
                    break;
                case UnaryOperators.Tan:
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        OutputValue.SetValue(Mathf.Tan(Operand.GetValue(subEntry)), subEntry);
                    }
                    break;
                default:
                    throw new Exception("Unknown operation type!");
            }
        }
    }
}
