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

using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.Axes.ArrityTypeAxes
{
    public class ConvertVector3ToFloats : DataMutator
    {
        private MutableField<Vector3> m_Vector = new MutableField<Vector3>() 
        { AbsoluteKey = "Position" };
        [Controllable(LabelText = "Vector")]
        public MutableField<Vector3> Vector { get { return m_Vector; } }

        private MutableTarget m_XValue = new MutableTarget() 
        { AbsoluteKey = "X Value" };
        [Controllable(LabelText = "X Value")]
        public MutableTarget XValue { get { return m_XValue; } }

        private MutableTarget m_YValue = new MutableTarget() 
        { AbsoluteKey = "Y Value" };
        [Controllable(LabelText = "Y Value")]
        public MutableTarget YValue { get { return m_YValue; } }
        
        private MutableTarget m_ZValue = new MutableTarget() 
        { AbsoluteKey = "Z Value" };
        [Controllable(LabelText = "Z Value")]
        public MutableTarget ZValue { get { return m_ZValue; } }

        private void Start()
        {
            XValue.SchemaParent = Vector;
            YValue.SchemaParent = Vector;
            ZValue.SchemaParent = Vector;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Vector.GetEntries( mutable ) )
            {
                var vector = Vector.GetValue( entry );

                XValue.SetValue( vector.x, entry );
                YValue.SetValue( vector.y, entry );
                ZValue.SetValue( vector.z, entry );
            }

            return mutable;
        }
    }
}
