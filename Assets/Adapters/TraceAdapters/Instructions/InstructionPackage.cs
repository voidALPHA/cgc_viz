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
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace Adapters.TraceAdapters.Instructions
{
    public class InstructionPackage
    {
        
        #region Creation

        public InstructionPackage()
        {
        }

        public InstructionPackage( InstructionCollection instructions, uint elementsPerBlock )
        {
            ElementsPerBlock = elementsPerBlock;

            ElementCollections = SplitElements( instructions, ElementsPerBlock );
        }

        public static List< InstructionCollection > SplitElements( InstructionCollection instructions, uint elementsPerBlock )
        {
            var collections = new List< InstructionCollection >();

            var elementIndex = 0;

            InstructionCollection curCollection = null;

            foreach ( var e in instructions )
            {
                if ( elementIndex % elementsPerBlock == 0 )
                {
                    if ( curCollection != null )
                    {
                        collections.Add( curCollection );
                    }

                    curCollection = new InstructionCollection();
                }

                elementIndex++;

// ReSharper disable once PossibleNullReferenceException
                curCollection.Add( e );
            }

            collections.Add( curCollection );

            return collections;
        }

        #endregion


        [XmlIgnore]
        private List< InstructionCollection > ElementCollections
        {
            get;
            set;
        }

        [XmlIgnore]
        private string DataDirectory { get; set; }


        #region Serialized Members

        private uint m_ElementsPerBlock = 8192;
        public uint ElementsPerBlock
        {
            get { return m_ElementsPerBlock; }
            set
            {
                if ( value != 0 )
                    m_ElementsPerBlock = value;
            }
        }

        private List< string > m_Filenames = new List< string >();
        [XmlArray( "Blocks" )]
        [XmlArrayItem( "File" )]
        public List< string > Filenames
        {
            get { return m_Filenames; }
            set { m_Filenames = value; }
        }

        #endregion


        #region Public query interface

        public InstructionCollection GetAll()
        {
            if ( !HasRead )
                ReadCollections();

            var all = new InstructionCollection();

            foreach ( var c in ElementCollections )
            {
                all.AddRange( c );
            }

            return all;
        }

        #endregion


        public static string GetTraceNameFromTracePath( string tracePath )
        {
            tracePath = Environment.ExpandEnvironmentVariables( tracePath );

            return Path.GetFileName( tracePath );
        }

        private static string GetDataDirectoryNameFromTracePath( string tracePath )
        {
            tracePath = Environment.ExpandEnvironmentVariables( tracePath );

            var traceDirectoryName = Path.GetDirectoryName( tracePath );

            var traceName = GetTraceNameFromTracePath( tracePath );

            return Path.Combine( traceDirectoryName, traceName + ".exported" );
        }

        public static string GetPackagePathFromTracePath( string tracePath )
        {
            tracePath = Environment.ExpandEnvironmentVariables( tracePath );

            var dataDirectoryName = GetDataDirectoryNameFromTracePath( tracePath );

            var packageFullPath = Path.Combine( dataDirectoryName, "_tracepackage.xml" );

            return packageFullPath;
        }


        #region Writing

        public void SerializeToFile( string tracePath )
        {
            DataDirectory = GetDataDirectoryNameFromTracePath( tracePath );

            PrepareDataDirectory();

            WriteCollections();

            WritePackage();
        }

        

        private void PrepareDataDirectory()
        {
            if ( Directory.Exists( DataDirectory ) )
            {
                var dirInfo = new DirectoryInfo( DataDirectory );

                foreach ( FileInfo file in dirInfo.GetFiles() )
                    file.Delete();

                foreach ( DirectoryInfo subDirectory in dirInfo.GetDirectories() )
                    subDirectory.Delete( true );
            }
            else
            {
                Directory.CreateDirectory( DataDirectory );
            }
        }

        public void WriteCollections()
        {
            var collectionIndex = 0;

            var digitCount = ElementCollections.Count.ToString( CultureInfo.InvariantCulture ).Length -1;  // Assumes positive or 0, whole number; -1 because we're 0-based.

            foreach ( var collection in ElementCollections )
            {
                var indexString = collectionIndex.ToString( "D" + digitCount );

                var fileName = indexString + ".instructions";
                var filePath = Path.Combine( DataDirectory, fileName );

                collection.SerializeToFile( filePath );

                Filenames.Add( fileName );

                collectionIndex++;
            }
        }

        private void WritePackage()
        {
            var packageFullPath = Path.Combine( DataDirectory, "_tracepackage.xml" );
            
            using ( var fileStream = new FileStream( packageFullPath, FileMode.Create, FileAccess.Write ) )
            {
                SerializeToStream( fileStream );
                fileStream.Flush();
            }
        }

        public void SerializeToStream( Stream stream )
        {
            var serializer = new XmlSerializer( typeof( InstructionPackage ) );
            serializer.Serialize( stream, this );
        }

        #endregion


        #region Reading

        [XmlIgnore]
        public bool HasRead { get; private set; }

        public void ReadCollections()
        {
            ElementCollections = new List< InstructionCollection >();

            foreach ( var filename in Filenames )
            {
                var fullPath = Path.Combine( DataDirectory, filename );

                ElementCollections.Add( InstructionCollection.DeserializeFromFile( fullPath ) );
            }

            HasRead = true;
        }

        public static InstructionPackage DeserializeFromFile( string filename )
        {
            using ( var fileStream = new FileStream( filename, FileMode.Open, FileAccess.Read ) )
            {
                var package = DeserializeFromStream( fileStream );

                package.DataDirectory = Path.GetDirectoryName( filename );

                return package;
            }
        }


        public static InstructionPackage DeserializeFromStream( Stream stream )
        {
            var serializer = new XmlSerializer( typeof( InstructionPackage ) );

            var result = serializer.Deserialize( stream );

            var package = (InstructionPackage)result;

            return package;
        }
        
        #endregion
        
        
    }
}
