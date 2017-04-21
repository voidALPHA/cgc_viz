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
using Mutation;
using Utility;

namespace Adapters.GlobalParameters
{
    public class GlobalVariableDataStore : ISchemaProvider
    {
        public event Action GlobalSchemaChanged = delegate { };


        private static GlobalVariableDataStore s_Instance;
        public static GlobalVariableDataStore Instance {
            get
            {
                return s_Instance ?? ( s_Instance = new GlobalVariableDataStore () ); 
            } }

        private readonly Dictionary< string, MutableObject > m_DataStore = new Dictionary< string, MutableObject >();
        private Dictionary< string, MutableObject > DataStore
        {
            get { return m_DataStore; }
        }

        public void WriteToDataStore( string writerKey, MutableObject value )
        {
            DataStore[ writerKey ] = value;

            foreach ( var inputValue in value )
            {
                foreach ( var pair in DataStore.Where( kvp => kvp.Key != writerKey ) )
                {
                    if (pair.Value.ContainsKey( inputValue.Key ))
                        pair.Value[ inputValue.Key ] = inputValue.Value;
                }
            }
            GlobalSchemaChanged();
        }

        public void RemoveFromDataStore( string key )
        {
            if ( DataStore.ContainsKey( key ) )
                DataStore.Remove( key );
            GlobalSchemaChanged();
        }

        public IEnumerable< KeyValuePair< string, object > > ParameterList()
        {
            return from foundMutable in
                       DataStore.Select(kvp => kvp.Value).OfType<MutableObject>() 
                   from pair in foundMutable select pair;
        }

        public IEnumerable< string > VariableKeys()
        {
            return from foundMutable in
                       DataStore.Select(kvp => kvp.Value).OfType<MutableObject>() 
                   from pair in foundMutable select pair.Key;
        }

        public bool IsGlobalKeyValid( string key )
        {
            var keysList = key.Split( '.' );

            if ( !VariableKeys().Contains( keysList.FirstOrDefault() ) )
                return false;
            
            if ( keysList.Length == 1 )
                return true;

            var currentLevel = GetParameter(keysList.FirstOrDefault());

            var mutableLevel = currentLevel as MutableObject;

            var testField = new MutableField< object >() { AbsoluteKey = keysList.Skip(1).Aggregate( (a,b)=>a+"."+b ) };
            return testField.IsFieldResolvable( mutableLevel );
        }

        public object GetParameter(string key)
        {
            var keysList = key.Split('.');

            var firstKey = keysList.First();

            var foundResult = (from foundMutable in
                        DataStore.Select(kvp => kvp.Value)
                from pair in foundMutable
                where pair.Key == firstKey 
                   select pair.Value).FirstOrDefault();

            if (keysList.Length==1)
                return foundResult;
            
            var mutableLevel = foundResult as MutableObject;

            var testField = new MutableField<object>() { AbsoluteKey = keysList.Skip(1).Aggregate((a, b) => a + "." + b) };
            return testField.GetFirstValue( mutableLevel );
        }

        public void Unload()
        {
            //DataStore = new MutableObject();
        }

        //public static MutableObject ReadFromDataStore
        //{
        //    get { return DataStore; }
        //}
        public string SchemaName { get { return "Global Parameters"; } }

        public MutableObject Schema
        {
            get
            {
                var schemaObj = new MutableObject();
                foreach ( var arg in DataStore )
                {
                    var foundMutable = arg.Value as MutableObject;
                    if ( foundMutable == null )
                        continue;
                    foreach ( var subArg in foundMutable )
                        schemaObj[ subArg.Key ] = subArg.Value;
                }
                return schemaObj;
            }
        }

        public virtual void CacheSchema()
        {
            CachedMutableDataStore.ClearDataCache();
        }

        public virtual void UnCacheSchema()
        {
            CachedMutableDataStore.ClearDataCache();
        }
    }
}
