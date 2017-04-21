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
using Visualizers;

namespace Mutation.Mutators.Axes.ArrityTypeAxes
{
    public abstract class TypeConversionAxis<tIn, tOut> : Axis<tIn, tOut>
    {
        private MutableField<tIn> m_AxisKey = new MutableField<tIn> { AbsoluteKey = "Entries.Challenge Set" };
        [Controllable(LabelText = "Input Value")]
        public MutableField<tIn> AxisKey
        {
            get { return m_AxisKey; }
            set { m_AxisKey = value; }
        }

        private MutableTarget m_TargetField = new MutableTarget() 
        { AbsoluteKey = "New Key" };
        [Controllable(LabelText = "Output Target")]
        public MutableTarget TargetField { get { return m_TargetField; } }

        protected TypeConversionAxis()
        {
            TargetField.SchemaParent = AxisKey;
        }
        
        protected abstract tOut ConversionFunc(tIn key, List<MutableObject> entry);

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
           // if (AxisKey.ValidateKey( newSchema ) && AxisKey.CouldResolve( newSchema ))
           //     TargetField.SetValue( AxisKey.GetFirstValue( newSchema ), newSchema );
           // else
                TargetField.SetValue(DefaultValue(), newSchema );

            Router.TransmitAllSchema(newSchema);
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            foreach ( var entry in AxisKey.GetEntries( mutable ) )
                TargetField.SetValue( OutputStack.TransformValue(
                    ConversionFunc(
                        InputStack.TransformValue( AxisKey.GetValue( entry ) ), entry ) ),
                    entry );

            return mutable;
        }

        protected virtual tOut DefaultValue()
        {
            return default ( tOut );
        }
    }
}
