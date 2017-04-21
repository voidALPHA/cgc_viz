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
using System.IO;
using System.Threading;
using ChainViews;
using Choreography;
using JsonDotNet.Extras.CustomConverters;
using Newtonsoft.Json;
using UnityEngine;
using Utility;
using Utility.JsonConverters;

#if !UNITY_EDITOR
using System.Linq;
using Application = UnityEngine.Application;
#endif


namespace Chains
{
    [JsonObject( MemberSerialization.OptIn )]
    public class HaxxisPackage
    {
        [JsonProperty]
        public Chain Chain { get; set; }

        [JsonProperty]
        public ChainView.ChainViewModel ChainViewModel { get; set; }

        [JsonProperty]
        public ChoreographyPackage Choreography { get; set; }

#if UNITY_EDITOR
        public static string RootPath { get { return Path.Combine( Directory.GetParent( Application.dataPath ).FullName, "HaxxisPackages" ).Replace( '\\', '/' ); } }
#else
        public static string RootPath
        {
            get
            {
                var packagesRoot = Environment.GetEnvironmentVariable( "CGC_LOCAL_BUILD_ROOT" );

                if(string.IsNullOrEmpty(packagesRoot)) // CGC_LOCAL_BUILD_ROOT not set
                {
#if UNITY_STANDALONE_OSX
                    return Path.GetFullPath(Path.Combine(Directory.GetParent(Directory.GetParent(Application.dataPath).FullName).FullName, "HaxxisPackages")).Replace('\\', '/');
#else
                    return Path.GetFullPath(Path.Combine(Directory.GetParent(Application.dataPath).FullName, "HaxxisPackages")).Replace('\\', '/');
#endif
                }

                var alternatePackagesRoot = CommandLineArgs.GetArgumentValue( "AlternatePackagesRoot" );
                if (!string.IsNullOrEmpty(alternatePackagesRoot))
                    packagesRoot = alternatePackagesRoot;

                var folder = Path.Combine( packagesRoot, "HaxxisPackages" ).Replace('\\', '/');
                var versionFilename = Path.Combine(folder, "LastPackagesBuildMade.txt");

                var versionNumber = File.ReadAllLines(versionFilename).First();

                return Path.GetFullPath( Path.Combine( folder, versionNumber )).Replace( '\\', '/' );
            }
        }
#endif

        public HaxxisPackage( Chain chain, ChainView.ChainViewModel chainViewModel, ChoreographyPackage choreography )
        {
            Chain = chain;
            ChainViewModel = chainViewModel;
            Choreography = choreography;
        }


#region Serialization

        public static JsonSerializerSettings GetSerializationSettings( TypeNameHandling typeNameHandling = TypeNameHandling.Auto )
        {
            return new JsonSerializerSettings
            {
                TypeNameHandling = typeNameHandling,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                /*Converters = new JsonConverter[]
                {
                    new ColorConverter(),
                    new Vector2Converter(),
                    new Vector3Converter(),
                    new Matrix4x4Converter(),
                    new QuaternionConverter(), 
                }*/
                ContractResolver = new HaxxisContractResolver()
            };
        }

        public static void SaveJson( HaxxisPackage package, string filename )
        {
            var contents = ToJsonString( package );

            File.WriteAllText( filename, contents );
        }

        private static string ToJsonString( HaxxisPackage package )
        {
            var indentSize = (Input.GetKey( KeyCode.LeftControl ) || Input.GetKey( KeyCode.RightControl )) ? 1 : 0;
            return JsonConvert.SerializeObject(package, Formatting.Indented, GetSerializationSettings(), ' ', indentSize);   // Use indentation to keep things on separate lines, but with zero indentation
        }

        public static HaxxisPackage LoadJson( string filename )
        {
            if ( Loading )
                throw new InvalidOperationException( "Cannot load while a load is occurring." );

            Loading = true;
            var thread = new Thread( () => LoadJsonJob( filename ), 2000000 );

            thread.Start();
            thread.Join();

            var toReturn = JobLoadedPackage;

            JobLoadedPackage = null;
            Loading = false;

            return toReturn;
        }

        private static bool Loading { get; set; }
        private static HaxxisPackage JobLoadedPackage { get; set; }

        private static void LoadJsonJob( string filename )
        {
            //RealtimeLogger.Log( "Beginning LoadJson." );

            //RealtimeLogger.Log( "Read Text." );

            var serializer = JsonSerializer.Create( GetSerializationSettings() );

            serializer.Error += ( sender, args ) =>
            {
                if ( args.CurrentObject != args.ErrorContext.OriginalObject )
                    return;

                ExceptionUtility.LogException( "Serializer error callback hit. ", args.ErrorContext.Error );
            };

            HaxxisPackage haxxisPackage;

            //RealtimeLogger.Log( "Starting Deserialize." );

            using ( Stream s = new FileStream( filename, FileMode.Open, FileAccess.Read ) )
            using ( StreamReader sr = new StreamReader( s ) )
            using ( JsonReader jsonReader = new JsonTextReader( sr ) )
            {
                haxxisPackage = serializer.Deserialize< HaxxisPackage >( jsonReader );
                if ( jsonReader.Read() && jsonReader.TokenType != JsonToken.Comment )
                    throw new JsonSerializationException( "Additional text found in JSON string after finishing deserializing object." );
            }

            //RealtimeLogger.Log( "Returning HaxxisPackage." );

            JobLoadedPackage = haxxisPackage;
        }

        public static bool FileExists(string filename)
        {
            return File.Exists( filename );
        }

        public static bool IsChanged( HaxxisPackage currentPackage, string fullPathToOriginal )
        {
            if ( String.IsNullOrEmpty( fullPathToOriginal ) )
            {
                if ( currentPackage.Chain.RootGroup.Nodes.Count > 0 )
                    return true;

                if ( currentPackage.Choreography != null )
                    return true;

                return false;
            }

            if ( !File.Exists( fullPathToOriginal ) )
                return true;

            var originalPackageJsonString = File.ReadAllText( fullPathToOriginal );

            var currentPackageJsonString = ToJsonString( currentPackage );

            var hasChanged = !currentPackageJsonString.Equals( originalPackageJsonString );

            return hasChanged;
        }

#endregion


        public static string GetRelativePath( string fullPackagePath )
        {
            if ( string.IsNullOrEmpty( fullPackagePath ) )
                return string.Empty;

            if ( !fullPackagePath.StartsWith( RootPath ) )
            {
                Debug.Log( "Unexpected package root path." );

                Debug.Log( "Root path is " + RootPath );
                Debug.Log( "fullPackagePath is" + fullPackagePath );

                return string.Empty;
            }

            var relativePath = fullPackagePath.Substring( RootPath.Length );
            relativePath = relativePath.TrimStart( '\\', '/', Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar );

            //Debug.Log( "relativePath being returned is: " + relativePath );

            return relativePath;
        }

        public static string GetFullPath( string relativePackagePath )
        {
            if ( string.IsNullOrEmpty( relativePackagePath ) )
                return string.Empty;

            return Path.Combine( RootPath, relativePackagePath ).Replace( '\\', '/' );
        }
    }
}
