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
using PayloadSelection.CriterionStatements;

namespace Experimental
{
    public class ExperimentalCromuCriterion : PredicateCriterionStatement
    {
        public override string Name
        {
            get { return "CROMU Selection"; }
        }

        private MutableField<string> m_CromuString = new MutableField<string> { LiteralValue = "cromu" };
        [CriterionDescriptionElement]
        [JsonProperty]
        [UsedImplicitly (ImplicitUseKindFlags.Assign)]
        private MutableField<string> CromuString
        {
            get { return m_CromuString; }
            set { m_CromuString = value; }
        }

        [CriterionDescriptionElement]
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        private string Symbol1 { get { return "is the first string in Challenge Set"; } }


        private MutableField<string> ChallengeSetField = new MutableField< string >(){AbsoluteKey = "Challenge Set"};
        
        protected override Func< MutableObject, bool > Predicate
        {
            get
            {
                return
                    mut =>
                    {
                        if (!CromuString.ValidateKey(mut) || !ChallengeSetField.ValidateKey( mut ))
                            return false;

                        return ChallengeSetField.GetFirstValue( mut )
                            .StartsWith( CromuString.GetFirstValue( mut ), true, CultureInfo.InvariantCulture );
                    };
            }
        }
    }
}
