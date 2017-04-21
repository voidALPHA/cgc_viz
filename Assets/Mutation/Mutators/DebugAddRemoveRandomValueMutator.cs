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

using JetBrains.Annotations;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators
{
    public class DebugAddRemoveRandomValueMutator : DataMutator
    {
        //Could be deduced based on key being in schema...?
        private bool IncludeRandomFloat { get; set; }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            if ( !IncludeRandomFloat )
            {
                if ( mutable.ContainsKey( "RandomFloat" ) )
                    mutable.Remove( "RandomFloat" );
            }
            else
            {
                if ( !mutable.ContainsKey( "RandomFloat" ) )
                    mutable.Add( "RandomFloat", Random.Range( 0.0f, 1.0f ) );
                else
                    mutable["RandomFloat"] = Random.Range( 0.0f, 1.0f );
            }

            return mutable;
        }
        

        [Controllable]
        [UsedImplicitly]
        private void AddKvpToMutableObject()
        {
            if ( IncludeRandomFloat )
                return;

            IncludeRandomFloat = true;
            
            Mutate( Schema );

            // NOTE: Don't call this normally. This is a debug node.
            OnProcessOutputSchema( Schema );
        }

        [Controllable]
        [UsedImplicitly]
        private void RemoveKvpFromMutableObject()
        {
            // NOTE: This violates convention! Do not remove mutable object values!
            // This only serves debugging to simulate values which have gone missing due to, say, changes in the chain.

            if ( !IncludeRandomFloat )
                return;

            IncludeRandomFloat = false;

            // Normally, we rely on Mutate() to do the mutation--but it doesn't support this ;)
            Schema.Remove( "RandomFloat" );

            // NOTE: Don't call this normally. This is a debug node.
            OnProcessOutputSchema( Schema );
        }
    }
}
