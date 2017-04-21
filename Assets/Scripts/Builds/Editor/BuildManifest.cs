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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Builds.Editor
{
    public class BuildManifest
    {
        private readonly List < BuildEntry > m_Builds = new List < BuildEntry >();
        public List < BuildEntry > Builds
        {
            get { return m_Builds; }
        }

        public event Action < BuildEntry > BuildAdded = delegate { }; 
        public void AddBuild()
        {
            var newBuild = new BuildEntry();

            Builds.Add( newBuild );

            BuildAdded( newBuild );
        }

        public event Action<BuildEntry> BuildRemoved = delegate { };
        private void RemoveBuild( BuildEntry build )
        {
            Builds.Remove( build );

            BuildRemoved( build );
        }

        public void Update()
        {
            foreach ( var build in Builds.ToList() )
            {
                if ( build.PendingDelete )
                {
                    RemoveBuild( build );
                    continue;
                }

                build.Update();
            }
        }

        #region JSON Serialization

        public static BuildManifest ReadFromFile( string path )
        {
            var json = File.ReadAllText( path );
        
            var manifest = JsonConvert.DeserializeObject < BuildManifest >( json );

            return manifest;
        }

        public static void WriteToFile( BuildManifest manifest, string path )
        {
            var json = JsonConvert.SerializeObject( manifest, Formatting.Indented );

            File.WriteAllText( path, json );
        }

        #endregion
    }
}