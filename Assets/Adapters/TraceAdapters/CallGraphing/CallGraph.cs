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
using System.Collections.Generic;
using Adapters.TraceAdapters.Instructions;
using Adapters.TraceAdapters.Traces.Elements;

namespace Adapters.TraceAdapters.CallGraphing
{
    public class CallGraph
    {
        public Dictionary< uint, FunctionDescriptor > FunctionDescriptorsByAddress { get; set; }

        public FunctionCall EntryCall { get; set; }

        public List<AnnotationElement> Annotations { get; set; }

        public CallGraph()
        {
            FunctionDescriptorsByAddress = new Dictionary <uint, FunctionDescriptor >();
            InstructionDescriptorsByAddress = new Dictionary<uint, InstructionDescriptor>();
        }


        #region Recording Stuff

        public bool IsRecording { get; private set; }

        public bool HasRecorded { get; private set; }

        private Stack < FunctionCall > CallStack { get; set; }
        

        public Dictionary< uint, InstructionDescriptor > InstructionDescriptorsByAddress { get; set; }

        public List<InstructionCall> InstructionCalls { get; set; }


        public void BeginRecording()
        {
            if ( HasRecorded )
                throw new InvalidOperationException("Cannot record more than once.");

            IsRecording = true;

            CallStack = new Stack<FunctionCall>();

            InstructionCalls = new List<InstructionCall>();

            Annotations = new List<AnnotationElement>();
        }

        public void EndRecording(double endCycle)
        {
            if ( !IsRecording )
                throw new InvalidOperationException("Cannot end recording if recording hasn't been started.");

            // close all outstanding functions
            foreach (var function in CallStack)
            {
                function.Return(endCycle);
            }

            IsRecording = false;

            HasRecorded = true;
        }

        public void RecordInstructionCall(InstInstruction instruction)
        {
            AssertRecordingMode();

            var instructionDescriptor = new InstructionDescriptor(instruction.Eip, instruction.Instruction);

            RegisterInstructionDescriptor(instructionDescriptor);

            var newInstructionCall = new InstructionCall(instructionDescriptor, instruction.CpuTime);

            InstructionCalls.Add(newInstructionCall);
        }

        public void RecordFunctionCall( uint functionAddress, double clockTime )
        {
            AssertRecordingMode();

            var functionDescriptor = new FunctionDescriptor( functionAddress );

            RegisterFunctionDescriptor(functionDescriptor);

            var newFunctionCall = new FunctionCall(functionDescriptor, clockTime);

            if ( EntryCall == null )
                EntryCall = newFunctionCall;

            if ( CallStack.Count > 0)
                CallStack.Peek().CallFunction(newFunctionCall);

            CallStack.Push( newFunctionCall );
        }

        public void RecordFunctionReturn( double clockTime )
        {
            AssertRecordingMode();

            if ( CallStack.Count == 0 )
                throw new InvalidOperationException("Could not record a return without a currently recording function.");
            
            if ( CallStack.Count > 0 )
                CallStack.Pop().Return( clockTime );
        }

        public void RecordAnnotation(AnnotationElement annotation)
        {
            Annotations.Add(annotation);
        }

        private void AssertRecordingMode()
        {
            if ( !IsRecording )
                throw new InvalidOperationException( "Not in recording mode." );
        }

        private void RegisterFunctionDescriptor( FunctionDescriptor descriptor )
        {
            if ( FunctionDescriptorsByAddress.ContainsKey( descriptor.Address ) )
                return;

            FunctionDescriptorsByAddress.Add( descriptor.Address, descriptor );
        }

        private void RegisterInstructionDescriptor( InstructionDescriptor descriptor )
        {
            if (InstructionDescriptorsByAddress.ContainsKey( descriptor.Eip ) )
                return;

            InstructionDescriptorsByAddress.Add( descriptor.Eip, descriptor );
        }

        #endregion

    }
}
