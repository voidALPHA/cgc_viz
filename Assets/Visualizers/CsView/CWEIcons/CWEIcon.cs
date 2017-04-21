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
using UnityEngine;

namespace Visualizers.CsView.CWEIcons
{
    [Serializable]
    public class CWEIcon
    {
        public CWEIcon( Texture2D texture, string name, string description )
        {
            Texture = texture;
            Name = name;
            Description = description;
        }

        public CWEIcon( CWEIcon toCopy )
        {
            Name = toCopy.Name;
            Description = toCopy.Description;
            Texture = m_Texture;
            Material = toCopy.Material;
        }

        private Material m_Material;

        public Material Material
        {
            get
            {
                if ( m_Material == null )
                {
                    if ( Texture != null )
                    {
                        m_Material = new Material( CWEIconFactory.Instance.IconMaterialTemplate );
                        m_Material.mainTexture = Texture;
                    }
                    else
                    {
                        m_Material = CWEIconFactory.Instance.IconMaterialTemplate;
                    }
                }

                return m_Material;
            }
            private set { m_Material = value; }
        }

        [SerializeField]
        private Texture2D m_Texture = null;
        public Texture2D Texture { get { return m_Texture; } set { m_Texture = value; } }

        [SerializeField]
        private string m_Name = string.Empty;
        public string Name { get { return m_Name; } set { m_Name = value; } }

        [SerializeField]
        private string m_Description = string.Empty;
        public string Description { get { return m_Description; } set { m_Description = value; } }
    }
}
