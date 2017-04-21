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
using Adapters.TraceAdapters.Commands;
using Adapters.TraceAdapters.Traces;
using Adapters.TraceAdapters.Traces.Elements;
using Chains;
using Mutation;
using Scripts.Utility.Misc;
using Ui.MainMenu;
using UnityEngine;
using Utility;
using Utility.DevCommand;
using Utility.Misc;
using Visualizers;

namespace Adapters.TraceAdapters
{
    public class ExecutionAdapter : Adapter
    {
        //private MutableField<string> m_TraceName = new MutableField<string>() 
        //{ LiteralValue = "TestTrace" };
        //[Controllable(LabelText = "TraceName")]
        //public MutableField<string> TraceName { get { return m_TraceName; } }

        
        public SelectionState DefaultState { get { return Router["Default"]; } }

        public ExecutionAdapter()
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
        {LiteralValue = 2160};
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
        {LiteralValue = 131};
        [Controllable(LabelText = "BinaryIdNumber")]
        public MutableField<int> BinaryIdNumber
        {
            get { return m_BinaryIdNumber; }
        }
        

        private MutableField<int> m_ExecutionId = new MutableField<int>() 
        { LiteralValue = -1 };
        [Controllable(LabelText = "ExecutionId")]
        public MutableField<int> ExecutionId { get { return m_ExecutionId; } }



        //private MutableField<bool> m_IncludeITrace = new MutableField<bool>() 
        //{ LiteralValue = true };
        //[Controllable(LabelText = "Include ITrace")]
        //public MutableField<bool> IncludeITrace { get { return m_IncludeITrace; } }

        //private MutableField<bool> m_IncludeRegs = new MutableField<bool>() 
        //{ LiteralValue = true };
        //[Controllable(LabelText = "Include Regs")]
        //public MutableField<bool> IncludeRegs { get { return m_IncludeRegs; } }


        //private MutableField<bool> m_IncludeDisasm = new MutableField<bool>() 
        //{ LiteralValue = true };
        //[Controllable(LabelText = "Include Disasm")]
        //public MutableField<bool> IncludeDisasm { get { return m_IncludeDisasm; } }
        
