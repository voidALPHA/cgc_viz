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
using Mutation;
using UnityEngine;
using Visualizers.IsoGrid;

namespace Visualizers.CsView
{
    public class CsViewAssemblerController : VisualizerController
    {
        private MutableField< int > m_FileSize = new MutableField< int >()
        { LiteralValue = 70000 };
        [Controllable( LabelText = "File Size" )]
        public MutableField< int > FileSize
        {
            get { return m_FileSize; }
        }

        private MutableField< ICsBitStream > m_SignificantBitstream = new MutableField< ICsBitStream >()
        { AbsoluteKey = "Significant Stream"};
        [Controllable( LabelText = "Significant Bitstream" )]
        public MutableField< ICsBitStream > SignificantBitstream
        {
            get { return m_SignificantBitstream; }
        }

        private MutableField<ICsBitStream> m_InsignificantBitStream = new MutableField<ICsBitStream>() 
        { AbsoluteKey = "Insignificant Stream"};
        [Controllable(LabelText = "Insignificant Bitstream")]
        public MutableField<ICsBitStream> InsignificantBitStream { get { return m_InsignificantBitStream; } }

        private MutableField<float> m_SizeOffset = new MutableField<float>() 
        { LiteralValue = .2f };
        [Controllable(LabelText = "Size Offset")]
        public MutableField<float> SizeOffset { get { return m_SizeOffset; } }
        
        private ComponentAssembler Assembler { get { return VisualizerFactory.GetChallengeSetAssembler; } }
        
        private MutableTarget m_CsVisualizerTarget = new MutableTarget() 
        { AbsoluteKey = "Cs Visualizer" };
        [Controllable(LabelText = "Cs Visualizer Target")]
        public MutableTarget CsVisualizerTarget { get { return m_CsVisualizerTarget; } }

        private MutableField<Texture> m_VisualizerTexture = new MutableField<Texture>() 
        { AbsoluteKey = "Texture"};
        [Controllable(LabelText = "Visualizer Texture")]
        public MutableField<Texture> VisualizerTexture { get { return m_VisualizerTexture; } }

        public CsViewAssemblerController()
        {
            Router.AddSelectionState( "Default" );
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            CsVisualizerTarget.SetValue( new CsVisContainer(), newSchema );

            Router.TransmitAllSchema( newSchema );
        }

        protected override IEnumerator ProcessPayload( VisualPayload payload )
        {
            var filesize = FileSize.GetFirstValue( payload.Data );

            var significantStream = SignificantBitstream
                .GetFirstValue( payload.Data );
            var insignificantStream = InsignificantBitStream
                .GetFirstValue( payload.Data );

            var sizeOffset = SizeOffset.GetFirstValue( payload.Data );

            var csView = Assembler.ConstructCsViewStep1( significantStream,
                insignificantStream, filesize, sizeOffset/* * filesize/700000*/);

            csView.Initialize( this, payload );
            
            csView.SetTexture(VisualizerTexture.GetFirstValue(payload.Data));

            CsVisualizerTarget.SetValue( new CsVisContainer(csView), payload.Data );
            
            var newPayload = new VisualPayload( payload.Data, new VisualDescription(csView.Bound) );

            var iterator = Router.TransmitAll(newPayload);
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
