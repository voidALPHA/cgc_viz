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
using Mutation;
using Mutation.Mutators;
using UnityEngine;
using Utility;
using ColorUtility = Assets.Utility.ColorUtility;

namespace Visualizers.CsView.Texturing
{
    public class ByteHistogramToColorTexture : DataMutator
    {
        private MutableField<List<int>> m_ByteHistogram = new MutableField<List<int>>() 
        { AbsoluteKey = "Byte Histogram"};
        [Controllable(LabelText = "Byte Histogram")]
        public MutableField<List<int>> ByteHistogram { get { return m_ByteHistogram; } }

        private MutableField<int> m_Keypoints = new MutableField<int>() 
        { LiteralValue = 256 };
        [Controllable(LabelText = "Number of Key Points")]
        public MutableField<int> Keypoints { get { return m_Keypoints; } }

        private MutableField<bool> m_SmoothGradient = new MutableField<bool>() 
        { LiteralValue = true };
        [Controllable(LabelText = "Smooth Gradient")]
        public MutableField<bool> SmoothGradient { get { return m_SmoothGradient; } }

        private MutableField<Color> m_PrimaryColor = new MutableField<Color>()
        { LiteralValue = Color.green };
        [Controllable(LabelText = "Primary Color")]
        public MutableField<Color> PrimaryColor { get { return m_PrimaryColor; } }

        private MutableField<float> m_PrimaryImpactOnBands = new MutableField<float>()
        { LiteralValue = .4f };
        [Controllable(LabelText = "PrimaryImpactOnBands")]
        public MutableField<float> PrimaryImpactOnBands { get { return m_PrimaryImpactOnBands; } }



        private MutableField<float> m_Saturation = new MutableField<float>()
        { LiteralValue = .8f };
        [Controllable(LabelText = "Band Saturation")]
        public MutableField<float> Saturation { get { return m_Saturation; } }

        private MutableField<float> m_Value = new MutableField<float>()
        { LiteralValue = .8f };
        [Controllable(LabelText = "Band Value")]
        public MutableField<float> Value { get { return m_Value; } }

        private MutableField<float> m_EdgeWidth = new MutableField<float>() 
        { LiteralValue = .001f };
        [Controllable(LabelText = "Edge Width")]
        public MutableField<float> EdgeWidth { get { return m_EdgeWidth; } }


        private MutableTarget m_HistogramGradient = new MutableTarget() 
        { AbsoluteKey = "Histogram Gradient" };
        [Controllable(LabelText = "Histogram Gradient")]
        public MutableTarget HistogramGradient { get { return m_HistogramGradient; } }



        private class FrequencyBand
        {
            public float Start { get; set; }
            public float End { get; set; }
            public int Byte { get; set; }
            public Color Color { get; set; }
            public int Frequency { get; set; }
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            var frequencies = ByteHistogram.GetFirstValue( mutable );

            var saturation = Saturation.GetFirstValue( mutable );
            var value = Value.GetFirstValue( mutable );

            var frequencyBands = new List< FrequencyBand >();

            var totalSize = (float)frequencies.Sum( f => f );

            var currentPosition = 0f;
            int i = 0;

            foreach ( var frequency in frequencies )
            {
                frequencyBands.Add( new FrequencyBand
                {
                    Byte = i++,
                    Color = ColorUtility.HsvtoRgb( currentPosition/totalSize, saturation, value),
                    Start = currentPosition / totalSize,
                    End = (currentPosition + frequency)/totalSize,
                    Frequency = frequency
                } );
                currentPosition = currentPosition + frequency;
            }

            frequencyBands.Sort((a,b)=>0-a.Frequency.CompareTo( b.Frequency ));
            
            var smoothGradient = SmoothGradient.GetFirstValue( mutable );
            var numberOfPoints = Keypoints.GetFirstValue( mutable );
            var edgeWidth = EdgeWidth.GetFirstValue( mutable );
            var primaryColor = PrimaryColor.GetFirstValue( mutable );
            var primaryImpactOnBands = PrimaryImpactOnBands.GetFirstValue( mutable );

            var newGradient = new ColorGradient( numberOfPoints* (smoothGradient?1:2));
            
            frequencyBands = frequencyBands.Take(numberOfPoints).OrderBy( f=>f.Start ).ToList();

            if ( smoothGradient )
            {
                for ( var k = 0; k < Mathf.Min( numberOfPoints, frequencyBands.Count ); k++ )
                {
                    {
                        newGradient.AddColorKey( new GradientColorKey(
                            frequencyBands[ k ].Color,
                            ( frequencyBands[ k ].Start + frequencyBands[ k ].End ) / 2f ) );
                        
                    }
                }
            }
            else
            {
                for ( var k = 1; k < Mathf.Min( numberOfPoints, frequencyBands.Count ); k++ )
                {
                    var k1 = frequencyBands[ k - 1 ].Color;
                    newGradient.AddColorKey( new GradientColorKey(
                        Color.Lerp( frequencyBands[ k - 1 ].Color,
                        primaryColor, primaryImpactOnBands),
                        frequencyBands[ k - 1 ].Start + edgeWidth / 2f ) );

                    var k2 = frequencyBands[ k - 1 ].Color;
                    newGradient.AddColorKey( new GradientColorKey(
                        Color.Lerp(frequencyBands[k - 1].Color,
                        primaryColor, primaryImpactOnBands),
                        frequencyBands[ k ].Start - edgeWidth / 2f ) );

                    if (k1!=k2)
                        throw new Exception("What?  How are these colors not matched?");
                }
            }
            

            HistogramGradient.SetValue( newGradient, mutable );

            return mutable;
        }
    }
}
