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
using Mutation;
using Mutation.Mutators;
using Visualizers;

namespace Experimental
{
    public class ExperimentalSpoofFreesMutator : DataMutator
    {
        private MutableTarget m_FreeTarget = new MutableTarget() 
        { AbsoluteKey = "Frees" };
        [Controllable(LabelText = "Free Target")]
        public MutableTarget FreeTarget { get { return m_FreeTarget; } }


        protected override MutableObject Mutate( MutableObject mutable )
        {
            var spoofedFrees = new List< MutableObject >();

            var newFree = new MutableObject();
            newFree["Address"] = (uint) 0 ;
            newFree[ "Index" ] = (int)1;
            spoofedFrees.Add( newFree );

            FreeTarget.SetValue( spoofedFrees, mutable );

            return mutable;
        }
    }
}
