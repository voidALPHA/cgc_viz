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
using System.IO;
using System.Linq;
using System.Text;
using Adapters.TraceAdapters.CallGraphing;
using Adapters.TraceAdapters.Instructions;
using Adapters.TraceAdapters.Memory;
using Adapters.TraceAdapters.Traces.Elements;
using Newtonsoft.Json.Schema;
using UnityEngine;

namespace Adapters.TraceAdapters.Traces
{
    public enum RequestNature
    {
        ServicePoll,
        Pov
    }

    public class Trace
    {
        private const bool SpoofAnnotations = false;

        public TraceHeader Header { get; private set; }

        public List< InstructionElement > Elements { get; private set; }

        public List<AnnotationElement> Annotations { get; private set; }

        public List<MemoryElementRead> MemoryReads { get; private set; }

        public List<MemoryElementWrite> MemoryWrites { get; private set; }


        private Dictionary<string, Func< TraceElementHeader, BinaryReader, TraceElement>> MasterGenerators = new Dictionary<string, Func< TraceElementHeader, BinaryReader, TraceElement>>
        {
            { "calltrace_call", CallElement.Generate },
            { "calltrace_return", ReturnElement.Generate },
            { "branch", BranchElement.Generate },
            { "instruction", InstructionOnlyElement.Generate },
            { "instruction_regs", InstructionRegsElement.Generate },
            { "instruction_disasm", InstructionDisasmElement.Generate },
            { "instruction_regs_disasm", InstructionRegsDisasmElement.Generate },
            { "mem_read", MemoryElementRead.Generate },
            { "mem_write", MemoryElementWrite.Generate },
            { "perf", PerfElement.Generate },
            { "log", AnnotationElement.Generate },
        };

        private Dictionary<uint, Func< TraceElementHeader, BinaryReader, TraceElement >> Generators = new Dictionary<uint, Func< TraceElementHeader, BinaryReader, TraceElement>>
        {
            { 0, TypeDeclarationElement.Generate },
        };

        private Trace()
        {
            Header = TraceHeader.ConstructSchemaHeader();

            Elements = new List<InstructionElement>();

            Annotations = new List<AnnotationElement>();

            var schemaInstruction = new InstructionDisasmSchemaElement(new TraceElementHeader());

            Elements.Add(schemaInstruction);
        }

        private Trace( BinaryReader reader )
        {
            Header = new TraceHeader( reader );

            ReadElements( reader );
        }

        private void ReadElements( BinaryReader reader )
        {
            ////////////DEBUG////////
            //Random rand = new Random(1337);

            //string[] elementTypes = new string[]
            //{
            //    "start1",
            //    "end1",
            //    "transmit",
            //    "receive",
            //    "terminate",
            //    "fdwait",
            //    "random",
            //    "allocate",
            //    "free",
            //};
            ///////////////////////

            Elements = new List < InstructionElement >();

            Annotations = new List < AnnotationElement >();

            MemoryReads = new List< MemoryElementRead >();

            MemoryWrites = new List< MemoryElementWrite >();

            while ( reader.PeekChar() != -1 )
            {
                var elementHeader = TraceElementHeader.Generate( reader );

                if ( !Generators.ContainsKey( elementHeader.RawTypeIndex ) )
                {
                    Console.WriteLine( "No generator for index {0}.", elementHeader.RawTypeIndex );
                    continue;
                }

                var newElement = Generators[ elementHeader.RawTypeIndex ]( elementHeader, reader );

                if ( newElement is TypeDeclarationElement )
                {
                    var typeDeclarationElement = newElement as TypeDeclarationElement;

                    if (typeDeclarationElement.ElementTypeStringIdentifier == null)
                        throw new Exception("Element isn't a type declaration...?");

                    if ( !MasterGenerators.ContainsKey( typeDeclarationElement.ElementTypeStringIdentifier ) )
                    {
                        Console.WriteLine( "No master generator specified for type {0} of index {1}.", typeDeclarationElement.ElementTypeStringIdentifier, typeDeclarationElement.ElementTypeIndex );
                        continue;
                    }

                    var generationMethod = MasterGenerators[ typeDeclarationElement.ElementTypeStringIdentifier ];
                    
                    Generators.Add( typeDeclarationElement.ElementTypeIndex, generationMethod );
                    
                }
                else if ( newElement is PerfElement )
                {
                }
                else if ( newElement is InstructionElement )
                {
                    Elements.Add( newElement as InstructionElement );


                    //////////////DEBUG
                    //if (SpoofAnnotations)
                    //{
                    //    if (rand.Next(40) > 38)
                    //    {
                    //        var eventType = elementTypes[rand.Next(elementTypes.Length)];
                    //        var newAnnotation = new AnnotationElement(newElement.Header,
                    //            eventType,
                    //            eventType + " rolled a " + rand.Next(20));
                    //        Annotations.Add(newAnnotation);
                    //    }
                    //}
                    ////////////////

                    continue;
                }
                else if (newElement is AnnotationElement)
                {
                    Annotations.Add( newElement as AnnotationElement );
                }
                else if ( newElement is MemoryElementRead )
                {
                    MemoryReads.Add( newElement as MemoryElementRead );
                }
                else if ( newElement is MemoryElementWrite )
                {
                    MemoryWrites.Add( newElement as MemoryElementWrite );
                }
                else
                {
                    Console.WriteLine("Unknown type! {0}", newElement.GetType() );
                }
            }
        }

