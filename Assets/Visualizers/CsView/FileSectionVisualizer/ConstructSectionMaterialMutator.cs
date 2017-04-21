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
using System.Linq;
using Adapters.TraceAdapters.Commands;
using Mutation;
using Mutation.Mutators;
using Mutation.Mutators.VisualModifiers;
using UnityEngine;
using Utility;

namespace Visualizers.CsView.FileSectionVisualizer
{
    public class ConstructSectionMaterialMutator : Mutator
    {


        private MutableTarget m_MaterialTarget = new MutableTarget() 
        { AbsoluteKey = "File Section Material" };
        [Controllable(LabelText = "Material Target")]
        public MutableTarget MaterialTarget { get { return m_MaterialTarget; } }

        private MutableScope m_SectionScope = new MutableScope()
        {
            AbsoluteKey = "Section Listings"
        };
        [Controllable(LabelText = "FileSectionScope")]
        public MutableScope SectionScope { get { return m_SectionScope; } }

        private MutableField<int> m_SectionFileSize = new MutableField<int>() 
        { AbsoluteKey = "Section Listings.File Size" };
        [Controllable(LabelText = "Section File Size")]
        public MutableField<int> SectionFileSize { get { return m_SectionFileSize; } }

        private MutableField<int> m_SectionMemorySize = new MutableField<int>() 
        { AbsoluteKey = "Section Listings.Memory Size"};
        [Controllable(LabelText = "Section Memory Size")]
        public MutableField<int> SectionMemorySize { get { return m_SectionMemorySize; } }

        private MutableField<BinarySectionFlags> m_SectionFlag = new MutableField<BinarySectionFlags>() 
        { AbsoluteKey = "Section Listings.Binary Flag"};
        [Controllable(LabelText = "Section Flag")]
        public MutableField<BinarySectionFlags> SectionFlag { get { return m_SectionFlag; } }

        private MutableField<BinarySectionTypes> m_SectionType = new MutableField<BinarySectionTypes>() 
        { AbsoluteKey = "SectionListings.Binary Type"};
        [Controllable(LabelText = "Section Type")]
        public MutableField<BinarySectionTypes> SectionType { get { return m_SectionType; } }

        private MutableField<int> m_TextureWidth = new MutableField<int>() 
        { LiteralValue = 1024 };
        [Controllable(LabelText = "Texture Width")]
        public MutableField<int> TextureWidth { get { return m_TextureWidth; } }

        private MutableField<int> m_TextureHeight = new MutableField<int>() 
        { LiteralValue = 8 };
        [Controllable(LabelText = "Texture Height")]
        public MutableField<int> TextureHeight { get { return m_TextureHeight; } }

        private MutableField<float> m_FlagColorImpact = new MutableField<float>() 
        { LiteralValue = .25f };
        [Controllable(LabelText = "Flag Color Impact")]
        public MutableField<float> FlagColorImpact { get { return m_FlagColorImpact; } }



        public ConstructSectionMaterialMutator()
        {
            SectionFileSize.SchemaParent = SectionScope;
            SectionMemorySize.SchemaParent = SectionScope;
            SectionFlag.SchemaParent = SectionScope;
            SectionType.SchemaParent = SectionScope;
        }

        private Material SchemaMaterial { get; set; }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            if (SchemaMaterial == null)
                SchemaMaterial = GameObject.Instantiate(MaterialFactory.GetDefaultMaterial());

            MaterialTarget.SetValue( SchemaMaterial, newSchema );

            Router.TransmitAllSchema( newSchema );
        }

        private class ColorPair
        {
            public float Width { get; set; }
            public Color Color { get; set; }

            public ColorPair( float width, Color color )
            {
                Width = width;
                Color = color;
            }
        }

        private const float Epsilon = .001f;

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var currentWidth = 0f;

            var flagColorImpact = FlagColorImpact.GetFirstValue( payload.Data );

            var sections = SectionScope.GetEntries( payload.Data ).ToList();

            var totalSize = (float)( from entry in SectionScope.GetEntries( payload.Data )
                select
                    SectionMemorySize.GetValue( entry ) ).Aggregate( ( a, b ) => a + b );

            ColorGradient sectionGradient= new ColorGradient( 2*sections.Count );
            
            foreach ( var section in SectionScope.GetEntries( payload.Data ) )
            {
                var bandColor = Color.Lerp(
                    SectionColorMapping.GetSectionTypeColor( SectionType.GetValue( section )),
                    SectionColorMapping.GetSectionFlagColor(SectionFlag.GetValue( section )),
                    flagColorImpact);
                sectionGradient.AddColorKey( new GradientColorKey(
                    bandColor,
                    currentWidth) );

                currentWidth += SectionMemorySize.GetValue( section );

                sectionGradient.AddColorKey(new GradientColorKey(
                    bandColor,
                    currentWidth));

                currentWidth += Epsilon * totalSize;
            }
            
            var textureWidth = TextureWidth.GetFirstValue( payload.Data );
            var textureHeight = TextureHeight.GetFirstValue( payload.Data );

            sectionGradient.RescaleColorKeys(currentWidth);


            var resultTexture = new Texture2D( textureWidth, textureHeight );

            var outColors = new Color[textureWidth * textureHeight];

            for ( int x = 0; x < textureWidth; x++ )
            {
                var localColor = sectionGradient.Evaluate( x/(float)textureWidth );
                for ( int y = 0; y < textureHeight; y++ )
                    outColors[ x + textureWidth * y ] = localColor;
            }

            resultTexture.SetPixels( outColors );
            resultTexture.Apply();

            var quadMaterial = GameObject.Instantiate( MaterialFactory.GetDefaultMaterial() );
            quadMaterial.mainTexture = resultTexture;
            
            MaterialTarget.SetValue( quadMaterial, payload.Data );

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
