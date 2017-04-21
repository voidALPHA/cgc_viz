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

using System.Collections.Generic;
using Adapters.TraceAdapters.Traces.Elements;
using Visualizers;

namespace Mutation.Mutators.Disassembly
{
    public class InsertDisasmMutator : DataMutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Element Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<InstructionElement> m_TraceInstruction = new MutableField<InstructionElement>() 
        { AbsoluteKey = "Instructions.Trace Instruction" };
        [Controllable(LabelText = "TraceInstruction")]
        public MutableField<InstructionElement> TraceInstruction { get { return m_TraceInstruction; } }
        
        private MutableTarget m_HasDisasmTarget = new MutableTarget() 
        { AbsoluteKey = "Instructions.Has Disasm" };
        [Controllable(LabelText = "Has Disasm")]
        public MutableTarget HasDisasmTarget { get { return m_HasDisasmTarget; } }
        
        private MutableTarget m_DisasmTarget = new MutableTarget() 
        { AbsoluteKey = "Instructions.Disasm" };
        [Controllable(LabelText = "Disasm Target")]
        public MutableTarget DisasmTarget { get { return m_DisasmTarget; } }

        public InsertDisasmMutator() : base()
        {
            TraceInstruction.SchemaParent = Scope;

            DisasmTarget.SchemaParent = Scope;
            HasDisasmTarget.SchemaParent = Scope;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {
                var disasm = TraceInstruction.GetValue( entry ) as IDisasmElement;

                HasDisasmTarget.SetValue( disasm != null, entry );
                
                DisasmTarget.SetValue((disasm != null)?disasm.Disassembly:"", entry );
            }
            return mutable;
        }

        /*
        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            foreach (var entry in EntryField.GetLastKeyValue(newSchema))
            {
                var disasm = TraceInstruction.GetLastKeyValue(entry) as IDisasmElement;

                HasDisasmTarget.SetValue(disasm != null, entry);

                if (disasm == null)
                    DisasmTarget.SetValue(disasm.Disassembly, entry);
            }
            return mutable;
        }*/
    }
}
