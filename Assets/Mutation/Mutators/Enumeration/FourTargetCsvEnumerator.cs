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

using System.Collections;
using Chains;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.Enumeration
{
    public class FourTargetCsvEnumerator : ChainNode
    {
        private MutableField<string> m_CsvField = new MutableField<string>() 
        { LiteralValue = "1,2,3,4" };
        [Controllable(LabelText = "Csv Field")]
        public MutableField<string> CsvField { get { return m_CsvField; } }

        private readonly MutableTarget m_Entry1Target = new MutableTarget() { AbsoluteKey = "Element 1" };
        [Controllable(LabelText = "Element 1 Target")]
        public MutableTarget Entry1Target { get { return m_Entry1Target; } }
        
        private readonly MutableTarget m_Entry2Target = new MutableTarget() { AbsoluteKey = "Element 2" };
        [Controllable(LabelText = "Element 2 Target")]
        public MutableTarget Entry2Target { get { return m_Entry2Target; } }
        
        private readonly MutableTarget m_Entry3Target = new MutableTarget() { AbsoluteKey = "Element 3" };
        [Controllable(LabelText = "Element 3 Target")]
        public MutableTarget Entry3Target { get { return m_Entry3Target; } }
        
        private readonly MutableTarget m_Entry4Target = new MutableTarget() { AbsoluteKey = "Element 4" };
        [Controllable(LabelText = "Element 4 Target")]
        public MutableTarget Entry4Target { get { return m_Entry4Target; } }

        private readonly MutableTarget m_NumberOfElementsTarget = new MutableTarget() { AbsoluteKey = "Number Of Elements" };
        [Controllable(LabelText = "Number Of Elements Target")]
        public MutableTarget NumberOfElementsTarget { get { return m_NumberOfElementsTarget; } }

        public SelectionState PerEntryState { get { return Router["Per Four Elements"]; } }

        public FourTargetCsvEnumerator()
        {
            Entry1Target.SchemaParent = CsvField;
            Entry2Target.SchemaParent = CsvField;
            Entry3Target.SchemaParent = CsvField;
            Entry4Target.SchemaParent = CsvField;

            Router.AddSelectionState("Per Four Elements");
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            Entry1Target.SetValue("1", newSchema);
            Entry2Target.SetValue("2", newSchema);
            Entry3Target.SetValue("3", newSchema);
            Entry4Target.SetValue("4", newSchema);
            NumberOfElementsTarget.SetValue(1, newSchema);

            Router.TransmitAllSchema(newSchema);
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach (var entry in CsvField.GetEntries(payload.Data))
            {
                string csvString = CsvField.GetValue(entry);

                if (string.IsNullOrEmpty(csvString))
                    yield break;

                var values = csvString.Split(',');

                NumberOfElementsTarget.SetValue(Mathf.FloorToInt(values.Length/4f), entry);

                for ( int i = 0; i < values.Length/4; i++ )
                {
                    Entry1Target.SetValue(values[4 * i], entry );
                    Entry2Target.SetValue(values[4 * i + 1], entry);
                    Entry3Target.SetValue(values[4 * i + 2], entry);
                    Entry4Target.SetValue(values[4 * i + 3], entry);

                    var iterator = PerEntryState.Transmit(payload);
                    while (iterator.MoveNext())
                        yield return null;
                }
            }
        }
    }
}