        private MutableField<bool> m_IncludeMemtrace = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "Include Memtrace")]
        public MutableField<bool> IncludeMemtrace { get { return m_IncludeMemtrace; } }


        private MutableField<bool> m_AllowCachedResults = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "Use Cached Results")]
        public MutableField<bool> AllowCachedResults { get { return m_AllowCachedResults; } }

        

        private MutableTarget m_ExecutionTarget = new MutableTarget() 
        { AbsoluteKey = "Execution" };
        [Controllable(LabelText = "Execution Target")]
        public MutableTarget ExecutionTarget { get { return m_ExecutionTarget; } }
        
        private MutableField<bool> m_SpoofExecution = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "Spoof Execution")]
        public MutableField<bool> SpoofExecution { get { return m_SpoofExecution; } }
        
        private MutableField<int> m_Skip = new MutableField<int>() 
        { LiteralValue = -1 };
        [Controllable(LabelText = "Skip Instructions")]
        public MutableField<int> Skip { get { return m_Skip; } }
        
        private MutableField<int> m_Max = new MutableField<int>() 
        { LiteralValue = -1 };
        [Controllable(LabelText = "Max Instructions")]
        public MutableField<int> Max { get { return m_Max; } }
        
        private MutableField<int> m_IdsId = new MutableField<int>() 
        { LiteralValue = -1 };
        [Controllable(LabelText = "IDS Id")]
        public MutableField<int> IdsId { get { return m_IdsId; } }
        

        private BinaryIdentifier m_Binary = new BinaryIdentifier(131, "");
        private BinaryIdentifier BinaryId { get { return m_Binary; } set { m_Binary = value; }}
        

        private IEnumerator UpdateExecutionId(MutableObject mutable)
        {
            BinaryId = new BinaryIdentifier((uint)BinaryIdNumber.GetFirstValue(mutable), "Unknown");

            FoundRequestId = (uint)RequestId.GetFirstValue(mutable);

            var idsId = IdsId.GetFirstValue( mutable );

            if (ServiceId.Id == 0 ||
                BinaryId == null ||
                 FoundRequestId == 0)
            {
                FoundExecutionId = 0;

                Debug.LogError("Incomplete selection: binary "+ (BinaryId==null?"null":(""+BinaryId)) +", request " + FoundRequestId);

                yield break;
            }

            var requestNature = RequestNature.GetFirstValue( mutable );

            if ( PriorExecutionIdCommand!=null 
                 && PriorExecutionIdCommand.RequestNature == requestNature
                 && PriorExecutionIdCommand.RequestId == FoundRequestId
                 && PriorExecutionIdCommand.BinaryId == BinaryId.Id
                 && PriorExecutionIdCommand.Ok
                 && AllowCachedResults.GetFirstValue( mutable )
                 )
            {
                Debug.Log( "Using cached trace results.");
                UseCachedResults = true;
                yield break;
            }

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


        

        private List<string> TraceFileNames { get; set; }

        private IEnumerator GetTraceFilenames(uint executionId, MutableObject mutable)
        {
            var postArgs = GeneratePostArgs( mutable );

            var traceFilenamesCommand = new GetVariableTraceFilenamesCommand(executionId, postArgs);

            Debug.Log("Executing command " + traceFilenamesCommand.RelativeUrl + " with postargs: " + traceFilenamesCommand.PostString);

            var iterator = CommandProcessor.Execute(traceFilenamesCommand);
            while (iterator.MoveNext())
                yield return null;

            TraceFileNames = traceFilenamesCommand.FileNames ?? new List< string >();

            foreach (var name in TraceFileNames)
                Debug.LogFormat("Found trace filename: {0}", name);

        }

        private string GeneratePostArgs( MutableObject mutable )
        {
            var postArgs = "{ \"config\" : [ \"";

            postArgs += "itrace,regs,disasm";
            //postArgs += IncludeITrace.GetFirstValue(mutable) ? "itrace," : "";
            //postArgs += IncludeRegs.GetFirstValue(mutable) ? "regs," : "";
            //postArgs += IncludeDisasm.GetFirstValue(mutable) ? "disasm," : "";
            //postArgs = postArgs.TrimEnd(new char[] { ',' });
            //postArgs += "\"";
            postArgs += IncludeMemtrace.GetFirstValue( mutable ) ? "\", \"memtrace" : "";

            var skipCount = Skip.GetFirstValue( mutable );
            var maxCount = Max.GetFirstValue( mutable );
            if ( skipCount != -1 )
                postArgs += "\", \"skip," + skipCount;
            if ( maxCount != -1 )
                postArgs += "\", \"max," + maxCount;
            
            postArgs += "\" ] }";
            return postArgs;
        }


        private IEnumerator GetTracesForCurrentSelections(MutableObject mutable)
        {
            if (FoundExecutionId == 0)
            {
                Debug.LogError("Execution not available for this pair. Aborting.");
            }

            IEnumerator iterator;


            iterator = GetTraceFilenames(FoundExecutionId, mutable);
            while (iterator.MoveNext( ))
                yield return null;

            iterator = CheckInstructionLimits();
            while ( iterator.MoveNext() )
                yield return null;

            if ( InstructionLimitExceeded )
            {
                // Uh, what should I do here?

                Debug.LogErrorFormat( "Instruction limit exceeded. Breaking from Getting Traces for Current Selections in Execution Adapter." );

                yield break;
            }

            iterator = GetTracesFromApiFilenames();
            while (iterator.MoveNext())
                yield return null;

            Debug.Log("Done getting traces. Count: " + Traces.Count);

            foreach (var t in Traces)
            {
                Debug.Log(" Trace has instruction count: " + t.Elements.Count);
            }
        }

        private IEnumerator ParseExecution(BoundingBox originalBound, MutableObject mutable)
        {
            VisualPayload outPayload = new VisualPayload(mutable,
                new VisualDescription(originalBound.CreateDependingBound(Name)));
            yield return null;

            var iterator = Router.TransmitAll(outPayload);
            while (iterator.MoveNext())
                yield return null;
        }

        private bool InstructionLimitExceeded { get; set; }

        private IEnumerator CheckInstructionLimits()
        {
            InstructionLimitExceeded = false;

            yield break;

            var executionPerformanceCommand = new GetExecutionPerformanceCommand( FoundExecutionId );

            var iterator = CommandProcessor.Execute( executionPerformanceCommand );
            while ( iterator.MoveNext() )
                yield return null;
            
            var instructionCount = (int)executionPerformanceCommand.InstructionCount;

            if ( instructionCount > HaxxisGlobalSettings.Instance.InstructionLimit )
            {
                InstructionLimitExceeded = true;

                var message = string.Format( "Instruction count exceeded ({0}/{1})", instructionCount, HaxxisGlobalSettings.Instance.InstructionLimit );
                
                HaxxisGlobalSettings.Instance.ReportVgsError( 11, message );

                throw new InvalidOperationException(message);
            }
        }

        private List<Trace> Traces { get; set; }

        private IEnumerator GetTracesFromApiFilenames()
        {
            Traces = new List<Trace>();

            foreach (var filename in TraceFileNames)
            {
                var getTraceCommand = new GetTraceCommand(filename);

                Debug.Log( "Executing command " + getTraceCommand.RelativeUrl );

                var iterator = CommandProcessor.Execute(getTraceCommand);
                while (iterator.MoveNext( ))
                    yield return null;

                var traceRetrieved = getTraceCommand.RelativeUrl == null;

                var traceRetrievedString = traceRetrieved ? "null" : "non-null";
                Debug.Log("Trace retrieval: " + traceRetrievedString);

                Traces.Add(getTraceCommand.Trace);
            }
        }


        #region Mutable Stuff

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            IEnumerator iterator;

            if ( SpoofExecution.GetFirstValue( payload.Data ) )
            {
                ExecutionTarget.SetValue( SpoofTrace( 10000, 1 ), payload.Data );

                iterator = ParseExecution( payload.VisualData.Bound, payload.Data );
                while ( iterator.MoveNext() )
                    yield return null;
            }
            else
            {

                UseCachedResults = false;

                bool isMemoryTrace = IncludeMemtrace.GetFirstValue( payload.Data );

                var execId = ExecutionId.GetFirstValue( payload.Data );
                if ( execId < 0 )
                {
                    iterator = UpdateExecutionId( payload.Data );
                    while ( iterator.MoveNext() )
                        yield return null;
                }
                else
                {
                    FoundExecutionId = (uint)execId;
                }

                if ( !UseCachedResults )
                {
                    iterator = GetTracesForCurrentSelections( payload.Data );
                    while ( iterator.MoveNext() )
                        yield return null;
                }

                var execution = new Execution()
                {
                    ServiceId = ServiceId,
                    CbId = BinaryId.Id,
                    CbAuthor = BinaryId.Author,
                    RequestId = FoundRequestId,
                    RequestNature = RequestNature.GetFirstValue( payload.Data ),
                    ExecutionId = FoundExecutionId,
                    Success = CsSuccess,
                    Traces = Traces,
                    PovType = PovType
                };


                ExecutionTarget.SetValue( ExecutionToMutable( execution, isMemoryTrace, PovType ), payload.Data );

                iterator = ParseExecution( payload.VisualData.Bound, payload.Data );
                while ( iterator.MoveNext() )
                    yield return null;
            }
        }
        
        private MutableObject SpoofTrace(int instructionCount, int binaryCount)
        {

            MutableObject executionMutable = new MutableObject();

            var execHeader = new MutableObject();

            execHeader.Add("Author", "Author");
            execHeader.Add("CB ID", 1);
            execHeader.Add("Execution ID", 1);
            execHeader.Add("Request ID", 1);
            execHeader.Add("Request Nature", TraceAdapters.Traces.RequestNature.ServicePoll);

            MutableObject service = new MutableObject();
            service.Add("Service Name", "service name");
            service.Add("Service ID", 1);
            execHeader.Add("Service", service);

            execHeader.Add("Success", true);
            execHeader.Add( "PovType", 0 );

            executionMutable.Add("Header", execHeader);

            List<MutableObject> traceMutables = new List<MutableObject>();

            MutableObject traceMutable = new MutableObject();

            ///

            MutableObject header = new MutableObject();

            header.Add("Binary ID", 1);
            header.Add("Magic", 1);
            header.Add("Version", 1);
            header.Add("Flags", 1);

            traceMutable.Add("Header", header);

            ////
            var instructionsList = new List<MutableObject>();

            //var usedEips = new List<uint>();
            var uniqueInstructions = Mathf.FloorToInt( Mathf.Log( instructionCount, 1.1f ) );

            var annotationTypes = new[] { "transmit", "receive", "start1", "allocate", "end1", "free", "fdwait", "random", "end1"};
            

            for (int i = 0; i < instructionCount; i++)
            {
                var newInstruction = new MutableObject();

                var newEip = (uint)Mathf.FloorToInt(UnityEngine.Random.value * uniqueInstructions);
                newInstruction.Add("Eip", newEip);

                newInstruction.Add("Wall Time", .01f*instructionCount);
                newInstruction.Add("Raw Type Index", 1);
                newInstruction.Add("Length", 1);
                newInstruction.Add("Original Index", i);
                newInstruction.Add("Processed Index", i);

                var disasmInstruction = new InstructionDisasmElement( new TraceElementHeader() );
                disasmInstruction.Disassembly = "Disasm " +
                                             (uint)Mathf.FloorToInt( UnityEngine.Random.value * uniqueInstructions );
                newInstruction.Add("Trace Instruction", disasmInstruction);

                if (UnityEngine.Random.value>.975f)
                {
                    newInstruction.Add("Annotated", true);
                    string annotationType =
                        annotationTypes[ Mathf.FloorToInt( UnityEngine.Random.value * annotationTypes.Length ) ];
                    string annotationString = annotationType;
                    if ( string.Compare( annotationType, "allocate", StringComparison.InvariantCultureIgnoreCase ) == 0 )
                    {
                        annotationString += " "+ Mathf.FloorToInt( UnityEngine.Random.value * 10000 )
                                            + " bytes at 0x"
                                            + Mathf.FloorToInt( UnityEngine.Random.value * 10000000 ).ToString("x8");
                    }
                    if ( string.Compare( annotationType, "transmit", StringComparison.InvariantCultureIgnoreCase ) == 0 )
                    {
                        annotationString += " \"A transmit number " + UnityEngine.Random.value + " \"";
                    }
                    if (string.Compare(annotationType, "receive", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        annotationString += " \"A receive number " + UnityEngine.Random.value + " \"";
                    }

                    newInstruction.Add("Annotation Event Type", annotationType);
                    newInstruction.Add("Annotation Text", annotationString);
                }
                else
                {
                    newInstruction.Add("Annotated", false);
                }

                instructionsList.Add(newInstruction);
            }

            traceMutable.Add("Instructions", instructionsList);
            traceMutable.Add("InstructionCount", instructionsList.Count);
            ////


            // set up memory reads and writes
            var memReads = new List<MutableObject>()
            {
                new MutableObject()
                {
                    { "Read Address", (uint)1500 },
                    { "Read Hex", "data" },
                    { "Read Size", (int)4 },
                    { "Original Index", (int)5 },
                    { "Processed Index", (int)5 }
                }
            };
            traceMutable.Add("Memory Reads", memReads);

            var memWrites = new List<MutableObject>()
            {
                new MutableObject()
                {
                    { "Write Address", (uint)1500 },
                    { "Write Hex", "data" },
                    { "Write Size", (int)4 },
                    { "Original Index", (int)5 },
                    { "Processed Index", (int)5 }
                }
            };
            traceMutable.Add("Memory Writes", memWrites);

            traceMutables.Add(traceMutable);

            executionMutable.Add("Traces", traceMutables);

            return executionMutable;
        }

        public static MutableObject GenerateExecutionSchema()
        {
            #region Set up execution schema

            MutableObject executionMutable = new MutableObject();

            var execHeader = new MutableObject();

            execHeader.Add("Author", "Author");
            execHeader.Add("CB ID", 1);
            execHeader.Add("Execution ID", 1);
            execHeader.Add("Request ID", 1);
            execHeader.Add("Request Nature", TraceAdapters.Traces.RequestNature.ServicePoll);

            MutableObject service = new MutableObject();
            service.Add("Service Name", "service name");
            service.Add("Service ID", 1);
            execHeader.Add("Service", service);

            execHeader.Add("Success", true);
            execHeader.Add("PovType", 0);

            executionMutable.Add("Header", execHeader);

            List<MutableObject> traceMutables = new List<MutableObject>();

            MutableObject traceMutable = new MutableObject();

            ///

            MutableObject header = new MutableObject();

            header.Add("Binary ID", 1);
            header.Add("Magic", 1);
            header.Add("Version", 1);
            header.Add("Flags", 1);

            traceMutable.Add("Header", header);

            ////
            var instructionsList = new List<MutableObject>();

            for (int i = 0; i < 2; i++)
            {
                var newInstruction = new MutableObject();

                newInstruction.Add("Eip", (uint)1);

                newInstruction.Add("Wall Time", 1f);
                newInstruction.Add("Raw Type Index", 1);
                newInstruction.Add("Length", 1);
                newInstruction.Add("Original Index", 1);
                newInstruction.Add("Processed Index", 1);

                newInstruction.Add("Trace Instruction", new InstructionOnlyElement(new TraceElementHeader()));

                if (i % 2 == 1)
                {
                    newInstruction.Add("Annotated", true);
                    newInstruction.Add("Annotation Text", "Transmit");
                    newInstruction.Add("Annotation Event Type", "transmit");
                }
                else
                {
                    newInstruction.Add("Annotated", false);
                }

                instructionsList.Add(newInstruction);
            }

            traceMutable.Add("Instructions", instructionsList);
            traceMutable.Add("InstructionCount", instructionsList.Count);
            ////

            traceMutables.Add(traceMutable);

            // set up memory reads and writes
            //if ( IncludeMemtrace.GetFirstValue( newSchema ) )
            //{
            var memReads = new List<MutableObject>()
                {
                    new MutableObject()
                    {
                        { "Read Address", (uint)1500 },
                        { "Read Hex", "data" },
                        { "Read Size", (int)4 },
                        { "Original Index", (int)5 },
                        { "Processed Index", (int)5 }
                    }
                };
            traceMutable.Add("Memory Reads", memReads);

            var memWrites = new List<MutableObject>()
                {
                    new MutableObject()
                    {
                        { "Write Address", (uint)1500 },
                        { "Write Hex", "data" },
                        { "Write Size", (int)4 },
                        { "Original Index", (int)5 },
                        { "Processed Index", (int)5 }
                    }
                };
            traceMutable.Add("Memory Writes", memWrites);
            //}

            executionMutable.Add("Traces", traceMutables);

            #endregion

            return executionMutable;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {   
            ExecutionTarget.SetValue(
                GenerateExecutionSchema(), newSchema );

            //    ExecutionToMutable(new Execution()
            //{
            //    RequestNature = RequestNature.GetFirstValue( newSchema ),
            //    Traces = new List<Trace>(){Trace.GenerateEmptyTrace()},
            //    CbAuthor = "Author",
            //    ServiceId = new ServiceIdentifier() { Name = "Service Name"},
            //}), newSchema);

            Router.TransmitAllSchema(newSchema);
        }

        public static MutableObject ExecutionToMutable(Execution execution, bool isMemoryTrace, int povType)
        {
            MutableObject mutable = new MutableObject();

            ProcessHeader(execution, mutable, povType);

            List<MutableObject> traceMutables = new List<MutableObject>();

            foreach (var trace in execution.Traces)
            {
                MutableObject traceMutable = new MutableObject();

                ProcessAllTraceElements(trace, traceMutable, isMemoryTrace);

                traceMutables.Add(traceMutable);
            }

            mutable.Add("Traces", traceMutables);

            return mutable;
        }

        private static void ProcessTraceHeader(TraceHeader traceHeader, MutableObject traceMutable)
        {
            MutableObject header = new MutableObject();

            header.Add("Binary ID", (int)traceHeader.BinaryId);
            header.Add("Magic", (int)traceHeader.Magic);
            header.Add("Version", (int)traceHeader.Version);
            header.Add("Flags", (int)traceHeader.Flags);

            traceMutable.Add("Header", header);
        }

        // write the instructions of this trace into a list of mutables
        private static void ProcessAllTraceElements(Trace trace, MutableObject mutable, bool isMemoryTrace)
        {
            ProcessTraceHeader(trace.Header, mutable);

            ProcessTrace(trace, mutable, isMemoryTrace);
        }

        // Used for debug outputting comms to console...
        private Dictionary<int, string> Comms = new Dictionary<int, string>();

        // write instructions and disassembly based on the generated call graph
        private class FlaggedAnnotation
        {
            public bool Flagged { get; set; }
            public AnnotationElement Annotation { get; set; }
        }

        private static void ProcessTrace(Trace trace, MutableObject traceMutable, bool memTrace = false)
        {
            //trace.GenerateCallGraph();


            //Dictionary< int, FlaggedAnnotation > foundAnnotations = new Dictionary< int, FlaggedAnnotation >();

         //   foreach ( var annotationElement in trace.Annotations )
         //   {
         //       foundAnnotations.Add( (int)annotationElement.Header.InstructionCount, new FlaggedAnnotation { Flagged = false, Annotation = annotationElement } );
         //   }

            Dictionary<int, MutableObject> instructionMutables = new Dictionary<int, MutableObject>();

            Dictionary<int, string> comms = new Dictionary< int, string >();

            int instructionCount = 0;
            foreach ( var element in trace.Elements )
            {
                if ( instructionMutables.ContainsKey( element.Header.InstructionCount ) )
                    continue;

                var newInstruction = CreateInstructionMutable( element, instructionCount );

                instructionMutables.Add( element.Header.InstructionCount, newInstruction );

                instructionCount++;
            }

            int lateAddedAnnotationCount = 0;
            // Add any annotations that were not interleaved in the trace (announcer additions; crashes...)
            foreach ( var annotation in trace.Annotations)
                //foundAnnotations.Where( p => p.Value.Flagged == false ) )
            {
                    //instructionMutables.FirstOrDefault( i => (int)i["Original Index"] == (int)pair.Key );

                MutableObject foundInstructionMutable;

                if ( !instructionMutables.ContainsKey(annotation.Header.InstructionCount) )
                {
                    var lastElement = trace.Elements.Last();

                    var newHeader = new TraceElementHeader(
                        lastElement.Header.RawTypeIndex,
                        lastElement.Header.Length,
                        lastElement.Header.WallTime,
                        lastElement.Header.InstructionCount + 1 + lateAddedAnnotationCount++ );

                    var newElement = new InstructionOnlyElement( newHeader )
                    { Eip = lastElement.Eip };


                    foundInstructionMutable = CreateInstructionMutable( newElement, instructionCount++ );
                    //foundInstructionMutable[ "Annotated" ] = true;

                    instructionMutables.Add( newHeader.InstructionCount, foundInstructionMutable );
                }
                else
                {
                    foundInstructionMutable = instructionMutables[annotation.Header.InstructionCount];
                }

                SetAnnotationOnInstructionMutable( foundInstructionMutable, annotation, comms);
            }

            traceMutable.Add( "Instructions", instructionMutables.Values );

            traceMutable.Add( "InstructionCount", instructionMutables.Count );
            
            if ( memTrace )
            {
                var memReads = new List< MutableObject >();

                foreach ( var memRead in trace.MemoryReads )
                {
                    var newRead = new MutableObject()
                    {
                        { "Read Address", (uint)memRead.Addr },
                        { "Read Hex", StringExtensions.ByteArrayToString( memRead.BytesRead ) },
                        { "Read Size", memRead.NumberOfBytesRead },
                        { "Original Index", memRead.Header.InstructionCount }

                    };
                    // match original index to get processed index
                    if ( instructionMutables.ContainsKey( memRead.Header.InstructionCount ) )
                    {
                        newRead[ "Processed Index" ] =
                            instructionMutables[ memRead.Header.InstructionCount ][ "Processed Index" ];
                    }
                    else
                    {
                        newRead[ "Processed Index" ] = 0;
                    }

                    memReads.Add( newRead );
                }
                
                traceMutable.Add("Memory Reads", memReads);

                var memWrites = new List< MutableObject >();

                foreach (var memWrite in trace.MemoryWrites)
                {
                    var newWrite = new MutableObject()
                    {
                        { "Write Address", (uint)memWrite.Addr },
                        { "Write Hex", StringExtensions.ByteArrayToString( memWrite.BytesWritten ) },
                        { "Write Size", memWrite.NumberOfBytesWritten },
                        { "Original Index", memWrite.Header.InstructionCount }
                            //memWrite.Header.InstructionCount }
                    };
                    // match original index to get processed index
                    if (instructionMutables.ContainsKey(memWrite.Header.InstructionCount))
                    {
                        newWrite["Processed Index"] =
                            instructionMutables[memWrite.Header.InstructionCount]["Processed Index"];
                    }
                    else
                    {
                        newWrite["Processed Index"] = 0;
                    }

                    memWrites.Add( newWrite );
                };
                traceMutable.Add("Memory Writes", memWrites);
            }
            
            var sb = new StringBuilder("Comms:\n");

            foreach ( var c in comms )
            {
                sb.AppendLine( string.Format( "{0:D5}    {1}", c.Key, c.Value ) );
            }
            //Debug.Log( sb.ToString() );
        }

        private static MutableObject CreateInstructionMutable( InstructionElement element, int instructionCount )
        {
            var newInstruction = new MutableObject();

            newInstruction.Add( "Eip", element.Eip );

            newInstruction.Add( "Wall Time", (float)element.Header.WallTime );
            newInstruction.Add( "Raw Type Index", (int)element.Header.RawTypeIndex );
            newInstruction.Add( "Length", (int)element.Header.Length );
            newInstruction.Add( "Original Index", (int)element.Header.InstructionCount );
            newInstruction.Add( "Processed Index", instructionCount );

            newInstruction.Add( "Annotated", false );

            newInstruction.Add( "Trace Instruction", element );

            return newInstruction;
        }

        private static void SetAnnotationOnInstructionMutable( MutableObject instruction, AnnotationElement annotation, Dictionary<int, string> comms )
        {
            if ( annotation.ByteString.StartsWith( "Receive" ) || annotation.ByteString.StartsWith( "Transmit" ) )
            {
                comms.Add( (int)annotation.Header.InstructionCount, annotation.ByteString );
            }

            if ( instruction.ContainsKey( "AnnotationText" ) || instruction.ContainsKey( "Annotation Event Type" ))
                Debug.LogWarningFormat( "Overwriting the annotation on an instruction (index {0})...!", annotation.Header.InstructionCount );

            instruction[ "Annotation Text" ] = annotation.ByteString;
            instruction[ "Annotation Event Type" ] = annotation.EventType;
            instruction[ "Annotated" ] = true;
        }


        // construct the execution header as a mutable object, and write it into this mutable
        private static void ProcessHeader(Execution execution, MutableObject mutable, int povType)
        {
            var header = new MutableObject();

            header.Add("Author", execution.CbAuthor);
            header.Add("CB ID", (int)execution.CbId);
            header.Add("Execution ID", (int)execution.ExecutionId);
            header.Add("Request ID", (int)execution.RequestId);
            header.Add("Request Nature", execution.RequestNature);

            // Taking this out as it seems not to be populated...
            //MutableObject service = new MutableObject();
            //service.Add("Service Name", execution.ServiceId.Name ?? "Unnamed");
            //service.Add("Service ID", (int)execution.ServiceId.Id);
            //header.Add("Service", service);

            header.Add("Success", execution.Success);

            header.Add( "PovType", povType );

            mutable.Add("Header", header);
        }

        #endregion
    }
}
