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
using System.Text;
using UnityEngine;
using Visualizers.PrefabController;

namespace Assets.Visualizers.ParticleSystemController
{
    public class ParticleSystemFactory : MonoBehaviour
    {
        public static ParticleSystemFactory Instance { get; set; }

        [SerializeField]
        private List<ParticleSystem> m_ParticleSystemPrefabs = new List<ParticleSystem>();
        private List<ParticleSystem> ParticleSystemPrefabs { get { return m_ParticleSystemPrefabs; } }

        public Dictionary<string, ParticleSystem> ParticleSystems { get; set; }

        public void Awake()
        {
            Instance = this;

            ParticleSystems = new Dictionary<string, ParticleSystem>();
            foreach (var prefab in ParticleSystemPrefabs)
            {
                var nameOverride = prefab.GetComponent<PrefabNameOverride>();
                ParticleSystems.Add(nameOverride != null ? nameOverride.PrefabName : prefab.name, prefab);
            }
        }

        public static ParticleSystem GenerateParticleSystem(String systemName)
        {
            return Instantiate(Instance.ParticleSystems[systemName].gameObject).GetComponent<ParticleSystem>();
        }
    }
}
