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
using Mutation;
using Utility.Undo;
using Visualizers;

namespace Adapters.GlobalParameters
{
    public class WriteGlobalValueNode : ChainNode
    {
        private static int WriterIndex = 0;
        private string WriterKey { get; set; }

        private MutableField<object> m_Parameter = new MutableField<object>() { AbsoluteKey = "Parameter" };
        [Controllable(LabelText = "Input Value")]
        public MutableField<object> Parameter { get { return m_Parameter; } }

        private MutableField<string> m_ParameterName = new MutableField<string>() { LiteralValue = "Param" };
        [Controllable(LabelText = "Global Value Name")]
        public MutableField<string> ParameterName { get { return m_ParameterName; } }

        private MutableObject m_WrittenParameters;
        private MutableObject WrittenParameters {
            get { return m_WrittenParameters; }
            set { m_WrittenParameters = value; }
        }

        
        
        private string PassThroughSelectionStateKey { get { return "Pass Through"; } }

        public SelectionState PassThroughState { get { return Router[ PassThroughSelectionStateKey ]; } }

        public WriteGlobalValueNode()
        {
            Router.AddSelectionState( PassThroughSelectionStateKey );

            WriterKey = "Parameter " + ++WriterIndex;
            WrittenParameters = new MutableObject();

            GlobalVariableDataStore.Instance.WriteToDataStore(WriterKey , WrittenParameters);
        }


        public override NodeDelete PrepareForDestruction(bool recurse)
        {
            GlobalVariableDataStore.Instance.RemoveFromDataStore(WriterKey);

            return base.PrepareForDestruction(recurse);
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            WriteParameter( payload.Data );

            var transmitIterator = Router.TransmitAll( payload );
            while ( transmitIterator.MoveNext() )
                yield return null;

            yield return null;
        }


        private void WriteParameter( MutableObject mutable )
        {
            var foundValue = Parameter.GetFirstValue(mutable);

            WrittenParameters[ ParameterName.GetFirstValue( mutable ) ] = foundValue;

            GlobalVariableDataStore.Instance.WriteToDataStore(WriterKey, WrittenParameters);
        }

        private void WriteSchema(MutableObject mutable)
        {
            WrittenParameters = new MutableObject();

            WriteParameter(mutable);

            GlobalVariableDataStore.Instance.WriteToDataStore(WriterKey, WrittenParameters);

        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            WriteSchema(newSchema);

            Router.TransmitAllSchema( newSchema );
        }

        public override void Unload()
        {
            GlobalVariableDataStore.Instance.RemoveFromDataStore(WriterKey);

            base.Unload();
        }
    }
}
