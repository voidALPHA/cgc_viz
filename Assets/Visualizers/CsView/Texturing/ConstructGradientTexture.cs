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
using Utility;

namespace Visualizers.CsView.Texturing
{
    public class ConstructGradientTexture : Mutator
    {
        private MutableField<ColorGradient> m_ColorGradient = new MutableField<ColorGradient>() 
        { AbsoluteKey = "Banding Gradient"};
        [Controllable(LabelText = "Color Gradient")]
        public MutableField<ColorGradient> ColorGradient { get { return m_ColorGradient; } }

        private MutableField<int> m_TextureSize = new MutableField<int>()
        { LiteralValue = 1024 };
        [Controllable(LabelText = "TextureSize")]
        public MutableField<int> TextureSize { get { return m_TextureSize; } }

        private MutableTarget m_TextureTarget = new MutableTarget() 
        { AbsoluteKey = "Texture" };
        [Controllable(LabelText = "Texture Target")]
        public MutableTarget TextureTarget { get { return m_TextureTarget; } }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            TextureTarget.SetValue(new Texture(), newSchema);

            Router.TransmitAllSchema(newSchema);
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var textureSize = TextureSize.GetFirstValue(payload.Data);

            var newTexture = new Texture2D(textureSize, textureSize);
            
            var newPixels = new Color[textureSize*textureSize];

            var gradient = ColorGradient.GetFirstValue( payload.Data );

            for (int x=0; x<textureSize; x++)
                for ( int y = 0; y < textureSize; y++ )
                {
                    var proportion = x / (float)textureSize;
                    var color = gradient.Evaluate( proportion );
                    newPixels[ x + y * textureSize ] = color;
                }

            newTexture.SetPixels(newPixels);

            newTexture.Apply();

            TextureTarget.SetValue(newTexture, payload.Data);

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
