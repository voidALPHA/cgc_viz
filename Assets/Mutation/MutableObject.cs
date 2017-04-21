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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utility;
using Object = System.Object;

namespace Mutation
{
    public class NoTypes
    {
    }

    public class MutableObject : IDictionary < string, object >
    { 

        public static MutableObject Empty { get { return new MutableObject(); } }
        
        private IDictionary < string, object > m_Objects = new Dictionary < string, object >();
        private IDictionary<string, object> Objects { get { return m_Objects; } set { m_Objects = value; } }

        public byte[] CheckSum { get; private set; }

        public void UpdateCheckSum()
        {
            CheckSum = Objects.Select(kvp => 
                CheckSumUtil.CheckSumFromStringAndType(
                    kvp.Key, kvp.Value.GetType())).ToArray();
        }

        public IEnumerator < KeyValuePair < string, object > > GetEnumerator()
        {
            return Objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool KeysMatch(MutableObject mutable)
        {
            return mutable.Keys.Count == Objects.Keys.Count 
                && mutable.Keys.All(key => Objects.ContainsKey(key));
        }

        public void Add( KeyValuePair<string, object> item )
        {
            Objects.Add( item );
        }

        public void Clear()
        {
            Objects.Clear();
        }

        public bool Contains( KeyValuePair < string, object > item )
        {
            return Objects.Contains( item );
        }

        public void CopyTo( KeyValuePair < string, object >[] array, int arrayIndex )
        {
            Objects.CopyTo( array, arrayIndex );
        }

        public bool Remove( KeyValuePair < string, object > item )
        {
            return (Objects as ICollection< KeyValuePair < string, object > >).Remove( item );
        }

        public int Count { get { return Objects.Count; } }

        public bool IsReadOnly { get { return Objects.IsReadOnly; } }

        public void Add( string key, object value )
        {
            if ( value == null )
                throw new ArgumentNullException("value", "Value of a mutable field property cannot be null.");

            Objects.Add( key, value );
        }

        public bool ContainsKey( string key )
        {
            return Objects.ContainsKey( key );
        }

        public bool Remove( string key )
        {
            return Objects.Remove( key );
        }

        public void RecursiveRemove( string[] keys )
        {
            if ( keys.Length == 0 )
                throw new Exception("Can't remove an entire mutable from itself!");

            if ( keys.Length == 1 )
                Remove( keys.Last() );

            if ( !this.ContainsKey( keys.First() ) )
                return;

            var subObject = this[ keys.First() ];

            var subMutable = subObject as MutableObject;
            if ( subMutable != null )
            {
                subMutable.RecursiveRemove( keys.Skip( 1 ).ToArray() );
                return;
            }

            var subList = subObject as IEnumerable< MutableObject >;
            if ( subList != null )
            {
                foreach (var listMutable in subList)
                    listMutable.RecursiveRemove(keys.Skip(1).ToArray());
                return;
            }

            throw new Exception("Cannot remove data with an intermediary object of type " + subObject.GetType() + "!");
        }

        public bool TryGetValue( string key, out object value )
        {
            return Objects.TryGetValue( key, out value );
        }

        public object this[ string key ]
        {
            get { return Objects[ key ]; }
            set
            {
                if ( value == null )
                    throw new ArgumentNullException("value", "Value of a mutable field property cannot be null.");

                Objects[ key ] = value;
            }
        }

        public MutableObject ShallowCopyMutableObject()
        {
            var newMutable = new MutableObject();

            foreach (var kvp in this)
                newMutable.Add(kvp);

            return newMutable;
        }

        public ICollection < string > Keys { get { return Objects.Keys; } }
        public ICollection < object > Values { get { return Objects.Values; } }

        public MutableObject()
        {
            
        }

        public MutableObject( IEnumerable< KeyValuePair< string, object > > keyValuePairs )
        {
            foreach (var kvp in keyValuePairs)
                Objects.Add( kvp );
        }

        public MutableObject( Dictionary< string, object > entries )
        {
            Objects = entries;
        }

        public static bool KeyAContainsB( string keyA, string keyB )
        {
            var ownKeys = keyA.Split( '.' ).WithoutLast().ToArray();
            var otherKeys = keyB.Split( '.' );

            if ( ownKeys.Length < otherKeys.Length )
                return false;

            for (int i=0; i<Mathf.Min(otherKeys.Length, ownKeys.Length); i++)
                if ( string.Compare( ownKeys[ i ], otherKeys[ i ], StringComparison.InvariantCulture ) != 0 )
                    return false;

            return true;
        }

        public MutableObject CloneKeys()
        {
            var newMutable = new MutableObject();

            foreach (var kvp in this)
            {
                if (kvp.Value is MutableObject)
                {
                    newMutable.Add(kvp.Key, (kvp.Value as MutableObject).CloneKeys());
                    continue;
                }

                if (kvp.Value is IEnumerable<MutableObject>)
                {
                    var enumerable = kvp.Value as IEnumerable<MutableObject>;
                    var newList = enumerable.Select(newObj => newObj.CloneKeys()).ToList();

                    newMutable.Add(kvp.Key, newList);
                    continue;
                }

                newMutable.Add(kvp.Key, kvp.Value);
            }

            return newMutable;
        }

      //  public static MutableObject UnionSchemas( List< MutableObject > schemaList )
      //  {
      //      var outSchema = new MutableObject();
      //      foreach ( var schema in schemaList )
      //      {
      //          foreach ( var kvp in schema )
      //          {
      //              if ( !outSchema.ContainsKey( kvp.Key ) )
      //              {
      //                  outSchema.Add( kvp );
      //              }
      //          }
      //      }
      //  }

        public static MutableObject UnionSchemas( MutableObject operand1, MutableObject operand2 )
        {
            var newMutable = operand1.CloneKeys();

            return UnionIntoClonedSchema( newMutable, operand1, operand2 );
        }

        private static MutableObject UnionIntoClonedSchema( MutableObject newMutable, MutableObject operand1, MutableObject operand2 )
        {
            foreach (var kvp in operand2)
            {
                if (operand1.ContainsKey(kvp.Key))
                {
                    var foundObj = operand1[kvp.Key];

                    newMutable[kvp.Key] = TypeCollisionList.CollideValues(foundObj, kvp.Value);
                    continue;
                }

                newMutable[kvp.Key] = kvp.Value;
            }

            return newMutable;
        }

        
        #region Schema Intersect


        public static MutableObject IntersectOwnSchema( MutableObject mutable )
        {
            if ( mutable == null )
                return null;

            var newValues = new Dictionary< string, object > ();

            foreach ( var kvp in mutable )
            {

                if ( kvp.Value is NoTypes )
                    continue;

                if ( kvp.Value is MutableObject )
                    newValues.Add( kvp.Key, IntersectOwnSchema( kvp.Value as MutableObject ) );
                else if ( kvp.Value is IEnumerable< MutableObject > )
                    newValues.Add( kvp.Key,
                        new List< MutableObject >()
                        {
                            IntersectSchemas( ( kvp.Value as IEnumerable< MutableObject > ).ToList() )
                        }
                        );
                else
                    newValues.Add( kvp.Key, kvp.Value );
            }

            return new MutableObject(newValues);
        }

        public static MutableObject IntersectSchemas( MutableObject operand1, MutableObject operand2 )
        {
            var schemaClone = operand1.GroupJoin( operand2, kvp => kvp.Key, kvp => kvp.Key,
                ( op1, matchedOp2s ) => 
                    IntersectPairs(op1, matchedOp2s)
                );

            var newSchema = new MutableObject(
                RemoveTypelessEntries(schemaClone).ToDictionary( 
                kvp=>kvp.Key, kvp=>kvp.Value ));

            return newSchema;
        }

        private static IEnumerable< KeyValuePair< string, object > > RemoveTypelessEntries(
            IEnumerable< KeyValuePair< string, object > > entries )
        {
            foreach ( var kvp in entries )
            {
                if ( !( kvp.Value is NoTypes ) )
                    yield return kvp;
            }
        }


       // private void IntersectIntoOwnSchema(MutableObject operand)
       // {
       //     Objects = this.GroupJoin(operand, kvp => kvp.Key, kvp => kvp.Key,
       //         IntersectPairs).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
       // }

        public static MutableObject IntersectSchemas( List< MutableObject > schemas )
        {
            if ( schemas.Count() < 2 )
                return IntersectOwnSchema(schemas.FirstOrDefault());

            var outSchema = schemas.First();

            foreach ( var otherSchema in schemas.Skip( 1 ) )
                outSchema = IntersectSchemas( outSchema, otherSchema );

            return outSchema;
        }

        public static KeyValuePair< string, object > IntersectPairs( KeyValuePair< string, object > op1,
            IEnumerable<KeyValuePair< string, object > > matchedOp2s )
        {
            if ( op1.Value is NoTypes )
                return op1;

            if (!matchedOp2s.Any())
                return new KeyValuePair<string, object>(op1.Key, new NoTypes());

            var op1Enumeration = op1.Value as IEnumerable< MutableObject >;
            if ( op1Enumeration !=null )
            {
                var op1List = op1Enumeration.ToList();
                if ( !op1List.Any() )
                    return op1;

                if ( matchedOp2s.Select( op2 => op2.Value as IEnumerable< MutableObject > ).Any( otherList => otherList==null ) )
                {
                    return new KeyValuePair< string, object >(op1.Key, new NoTypes());
                }

                var foundSchema = IntersectSchemas( op1List.Concat(
                    matchedOp2s.Select( op2 => op2.Value as IEnumerable< MutableObject > )
                        .SelectMany( x => x ) ).ToList());

                return new KeyValuePair<string, object>(op1.Key, new List<MutableObject>(){foundSchema});
            }

            var mutOp = op1.Value as MutableObject;
            if ( mutOp != null )
            {
                if ( !mutOp.Any() )
                    return op1;

                if (matchedOp2s.Select(op2 => op2.Value as MutableObject).Any(otherMut => otherMut == null))
                {
                    return new KeyValuePair<string, object>(op1.Key, new NoTypes());
                }

                var foundSchema = IntersectSchemas( new []{mutOp}.Concat( matchedOp2s.Select(
                    op2 => op2.Value as MutableObject)).ToList());

                return new KeyValuePair< string, object >(op1.Key, foundSchema);
            }
                
            foreach ( var op2 in matchedOp2s )
            {
                if ( !op1.Value.GetType().IsInstanceOfType( op2.Value ) )
                {
                    return new KeyValuePair< string, object >(op1.Key, new NoTypes());
                }
            }
            return op1;

        }

        

        #endregion
        
          
        public static List< MutableObject > GetUniqueSchemas( IEnumerable< MutableObject > mutables )
        {
            var uniqueSchemas = new Dictionary< byte[], MutableObject >();
            foreach ( var mut in mutables )
            {
                mut.UpdateCheckSum();

                bool foundCheckSum = FindExistingCheckSum( uniqueSchemas, mut );

                if (!foundCheckSum)
                    uniqueSchemas.Add( mut.CheckSum, mut );
            }

            return uniqueSchemas.Values.ToList();
        }

        private static bool FindExistingCheckSum( Dictionary< byte[], MutableObject > uniqueSchemas, MutableObject mut )
        {
            foreach ( var kvp in uniqueSchemas )
            {
                if ( kvp.Key.SequenceEqual( mut.CheckSum ) )
                    return true;
            }
            return false;
        }

        public static MutableObject UnionSchemas( MutableObject schema, IEnumerable< MutableObject > otherSchemas )
        {
            var newSchema = schema.CloneKeys();
            foreach ( var entry in otherSchemas )
                newSchema = UnionIntoClonedSchema( newSchema, newSchema, entry );
            return newSchema;
        }

        public static MutableObject UnionSchemas( IEnumerable< MutableObject > schemas )
        {
            if ( !schemas.Any() )
            {
                return new MutableObject();
            }
            return UnionSchemas(schemas.First(), schemas.Skip( 1 ));
        }

        public void DebugMutable()
        {
            Debug.Log( DebugMutable(this));
        }

        public static string DebugMutable(MutableObject mutable, string keySignature = "")
        {
            string DebugString = "";

            foreach (var kvp in mutable)
            {
                DebugString+=(keySignature + kvp.Key + " : " + kvp.Value)+" \n ";

                var subMutable = kvp.Value as MutableObject;
                if (subMutable != null)
                    DebugMutable(subMutable, keySignature + kvp.Key + ".");

                var subList = kvp.Value as IEnumerable<MutableObject>;
                if (subList != null)
                    foreach (var entry in subList)
                        DebugMutable(entry, keySignature + kvp.Key + ".");
            }

            return DebugString;
        }


        public static MutableObject SchemaFromType( Type t )
        {
            var output = new MutableObject();

            foreach ( var property in t.GetProperties() )
            {
                var value =  Activator.CreateInstance( t );

                output.Add( property.Name, value );
            }


            return output;
        }


        public static MutableObject FromObject( System.Object original )
        {
            var output = new MutableObject();
            
            foreach ( var property in original.GetType().GetProperties() )
            {
                AddPropertyToMutableObject( property, original, output );
            }

            return output;
        }

        private static void AddPropertyToMutableObject( PropertyInfo propertyInfo, System.Object resolutionObject, MutableObject mutableObject )
        {
            var value = propertyInfo.GetValue( resolutionObject, null );

            //var array = (Array)value;
            //if ( array != null )
            //{
            //    mutableObject.Add( propertyInfo.Name, "Array Skipped" );

            //    return;
            //}

            //var properties = propertyInfo.PropertyType.GetProperties();

            //if ( properties.Any() )
            //{
            //    mutableObject.Add( propertyInfo.Name, value );

            //    return;
            //}


            mutableObject.Add( propertyInfo.Name, value );

            //foreach ( var childPropertyInfo in value.GetType().GetProperties() )
            //{
            //    var childMutable = new MutableObject();
            //    AddPropertyToMutableObject( childPropertyInfo, value, childMutable );
            //    mutableObject.Add( childPropertyInfo.Name, childMutable );
            //}
        }
    }
}
