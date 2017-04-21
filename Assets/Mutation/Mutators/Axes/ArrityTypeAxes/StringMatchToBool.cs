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

namespace Mutation.Mutators.Axes.ArrityTypeAxes
{
    public class StringMatchToBool : Axis<string, bool>
    {
        private MutableField<string> m_AxisKey = new MutableField<string> { AbsoluteKey = "Entries.Challenge Set" };
        [Controllable(LabelText = "Input Value")]
        public MutableField<string> AxisKey
        {
            get { return m_AxisKey; }
            set { m_AxisKey = value; }
        }
        
        private MutableField<string> m_ToMatch = new MutableField<string>()
        { LiteralValue = "String" };
        [Controllable(LabelText = "String To Match")]
        public MutableField<string> ToMatch { get { return m_ToMatch; } }

        private MutableTarget m_TargetField = new MutableTarget()
        { AbsoluteKey = "New Key" };
        [Controllable(LabelText = "Output Target")]
        public MutableTarget TargetField { get { return m_TargetField; } }

        public StringMatchToBool()
        {
            TargetField.SchemaParent = AxisKey;
            ToMatch.SchemaPattern = AxisKey;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            TargetField.SetValue( true, newSchema );

            Router.TransmitAllSchema( newSchema );
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {

            foreach (var entry in AxisKey.GetEntries(mutable))
                TargetField.SetValue(OutputStack.TransformValue(
                    string.Compare( InputStack.TransformValue(AxisKey.GetValue(entry)),
                    InputStack.TransformValue( ToMatch.GetValue( entry ) ), 
                    StringComparison.InvariantCultureIgnoreCase)==0),
                    entry);

            return mutable;
        }
    }
}
