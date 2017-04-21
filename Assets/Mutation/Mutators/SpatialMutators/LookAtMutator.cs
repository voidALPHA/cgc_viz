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
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.SpatialMutators
{
    public class LookAtMutator : DataMutator
    {
        private MutableField<Vector3> m_FacingDirection = new MutableField<Vector3>() 
        { LiteralValue = Vector3.one };
        [Controllable(LabelText = "Facing Direction")]
        public MutableField<Vector3> FacingDirection { get { return m_FacingDirection; } }

        private MutableTarget m_QuaternionTarget = new MutableTarget() 
        { AbsoluteKey = "Facing" };
        [Controllable(LabelText = "Look Quaternion")]
        public MutableTarget QuaternionTarget { get { return m_QuaternionTarget; } }

        public LookAtMutator() : base()
        {
            QuaternionTarget.SchemaParent = FacingDirection;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in FacingDirection.GetEntries( mutable ) )
            {
                QuaternionTarget.SetValue( Quaternion.LookRotation( 
                    FacingDirection.GetValue( entry ) ), entry );
            }

            return mutable;
        }
    }
}
