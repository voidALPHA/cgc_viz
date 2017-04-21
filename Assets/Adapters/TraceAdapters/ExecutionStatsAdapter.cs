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
using Adapters.TraceAdapters.Commands;
using Adapters.TraceAdapters.Traces;
using Chains;
using Mutation;
using UnityEngine;
using Visualizers;

namespace Adapters.TraceAdapters
{
    public class ExecutionStatsAdapter : Adapter
    {
        public SelectionState DefaultState { get { return Router["Default"]; } }

        public ExecutionStatsAdapter()
        {
            Router.AddSelectionState("Default");
        }

        private bool UseCachedResults { get; set; }
        private GetExecutionIdCommand PriorExecutionIdCommand { get; set; }

        private ServiceIdentifier m_ServiceId = new ServiceIdentifier() { Id = 8 };
        private ServiceIdentifier ServiceId { get { return m_ServiceId; } set { m_ServiceId = value; } }

        //private uint m_RequestId = 2160;
        //private uint RequestId { get { return m_RequestId; } set { m_RequestId = value; } }

        private uint FoundRequestId { get; set; }
        private MutableField<int> m_RequestId = new MutableField<int>()
        { LiteralValue = 2160 };
        [Controllable(LabelText = "RequestId")]
        public MutableField<int> RequestId
        {
            get { return m_RequestId; }
        }

        private MutableField<RequestNature> m_RequestNature = new MutableField<RequestNature>()
        { LiteralValue = TraceAdapters.Traces.RequestNature.Pov };
        [Controllable(LabelText = "RequestNature")]
        public MutableField<RequestNature> RequestNature { get { return m_RequestNature; } }


        private uint m_FoundExecutionId;
        private uint FoundExecutionId { get { return m_FoundExecutionId; } set { m_FoundExecutionId = value; } }

        private bool CsSuccess { get; set; }

        private int PovType { get; set; }


        private MutableField<int> m_BinaryIdNumber = new MutableField<int>()
        { LiteralValue = 131 };
        [Controllable(LabelText = "BinaryIdNumber")]
        public MutableField<int> BinaryIdNumber
        {
            get { return m_BinaryIdNumber; }
        }


        private MutableField<int> m_ExecutionId = new MutableField<int>()
        { LiteralValue = -1 };
        [Controllable(LabelText = "ExecutionId")]
        public MutableField<int> ExecutionId { get { return m_ExecutionId; } }


        private MutableField<int> m_IdsId = new MutableField<int>()
        { LiteralValue = -1 };
        [Controllable(LabelText = "IDS Id")]
        public MutableField<int> IdsId { get { return m_IdsId; } }


        private MutableTarget m_ExecutionStatsTarget = new MutableTarget()
        { AbsoluteKey = "Execution" };
        [Controllable(LabelText = "Execution Stats Target")]
        public MutableTarget ExecutionStatsTarget { get { return m_ExecutionStatsTarget; } }


        private BinaryIdentifier m_Binary = new BinaryIdentifier(131, "");
        private BinaryIdentifier BinaryId { get { return m_Binary; } set { m_Binary = value; } }

        private IEnumerator UpdateExecutionId(MutableObject mutable)
        {
            BinaryId = new BinaryIdentifier((uint)BinaryIdNumber.GetFirstValue(mutable), "Unknown");

            FoundRequestId = (uint)RequestId.GetFirstValue(mutable);

            var idsId = IdsId.GetFirstValue(mutable);

            if (ServiceId.Id == 0 ||
                BinaryId == null ||
                 FoundRequestId == 0)
            {
                FoundExecutionId = 0;

                Debug.LogError("Incomplete selection: binary " + (BinaryId == null ? "null" : ("" + BinaryId)) + ", request " + FoundRequestId);

                yield break;
            }

            var requestNature = RequestNature.GetFirstValue(mutable);
            
            var executionCommand = new GetExecutionIdCommand(requestNature,
                FoundRequestId, BinaryId.Id, idsId);

            PriorExecutionIdCommand = executionCommand;

            Debug.Log("Executing command " + executionCommand.RelativeUrl);

            var iterator = CommandProcessor.Execute(executionCommand);
            while (iterator.MoveNext())
                yield return null;

            //Debug.LogFormat( "Execution id is {0}", executionIdCommand.ExecutionId );

            if (!executionCommand.Ok)
            {
                FoundExecutionId = 0;

                yield break;
            }

            CsSuccess = executionCommand.CsSuccess;

            PovType = executionCommand.PovType;

            FoundExecutionId = executionCommand.ExecutionId;

            
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {

            var iterator = UpdateExecutionId(payload.Data);
            while (iterator.MoveNext())
                yield return null;

            var executionPerformanceCommand = new GetExecutionPerformanceCommand(FoundExecutionId);

            iterator = CommandProcessor.Execute(executionPerformanceCommand);
            while (iterator.MoveNext())
                yield return null;

            var statsMutable = ConstructStatsMutable( executionPerformanceCommand.InstructionCount );

            ExecutionStatsTarget.SetValue( statsMutable, payload.Data );

            iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }

        private MutableObject ConstructStatsMutable(ulong instructionCount)
        {
            var statsMutable = new MutableObject();

            statsMutable.Add("Instruction Count",
                instructionCount);
            
            return statsMutable;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            ExecutionStatsTarget.SetValue( 
                ConstructStatsMutable( 1000L ), newSchema);

            Router.TransmitAllSchema( newSchema );
        }
    }
}
