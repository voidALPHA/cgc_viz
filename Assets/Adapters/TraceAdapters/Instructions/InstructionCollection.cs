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
using System.Runtime.Serialization.Formatters.Binary;
using Adapters.TraceAdapters.CallGraphing;
using Adapters.TraceAdapters.Common;

namespace Adapters.TraceAdapters.Instructions
{
    [Serializable]
    public class InstructionCollection : IEnumerable< Instruction >, IDataElement
    {
        private readonly List< Instruction > m_Elements = new List< Instruction >();
        private List< Instruction > Elements
        {
            get { return m_Elements; }
        }

        public CallGraph GenerateCallGraph()
        {
            var callGraph = new CallGraph();

            callGraph.BeginRecording();

            Console.WriteLine("Recording "+Elements.Count+" instructions in the call graph!");

            var firstCall = Elements.First();
            var lastCall = Elements.Last();

            foreach (var instruction in Elements)
            {
                //if (instruction.Eip!=0)
                //    Console.WriteLine("Found an instruction that's not at zero!  It's at " + instruction.Eip);
                var instVersion = instruction as InstInstruction;
                if (instVersion != null)
                    callGraph.RecordInstructionCall(instVersion);

                if (instruction.CpuTime > lastCall.CpuTime)
                    lastCall = instruction;

                if (instruction.CpuTime < firstCall.CpuTime)
                    firstCall = instruction;
            }

            callGraph.RecordFunctionCall(0, firstCall.CpuTime);

            foreach ( var instruction in Elements.Where( i => i is InstInstruction ).Cast< InstInstruction >() )
            {
                if ( instruction.Nature == InstInstructionNature.FunctionCall)
                {
                    callGraph.RecordFunctionCall( instruction.GetCalledAddress(), instruction.CpuTime );
                }
                else if ( instruction.Nature == InstInstructionNature.FunctionReturn )
                {
                    callGraph.RecordFunctionReturn( instruction.CpuTime );
                }
            }

            callGraph.RecordFunctionReturn(lastCall.CpuTime);

            callGraph.EndRecording(lastCall.CpuTime);

            return callGraph;
        }

        #region List-like features...

        public int Count
        {
            get { return Elements.Count; }
        }

        public void Add( Instruction instruction )
        {
            Elements.Add( instruction );
        }

        public void AddRange( IEnumerable< Instruction > range )
        {
            Elements.AddRange( range );
        }
        
        #endregion



        #region IEnumerable Implementation

        IEnumerator< Instruction > IEnumerable< Instruction >.GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Elements.GetEnumerator();
        }

        #endregion


        #region Serialization Convenience Methods

        public static InstructionCollection DeserializeFromFile( string filename )
        {
            using ( var fileStream = new FileStream( filename, FileMode.Open, FileAccess.Read ) )
            {
                return DeserializeFromStream( fileStream );
            }
        }

        public static InstructionCollection DeserializeFromStream( Stream stream )
        {
            var formatter = new BinaryFormatter();
            var result = formatter.Deserialize( stream );

            var elements = (InstructionCollection)result;
            
            return elements;
        }

        public void SerializeToFile( string filename )
        {
            filename = Environment.ExpandEnvironmentVariables( filename );

            using ( var fileStream = new FileStream( filename, FileMode.Create, FileAccess.Write ) )
            {
                SerializeToStream( fileStream );
                fileStream.Flush();
            }
        }

        public void SerializeToStream( Stream stream )
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize( stream, this );
        }

        #endregion

        public override string ToString()
        {
            return String.Format( "[InstructionCollection/Trace]: Elements: {0}", Elements.Count );
        }
    }
}
