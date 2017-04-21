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

using System.Collections.Generic;
using Visualizers;

namespace Mutation.Mutators.ToBoolMutators
{
    public abstract class ToBoolMutator : DataMutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable( LabelText = "Scope" )]
        protected MutableScope Scope { get { return m_Scope; } }

        private MutableTarget m_TargetBool = new MutableTarget() { AbsoluteKey = "Starts With" };
        [Controllable( LabelText = "Output Bool" )]
        protected MutableTarget TargetBool { get { return m_TargetBool; } }

        protected ToBoolMutator()
        {
            TargetBool.SchemaParent = Scope;
        }

        protected abstract bool GetBool( List< MutableObject > entry );

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {
                TargetBool.SetValue( GetBool( entry ), entry );
            }

            return mutable;
        }
    }
}