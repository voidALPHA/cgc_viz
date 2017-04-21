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
using Adapters.TraceAdapters.Commands;
using UnityEngine;

namespace Visualizers.CsView.FileSectionVisualizer
{
    public class SectionColorMapping : MonoBehaviour
    {
        [Serializable]
        private class SectionColor
        {
            [SerializeField]
            private Color m_Color = Color.magenta;
            public Color Color { get { return m_Color; } }

            [SerializeField]
            private BinarySectionTypes m_SectionType = BinarySectionTypes.PT_NULL;
            public BinarySectionTypes SectionType { get { return m_SectionType; } }
        }

        [Serializable]
        private class SectionFlagColor
        {
            [SerializeField]
            private Color m_Color = Color.magenta;
            public Color Color { get { return m_Color; } }

            [SerializeField]
            private BinarySectionFlags m_SectionFlag = BinarySectionFlags.READ;
            public BinarySectionFlags SectionFlag { get { return m_SectionFlag; } }
        }

        public static SectionColorMapping Instance{ get;set; }

        void Awake()
        {
            if ( Instance == null )
            {
                Instance = this;

                SectionColors = DefaultColorMappings.ToDictionary( section => section.SectionType,
                    section => section.Color );

                SectionFlagColors = DefaultFlagColorMappings.ToDictionary(section => section.SectionFlag,
                    section => section.Color);
            }
        }

        
        [SerializeField]
        private List<SectionColor> m_DefaultColorMappings;
        private List<SectionColor> DefaultColorMappings { get { return m_DefaultColorMappings; } }

        [SerializeField]
        private List<SectionFlagColor> m_DefaultFlagColorMappings;
        private List<SectionFlagColor> DefaultFlagColorMappings { get { return m_DefaultFlagColorMappings; } }


        public static Dictionary<BinarySectionTypes, Color> SectionColors { get; set; }
        public static Dictionary<BinarySectionFlags, Color> SectionFlagColors { get; set; }

        [SerializeField]
        private Color m_DefaultColor = Color.magenta;
        public static Color DefaultColor { get { return Instance.m_DefaultColor; } }

        public static Color GetSectionTypeColor( BinarySectionTypes sectionType )
        {
            if ( SectionColors.ContainsKey( sectionType ) )
                return SectionColors[ sectionType ];
            return DefaultColor;
        }

        public static Color GetSectionFlagColor( BinarySectionFlags sectionFlags )
        {
            if (SectionFlagColors.ContainsKey(sectionFlags))
                return SectionFlagColors[sectionFlags];
            return DefaultColor;
        }

    }
}
