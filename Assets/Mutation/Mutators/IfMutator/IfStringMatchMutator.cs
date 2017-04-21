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
using Visualizers;

namespace Mutation.Mutators.IfMutator
{
    public class IfStringMatchMutator : IfMutator
    {
        private MutableField<string> m_StringArgument = new MutableField<string>() 
        { AbsoluteKey = "Comparison String"};
        [Controllable(LabelText = "First String")]
        public MutableField<string> StringArgument { get { return m_StringArgument; } }

        private MutableField<string> m_StringArgument2 = new MutableField<string>() 
        { AbsoluteKey = "Comparison String"};
        [Controllable(LabelText = "Second String")]
        public MutableField<string> StringArgument2 { get { return m_StringArgument2; } }

        protected override bool MeetsCriterion( VisualPayload payload )
        {
            return
                String.Compare( StringArgument.GetFirstValue( payload.Data ),
                StringArgument2.GetFirstValue( payload.Data ),
                StringComparison.CurrentCultureIgnoreCase)==
                0;
        }
    }
}
