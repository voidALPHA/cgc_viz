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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.FilamentPositioning
{
    public abstract class ValueToXYMutator : Mutator
    {
        private MutableField<int> m_IndexValue = new MutableField<int>()
        {AbsoluteKey = "Eip"};
        [Controllable(LabelText = "Index Value")]
        public MutableField<int> IndexValue
        {
            get { return m_IndexValue; }
        }

        private MutableTarget m_XAxisTarget = new MutableTarget() 
        { AbsoluteKey = "X Axis" };
        [Controllable(LabelText = "X Axis Target")]
        public MutableTarget XAxisTarget { get { return m_XAxisTarget; } }

        private MutableTarget m_YAxisTarget = new MutableTarget() 
        { AbsoluteKey = "Y Axis" };
        [Controllable(LabelText = "Y Axis Target")]
        public MutableTarget YAxisTarget { get { return m_YAxisTarget; } }

        public void Start()
        {
            XAxisTarget.SchemaParent = IndexValue;
            YAxisTarget.SchemaParent = IndexValue;
        }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var entries = IndexValue.GetEntries(payload.Data);

            foreach (var entry in entries)
            {
                var location = ComputeXYPosition(IndexValue.GetValue(entry));
                XAxisTarget.SetValue( location.x, entry);
                YAxisTarget.SetValue(location.y, entry);
            }

            var iterator = Router.TransmitAll(payload);

            while (iterator.MoveNext())
                yield return null;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            var entries = IndexValue.GetEntries(newSchema);

            foreach (var entry in entries)
            {
                XAxisTarget.SetValue(0f, entry);
                YAxisTarget.SetValue(0f, entry);
            }

            Router.TransmitAllSchema(newSchema);
        }

        protected abstract Vector2 ComputeXYPosition(int index);
    }
}
