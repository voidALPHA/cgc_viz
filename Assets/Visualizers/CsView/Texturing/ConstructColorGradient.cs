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

using Mutation;
using Mutation.Mutators;
using UnityEngine;
using Utility;

namespace Visualizers.CsView.Texturing
{
    public class ConstructColorGradient : DataMutator
    {
        private MutableField<Color> m_StartColor = new MutableField<Color>() 
        { LiteralValue = Color.black };
        [Controllable(LabelText = "Start Color")]
        public MutableField<Color> StartColor { get { return m_StartColor; } }

        private MutableField<Color> m_EndColor = new MutableField<Color>() 
        { LiteralValue = Color.white };
        [Controllable(LabelText = "End Color")]
        public MutableField<Color> EndColor { get { return m_EndColor; } }

        private MutableTarget m_GradientTarget = new MutableTarget() 
        { AbsoluteKey = "Gradient" };
        [Controllable(LabelText = "Gradient Target")]
        public MutableTarget GradientTarget { get { return m_GradientTarget; } }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            var newGradient = new ColorGradient(2);
            newGradient.ColorKeys[ 0 ] = new GradientColorKey(StartColor.GetFirstValue( mutable ),0);
            newGradient.ColorKeys[ 1 ] = new GradientColorKey(EndColor.GetFirstValue(mutable), 1);

            GradientTarget.SetValue( newGradient, mutable );

            return mutable;
        }
    }
}
