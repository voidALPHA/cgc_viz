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
    public class EnumeratorDecombiner : ChainNode
    {
        private MutableField<IEnumerable<MutableObject>> m_EntriesList = new MutableField<IEnumerable<MutableObject>>() 
        { AbsoluteKey = "Entries" };
        [Controllable(LabelText = "EntriesList")]
        public MutableField<IEnumerable<MutableObject>> EntriesList { get { return m_EntriesList; } }
        
        private MutableTarget m_SingleEntryTarget = new MutableTarget() 
        { AbsoluteKey = "Element" };
        [Controllable(LabelText = "Single Element Target")]
        public MutableTarget SingleEntryTarget { get { return m_SingleEntryTarget; } }

        private MutableTarget m_NumberOfEntriesTarget = new MutableTarget()
        { AbsoluteKey = "Number Of Elements" };
        [Controllable(LabelText = "Number Of Elements Target")]
        public MutableTarget NumberOfEntriesTarget { get { return m_NumberOfEntriesTarget; } }

        public SelectionState PerEntryState { get { return Router["Per Element"]; } }



        public EnumeratorDecombiner()
        {
            Router.AddSelectionState("Per Element");
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            foreach ( var entry in EntriesList.GetEntries( newSchema ) )
            {
                var elementList = EntriesList.GetValue( entry ).ToList();
                var elementCount = elementList.Count;
                NumberOfEntriesTarget.SetValue( elementCount, entry );

                if (elementCount==0)
                    throw new Exception("Can't act on an empty list!");
                SingleEntryTarget.SetValue( elementList.First(), entry);
            }

            Router.TransmitAllSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach (var entry in EntriesList.GetEntries(payload.Data))
            {
                var elementList = EntriesList.GetValue(entry).ToList();
                var elementCount = elementList.Count;
                NumberOfEntriesTarget.SetValue(elementCount, entry);

                foreach ( var element in elementList )
                {
                    SingleEntryTarget.SetValue( element, entry );
                    var iterator = PerEntryState.Transmit( payload );
                    while (iterator.MoveNext())
                        yield return null;
                }
            }
        }
    }
}
