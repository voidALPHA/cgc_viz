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
using Visualizers;

namespace Mutation.Mutators.ColorMutators
{
    [UsedImplicitly]
    public class InsertMutableMutator : DataMutator
    {
        private MutableTarget m_MutableTarget = new MutableTarget() { AbsoluteKey = "New Mutable" };
        [Controllable( LabelText = "Mutable Target Field" )]
        public MutableTarget MutableTarget { get { return m_MutableTarget; } }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            MutableTarget.SetValue( new MutableObject(), mutable );

            return mutable;
        }
    }
}