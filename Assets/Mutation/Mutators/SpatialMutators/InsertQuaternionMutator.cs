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
    public class InsertQuaternionMutator : DataMutator
    {
        private MutableField<float> m_XValue = new MutableField<float>() { LiteralValue = 0 };
        [Controllable(LabelText = "X Value")]
        public MutableField<float> XValue { get { return m_XValue; } }

        private MutableField<float> m_YValue = new MutableField<float>() { LiteralValue = 0 };
        [Controllable(LabelText = "Y Value")]
        public MutableField<float> YValue { get { return m_YValue; } }

        private MutableField<float> m_ZValue = new MutableField<float>() { LiteralValue = 0 };
        [Controllable(LabelText = "Z Value")]
        public MutableField<float> ZValue { get { return m_ZValue; } }

        private MutableField<float> m_WValue = new MutableField<float>() { LiteralValue = 0 };
        [Controllable(LabelText = "W Value")]
        public MutableField<float> WValue { get { return m_WValue; } }

        private MutableTarget m_QuaternionTarget = new MutableTarget() 
        { AbsoluteKey = "Quaternion" };
        [Controllable(LabelText = "Quaternion Target")]
        public MutableTarget QuaternionTarget { get { return m_QuaternionTarget; } }


        protected override MutableObject Mutate(MutableObject mutable)
        {
            QuaternionTarget.SetValue( new Quaternion(
                XValue.GetFirstValue(mutable),
                YValue.GetFirstValue(mutable),
                ZValue.GetFirstValue(mutable),
                WValue.GetFirstValue( mutable)), mutable);

            return mutable;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            QuaternionTarget.SetValue( Quaternion.identity, newSchema );

            Router.TransmitAllSchema(newSchema);
        }
    }
}
