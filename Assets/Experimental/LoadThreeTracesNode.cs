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
    public class LoadThreeTracesNode : Adapter
    {
        private MutableField<int> m_Trace1BinaryIdField = new MutableField<int> { LiteralValue = 131 };
        [Controllable(LabelText = "Trace 1 Binary ID")]
        public MutableField<int> Trace1BinaryIdField
        {
            get { return m_Trace1BinaryIdField; }
        }

        private MutableField<int> m_Trace1RequestIdField = new MutableField<int> { LiteralValue = 2160 };
        [Controllable( LabelText = "Trace 1 Request ID")]
        public MutableField< int > Trace1RequestIdField
        {
            get { return m_Trace1RequestIdField; }
        }

        private MutableField<int> m_Trace2BinaryIdField = new MutableField<int> { LiteralValue = 131 };
        [Controllable(LabelText = "Trace 2 Binary ID")]
        public MutableField<int> Trace2BinaryIdField
        {
            get { return m_Trace2BinaryIdField; }
        }

        private MutableField<int> m_Trace2RequestIdField = new MutableField<int> { LiteralValue = 2160 };
        [Controllable( LabelText = "Trace 2 Request ID" )]
        public MutableField<int> Trace2RequestIdField
        {
            get { return m_Trace2RequestIdField; }
        }

        private MutableField<int> m_Trace3BinaryIdField = new MutableField<int> { LiteralValue = 131 };
        [Controllable(LabelText = "Trace 3 Binary ID")]
        public MutableField<int> Trace3BinaryIdField
        {
            get { return m_Trace3BinaryIdField; }
        }

        private MutableField<int> m_Trace3RequestIdField = new MutableField<int> { LiteralValue = 2160 };
        [Controllable( LabelText = "Trace 3 Request ID" )]
        public MutableField<int> Trace3RequestIdField
        {
            get { return m_Trace3RequestIdField; }
        }

        public SelectionState DefaultState { get { return Router["Default"]; } }

        public LoadThreeTracesNode()
        {
            Router.AddSelectionState( "Default" );
        }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var ids = new List<MutableObject>();

            var id1 = new MutableObject();
            var ignore = Trace1RequestIdField.GetFirstValue(payload.Data) == -1 ||
                         Trace1BinaryIdField.GetFirstValue(payload.Data) == -1;
            if (!ignore)
            {
                id1.Add("TraceRequestID", Trace1RequestIdField.GetFirstValue(payload.Data));
                id1.Add("TraceBinaryID", Trace1BinaryIdField.GetFirstValue(payload.Data));
                ids.Add(id1);
            }

            var id2 = new MutableObject();
            ignore = Trace2RequestIdField.GetFirstValue(payload.Data) == -1 ||
                         Trace2BinaryIdField.GetFirstValue(payload.Data) == -1;
            if (!ignore)
            {
                id2.Add("TraceRequestID", Trace2RequestIdField.GetFirstValue(payload.Data));
                id2.Add("TraceBinaryID", Trace2BinaryIdField.GetFirstValue(payload.Data));
                ids.Add(id2);
            }

            var id3 = new MutableObject();
            ignore = Trace3RequestIdField.GetFirstValue(payload.Data) == -1 ||
                         Trace3BinaryIdField.GetFirstValue(payload.Data) == -1;
            if (!ignore)
            {
                id3.Add("TraceRequestID", Trace3RequestIdField.GetFirstValue(payload.Data));
                id3.Add("TraceBinaryID", Trace3BinaryIdField.GetFirstValue(payload.Data));
                ids.Add(id3);
            }

            payload.Data.Add("Trace IDs", ids);

            var iterator = Router.TransmitAll(payload);
            while (iterator.MoveNext())
                yield return null;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            var ids = new List<MutableObject>();

            var id1 = new MutableObject();
            id1.Add("TraceRequestID", Trace1RequestIdField.GetFirstValue( newSchema ));
            id1.Add( "TraceBinaryID", Trace1BinaryIdField.GetFirstValue( newSchema ) );
            ids.Add(id1);

            var id2 = new MutableObject();
            id2.Add( "TraceRequestID", Trace2RequestIdField.GetFirstValue( newSchema ) );
            id2.Add( "TraceBinaryID", Trace2BinaryIdField.GetFirstValue( newSchema ) );
            ids.Add(id2);

            var id3 = new MutableObject();
            id3.Add( "TraceRequestID", Trace3RequestIdField.GetFirstValue( newSchema ) );
            id3.Add( "TraceBinaryID", Trace3BinaryIdField.GetFirstValue( newSchema ) );
            ids.Add(id3);

            newSchema.Add("Trace IDs", ids);

            Router.TransmitAllSchema(newSchema);
        }
    }
}
