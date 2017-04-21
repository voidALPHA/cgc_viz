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
using System.Linq;
using Mutation;
using Mutation.Mutators;
using UnityEngine;

namespace Visualizers.CsView.Texturing
{
    public class GenerateEntropyGridMutator : Mutator
    {
        private MutableField<float> m_Entropy = new MutableField<float>() 
        { LiteralValue = .5f };
        [Controllable(LabelText = "Entropy")]
        public MutableField<float> Entropy { get { return m_Entropy; } }

        private MutableField<string> m_EntropySteps = new MutableField<string>() 
        { LiteralValue = ".3,.5,.6,.7,.85" };
        [Controllable(LabelText = "Entropy Steps")]
        public MutableField<string> EntropySteps { get { return m_EntropySteps; } }

        private MutableField<int> m_TextureSize = new MutableField<int>() 
        { LiteralValue = 256 };
        [Controllable(LabelText = "Texture Size")]
        public MutableField<int> TextureSize { get { return m_TextureSize; } }


        private MutableField<int> m_MinimumEntropyLevels = new MutableField<int>() 
        { LiteralValue = 2 };
        [Controllable(LabelText = "Minimum Grid Levels")]
        public MutableField<int> MinimumEntropyLevels { get { return m_MinimumEntropyLevels; } }



        private MutableField<Color> m_ColorOne = new MutableField<Color>() 
        { LiteralValue = Color.white };
        [Controllable(LabelText = "Color One")]
        public MutableField<Color> ColorOne { get { return m_ColorOne; } }

        private MutableField<Color> m_ColorTwo = new MutableField<Color>() 
        { LiteralValue = Color.black };
        [Controllable(LabelText = "Color Two")]
        public MutableField<Color> ColorTwo { get { return m_ColorTwo; } }
        

        private MutableTarget m_TextureTarget = new MutableTarget() 
        { AbsoluteKey = "Entropy Texture" };
        [Controllable(LabelText = "Entropy Texture")]
        public MutableTarget TextureTarget { get { return m_TextureTarget; } }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var textureSize = TextureSize.GetFirstValue( payload.Data );

            var entropy = Entropy.GetFirstValue(payload.Data);

            var entropySteps = (from step in EntropySteps.GetFirstValue(payload.Data).Split( ',' ) select float.Parse( step.Trim() )).ToArray();

            var minimumEntropyLevels = MinimumEntropyLevels.GetFirstValue( payload.Data );

            var colorOne = ColorOne.GetFirstValue(payload.Data);
            var colorTwo = ColorTwo.GetFirstValue(payload.Data);

            var entropyGridTexture = new Texture2D(textureSize, textureSize);

            var entropyLevel=0;
            while ( entropyLevel < entropySteps.Length && entropy > entropySteps[ entropyLevel ] )
                entropyLevel++;

            entropyLevel += minimumEntropyLevels;

            var pixels = new Color[textureSize * textureSize];

            for (int x=0; x<textureSize; x++)
                for ( int y = 0; y < textureSize; y++ )
                {
                    var xImpact = Mathf.FloorToInt((x / (float)textureSize) * entropyLevel);
                    var yImpact = Mathf.FloorToInt((y / (float)textureSize) * entropyLevel);
                    pixels[ x + y * textureSize ] = ( ( xImpact + yImpact ) % 2 ) == 1 ? colorOne : colorTwo;
                }

            entropyGridTexture.SetPixels( pixels );

            entropyGridTexture.Apply();

            TextureTarget.SetValue( entropyGridTexture, payload.Data);

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }

        private Texture2D SchemaTexture { get; set; }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            if (SchemaTexture==null)
                SchemaTexture = new Texture2D( 1,1 );

            TextureTarget.SetValue(SchemaTexture, newSchema);

            base.OnProcessOutputSchema( newSchema );
        }
    }
}
