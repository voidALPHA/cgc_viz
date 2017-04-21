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
using Mutation.Mutators;
using UnityEngine;
using Utility;
using ColorUtility = Assets.Utility.ColorUtility;

namespace Visualizers.CsView
{
    public class BitstreamToColorMutator : Mutator
    {
        private MutableField<ICsBitStream> m_BitStream = new MutableField<ICsBitStream>() 
        { AbsoluteKey= "Bitstream" };
        [Controllable(LabelText = "BitStream")]
        public MutableField<ICsBitStream> BitStream { get { return m_BitStream; } }

        private MutableField<float> m_Saturation = new MutableField<float>() 
        { LiteralValue = .8f };
        [Controllable(LabelText = "Saturation")]
        public MutableField<float> Saturation { get { return m_Saturation; } }

        private MutableField<float> m_Value = new MutableField<float>() 
        { LiteralValue = .8f };
        [Controllable(LabelText = "Value")]
        public MutableField<float> Value { get { return m_Value; } }

        private MutableField<int> m_BitsToSample = new MutableField<int>() 
        { LiteralValue = 11 };
        [Controllable(LabelText = "Bits To Sample")]
        public MutableField<int> BitsToSample { get { return m_BitsToSample; } }



        private MutableTarget m_ColorTarget = new MutableTarget() 
        { AbsoluteKey = "New Color" };
        [Controllable(LabelText = "ColorTarget")]
        public MutableTarget ColorTarget { get { return m_ColorTarget; } }

        public BitstreamToColorMutator()
        {
            Saturation.SchemaPattern = BitStream;
            Value.SchemaPattern = BitStream;
            BitsToSample.SchemaPattern = BitStream;
            ColorTarget.SchemaParent = BitStream;
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach ( var entry in BitStream.GetEntries( payload.Data ) )
            {
                var bitstream = BitStream.GetValue( entry );
                var saturation = Saturation.GetValue( entry );
                var value = Value.GetValue( entry );

                var bitsToSample = BitsToSample.GetValue( entry );

                var hue = bitstream.ReadInt(bitsToSample)/Mathf.Pow( 2f,bitsToSample );

                var newColor = ColorUtility.HsvtoRgb( hue, saturation, value );

                ColorTarget.SetValue( newColor, entry );
            }

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            //foreach (var entry in BitStream.GetEntries(  ))
            ColorTarget.SetValue( Color.magenta, newSchema );

            Router.TransmitAllSchema( newSchema );
        }
    }
}
