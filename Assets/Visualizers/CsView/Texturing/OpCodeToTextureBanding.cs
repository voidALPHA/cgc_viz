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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;

namespace Visualizers.CsView.Texturing
{
    [Serializable]
    public class OpcodePair
    {
        public OpcodePair(int frequency, string opcode)
        {
            Frequency = frequency;
            Opcode = opcode;
        }

        public int Frequency { get; set; }
        public string Opcode { get; set; }

        public Color ColorBuffer { get; set; }
        public float FloatBuffer { get; set; }
    }

    [Serializable]
    public class OpcodeHistogram : Dictionary<String, OpcodePair>
    {
    }

    public class OpcodeToTextureBanding
    {
        public static ColorGradient GenerateDebugGradient(OpcodeHistogram opcodes,
            Color primaryColor, int numberOfBands = 5)
        {
            ColorGradient newGradient = new ColorGradient(8);

            newGradient.AddColorKey(new GradientColorKey(Color.green, .15f));
            newGradient.AddColorKey(new GradientColorKey(Color.blue, .2f));
            newGradient.AddColorKey(new GradientColorKey(Color.green, .25f));

            newGradient.AddColorKey(new GradientColorKey(Color.green, .35f));
            newGradient.AddColorKey(new GradientColorKey(Color.blue, .4f));
            newGradient.AddColorKey(new GradientColorKey(Color.green, .45f));

            newGradient.AddColorKey(new GradientColorKey(Color.blue, .65f));
            newGradient.AddColorKey(new GradientColorKey(Color.red, .75f));

            return newGradient;
        }

        public static ColorGradient GenerateTextureBanding(OpcodeHistogram opcodes,
            Color primaryColor, float hueRange, int numberOfBands,
            float bandWidth, float bandSaturation, float bandValue, float bandEdgeWidth)
        {
            var localCodes = opcodes.Values.ToList();

            // sort opcodes 
            localCodes.Sort((a, b) => { return a.Opcode.CompareTo(b.Opcode); });

            float primaryH, primaryS, primaryV;
            Assets.Utility.ColorUtility.RGBToHSV(primaryColor, out primaryH, out primaryS, out primaryV);

            for (int i = 0; i < localCodes.Count; i++)
            {
                var newHue = primaryH + ((i / (float)localCodes.Count)-.5f)*hueRange*2;

                localCodes[i].ColorBuffer = Assets.Utility.ColorUtility.HsvtoRgb( newHue, bandSaturation, bandValue );
            }

            localCodes.Sort((a, b) => { return a.Frequency.CompareTo(b.Frequency); });
            localCodes.Reverse();

            int codeCount = Mathf.Min(localCodes.Count, numberOfBands);

            int minFrequency = localCodes.Take(codeCount).Min(code => code.Frequency) - 1;
            int maxFrequency = localCodes.Take(codeCount).Max(code => code.Frequency) + 1;

            float inverseFrequencyMult = 1f / (maxFrequency - minFrequency);

            ColorGradient newGradient = new ColorGradient(codeCount * 4);

            for (int i = 0; i < codeCount; i++)
            {
                float frequencyMult = (localCodes[i].Frequency - minFrequency) * inverseFrequencyMult;

                DefineColorBand(i, frequencyMult,
                    bandWidth, bandEdgeWidth, localCodes[i].ColorBuffer,
                    primaryColor, ref newGradient);
            }

            //var colorPoints = newGradient.ColorKeys.Select(kvp => kvp.Value).ToList();

            return newGradient;
        }

        private static void DefineColorBand(int bandIndex, float bandPosition,
            float bandWidth, float bandEdgeWidth, Color bandColor,
            Color primaryColor, ref ColorGradient gradient)
        {
            const float indexOffset = 0f;

            gradient.AddColorKey(new GradientColorKey(primaryColor, bandPosition - bandWidth * .5f - bandEdgeWidth + indexOffset * bandIndex));
            gradient.AddColorKey(new GradientColorKey(bandColor, bandPosition - bandWidth * .5f + indexOffset * bandIndex));
            //gradient.AddColorKey(new GradientColorKey(bandColor, bandPosition + indexOffset * bandIndex));
            gradient.AddColorKey(new GradientColorKey(bandColor, bandPosition + bandWidth * .5f + indexOffset * bandIndex));
            gradient.AddColorKey(new GradientColorKey(primaryColor, bandPosition + bandWidth * .5f + bandEdgeWidth + indexOffset * bandIndex));
        }

    }
}
