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
using System.Collections;
using UnityEngine;
using Visualizers;
using Object = System.Object;

namespace Mutation.Mutators
{
    public class FormatStringMutator : DataMutator
    {
        private MutableField<string> m_FormatString = new MutableField<string>() 
        { LiteralValue = "String: {0}" };
        [Controllable(LabelText = "Format String")]
        public MutableField<string> FormatString { get { return m_FormatString; } }


        private MutableField<Object> m_PrimaryArgument = new MutableField<Object>() 
        { LiteralValue = "" };
        [Controllable(LabelText = "PrimaryArgument")]
        public MutableField<Object> PrimaryArgument { get { return m_PrimaryArgument; } }

        private MutableField<Object> m_SecondaryArgument = new MutableField<Object>() 
        { LiteralValue = "" };
        [Controllable(LabelText = "SecondaryArgument")]
        public MutableField<Object> SecondaryArgument { get { return m_SecondaryArgument; } }

        private MutableTarget m_TargetString = new MutableTarget() 
        { AbsoluteKey = "Formatted String" };
        [Controllable(LabelText = "Formatted String Target")]
        public MutableTarget TargetString { get { return m_TargetString; } }

        public FormatStringMutator()
        {
            SecondaryArgument.SchemaPattern = PrimaryArgument;
            TargetString.SchemaParent = PrimaryArgument;
        }


        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in PrimaryArgument.GetEntries( mutable ) )
            {
                try
                {
                    var formattedString = string.Format( FormatString.GetFirstValueBelowArrity( entry ),
                        PrimaryArgument.GetValue( entry ),
                        SecondaryArgument.GetValue( entry ) );

                    TargetString.SetValue( formattedString, entry );
                }
                catch ( Exception e )
                {
                    Debug.LogWarning( "Warning: intermediary format exception during string parsing: " +e);
                }
            }

            return mutable;
        }
    }
}
