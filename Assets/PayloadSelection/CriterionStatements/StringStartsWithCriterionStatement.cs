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
using System.Globalization;
using JetBrains.Annotations;
using Mutation;
using Newtonsoft.Json;
using Visualizers;

namespace PayloadSelection.CriterionStatements
{
    public class StringStartsWithCriterionStatement : PredicateCriterionStatement
    {
        public override string Name { get { return "String Starts With"; } }

        private MutableField< string > m_Operand1 = new MutableField< string > { LiteralValue = "" };
        [CriterionDescriptionElement]
        [JsonProperty]
        [UsedImplicitly( ImplicitUseKindFlags.Assign )]
        public MutableField< string > Operand1
        {
            get { return m_Operand1; }
            set { m_Operand1 = value; }
        }


        [CriterionDescriptionElement]
        public string Symbol { get { return "Starts With"; } }


        private MutableField<string> m_Operand2 = new MutableField<string> { LiteralValue = "" };
        [CriterionDescriptionElement]
        [JsonProperty]
        [UsedImplicitly( ImplicitUseKindFlags.Assign )]
        public MutableField< string > Operand2
        {
            get { return m_Operand2; }
            set { m_Operand2 = value; }
        }

        // TODO: Can we expose this in the CriterionStatement UI?
        [Controllable]
        public bool IgnoreCase { get; set; }

        protected override Func<MutableObject, bool> Predicate
        {
            get
            {
                return mutable =>
                {
                    if (!Operand1.ValidateKey(mutable) || !Operand2.ValidateKey(mutable))
                        return false;
                    return 
                        Operand1.GetFirstValue( mutable )
                            .StartsWith( Operand2.GetFirstValue( mutable ), IgnoreCase,
                                CultureInfo.InvariantCulture );
                };
            }
        }
    }
}