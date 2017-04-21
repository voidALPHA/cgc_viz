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
using UnityEngine;

namespace Visualizers.DisassemblyWindow
{

    public class CommsEntryDescriptor : IComparable<CommsEntryDescriptor>
    {
        public int InstructionIndex { get; set; }

        public string Message { get; set; }

        public bool IsFromRequestSide { get; set; }

        public Color Color { get; set; }

        public CommsEntryDescriptor( int instructionIndex, string message, bool isFromRequestSide, Color color )
        {
            InstructionIndex = instructionIndex;
            Message = message;
            IsFromRequestSide = isFromRequestSide;
            Color = color;
        }

        public CommsEntryDescriptor( int instructionIndex )
            : this ( instructionIndex, string.Empty, true, Color.magenta)
        {
        }
        
        public int CompareTo( CommsEntryDescriptor other )
        {
            return InstructionIndex - other.InstructionIndex;
        }
    }

    

    [UsedImplicitly]
    public class DisassemblyWindowCommsIndexController : DisassemblyWindowController
    {
        private MutableField< int > m_CurrentInstructionIndexField = new MutableField< int > { LiteralValue = 0 };
        [Controllable( LabelText = "Current Inst. Index" )]
        public MutableField< int > CurrentInstructionIndexField
        {
            get { return m_CurrentInstructionIndexField; }
        }

        protected override IEnumerator ProcessPayload( VisualPayload payload )
        {
            var window = GetWindowVisualizer( payload );

            window.InstructionIndex = CurrentInstructionIndexField.GetFirstValue( payload.Data );

            yield return null;
        }
    }

    [UsedImplicitly]
    public class DisassemblyWindowCommsEntriesController : DisassemblyWindowController
    {
        private MutableScope m_CollectionScope = new MutableScope();
        [Controllable( LabelText = "Collection" )]
        private MutableScope CollectionScope { get { return m_CollectionScope; } }

        private MutableField< int > m_InstructionIndexField = new MutableField< int > { LiteralValue = 0 };
        [Controllable( LabelText = "InstructionIndexField" )]
        public MutableField< int > InstructionIndexField
        {
            get { return m_InstructionIndexField; }
        }


        private MutableField< string > m_MessageField = new MutableField< string >()
        { LiteralValue = "ASL?" };
        [Controllable( LabelText = "Text" )]
        private MutableField< string > MessageField
        {
            get { return m_MessageField; }
        }

        private MutableField< bool > m_IsFromRequestSideField = new MutableField< bool >()
        { LiteralValue = true };
        [Controllable( LabelText = "Is From Request Side" )]
        private MutableField< bool > IsFromRequestSideField
        {
            get { return m_IsFromRequestSideField; }
        }

        private MutableField< Color > m_ColorField = new MutableField< Color >()
        { LiteralValue = Color.magenta };
        [Controllable( LabelText = "Team Color" )]
        private MutableField< Color > ColorField
        {
            get { return m_ColorField; }
        }


        public DisassemblyWindowCommsEntriesController()
        {
            InstructionIndexField.SchemaParent = CollectionScope;
            MessageField.SchemaParent = CollectionScope;
            IsFromRequestSideField.SchemaParent = CollectionScope;
            ColorField.SchemaParent = CollectionScope;
        }

        protected sealed override IEnumerator ProcessPayload( VisualPayload payload )
        {
            var window = GetWindowVisualizer( payload );

            var commsEntries = new List< CommsEntryDescriptor >();

            foreach ( var entry in CollectionScope.GetEntries( payload.Data ) )
            {
                var index = InstructionIndexField.GetValue( entry );
                var message = MessageField.GetValue( entry );
                var isFromRequestSide = IsFromRequestSideField.GetValue( entry );
                var color = ColorField.GetValue( entry );

                commsEntries.Add( new CommsEntryDescriptor( index, message, isFromRequestSide, color ) );
            }

            window.CommsEntries = commsEntries;
            
            yield return null;
        }
    }
}