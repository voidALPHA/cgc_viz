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
using UnityEngine;

namespace Mutation.Mutators.VisualModifiers
{
    public class MaterialFactory : MonoBehaviour
    {
        public static MaterialFactory Instance { get; set; }

        [SerializeField]
        private List<Material> m_Materials = new List<Material>();
        private List<Material> Materials { get { return m_Materials; } }

        [SerializeField]
        private Material m_DefaultMaterial;
        private Material DefaultMaterial { get { return m_DefaultMaterial; } }

        public Dictionary<string, Material> AvailableMaterials { get; set; }

        public void Awake()
        {
            Instance = this;

            AvailableMaterials = new Dictionary<string, Material>();
            foreach ( var mat in Materials )
            {
                try
                {
                    AvailableMaterials.Add( mat.name, mat );
                }
                catch ( System.Exception e )
                {
                    Debug.LogError( e.Message );
                }
            }
        }

        public static Material GetMaterial( string materialName )
        {
            if ( Instance.AvailableMaterials.ContainsKey( materialName ) )
                return Instance.AvailableMaterials[materialName];
            return Instance.DefaultMaterial;
        }

        public static Material GetDefaultMaterial()
        {
            return Instance.DefaultMaterial;
        }
    }
}
