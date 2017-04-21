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

using System.Collections;
using System.Collections.Generic;
using Visualizers;

namespace Mutation.Mutators.BoundManipulationMutators
{
    public class ToggleRenderersInBoundsListMutator : Mutator
    {
        private MutableField<IEnumerable<BoundingBox>> m_BoundsList = new MutableField<IEnumerable<BoundingBox>>() 
        { AbsoluteKey = "BoundsList"};
        [Controllable(LabelText = "Bounds List")]
        public MutableField<IEnumerable<BoundingBox>> BoundsList { get { return m_BoundsList; } }

        private MutableField<bool> m_VisibilityState = new MutableField<bool>() 
        { LiteralValue = true };
        [Controllable(LabelText = "Visibility State")]
        public MutableField<bool> VisibilityState { get { return m_VisibilityState; } }


        public ToggleRenderersInBoundsListMutator()
        {
            VisibilityState.SchemaParent = BoundsList;
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach ( var entry in BoundsList.GetEntries( payload.Data ) )
            {
                var visualState = VisibilityState.GetValue( entry );

                foreach ( var bound in BoundsList.GetValue( entry ) )
                {
                    bound.ToggleRenderers( visualState );
                }
            }

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
