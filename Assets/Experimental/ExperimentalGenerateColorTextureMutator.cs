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
using System.Linq;
using Mutation;
using Mutation.Mutators;
using UnityEngine;
using Visualizers;

namespace Experimental
{
    public class ExperimentalGenerateColorTextureMutator : Mutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<int> m_TextureSize = new MutableField<int>() 
        { LiteralValue = 1024 };
        [Controllable(LabelText = "TextureSize")]
        public MutableField<int> TextureSize { get { return m_TextureSize; } }


        private MutableField<Color> m_PrimaryColor = new MutableField<Color>() 
        { LiteralValue = Color.magenta };
        [Controllable(LabelText = "Primary Color")]
        public MutableField<Color> PrimaryColor { get { return m_PrimaryColor; } }

        private MutableTarget m_TextureTarget = new MutableTarget() 
        { AbsoluteKey = "Texture" };
        [Controllable(LabelText = "Texture Target")]
        public MutableTarget TextureTarget { get { return m_TextureTarget; } }

        public ExperimentalGenerateColorTextureMutator()
        {
            TextureSize.SchemaParent = Scope;
            PrimaryColor.SchemaParent = Scope;
            TextureTarget.SchemaParent = Scope;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            TextureTarget.SetValue( new Texture(), newSchema );

            Router.TransmitAllSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach ( var entry in Scope.GetEntries( payload.Data ) )
            {
                var textureSize = TextureSize.GetValue( entry );

                var newTexture = new Texture2D(textureSize, textureSize);

                var primaryColor = PrimaryColor.GetValue( entry );
                
                var newPixels = Enumerable.Repeat( primaryColor, textureSize * textureSize ).ToArray();

                newTexture.SetPixels( newPixels );

                newTexture.Apply();

                TextureTarget.SetValue( newTexture, entry );
            }

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
