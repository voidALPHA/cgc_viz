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
using JetBrains.Annotations;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.BoundManipulationMutators
{
    [UsedImplicitly]
    public class MoveBoundsToAbsolutePosition:DataMutator
    {
        private MutableField<Vector3> m_AbsolutePosition = new MutableField<Vector3>() 
        { LiteralValue = Vector3.one };
        [Controllable(LabelText = "Absolute Position")]
        public MutableField<Vector3> AbsolutePosition { get { return m_AbsolutePosition; } }

        private MutableField<IEnumerable<BoundingBox> > m_Bounds = new MutableField<IEnumerable<BoundingBox> >() 
        { AbsoluteKey = "Entries.Bounds" };
        [Controllable(LabelText = "Bounds")]
        public MutableField<IEnumerable<BoundingBox> > Bounds { get { return m_Bounds; } }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            var absolutePosition = AbsolutePosition.GetFirstValue( mutable );

            foreach (var entry in Bounds.GetEntries( mutable ))
                foreach ( var bound in Bounds.GetValue( entry ) )
                    bound.transform.position = absolutePosition;

            return mutable;
        }
    }
}
