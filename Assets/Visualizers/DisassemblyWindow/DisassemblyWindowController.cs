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

using JetBrains.Annotations;
using Mutation;
using Visualizers.IsoGrid;

namespace Visualizers.DisassemblyWindow
{
    public abstract class DisassemblyWindowController : VisualizerController
    {
        private MutableField<string> m_WindowGroupKeyField = new MutableField<string>() { LiteralValue = "" };
        [Controllable( LabelText = "Window Group Key" ), UsedImplicitly]
        protected MutableField< string > WindowGroupKeyField
        {
            get { return m_WindowGroupKeyField; }
        }


        private MutableField< int > m_WindowKeyField = new MutableField< int > { AbsoluteKey = "TraceIndex" };
        [Controllable( LabelText = "Window Key" ), UsedImplicitly]
        protected MutableField< int > WindowKeyField
        {
            get { return m_WindowKeyField; }
        }

        protected DisassemblyWindowVisualizer GetWindowVisualizer( VisualPayload payload )
        {
            var groupKey = WindowGroupKeyField.GetFirstValue( payload.Data );

            var windowKey = WindowKeyField.GetFirstValue( payload.Data ).ToString();

            return DisassemblyGroupController.GetWindowVisualizer( groupKey, windowKey );
        }
    }
}
