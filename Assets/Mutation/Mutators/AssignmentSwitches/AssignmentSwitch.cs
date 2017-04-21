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

namespace Mutation.Mutators.AssignmentSwitches
{
    public abstract class AssignmentSwitch<T> : Mutator
    {
        private MutableField<string> m_FieldKey = new MutableField<string>() 
        { LiteralValue = "Entries.Round" };
        [Controllable(LabelText = "Field Key")]
        public MutableField<string> FieldKey { get { return m_FieldKey; } }

        private MutableTarget m_AssignmentTarget = new MutableTarget() 
        { AbsoluteKey = "Entries.Target Axis" };
        [Controllable(LabelText = "Assignment Target")]
        public MutableTarget AssignmentTarget { get { return m_AssignmentTarget; } }

        public virtual T GetDefaultValue()
        {
            return default ( T );
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            AssignmentTarget.SetValue( GetDefaultValue(), newSchema );

            Router.TransmitAllSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var samplerField = new MutableField<T> {AbsoluteKey = FieldKey.GetFirstValue( payload.Data )};

            if ( !samplerField.CouldResolve( payload.Data ) )
            {
                Debug.Log( "Couldn't resolve assignment switch " + samplerField.AbsoluteKey + ".  Using default value." );
                AssignmentTarget.SetValue( GetDefaultValue(), payload.Data );
            }
            else
            {
                try
                {
                    foreach ( var entry in AssignmentTarget.GetEntries( payload.Data ) )
                    {
                        AssignmentTarget.SetValue(samplerField.GetValue( entry ), entry);
                    }
                }
                catch ( Exception e )
                {
                    Debug.LogError( "Assignment switch invalid! Probably can't target " + AssignmentTarget.AbsoluteKey + " from theoretical field " + samplerField.AbsoluteKey + "!  Exception: " + e );
                    throw e;
                }
            }

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
