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
using System.Text;
using UnityEditor;

namespace Assets.Builds.Editor
{
    public class BuildPathEnvironmentVariable
    {
        public string Key { get; private set; }

        public string Purpose { get; private set; }

        
        public string Value { get; private set; }

        public bool Validated { get; private set; }

        public string Error { get; private set; }


        public BuildPathEnvironmentVariable( string key, string purpose )
        {
            Key = key;
            Purpose = purpose;

            Value = Environment.GetEnvironmentVariable( Key );

            if ( Value != null )
                if ( !Value.EndsWith( "/" ) )
                    Value += "/";

            Test();
        }


        private void Test()
        {
            Validated = true;
            if ( Value == null )
            {
                Validated = false;
                Error = "Environment variable not set. Ensure Unity is restarted after setting it.";

                return;
            }

            if ( !Validated )
            {
                if ( !Directory.Exists( Value ) )
                {
                    Validated = false;
                    Error = String.Format( "Environment variable set, but value ({0}) is not a valid path.", Value );
                }
            }
        }

        public void DrawStatusGui()
        {
            if ( Validated )
            {
                var heading = string.Format( "Environment variable {0} set to {1}.", Key, Value );
                var purpose = Purpose;
                var detail = "Details: No apparent problems.";

                var message = String.Format( "{0}\n\n{1}\n\n{2}", heading, purpose, detail );

                EditorGUILayout.HelpBox( message, MessageType.None );

            }
            else
            {
                var heading = string.Format( "Environment variable {0} must be set.", Key );
                var purpose = Purpose;
                var detail = "Details: " + Error;

                var message = String.Format( "{0}\n\n{1}\n\n{2}", heading, purpose, detail );

                EditorGUILayout.HelpBox( message, MessageType.Error );
            }
        }
    }
}