        #region Generate Call Graph

        private double GetCallTime(InstructionElement element)
        {
            return element.Header.WallTime;
        }

        public CallGraph GenerateCallGraph()
        {
            var callGraph = new CallGraph();

            callGraph.BeginRecording();

            Console.WriteLine("Recording " + Elements.Count + " instructions in the call graph!");

            if (Elements.Count == 0)
            {
                Console.WriteLine("No elements in this trace!");

                callGraph.RecordFunctionCall(0, 0);
                callGraph.RecordFunctionReturn(1);
                callGraph.EndRecording(1);

                return callGraph;
            }

            var firstCall = Elements.First();
            var lastCall = Elements.Last();

            callGraph.Annotations = Annotations;

            foreach (var instruction in Elements.Where(i=>i is IDisasmElement))
            {
                //if (instruction.Eip!=0)
                //    Console.WriteLine("Found an instruction that's not at zero!  It's at " + instruction.Eip);
                var instVersion = new InstInstruction();

                instVersion.CpuTime = GetCallTime(instruction);
                instVersion.Eip = instruction.Eip;
                var disAsm = (instruction as IDisasmElement);
                if (disAsm != null)
                    instVersion.Instruction = disAsm.Disassembly;
                else
                    instVersion.Instruction = "N/A";

                //if (instVersion != null)
                callGraph.RecordInstructionCall(instVersion);

                if (GetCallTime(instruction) > GetCallTime(lastCall))
                    lastCall = instruction;

                if (GetCallTime(instruction) < GetCallTime(firstCall))
                    firstCall = instruction;

                //Console.WriteLine("Recording instruction timing " + GetCallTime(instruction));
            }

            //Console.WriteLine("Greatest instruction timing is " + GetCallTime(lastCall));

            callGraph.RecordFunctionCall(0, GetCallTime(firstCall));

            foreach (var instruction in Elements.Where(i => i is BranchElement).Cast<BranchElement>())
            {
                if (instruction.BranchTypeName == BranchElement.BranchTypes.Call)
                {
                    callGraph.RecordFunctionCall(instruction.ToEip, GetCallTime(instruction));
                }
                else if (instruction.BranchTypeName == BranchElement.BranchTypes.Return)
                {
                    callGraph.RecordFunctionReturn(GetCallTime(instruction));
                }
            }

            callGraph.RecordFunctionReturn(GetCallTime(lastCall));

            callGraph.EndRecording(GetCallTime(lastCall));

            return callGraph;
        }

        #endregion

        #region Factory

        public static Trace GenerateEmptyTrace()
        {
            var newTrace = new Trace();

            var instHeader = new TraceElementHeader();

            newTrace.Annotations.Add(new AnnotationElement(new TraceElementHeader(), "transmit", "transmit"));

            newTrace.Elements.Add(new InstructionOnlyElement(instHeader));
            newTrace.Elements.Add(new InstructionDisasmElement(instHeader));
            newTrace.Elements.Add(new InstructionDisasmSchemaElement(instHeader));
            newTrace.Elements.Add(new InstructionRegsDisasmElement(instHeader));
            newTrace.Elements.Add(new InstructionRegsElement(instHeader));

            newTrace.Elements.Add( new InstructionOnlyElement( new TraceElementHeader() ) );

            newTrace.Elements.Add( new CallElement( new TraceElementHeader() ) );
            newTrace.Elements.Add(new BranchElement(new TraceElementHeader()));
            newTrace.Elements.Add(new ReturnElement(new TraceElementHeader()));

            return newTrace;
        }

        public static Trace LoadFromFile( string fullPath )
        {
            using ( var stream = new FileStream( fullPath, FileMode.Open, FileAccess.Read ) )
            {
                return LoadFromStream( stream );
            }
        }

        public static Trace LoadFromStream( Stream stream )
        {
            using ( var reader = new BinaryReader( stream, Encoding.ASCII ) )
            {
                var trace = new Trace( reader );

                return trace;
            }
        }

        #endregion

        public override string ToString()
        {
            var header = Header.ToString();

            var elementHeader = string.Format( "Element count: {0}", Elements.Count );

            var elementContents = string.Empty;

            foreach ( var element in Elements )
            {
                elementContents += string.Format( "{0}\n", element );
            }

            return string.Format( "Trace:\n{0}\n{1}\n{2}", header, elementHeader, elementContents );
        }
    }
}
