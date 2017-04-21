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
using JetBrains.Annotations;
using Mutation;

namespace Visualizers.DisassemblyWindow
{
    public class DisasmEntryDescriptor : IComparable< DisasmEntryDescriptor >
    {
        public int InstructionIndex { get; set; }

        public string DisasmText { get; set; }

        public DisasmEntryDescriptor(int instructionIndex, string text)
        {
            InstructionIndex = instructionIndex;
            DisasmText = text;
        }

        public DisasmEntryDescriptor(int instructionIndex)
            : this ( instructionIndex, string.Empty)
        {
        }

        public int CompareTo(DisasmEntryDescriptor other)
        {
            return InstructionIndex - other.InstructionIndex;
        }
    }

    [UsedImplicitly]
    public class DisasmEntriesController : DisassemblyWindowController
    {
        private MutableScope m_CollectionScope = new MutableScope() {AbsoluteKey = "Disasms"};
        [Controllable(LabelText = "Collection Scope")]
        public MutableScope CollectionScope { get { return m_CollectionScope; } }

        private MutableField<int> m_InstructionIndex = new MutableField<int>() 
        { AbsoluteKey= "Disasms.Instruction Index" };
        [Controllable(LabelText = "InstructionIndex")]
        public MutableField<int> InstructionIndex { get { return m_InstructionIndex; } }

        private MutableField<string> m_DisasmText = new MutableField<string>() 
        { AbsoluteKey = "Disasms.Disasm"};
        [Controllable(LabelText = "Disasm Text")]
        public MutableField<string> DisasmText { get { return m_DisasmText; } }

        public DisasmEntriesController()
        {
            InstructionIndex.SchemaParent = CollectionScope;
            DisasmText.SchemaParent = CollectionScope;
        }

        protected sealed override IEnumerator ProcessPayload( VisualPayload payload )
        {
            var window = GetWindowVisualizer( payload );

            var disasmsEntries = new List< DisasmEntryDescriptor >();

            foreach ( var entry in CollectionScope.GetEntries( payload.Data ) )
            {
                var index = InstructionIndex.GetValue( entry );
                var disasmText = DisasmText.GetValue( entry );

                disasmsEntries.Add( new DisasmEntryDescriptor( index, disasmText ) );
            }

            window.DisasmEntries = disasmsEntries;

            yield return null;
        }
    }
}
