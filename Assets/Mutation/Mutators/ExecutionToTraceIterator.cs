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
using Adapters.TraceAdapters.Traces;
using Chains;
using Visualizers;

namespace Mutation.Mutators
{
    public class ExecutionToTraceIterator : Mutator
    {
        public SelectionState TraceState { get { return Router["Trace State"]; } }

        private MutableField<IEnumerable<MutableObject>> m_TraceList
            = new MutableField<IEnumerable<MutableObject>>()
        {AbsoluteKey = "Execution.Traces" };
        [Controllable(LabelText = "Trace List")]
        public MutableField<IEnumerable<MutableObject>> TraceList
        {
            get { return m_TraceList; }
        }

        private MutableField<MutableObject> m_Header = new MutableField<MutableObject>()
        {AbsoluteKey = "Execution.Header" };
        [Controllable(LabelText = "Header")]
        public MutableField<MutableObject> Header
        {
            get { return m_Header; }
        }

        private MutableTarget m_ExecutionHeaderTarget = new MutableTarget() 
        { AbsoluteKey = "Execution.Traces.Execution Header" };
        [Controllable(LabelText = "Execution Header Target")]
        public MutableTarget ExecutionHeaderTarget { get { return m_ExecutionHeaderTarget; } }

        public ExecutionToTraceIterator() : base()
        {
            Router.AddSelectionState("Trace State");

            ExecutionHeaderTarget.SchemaParent = TraceList;
        }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var headerMutable = Header.GetFirstValue(payload.Data);

            IEnumerator iterator;

            foreach ( var entry in TraceList.GetEntries( payload.Data ) )
            {
                ExecutionHeaderTarget.SetValue( headerMutable, entry );

                foreach ( var traceMutable in TraceList.GetValue( entry ) )
                {
                    var traceVisualDescription = new VisualDescription(payload.VisualData.Bound.CreateDependingBound(Name));

                    VisualPayload tracePayload = new VisualPayload(traceMutable, traceVisualDescription);

                    iterator = TraceState.Transmit(tracePayload);
                    while (iterator.MoveNext())
                        yield return null;
                }
            }
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            var headerMutable = Header.GetFirstValue(newSchema);
            
            foreach ( var entry in TraceList.GetEntries( newSchema ) )
            {
                ExecutionHeaderTarget.SetValue( headerMutable, entry );

                foreach (var traceSchema in TraceList.GetValue(entry) )
                    TraceState.TransmitSchema(traceSchema);
            }

            DefaultState.TransmitSchema(newSchema);
        }
    }
}
