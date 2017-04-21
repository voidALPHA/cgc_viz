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
using System.Collections;
using Assets.Utility;
using Chains;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Utility;
using Visualizers;

namespace Choreography.Steps
{
    public class ErrorStep : Step
    {
        private string m_FailedTypeString = String.Empty;
        [Controllable( LabelText = "Failed Type" ), UsedImplicitly]
        public string FailedTypeString
        {
            get { return m_FailedTypeString; }
            set
            {
                m_FailedTypeString = value;
                NewTypeString = m_FailedTypeString;
            }
        }

        [JsonProperty, UsedImplicitly]
        public JToken JToken { get; set; }

        [Controllable( LabelText = "JSON" ), UsedImplicitly, JsonIgnore]  // Good candidate (and others here) for a ReadOnlyControllable. Uncontrollable?
        public string JTokenString { get { return JToken == null ? String.Empty : JToken.ToString( Formatting.Indented ); } set { /*Intentionally drop value*/ } }


        private string m_NewTypeString = String.Empty;
        [Controllable, UsedImplicitly, JsonIgnore]
        public string NewTypeString
        {
            get { return m_NewTypeString; }
            set { m_NewTypeString = value; }
        }

        [Controllable, UsedImplicitly]
        public void Fix()
        {
            if ( string.IsNullOrEmpty( NewTypeString ) )
            {
                Debug.Log( "NewTypeString is not valid." );
                return;
            }

            var foundType = Type.GetType( NewTypeString );

            if ( foundType == null )
            {
                Debug.Log("Could not find type " + NewTypeString + "." );
                return;
            }

            var jObject = JToken as JObject;
            if ( jObject == null )
            {
                Debug.Log( "jObject null" );
                return;
            }


            jObject.Property( "$type" ).Value = foundType.ShortAssemblyQualifiedName();


            var serializer = JsonSerializer.Create( HaxxisPackage.GetSerializationSettings() );
            
            var item = serializer.Deserialize< Step >( jObject.CreateReader() );

            RequestReplacement( item );
        }
        
        protected override IEnumerator ExecuteStep()
        {
            throw new InvalidOperationException("Cannot execute an Error Step. It should be manually replaced or removed.");
        }
    }
}