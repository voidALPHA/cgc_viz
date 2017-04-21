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
using UnityEngine;

namespace Visualizers.LabelController
{
    [Serializable]
    public class FontPair
    {
        [SerializeField]
        private Font m_Font;
        public Font Font { get { return m_Font; } }

        [SerializeField]
        private Texture2D m_FontTexture = null;
        public Texture2D FontTexture { get { return m_FontTexture; } }

        [SerializeField]
        [Tooltip("The height difference per point of font")]
        private float m_VerticalOffset = 0f;
        public float VerticalOffset { get { return m_VerticalOffset; } }

        [SerializeField]
        [Tooltip("The multiplicative scaling of the font")]
        private float m_FontScale = 1f;
        public float FontScale { get { return m_FontScale; } }
    }

    public class FontFactory : MonoBehaviour
    {
        public static FontFactory Instance { get; set; }

        [SerializeField]
        private List<FontPair> m_FontMaterials = new List<FontPair>();
        private List<FontPair> FontMaterials { get { return m_FontMaterials; } }

        [SerializeField]
        private FontPair m_DefaultFont;
        private FontPair DefaultFont { get { return m_DefaultFont; } }

        [SerializeField]
        private Material m_SpatialTextMaterial = null;
        private Material SpatialTextMaterial { get { return m_SpatialTextMaterial; } }

        public Dictionary<string, FontPair> Fonts { get; set; }

        public void Awake()
        {
            Instance = this;

            Fonts = new Dictionary< string, FontPair >();

            foreach ( var font in FontMaterials )
            {
                Fonts.Add( font.Font.name, font);
            }
        }

        public static FontPair GetFontPair(string fontName)
        {
            if ( Instance.Fonts.ContainsKey( fontName ) )
                return Instance.Fonts[ fontName ];
            return Instance.DefaultFont;
        }

        public static Material GenerateNewSpatialMaterial( Texture2D fontTexture )
        {
            var newMat = GameObject.Instantiate( Instance.SpatialTextMaterial );

            newMat.SetTexture( "_MainTex", fontTexture );

            return newMat;
        }
    }
}
