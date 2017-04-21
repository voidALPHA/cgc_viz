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
    public class KeyExistsCriterionStatement : PredicateCriterionStatement
    {
        public override string Name
        {
            get { return "Key Exists"; }
        }

        [CriterionDescriptionElement]
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        private string Symbol1 { get { return "Key: "; } }

        private MutableField<object> m_KeyField = new MutableField<object> { LiteralValue = "Name" };
        [CriterionDescriptionElement]
        [JsonProperty]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        private MutableField<object> KeyField
        {
            get { return m_KeyField; }
            set { m_KeyField = value; }
        }

        [CriterionDescriptionElement]
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        private string Symbol2 { get { return " exists"; } }

        protected override Func< MutableObject, bool > Predicate
        {
            get
            {
                //return mut =>
                //{
                //    string keyToSeek;
                //    keyToSeek = KeyField.SchemaSource != SchemaSource.Cached
                //        ? KeyField.GetFirstValue( mut )
                //        : KeyField.AbsoluteKey;

                //    var testField = new MutableField< object > { AbsoluteKey = keyToSeek };

                //    return testField.ValidateKey( mut );
                //};
                return mut =>
                {
                    if ( KeyField.UseLiteralValue )
                        return new MutableField< object > { AbsoluteKey = (string)KeyField.LiteralValue }
                            .ValidateKey( mut );
                    return KeyField.ValidateKey( mut );
                };
            }
        }
    }
}
