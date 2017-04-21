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

namespace Mutation.Mutators.BoundManipulationMutators
{
    public class GetCountOfBoundsList : DataMutator
    {
        private MutableField<List<BoundingBox>> m_BoundsList = new MutableField<List<BoundingBox>>() 
        { AbsoluteKey = "Bounds" };
        [Controllable(LabelText = "Bounds List")]
        public MutableField<List<BoundingBox>> BoundsList { get { return m_BoundsList; } }


        private MutableTarget m_CountTarget = new MutableTarget() 
        { AbsoluteKey = "Number Of Bounds" };
        [Controllable(LabelText = "Count Target")]
        public MutableTarget CountTarget { get { return m_CountTarget; } }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach (var entry in BoundsList.GetEntries( mutable ))
                CountTarget.SetValue( BoundsList.GetValue( entry ).Count, entry );

            return mutable;
        }
    }
}
