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
using Chains;
using UnityEngine;
using Visualizers;
using Visualizers.MetaSelectors;

namespace Mutation.Mutators
{
    public class EnumeratorIterator : ChainNode
    {
        private MutableScope m_EntryField = new MutableScope()
        {AbsoluteKey = ""};
        [Controllable(LabelText = "Entry Field")]
        public MutableScope EntryField
        {
            get { return m_EntryField; }
        }
        
        #region Selection Graph States
        public SelectionState NormalState { get { return Router["Normal"]; } }
        public SelectionState SelectedState { get { return Router["Selected"]; } }
        public SelectionState NoneSelectedState { get { return Router["None Selected"]; } }
        public SelectionState NormalStateMulti { get { return Router["Normal (Group)"]; } }
        public SelectionState SelectedStateMulti { get { return Router["Selected (Group)"]; } }
        public SelectionState NoneSelectedStateMulti { get { return Router["None Selected (Group)"]; } }
        #endregion

        public EnumeratorIterator()
        {
            Router.AddSelectionState("Normal", "Single");
            Router.AddSelectionState("Selected", "Single");
            Router.AddSelectionState("None Selected", "Single");

            Router.AddSelectionState("Normal (Group)", "Multi");
            Router.AddSelectionState("Selected (Group)", "Multi");
            Router.AddSelectionState("None Selected (Group)", "Multi");

        }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            //payload.VisualData.Bound.Data = payload.Data;
            
            var metaEntries = EntryField.GetEntries( payload.Data );

            var createdPayloads = new List<VisualPayload>();

            foreach ( var metaEntry in metaEntries )
            {
                // foreach ( var entry in entries )
                // {
                var newBound = payload.VisualData.Bound.CreateDependingBound( Name );

                var newPayload = new VisualPayload( metaEntry.Last(), new VisualDescription( newBound ) );

                //newBound.Data = metaEntry.Last();

                createdPayloads.Add( newPayload );
            }

            MetaSelectionSet selectionManager = MetaSelectionSet.ConstructPayloadSelectionSet(
                    payload.VisualData.Bound, createdPayloads,
                    NormalState, SelectedState, NoneSelectedState,
                    NormalStateMulti, SelectedStateMulti, NoneSelectedStateMulti );

                var iterator = selectionManager.TransmitAll();
                while ( iterator.MoveNext() )
                    yield return null;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            var entry = EntryField.GetEntries( newSchema );

            var foundEntries = entry.ToList();

            if (!foundEntries.Any())
            {
                Debug.Log("No valid entries in " + EntryField.AbsoluteKey);
                Router.TransmitAllSchema(new MutableObject());
                return;
            }

            MutableObject outMutable = null;
            foreach ( var foundEntry in EntryField.GetEntries( newSchema ) )
            {
                outMutable = foundEntry.Last();
                break;
            }

            NormalState.TransmitSchema(outMutable);
            SelectedState.TransmitSchema(outMutable);
            NoneSelectedState.TransmitSchema(outMutable);

            var multiSchema = new MutableObject()
            { { "Entries", new List<MutableObject>{ outMutable }}};

            NormalStateMulti.TransmitSchema( multiSchema );
            SelectedStateMulti.TransmitSchema(multiSchema);
            NoneSelectedStateMulti.TransmitSchema(multiSchema);
        }
    }
}
