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
using Mutation;
using UnityEngine;

namespace Visualizers.MetaSelectors
{
    public class MetaSelectionSet : MonoBehaviour, IMetaSelectable
    {
        public static MetaSelectionSet ConstructPayloadSelectionSet(BoundingBox bound, List<VisualPayload> selectablePayloads,
            SelectionState normalState, SelectionState selectedState, SelectionState noneSelectedState, 
            SelectionState normalStateMulti, SelectionState selectedStateMulti, SelectionState noneSelectedStateMulti )
        {
            MetaSelectionSet selectionSet = bound.gameObject.AddComponent< MetaSelectionSet >();

            selectionSet.Bound = bound;
            selectionSet.SelectablePayloads = selectablePayloads;

            selectionSet.UnSelectedList.AddRange(selectablePayloads);

            selectionSet.NormalState = normalState;
            selectionSet.SelectedState = selectedState;
            selectionSet.NoneSelectedState = noneSelectedState;

            selectionSet.NormalStateMulti = normalStateMulti;
            selectionSet.SelectedStateMulti = selectedStateMulti;
            selectionSet.NoneSelectedStateMulti = noneSelectedStateMulti;

            return selectionSet;
        }

        public static MetaSelectionSet ConstructStatelessSelectionSet( BoundingBox bound,
            List< VisualPayload > selectablePayloads )
        {
            var emptySingleState = new SelectionState( "Empty Single State" );
            emptySingleState.GroupId = "Empty Single";
            var emptyMultiState = new SelectionState( "Empty Multi State" );
            emptyMultiState.GroupId = "Empty Multi";

            return ConstructPayloadSelectionSet( bound, selectablePayloads,
                emptySingleState, emptySingleState, emptySingleState,
                emptyMultiState, emptyMultiState, emptyMultiState );
        }

        public static List<VisualPayload> GetLocalSelectables(IEnumerable<BoundingBox> boundsToSelect, IMetaSelectable selectable)
        {
            var foundPayloads = new List<VisualPayload>();

            // the slow, but valid way
            foreach (var element in selectable.SelectablePayloads)
            {
                if (boundsToSelect.Contains(element.VisualData.Bound))
                {
                    foundPayloads.Add(element);
                }
            }

            //  // the fast, but technically sketchy way
            //  foreach ( var element in boundsToSelect )
            //  {
            //      foundPayloads.Add( new VisualPayload( element.Data, new VisualDescription( element ) ) );
            //  }

            return foundPayloads;
        }

        private BoundingBox Bound { get; set; }

        private List<VisualPayload> m_UnSelectedList = new List<VisualPayload>();
        private List<VisualPayload> UnSelectedList { get { return m_UnSelectedList; } set { m_UnSelectedList = value; } }
        //private List<MutableObject> UnSelectedListAsMutables { get { return UnSelectedList.ConvertAll(p => p.Data); } }

        private List<VisualPayload> m_SelectedList = new List<VisualPayload>();
        private List<VisualPayload> SelectedList { get { return m_SelectedList; } set { m_SelectedList = value; } }
        //private List<MutableObject> SelectedListAsMutables { get { return SelectedList.ConvertAll(p => p.Data); } }

        private BoundingBox UnselectedBound { get; set; }
        private BoundingBox SelectedBound { get; set; }
        private BoundingBox NoneSelectedBound { get; set; }

        private VisualPayload UnSelectedMutable
        {
            get
            {
                if (UnselectedBound == null)
                    UnselectedBound = Bound.CreateDependingBound(GetType().Name + " (Unselected)");

                  return new VisualPayload(
                  new MutableObject() { new KeyValuePair<string, object>("Entries", UnSelectedList.ConvertAll(p=>p.Data)) },
                      new VisualDescription(UnselectedBound));
            }
        }

        private VisualPayload SelectedMutable
        {
            get
            {
                if (SelectedBound == null)
                    SelectedBound = Bound.CreateDependingBound(GetType().Name + " (Selected)");

                return new VisualPayload(
                  new MutableObject() { new KeyValuePair<string, object>("Entries", SelectedList.ConvertAll(p => p.Data)) },
                    new VisualDescription(SelectedBound));
            }
        }

