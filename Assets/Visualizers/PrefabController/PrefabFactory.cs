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

namespace Visualizers.PrefabController
{
    public class PrefabFactory : MonoBehaviour
    {
        public static PrefabFactory Instance { get; set; }

        [SerializeField]
        private List<GameObject> m_PrefabTypes = new List< GameObject >();
        private List<GameObject> PrefabTypes { get { return m_PrefabTypes; } }

        public Dictionary< string, GameObject > Prefabs { get; set; }

        public void Awake()
        {
            Instance = this;

            Prefabs = new Dictionary< string, GameObject >();
            foreach ( var prefab in PrefabTypes )
            {
                try
                {
                    var nameOverride = prefab.GetComponent<PrefabNameOverride>();
                    Prefabs.Add((nameOverride != null ? nameOverride.PrefabName : prefab.name).ToLowerInvariant(), prefab);
                }
                catch ( System.Exception e )
                {
                    Debug.LogError( e.Message );
                }
            }
        }
    }
}
