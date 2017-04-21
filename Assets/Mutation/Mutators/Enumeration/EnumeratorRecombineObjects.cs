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
using Chains;
using Visualizers;

namespace Mutation.Mutators.Enumeration
{
    public class EnumeratorRecombineObjects : ChainNode
    {
        private MutableField<object> m_SingleObject = new MutableField<object>() 
        { AbsoluteKey = "Element" };
        [Controllable(LabelText = "Object to Recombine")]
        public MutableField<object> SingleObject { get { return m_SingleObject; } }

        private MutableField<int> m_NumberOfEntries = new MutableField<int>()
        { AbsoluteKey = "Number Of Entries" };
        [Controllable(LabelText = "Number Of Entries")]
        public MutableField<int> NumberOfEntries { get { return m_NumberOfEntries; } }

        private MutableTarget m_RecombinedListTarget = new MutableTarget()
        { AbsoluteKey = "Recombined List" };

        [Controllable(LabelText = "Recombined List Target")]
        public MutableTarget RecombinedListTarget { get { return m_RecombinedListTarget; } }


        private MutableField<string> m_GroupId = new MutableField<string>() { LiteralValue = "" };
        [Controllable(LabelText = "Shared Group Id")]
        public MutableField<string> GroupId
        {
            get { return m_GroupId; }
        }


        public SelectionState Default { get { return Router["Default"]; } }

        private static NodeDataShare<List<Object>> m_DataShare
            = new NodeDataShare<List<Object>>();
        public static NodeDataShare<List<Object>> DataShare
        { get { return m_DataShare; } set { m_DataShare = value; } }

        public EnumeratorRecombineObjects() : base()
        {
            Router.AddSelectionState("Default");
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            RecombinedListTarget.SetValue(new List<MutableObject>
            {
                new MutableObject{ { "Value",
                SingleObject.GetFirstValue( newSchema )}
                }
            }, newSchema);

            Router.TransmitAllSchema(newSchema);
        }

        public override void Unload()
        {
            DataShare.Clear();

            base.Unload();
        }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var groupId = GroupId.GetFirstValue(payload.Data);

            var singleElement = SingleObject.GetFirstValue(payload.Data);

            var targetElementCount = NumberOfEntries.GetFirstValue(payload.Data);

            if (!DataShare.ContainsKey(groupId))
                DataShare[groupId] = new List<Object>();
            DataShare[groupId].Add(singleElement);

            if (DataShare[groupId].Count >= targetElementCount)
            {
                var dataList = DataShare[groupId];
                RecombinedListTarget.SetValue(
                    (from obj in dataList select new MutableObject { {"Value", obj} }).ToList(), 
                    payload.Data);

                DataShare[groupId] = new List<Object>();

                var iterator = Router.TransmitAll(payload);
                while (iterator.MoveNext())
                    yield return null;
            }
        }
    }
}
