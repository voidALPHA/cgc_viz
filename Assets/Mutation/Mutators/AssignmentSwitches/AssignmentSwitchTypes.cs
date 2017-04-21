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

using Mutation.Mutators.ArithmeticOperators;
using UnityEngine;

namespace Mutation.Mutators.AssignmentSwitches
{
    public class AssignmentSwitchColor : AssignmentSwitch< Color >
    {
        public override Color GetDefaultValue()
        {
            return Color.magenta;
        }
    }

    public class AssignmentSwitchInt : AssignmentSwitch<int> { }

    public class AssignmentSwitchFloat : AssignmentSwitch<float>{}

    public class AssignmentSwitchString : AssignmentSwitch<string>
    {
        public override string GetDefaultValue()
        {
            return "string";
        }
    }

    public class AssignmentSwitchObject : AssignmentSwitch< object >
    {
        public override object GetDefaultValue()
        {
            return "default";
        }
    }

    public class AssignmentSwitchUnaryOperator : AssignmentSwitch< UnaryOperators >
    {
        public override UnaryOperators GetDefaultValue()
        {
            return UnaryOperators.Value;
        }
    }
    
}
