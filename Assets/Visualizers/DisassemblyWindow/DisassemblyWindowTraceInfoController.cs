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
using Adapters.TraceAdapters.Traces;
using JetBrains.Annotations;
using Mutation;
using UnityEngine;

namespace Visualizers.DisassemblyWindow
{
    [UsedImplicitly]
    public class DisassemblyWindowTraceInfoController : DisassemblyWindowController
    {

        private MutableField<int> m_ElementCountField = new MutableField<int> { AbsoluteKey = "ElementCount" };
        [Controllable( LabelText = "Element Count" )]
        private MutableField<int> ElementCountField
        {
            get { return m_ElementCountField; }
        }


        private MutableField<int> m_BinaryIdField = new MutableField<int> { AbsoluteKey = "BinaryId" };
        [Controllable( LabelText = "Binary Id" )]
        private MutableField<int> BinaryIdField
        {
            get { return m_BinaryIdField; }
        }
        

        private MutableField<Color> m_FilamentColorField = new MutableField<Color> { LiteralValue = Color.magenta };
        [Controllable( LabelText = "Filament Color" )]
        private MutableField<Color> FilamentColorField
        {
            get { return m_FilamentColorField; }
        }

        private MutableField< List< int > > m_BinsetTeamIndices = new MutableField< List< int > > { AbsoluteKey = "BinsetTeamIndices" };
        [Controllable( LabelText = "Binset Team Indices" )]
        public MutableField< List< int > > BinsetTeamIndices
        {
            get { return m_BinsetTeamIndices; }
        }

        private MutableField< string > m_BinsetShortNameField = new MutableField< string > { LiteralValue = "Binset Short Name" };
        [Controllable( LabelText = "BinsetShortNameField" )]
        private MutableField< string > BinsetShortNameField
        {
            get { return m_BinsetShortNameField; }
        }



        private MutableField<int> m_RequestIdField = new MutableField<int> { AbsoluteKey = "RequestId" };
        [Controllable( LabelText = "Request Id" )]
        private MutableField<int> RequestIdField
        {
            get { return m_RequestIdField; }
        }

        private MutableField<RequestNature> m_RequestNatureField = new MutableField<RequestNature> { AbsoluteKey = "RequestNature" };
        [Controllable( LabelText = "Request Nature" )]
        private MutableField<RequestNature> RequestNatureField
        {
            get { return m_RequestNatureField; }
        }

        private MutableField<Color> m_RequestColorField = new MutableField<Color> { LiteralValue = Color.magenta };
        [Controllable( LabelText = "Request Color" )]
        private MutableField<Color> RequestColorField
        {
            get { return m_RequestColorField; }
        }

        private MutableField<Material> m_RequestTeamLogoField = new MutableField<Material> { AbsoluteKey = "Logo Material" };
        [Controllable( LabelText = "Request Team Logo" )]
        private MutableField<Material> RequestTeamLogoField
        {
            get { return m_RequestTeamLogoField; }
        }


        private MutableField<bool> m_SuccessField = new MutableField<bool> { AbsoluteKey = "Success" };
        [Controllable( LabelText = "Success" )]
        private MutableField<bool> SuccessField
        {
            get { return m_SuccessField; }
        }

        private MutableField< int > m_PovTypeField = new MutableField< int > { LiteralValue = 0 };
        [Controllable( LabelText = "Pov Type" )]
        private MutableField< int > PovTypeField
        {
            get { return m_PovTypeField; }
        }



        private MutableField<bool> m_ShowDisassemblyField = new MutableField<bool> { LiteralValue = true };
        [Controllable( LabelText = "Show Disassembly" )]
        private MutableField<bool> ShowDisassemblyField
        {
            get { return m_ShowDisassemblyField; }
        }

        private MutableField<bool> m_ShowCommsField = new MutableField<bool> { LiteralValue = true };
        [Controllable( LabelText = "Show Comms" )]
        private MutableField<bool> ShowCommsField
        {
            get { return m_ShowCommsField; }
        }



        protected sealed override IEnumerator ProcessPayload( VisualPayload payload )
        {
            var windowVisualizer = GetWindowVisualizer( payload );

            windowVisualizer.RequestColor = RequestColorField.GetFirstValue( payload.Data );

            windowVisualizer.FilamentColor = FilamentColorField.GetFirstValue( payload.Data );

            windowVisualizer.RequestTeamLogo = RequestTeamLogoField.GetFirstValue( payload.Data );

            windowVisualizer.BinaryId = BinaryIdField.GetFirstValue( payload.Data );

            windowVisualizer.BinsetTeamIndices = BinsetTeamIndices.GetFirstValue( payload.Data );

            windowVisualizer.BinsetShortName = BinsetShortNameField.GetFirstValue( payload.Data );

            windowVisualizer.RequestId = RequestIdField.GetFirstValue( payload.Data );

            windowVisualizer.RequestNature = RequestNatureField.GetFirstValue( payload.Data );

            windowVisualizer.ElementCount = ElementCountField.GetFirstValue( payload.Data );

            // Order issue, must come after RequestNature!
            windowVisualizer.Success = SuccessField.GetFirstValue( payload.Data );

            windowVisualizer.PovType = PovTypeField.GetFirstValue( payload.Data );

            windowVisualizer.ShowDisassembly = ShowDisassemblyField.GetFirstValue( payload.Data );

            windowVisualizer.ShowComms = ShowCommsField.GetFirstValue( payload.Data );



            yield return null;
        }
    }
}