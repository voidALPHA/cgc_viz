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
using JetBrains.Annotations;
using Mutation;
using Newtonsoft.Json;

namespace PayloadSelection.CriterionStatements
{
    public abstract class BinaryPredicateStatement<T> : PredicateCriterionStatement where T : IComparable
    {
        private MutableField<T> m_Operand1 = new MutableField<T> { LiteralValue = default(T) };
        [CriterionDescriptionElement]
        [JsonProperty]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public MutableField<T> Operand1
        {
            get { return m_Operand1; }
            set { m_Operand1 = value; }
        }

        [CriterionDescriptionElement]
        public string DisplaySymbol { get { return Symbol; } }

        protected abstract string Symbol { get; }

        private MutableField<T> m_Operand2 = new MutableField<T> { LiteralValue = default(T) };
        [CriterionDescriptionElement]
        [JsonProperty]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public MutableField<T> Operand2
        {
            get { return m_Operand2; }
            set { m_Operand2 = value; }
        }

        protected override Func<MutableObject, bool> Predicate
        {
            get
            {
                return mutable =>
                {
                    if (!Operand1.IsFieldResolvable(mutable) || !Operand2.IsFieldResolvable(mutable))
                        return false;

                    return CompareValues( Operand1.GetFirstValue( mutable ), Operand2.GetFirstValue( mutable ));
                };
            }
        }

        protected abstract bool CompareValues( T value1, T value2 );
    }
}