        private VisualPayload NoneSelectedMutable
        {
            get
            {
                if (NoneSelectedBound == null)
                    NoneSelectedBound = Bound.CreateDependingBound(GetType().Name + " (NoneSelected)");

                return new VisualPayload(
                  new MutableObject() { new KeyValuePair<string, object>("Entries", UnSelectedList.ConvertAll(p => p.Data)) },
                    new VisualDescription(NoneSelectedBound));
            }
        }

        #region Selection States
        public SelectionState NormalState { get; set; }
        public SelectionState SelectedState { get; set; }
        public SelectionState NoneSelectedState { get; set; }
        public SelectionState NormalStateMulti { get; set; }
        public SelectionState SelectedStateMulti { get; set; }
        public SelectionState NoneSelectedStateMulti { get; set; }
        //private bool NormalSelectedMultiOn = true;
        //private bool NoneSelectedMultiOn = true;
        #endregion

        #region Payload Selectable Implementation

        public Transform GetTransform()
        {
            return Bound.transform;
        }

        public List<VisualPayload> SelectablePayloads { get; set; }

        private IEnumerator SendUnselected()
        {
            foreach (var payload in UnSelectedList)
            {
                payload.VisualData.Bound.ClearBounds();

                var iterator = NormalState.Transmit(payload);
                while (iterator.MoveNext())
                    yield return null;
            }
        }

        private IEnumerator SendNoneSelected()
        {
            foreach (var payload in UnSelectedList)
            {
                payload.VisualData.Bound.ClearBounds();

                var iterator = NoneSelectedState.Transmit(payload);
                while (iterator.MoveNext())
                    yield return null;
            }
        }

        public IEnumerator Select(VisualPayload payload)
        {
            bool sendNormal = !SelectedList.Any();

            UnSelectedList.Remove(payload);

            SelectedList.Add(payload);

            payload.VisualData.Bound.ClearBounds();

            var iterator = SelectedState.Transmit(payload);
            while (iterator.MoveNext())
                yield return null;

            if (sendNormal)
            {
                iterator = SendUnselected();
                while (iterator.MoveNext())
                    yield return null;
            }

            iterator = TransmitMultiStates();
            while (iterator.MoveNext())
                yield return null;
        }

        public IEnumerator Deselect(VisualPayload payload)
        {
            SelectedList.Remove(payload);

            UnSelectedList.Add(payload);

            payload.VisualData.Bound.ClearBounds();

            IEnumerator iterator;

            if (!SelectedList.Any())
            {
                iterator = SendNoneSelected();
                while (iterator.MoveNext())
                    yield return null;
            }
            else
            {
                iterator = NormalState.Transmit(payload);
                while (iterator.MoveNext())
                    yield return null;
            }
            
            iterator = TransmitMultiStates();
            while (iterator.MoveNext())
                yield return null;
        }

        public IEnumerator Select(IEnumerable<VisualPayload> payloads)
        {
            if (!payloads.Any())
            {
                yield return null;
                yield break;
            }

            bool sendNormal = !SelectedList.Any();

            IEnumerator iterator;

            foreach (var payload in payloads)
            {
                if (SelectedList.Contains(payload))
                    continue;

                UnSelectedList.Remove(payload);
                SelectedList.Add(payload);

                payload.VisualData.Bound.ClearBounds();

                iterator = SelectedState.Transmit(payload);
                while (iterator.MoveNext())
                    yield return null;
            }

            if (sendNormal)
            {
                iterator = SendUnselected();
                while (iterator.MoveNext())
                    yield return null;
            }

            iterator = TransmitMultiStates();
            while (iterator.MoveNext())
                yield return null;
        }

        public IEnumerator Deselect(IEnumerable<VisualPayload> payloads)
        {
            if (!payloads.Any())
            {
                yield return null;
                yield break;
            }

            IEnumerator iterator;

            foreach (var payload in payloads)
            {
                if (UnSelectedList.Contains(payload))
                    continue;

                SelectedList.Remove(payload);
                UnSelectedList.Add(payload);
            }

            //var newNodeState = SelectedList.Count == 0 ? NoneSelectedState : NormalState;

            if (SelectedList.Any())
            {
                foreach (var payload in payloads)
                {
                    payload.VisualData.Bound.ClearBounds();

                    iterator = NormalState.Transmit(payload);
                    while (iterator.MoveNext())
                        yield return null;
                }
            }
            else
            {
                iterator = SendNoneSelected();
                while (iterator.MoveNext())
                    yield return null;
            }


            iterator = TransmitMultiStates();
            while (iterator.MoveNext())
                yield return null;
        }

