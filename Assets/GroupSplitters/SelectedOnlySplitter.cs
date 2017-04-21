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
using System.Collections.Generic;
using System.Linq;
using Chains;
using Mutation;
using Visualizers;

namespace GroupSplitters
{
    public abstract class SelectedOnlySplitter : ChainNode
    {
        private string m_EntryFieldName = "Entries";
        [Controllable(LabelText = "Group Field Name")]
        public string EntryFieldName { get { return m_EntryFieldName; } set { m_EntryFieldName = value; } }
        
        private MutableScope m_EntryField = new MutableScope();
        [Controllable(LabelText = "Entry Scope")]
        public MutableScope EntryField { get { return m_EntryField; } }

        private MutableTarget m_SelectedListTarget = new MutableTarget() 
        { AbsoluteKey = "Selected List" };
        [Controllable(LabelText = "Selected List Target")]
        public MutableTarget SelectedListTarget { get { return m_SelectedListTarget; } }

        private MutableField<bool> m_NewPayloadOnly = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "New Payload Only?")]
        public MutableField<bool> NewPayloadOnly { get { return m_NewPayloadOnly; } }


        protected virtual string SelectedName { get { return "Selected"; } }

        private SelectionState SelectedSet { get { return Router[SelectedName]; } }

        protected List<MutableObject> SelectedList { get; set; }

        protected SelectedOnlySplitter()
        {
            Router.AddSelectionState( SelectedName );

            SelectedList = new List< MutableObject >();
        }

        protected abstract void SelectGroups(MutableObject mutable);

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            SelectGroups(payload.Data);

            VisualPayload selectedPayload;

            if ( NewPayloadOnly.GetFirstValue( payload.Data ) )
            {
                var newMutable = new MutableObject();
                newMutable[ EntryFieldName ] = SelectedList;
                selectedPayload = new VisualPayload(
                    newMutable,
                    payload.VisualData );
            }
            else
            {
                selectedPayload = payload;
                SelectedListTarget.SetValue(SelectedList, payload.Data);
            }
            
            var iterator = SelectedSet.Transmit(selectedPayload);
            while (iterator.MoveNext())
                yield return null;
        }


        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            SelectGroups(newSchema);


            var firstEntry = EntryField.GetEntries(newSchema).First().Last();


            if (firstEntry != null)
                firstEntry = firstEntry.CloneKeys();
            else
                firstEntry = new MutableObject();

            if (!firstEntry.ContainsKey(GroupSplitter.ImplicitSchemaIndicator.KeyName))
                firstEntry.Add(GroupSplitter.ImplicitSchemaIndicator.KeyName, new GroupSplitter.ImplicitSchemaIndicator(this));

            var outgoingList = SelectedList.Count == 0
                ? new List< MutableObject > { firstEntry }
                : SelectedList;

            MutableObject outgoingSchema;


            if ( NewPayloadOnly.GetFirstValue( newSchema ) )
            {
                outgoingSchema = new MutableObject();
                outgoingSchema[ EntryFieldName ] = outgoingList;
            }
            else
            {
                outgoingSchema = newSchema;
                SelectedListTarget.SetValue(outgoingList, outgoingSchema);
            }

            
            SelectedSet.TransmitSchema(outgoingSchema);
        }
    }
}
