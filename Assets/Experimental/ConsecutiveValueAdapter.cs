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
using Adapters;
using Chains;
using Mutation;
using Visualizers;

namespace Experimental
{
    public class ConsecutiveValueAdapter : Adapter
    {
        private string m_EntryFieldName = "Entries";
        [Controllable(LabelText = "Entries")]
        public string EntryFieldName { get { return m_EntryFieldName; } set { m_EntryFieldName = value; } }

        private MutableField<int> m_NumberOfEntries = new MutableField<int>() 
        { LiteralValue = 10 };
        [Controllable(LabelText = "Number Of Entries")]
        public MutableField<int> NumberOfEntries { get { return m_NumberOfEntries; } }

        private string m_IndexFieldName = "Index";
        [Controllable(LabelText = "Index Field Name")]
        public string IndexFieldName { get { return m_IndexFieldName; } set { m_IndexFieldName = value; } }

        private SelectionState Default { get { return Router["Default"]; } }

        public ConsecutiveValueAdapter() : base()
        {
            Router.AddSelectionState("Default");
        }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var mutables = new List<MutableObject>();

            for (int i = 0; i < NumberOfEntries.GetFirstValue(payload.Data); i++)
            {
                var newObj = new MutableObject();
                newObj[IndexFieldName] = i;
                mutables.Add(newObj);
            }

            var iterator = Router.TransmitAll(new VisualPayload(new MutableObject(){new KeyValuePair<string, object>(EntryFieldName, mutables)}, payload.VisualData ));
            while (iterator.MoveNext())
                yield return null;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            var outputSchema = new MutableObject()
            {
                new KeyValuePair<string, object>(EntryFieldName,
                    new List<MutableObject>(){new MutableObject()
                    {
                        new KeyValuePair<string, object>(IndexFieldName, 1)
                    }})
            };

            Router.TransmitAllSchema(outputSchema);
        }
    }
}
