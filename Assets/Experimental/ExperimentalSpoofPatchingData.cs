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
using Mutation;
using Mutation.Mutators;
using Visualizers;

namespace Experimental
{
    public class ExperimentalSpoofPatchingData : Mutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }


        private MutableField< float > m_Availability = new MutableField< float >()
        { LiteralValue = 1.0f };

        [Controllable( LabelText = "Availability Total" )]
        public MutableField< float > Availability
        {
            get { return m_Availability; }
        }

        private MutableTarget m_PatchingTarget = new MutableTarget() 
        { AbsoluteKey = "Scores.Patching" };
        [Controllable(LabelText = "Patching Target")]
        public MutableTarget PatchingTarget { get { return m_PatchingTarget; } }

        public ExperimentalSpoofPatchingData()
        {
            Availability.SchemaParent = Scope;
            PatchingTarget.SchemaParent = Scope;
        }

        private static Random s_rand;
        private static Random Rand { get { return s_rand ?? (s_rand = new Random(1337)); } }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach ( var entry in Scope.GetEntries( payload.Data ) )
            {
                PatchingTarget.SetValue((Availability.GetValue(entry) < .1f)&&(Rand.NextDouble() > .4),entry);
            }
            
            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            foreach (var entry in Scope.GetEntries( newSchema ))
                PatchingTarget.SetValue( true, entry );

            Router.TransmitAllSchema( newSchema );
        }
    }
}
