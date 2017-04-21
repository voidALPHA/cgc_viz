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
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Assets.Utility
{
    public class ColorUtility
    {
        public static bool TryParse( string input, ref Color outputColor )
        {
            var colorProperties = typeof( Color ).GetProperties( BindingFlags.Static | BindingFlags.Public ).Where( p => p.PropertyType == typeof( Color ) );

            foreach ( var colorProperty in colorProperties )
            {
                if ( colorProperty.Name.Equals( input ) )
                {
                    outputColor = (Color) colorProperty.GetValue( null, null );
                    return true;
                }
            }

            // TODO: Add custom colors, and consider things such as "random," etc.

            // Try parsing RGBA value...

            // ToString yields this format: 
            //   RGBA(1.000, 0.650, 0.000, 1.000)

            if ( !input.StartsWith( "RGBA" ))
                return false;

            input = input.Replace( "RGBA(", "" );
            input = input.Replace( ")", "" );

            var tokens = input.Split( ',' );
            if ( tokens.Count() != 4 )
                return false;

            var values = new float[4];
            for ( int i = 0; i < 4; i++ )
            {
                if ( ! float.TryParse( tokens[i], out values[i] ) )
                    return false;
            }

            outputColor = new Color( values[0], values[1], values[2], values[3] );

            return true;
        }



        // Color translator, reference: http://answers.unity3d.com/questions/701956/hsv-to-rgb-without-editorguiutilityhsvtorgb.html
        public static Color HsvtoRgb(float H, float S, float V)
        {
            if (S <= 0.0001f)
                return new Color(V, V, V);
            if (V <= 0.0001f)
                return new Color(0, 0, 0);

            Color col = Color.black;
            float Hval = H*6f;
            int sel = Mathf.FloorToInt(Hval);
            float mod = Hval - sel;
            float v1 = V*(1f - S);
            float v2 = V*(1f - S*mod);
            float v3 = V*(1f - S*(1f - mod));
            switch (sel + 1)
            {
                case 0:
                    col.r = V;
                    col.g = v1;
                    col.b = v2;
                    break;
                case 1:
                    col.r = V;
                    col.g = v3;
                    col.b = v1;
                    break;
                case 2:
                    col.r = v2;
                    col.g = V;
                    col.b = v1;
                    break;
                case 3:
                    col.r = v1;
                    col.g = V;
                    col.b = v3;
                    break;
                case 4:
                    col.r = v1;
                    col.g = v2;
                    col.b = V;
                    break;
                case 5:
                    col.r = v3;
                    col.g = v1;
                    col.b = V;
                    break;
                case 6:
                    col.r = V;
                    col.g = v1;
                    col.b = v2;
                    break;
                case 7:
                    col.r = V;
                    col.g = v3;
                    col.b = v1;
                    break;
            }
            col.r = Mathf.Clamp(col.r, 0f, 1f);
            col.g = Mathf.Clamp(col.g, 0f, 1f);
            col.b = Mathf.Clamp(col.b, 0f, 1f);
            return col;
        }

        public static void RGBToHSV(Color rgbColor, out float H, out float S, out float V)
        {
            if (rgbColor.b > rgbColor.g && rgbColor.b > rgbColor.r)
            {
                RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V);
            }
            else
            {
                if (rgbColor.g > rgbColor.r)
                {
                    RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V);
                }
                else
                {
                    RGBToHSVHelper(0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V);
                }
            }
        }

        private static void RGBToHSVHelper(float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V)
        {
            V = dominantcolor;
            if (V != 0f)
            {
                float num = 0f;
                if (colorone > colortwo)
                {
                    num = colortwo;
                }
                else
                {
                    num = colorone;
                }
                float num2 = V - num;
                if (num2 != 0f)
                {
                    S = num2 / V;
                    H = offset + (colorone - colortwo) / num2;
                }
                else
                {
                    S = 0f;
                    H = offset + (colorone - colortwo);
                }
                H /= 6f;
                if (H < 0f)
                {
                    H += 1f;
                }
            }
            else
            {
                S = 0f;
                H = 0f;
            }
        }

        public static Color SqrLerpColors( Color a, Color b, float proportion )
        {
            return new Color(
                SqrLerpFloats( a.r, b.r, proportion ),
                SqrLerpFloats( a.g, b.g, proportion ),
                SqrLerpFloats( a.b, b.b, proportion ));
        }

        public static float SqrLerpFloats( float a, float b, float proportion )
        {
            return Mathf.Sqrt( a * a + ( b * b - a * a ) * proportion );
        }
    }
}
