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

using UnityEngine;
using Utility;

namespace Visualizers.CsView.Texturing
{
    public class ConstructNoiseTexture : MonoBehaviour
    {

        public static Texture2D GenerateGradientTexture(int textureSize, ColorGradient gradient)
        {
            var newTexture = new Texture2D(textureSize, textureSize);
            var outColors = new Color[textureSize * textureSize];

            int i = 0;
            for (int x = 0; x < textureSize; x++)
            {
                for (int y = 0; y < textureSize; y++)
                {
                    float proportion = (x + y) / (float)(textureSize + textureSize);

                    outColors[i++] = gradient.Evaluate(proportion);
                }
            }

            newTexture.SetPixels(outColors);
            newTexture.Apply();

            return newTexture;
        }

        public static Texture2D GenerateNoiseTexture(int textureSize, int lowerOctave, int upperOctave,
            float persistence, ColorGradient colorGradient, int bitDepth=64, float spectrumMultiplier = 1f)
        {
            var newTexture = new Texture2D(textureSize, textureSize);

            var outColors = new Color[textureSize * textureSize];

            int i = 0;
            for (int x = 0; x < textureSize; x++)
            {
                for ( int y = 0; y < textureSize; y++ )
                {
                    //outColors[i++] = Color.red;

                    int xS = Mathf.FloorToInt( ( x / (float)textureSize ) * bitDepth) * (textureSize/ bitDepth);
                    int yS = Mathf.FloorToInt((y / (float)textureSize) * bitDepth) * (textureSize / bitDepth);

                    outColors[ i++ ] = colorGradient.Evaluate(
                        MultiplySpectrum(
                            Perlin2d( xS / (float)textureSize, yS / (float)textureSize, lowerOctave, upperOctave,
                                persistence )
                            , spectrumMultiplier )
                        );
                }
                //outColors[i++] = Color.white*Mathf.PerlinNoise(5f * x / (float)TextureSize, 5f * y / (float)TextureSize), .5f, .5f)
                //    + new Color(.5f, Mathf.PerlinNoise(50f * x / (float)TextureSize, 50f * y / (float)TextureSize), .5f);
            }

            newTexture.SetPixels(outColors);
            newTexture.Apply();

            return newTexture;
        }

        private static float MultiplySpectrum(float spectrumInput, float spectrumMultiplier)
        {
            return spectrumInput * spectrumMultiplier - spectrumMultiplier * .5f;
        }

        private static float PositiveMod(float a, float b)
        {
            float m = a % b;
            return m < 0 ? m + b : m;
        }

        //reference: http://tuttlem.github.io/2013/12/31/perlin-noise.html
        public static float Noise(int x, int y)
        {
            int n = x + y * 57;
            n = (n << 13) ^ n;
            var noise = (1.0f - ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7FFFFFFF) / 1073741824.0f);

            return PositiveMod(noise, 1);
        }

        public static float SmoothNoise(float x, float y)
        {
            int ix = (int)x;
            int iy = (int)y;

            // sample the corners
            float corners = (Noise(ix - 1, iy - 1) +
                             Noise(ix + 1, iy - 1) +
                             Noise(ix - 1, iy + 1) +
                             Noise(ix + 1, iy + 1)) / 16f;

            // sample the sides
            float sides = (Noise(ix - 1, iy) +
                           Noise(ix + 1, iy) +
                           Noise(ix, iy - 1) +
                           Noise(ix, iy + 1)) / 8f;

            // sample the centre
            float centre = Noise(ix, iy);

            // send out the accumulated result
            return corners + sides + centre;
        }
        /* Linear interpolation */
        private static float lerp(float a, float b, float x)
        {
            return a * (1 - x) + b * x;
        }

        /* Trigonometric interpolation */
        private static float terp(float a, float b, float x)
        {
            float ft = x * 3.1415927f;
            float f = (1 - Mathf.Cos(ft)) * 0.5f;

            return a * (1 - f) + b * f;
        }

        /* Noise interpolation */
        public static float InterpolateNoise(float x, float y)
        {
            //return Random.value;

            //return Mathf.PerlinNoise( x,y );

            int ix = (int)x;
            float fx = x - ix;

            int iy = (int)y;
            float fy = y - iy;

            float v1 = SmoothNoise(ix, iy),
                  v2 = SmoothNoise(ix + 1, iy),
                  v3 = SmoothNoise(ix, iy + 1),
                  v4 = SmoothNoise(ix + 1, iy + 1);

            float i1 = terp(v1, v2, fx),
                  i2 = terp(v3, v4, fx);

            return terp(i1, i2, fy);
        }

        public static float Perlin2d(float x, float y,
            int lowerOctave, int upperOctave,
            float persistence)
        {

            float total = 0.0f;

            float max = 0f;

            for (int i = lowerOctave; i <= upperOctave; i++)
            {
                float frequency = Mathf.Pow(2, i);
                float amplitude = Mathf.Pow(persistence, i - lowerOctave);

                max += amplitude;

                total = total + InterpolateNoise(x * frequency, y * frequency) * amplitude;
            }

            total /= max;

            return total;
        }
    }
}