        public IEnumerator TransmitAll()
        {
            IEnumerator iterator;

            if (SelectedList.Count > 0)
                foreach (var payload in UnSelectedList)
                {
                    payload.VisualData.Bound.ClearBounds();

                    iterator = NormalState.Transmit(payload);
                    while (iterator.MoveNext())
                        yield return null;
                }
            else
                foreach (var payload in UnSelectedList)
                {
                    payload.VisualData.Bound.ClearBounds();

                    iterator = NoneSelectedState.Transmit(payload);
                    while (iterator.MoveNext())
                        yield return null;
                }

            foreach (var payload in SelectedList)
            {
                payload.VisualData.Bound.ClearBounds();

                iterator = SelectedState.Transmit(payload);
                while (iterator.MoveNext())
                    yield return null;
            }

            iterator = TransmitMultiStates();
            while (iterator.MoveNext())
                yield return null;
        }

        private IEnumerator TransmitMultiStates()
        {
            IEnumerator iterator;

            UnSelectedMutable.VisualData.Bound.ClearBounds();

            SelectedMutable.VisualData.Bound.ClearBounds();

            NoneSelectedMutable.VisualData.Bound.ClearBounds();

            if (SelectedList.Count == 0)
            {
                iterator = NoneSelectedStateMulti.Transmit(NoneSelectedMutable);
                while (iterator.MoveNext())
                    yield return null;
            }
            //NoneSelectedMultiOn = SelectedList.Count == 0;


            if (SelectedList.Count > 0)
            {
                iterator = NormalStateMulti.Transmit(UnSelectedMutable);
                while (iterator.MoveNext())
                    yield return null;
            }
            //NormalSelectedMultiOn = SelectedList.Count > 0;


            iterator = SelectedStateMulti.Transmit(SelectedMutable);
            while (iterator.MoveNext())
                yield return null;
        }

        public IEnumerator SelectAll()
        {
            IEnumerator iterator;

            foreach (var payload in UnSelectedList)
            {
                payload.VisualData.Bound.ClearBounds();

                iterator = SelectedState.Transmit(payload);
                while (iterator.MoveNext())
                    yield return null;
            }

            SelectedList.AddRange(UnSelectedList);
            UnSelectedList.Clear();

            iterator = TransmitMultiStates();
            while (iterator.MoveNext())
                yield return null;
        }


        public IEnumerator DeselectAll()
        {
            IEnumerator iterator;

            UnSelectedList.AddRange(SelectedList);
            SelectedList.Clear();

            iterator = SendNoneSelected();
            while (iterator.MoveNext())
                yield return null;

            iterator = TransmitMultiStates();
            while (iterator.MoveNext())
                yield return null;
        }

        public IEnumerator ToggleFullySelected(IEnumerable<VisualPayload> payloads)
        {
            IEnumerator iterator;

            if (payloads.All(payload => SelectedList.Contains(payload)))
            {
                iterator = Deselect(payloads);
                while (iterator.MoveNext())
                    yield return null;
            }
            else
            {
                iterator = Select(payloads);
                while (iterator.MoveNext())
                    yield return null;
            }
        }

        public IEnumerator SelectOnly(IEnumerable<VisualPayload> payloads)
        {
            IEnumerator iterator;

            UnSelectedList.AddRange(SelectedList);
            SelectedList.Clear();
            
            foreach (var payload in payloads)
            {
                UnSelectedList.Remove(payload);
                SelectedList.Add(payload);

                payload.VisualData.Bound.ClearBounds();

                iterator = SelectedState.Transmit(payload);
                while (iterator.MoveNext())
                    yield return null;
            }

            iterator = SendUnselected();
            while (iterator.MoveNext())
                yield return null;

            iterator = TransmitMultiStates();
            while (iterator.MoveNext())
                yield return null;
        }

        #endregion

        // default utilization of a PayloadSelectionListManager

    }
}
