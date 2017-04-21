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
using System.Linq;
using System.Text;
using UnityEngine;

namespace Mutation.Mutators.ArithmeticOperators
{
    public class Vector3UnaryOperationsMutator : UnaryOperationsMutator<Vector3>
    {
        protected override void MetaOperate(List<MutableObject> entryList, UnaryOperators operation)
        {
            switch (operation)
            {
                case UnaryOperators.Value:
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        OutputValue.SetValue(Operand.GetValue(subEntry), subEntry);
                    }
                    break;
                case UnaryOperators.Accumulate:
                    var total = Vector3.zero;
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        total += Operand.GetValue(subEntry);

                        OutputValue.SetValue(total, subEntry);
                    }
                    break;
                case UnaryOperators.Diff:
                    bool first = true;
                    var prior = Vector3.zero;
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        var foundEntry = Operand.GetValue(subEntry);
                        prior = foundEntry - prior;

                        if (first)
                        {
                            first = false;
                            OutputValue.SetValue(0, subEntry);
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
                        OutputValue.SetValue(Vector3.Normalize(Operand.GetValue(subEntry)), subEntry);
                    }
                    break;
                case UnaryOperators.Sin:
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        var result = Operand.GetValue( subEntry );
                        OutputValue.SetValue(
                            new Vector3(
                                Mathf.Sin( result.x ),
                                Mathf.Sin(result.y),
                                Mathf.Sin(result.z)), subEntry);
                    }
                    break;
                case UnaryOperators.Cos:
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        var result = Operand.GetValue(subEntry);
                        OutputValue.SetValue(
                            new Vector3(
                                Mathf.Cos(result.x),
                                Mathf.Cos(result.y),
                                Mathf.Cos(result.z)), subEntry);
                    }
                    break;
                case UnaryOperators.Tan:
                    foreach (var subEntry in Operand.GetEntries(entryList))
                    {
                        var result = Operand.GetValue(subEntry);
                        OutputValue.SetValue(
                            new Vector3(
                                Mathf.Tan(result.x),
                                Mathf.Tan(result.y),
                                Mathf.Tan(result.z)), subEntry);
                    }
                    break;
                default:
                    throw new Exception("Unhandled operation type!");
            }
        }
    }
}
