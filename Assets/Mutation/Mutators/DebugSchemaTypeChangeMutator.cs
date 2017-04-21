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
    class DebugSchemaTypeChangeMutator : DataMutator
    {
        //Could be deduced based on key being in schema...?
        //private bool IncludeRandomFloat { get; set; }

        private string m_Key = "TypeChangingDebugObject";
        private string Key { get { return m_Key; } set { m_Key = value; } }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            if (!Schema.ContainsKey( Key ))
                Schema.Add( Key, "Pi" );

            if ( !mutable.ContainsKey( Key ) )
                mutable.Add( Key, Schema[Key] );

            return mutable;
        }

        [Controllable]
        [UsedImplicitly]
        private void ChangeToString()
        {
            Schema[Key] = "Pi";

            // NOTE: Don't call this normally. This is a debug node.
            OnProcessOutputSchema( Schema );
        }

        [Controllable]
        [UsedImplicitly]
        private void ChangeToFloat()
        {
            Schema[Key] = 3.14159f;

            // NOTE: Don't call this normally. This is a debug node.
            OnProcessOutputSchema( Schema );
        }
    }
}
