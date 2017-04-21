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

namespace Mutation.Mutators.SpatialMutators
{
    public class InsertVector3Mutator : DataMutator
    {

        private MutableScope m_VectorScope = new MutableScope() 
        { AbsoluteKey = "" };
        [Controllable(LabelText = "Vector Scope")]
        public MutableScope VectorScope { get { return m_VectorScope; } }

        private MutableField<float> m_XValue = new MutableField<float>() 
        { LiteralValue = 0 };
        [Controllable(LabelText = "X Value")]
        public MutableField<float> XValue { get { return m_XValue; } }

        private MutableField<float> m_YValue = new MutableField<float>() 
        { LiteralValue = 0 };
        [Controllable(LabelText = "Y Value")]
        public MutableField<float> YValue { get { return m_YValue; } }

        private MutableField<float> m_ZValue = new MutableField<float>() 
        { LiteralValue = 0 };
        [Controllable(LabelText = "Z Value")]
        public MutableField<float> ZValue { get { return m_ZValue; } }

        private MutableTarget m_Vector3Target = new MutableTarget() 
        { AbsoluteKey = "Vector" };
        [Controllable(LabelText = "Vector3 Target")]
        public MutableTarget Vector3Target { get { return m_Vector3Target; } }

        public InsertVector3Mutator() : base()
        {
            XValue.SchemaParent = VectorScope;
            YValue.SchemaParent = VectorScope;
            ZValue.SchemaParent = VectorScope;

            Vector3Target.SchemaParent = VectorScope;
        }


        protected override MutableObject Mutate(MutableObject mutable)
        {
            foreach ( var entry in VectorScope.GetEntries( mutable ) )
            {
                Vector3Target.SetValue( new Vector3(
                    XValue.GetFirstValueBelowArrity(entry),
                    YValue.GetFirstValueBelowArrity(entry),
                    ZValue.GetFirstValueBelowArrity(entry)), entry);
            }
            return mutable;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            Vector3Target.SetValue( Vector3.zero, newSchema );

            Router.TransmitAllSchema(newSchema);
        }
    }
}
