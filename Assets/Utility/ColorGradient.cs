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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utility
{
    public class ColorGradient
    {
        private SortedList<float, GradientColorKey> m_ColorKeys;
        public SortedList<float, GradientColorKey> ColorKeys { get { return m_ColorKeys; } set { m_ColorKeys = value; } }

        public ColorGradient(int nKeys)
        {
            m_ColorKeys = new SortedList<float, GradientColorKey>(nKeys);
            //for (int i=0;i<nKeys;i++)
            //    m_ColorKeys.Add(i/(float)nKeys, new GradientColorKey());
        }

        public void RescaleColorKeys( float totalTime )
        {
            var newKeys = new SortedList< float, GradientColorKey >();
            foreach ( var key in ColorKeys )
            {
                var assortKey = key.Key/ totalTime;
                if ( newKeys.ContainsKey(assortKey) )
                    assortKey += .001f;
                    //Debug.LogError( "Key already exists!  Seek better indexing" );
                newKeys.Add( assortKey, new GradientColorKey( key.Value.color, key.Value.time / totalTime ) );
            }
            ColorKeys = newKeys;
        }

        public void AddColorKey(GradientColorKey newKey)
        {
            if (ColorKeys.ContainsKey(newKey.time))
            {
                GradientColorKey foundKey;
                ColorKeys.TryGetValue(newKey.time, out foundKey);
                foundKey.color = newKey.color;
                return;
            }

            ColorKeys.Add(newKey.time, newKey);
        }

        public Color Evaluate(float time)
        {
            //return Color.magenta;

            var keyPair = KeysAround( time );

            var lowerKey = keyPair.FirstKey;
            var upperKey = keyPair.LastKey;

            var proportion = (time - lowerKey.time) / (upperKey.time - lowerKey.time);

            return LerpColors(lowerKey.color, upperKey.color, proportion);

        }

        private class ColorKeyPair
        {
            public GradientColorKey FirstKey { get; set; }
            public GradientColorKey LastKey { get; set; }
        }

        private ColorKeyPair KeysAround( float targetTime )
        {
            var upperKey = ColorKeys.Count - 1;
            var lowerKey = 0;

            while ( upperKey - lowerKey > 1 )
            {
                var middleKey = ( upperKey + lowerKey ) / 2;
                var middleValue = ColorKeys.Values[middleKey].time;
                if ( middleValue > targetTime )
                    upperKey = middleKey;
                else lowerKey = middleKey;
            }

            return new ColorKeyPair {FirstKey = ColorKeys.Values[lowerKey], LastKey = ColorKeys.Values[upperKey]};
        }

        public static Color LerpColors(Color startColor, Color endColor, float value)
        {
            return Color.Lerp(startColor, endColor, value);


            //return new Color(
            //    startColor.r + (endColor.r - startColor.r) * value,
            //    startColor.g + (endColor.g - startColor.g) * value,
            //    startColor.b + (endColor.b - startColor.b) * value,
            //    startColor.a + (endColor.a - startColor.a) * value);
        }
    }
}
