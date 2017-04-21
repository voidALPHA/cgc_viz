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

namespace Mutation.Mutators.DefaultValue
{
    public class DefaultFloatFromPayloadExpression : DefaultValueFromPayloadExpressionMutator< float >
    {
        protected override float GetDefaultValue( MutableObject mutable )
        {
            return default( float );
        }
    }

    public class DefaultIntFromPayloadExpression : DefaultValueFromPayloadExpressionMutator<int>
    {
        protected override int GetDefaultValue(MutableObject mutable)
        {
            return default(int);
        }
    }

    public class DefaultStringFromPayloadExpression : DefaultValueFromPayloadExpressionMutator<string>
    {
        protected override string GetDefaultValue(MutableObject mutable)
        {
            return "string";
        }
    }

    public class DefaultBoolFromPayloadExpression : DefaultValueFromPayloadExpressionMutator<bool>
    {
        protected override bool GetDefaultValue(MutableObject mutable)
        {
            return default(bool);
        }
    }
}
