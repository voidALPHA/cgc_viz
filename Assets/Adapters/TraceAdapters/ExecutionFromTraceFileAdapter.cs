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
using System.IO;
using Adapters.TraceAdapters.Traces;
using Chains;
using Mutation;
using UnityEngine;
using Visualizers;

namespace Adapters.TraceAdapters
{
    public class ExecutionFromTraceFileAdapter : Adapter
    {
        private MutableField<string> m_TraceFilename = new MutableField<string>() 
        { AbsoluteKey = "Trace Filename"};
        [Controllable(LabelText = "TraceFilename")]
        public MutableField<string> TraceFilename { get { return m_TraceFilename; } }
        
        private MutableField<string> m_ServiceName = new MutableField<string>() 
        { LiteralValue = "Service Name" };
        [Controllable(LabelText = "Service Name")]
        public MutableField<string> ServiceName { get { return m_ServiceName; } }
        
        private MutableField<string> m_CbAuthor = new MutableField<string>() 
        { LiteralValue = "CB Author" };
        [Controllable(LabelText = "CB Author")]
        public MutableField<string> CbAuthor { get { return m_CbAuthor; } }
        
        private MutableField<int> m_ExecutionId = new MutableField<int>() 
        { LiteralValue = -1 };
        [Controllable(LabelText = "ExecutionId")]
        public MutableField<int> ExecutionID { get { return m_ExecutionId; } }

        private MutableField<int> m_RequestId = new MutableField<int>() 
        { LiteralValue = -1 };
        [Controllable(LabelText = "RequestId")]
        public MutableField<int> RequestId { get { return m_RequestId; } }

        private MutableField<RequestNature> m_RequestNature = new MutableField<RequestNature>() 
        { LiteralValue = Traces.RequestNature.Pov };
        [Controllable(LabelText = "Request Nature")]
        public MutableField<RequestNature> RequestNature { get { return m_RequestNature; } }

        private MutableField<bool> m_Success = new MutableField<bool>() 
        { LiteralValue = true };
        [Controllable(LabelText = "Success")]
        public MutableField<bool> Success { get { return m_Success; } }

        private MutableField<int> m_PovType = new MutableField<int>() 
        { LiteralValue = 1 };
        [Controllable(LabelText = "PovType")]
        public MutableField<int> PovType { get { return m_PovType; } }

        private MutableField<bool> m_IncludeMemory = new MutableField<bool>() 
        { LiteralValue = true };
        [Controllable(LabelText = "Include Memory")]
        public MutableField<bool> IncludeMemory { get { return m_IncludeMemory; } }


        /*
        private MutableField<int> m_Skip = new MutableField<int>()
        { LiteralValue = -1 };
        [Controllable(LabelText = "Skip Instructions")]
        public MutableField<int> Skip { get { return m_Skip; } }

        private MutableField<int> m_Max = new MutableField<int>()
        { LiteralValue = -1 };
        [Controllable(LabelText = "Max Instructions")]
        public MutableField<int> Max { get { return m_Max; } }
        */


        private MutableTarget m_ExecutionTarget = new MutableTarget() 
        { AbsoluteKey = "Execution" };
        [Controllable(LabelText = "Execution Target")]
        public MutableTarget ExecutionTarget { get { return m_ExecutionTarget; } }



        public SelectionState DefaultState { get { return Router["Default"]; } }
        public ExecutionFromTraceFileAdapter()
        {
            Router.AddSelectionState( "Default" );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var filename = TraceFilename.GetFirstValue( payload.Data );
            
            var trace = Trace.LoadFromFile( filename );
            
            var newExecution = new Execution
            {
                CbAuthor = CbAuthor.GetFirstValue( payload.Data ),
                CbId = 0,
                ExecutionId = (uint)ExecutionID.GetFirstValue( payload.Data ),
                PovType = PovType.GetFirstValue( payload.Data ),
                RequestId = (uint)RequestId.GetFirstValue(payload.Data),
                ServiceId = new ServiceIdentifier(),
                RequestNature = RequestNature.GetFirstValue( payload.Data ),
                Success = Success.GetFirstValue( payload.Data ),
                Traces = new List<Trace> { trace }
            };

            var executionMutable = ExecutionAdapter.ExecutionToMutable( 
                newExecution, 
                IncludeMemory.GetFirstValue( payload.Data ), 
                PovType.GetFirstValue( payload.Data ) );

            ExecutionTarget.SetValue( executionMutable, payload.Data );

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            ExecutionTarget.SetValue(
                ExecutionAdapter.GenerateExecutionSchema(), newSchema);

            Router.TransmitAllSchema(newSchema);
        }
    }
}
