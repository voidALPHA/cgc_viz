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
using Adapters.GlobalParameters;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using Utility;

namespace Mutation
{

    // an object that may be retrieved by absolute key
    public interface IAbsoluteKeyAssignable
    {
        string AbsoluteKey { get; set; }

        int NumberOfIntermediates { get; }

        bool ValidateKey( MutableObject mutable );

        //bool AbsoluteResolvable(MutableObject mutable);

        event Action AbsoluteKeyChanged; 

        bool KeyValid { get; }

        event Action<bool> KeyValidChanged;

        SchemaSource SchemaSource { get; }
    }

    // an object that may have a literal value assigned to it rather than a key reference
    public interface ILiteralValueAssignable
    {
        bool SetLiteralValueAsObject( object value );

        string GetLiteralValueAsString();

        bool UseLiteralValue { get; }
    }

    // an object that may be specified to mean a keyed value from the global values storage
    public interface IGlobalParameterAssignable
    {
        bool UseGlobalParameter { get; }

        string GlobalParameterKey { get; set; }

        //MutableObject GetCurrentScopeMutable( MutableObject mutable );
    }

    // an object that may be specified to mean a keyed value from the currently cached data collection (rather than the normal payload object)
    public interface ICachedSchemaAssignable : IAbsoluteKeyAssignable
    {
        bool UseCachedData { get; set; }

        bool UseMutableData { get; set; }
    }

    public interface IMutableField : ICachedSchemaAssignable, IGlobalParameterAssignable, ILiteralValueAssignable
    {
        event Action ValueChanged;

        new SchemaSource SchemaSource { get; set; }

        IEnumerable< List< MutableObject > > GetEntries( MutableObject mutable );
    }
    
    // the sources to which a mutable object may refer
    public enum SchemaSource
    {
        Literal,
        Mutable,
        Cached,
        Global
    }

    public static class MutableFieldTester
    {
        public static bool IsMutableField(Type givenType)
        {
            if (!givenType.IsGenericType)
                return false;

            return givenType.GetGenericTypeDefinition() == typeof(MutableField<int>).GetGenericTypeDefinition();
        }
    }

    // The mutablefield is the normal way by which a chainNode may retrieve information.  Each mutablefield has a specific
    //  type which may be satisfied by the package designer in one of four ways:
    //  Literal: The mutable field contains a string value which can be parsed to produce the output type.  This value
    //   is serialized alongside the chainNode.
    //  Mutable: The mutable field contains a key into a the dictionary structure it expects to be passed as the local 
    //   payload.  When evaluated this mutable field will be passed a mutableObject, which it will use the key to index
    //   into.  The value contained in the mutableObject at this key will be of the output type.  This allows a mutableField 
    //   to utilize data produced by the chain it is part of.
    //  Global: The mutable field contains a key into a dictionary of global values.  When evaluated this mutable field will
    //   not use the local mutable object but the statically-accessible global domain to resolve its key.  This allows a 
    //   mutableField to make use of values stored by WriteGlobalValue nodes.
    //  Cached: The mutable field contains a key into a mutable object that is cached by its local context.  This cached
    //   object differs from the normal payload, but is otherwise treated like the mutable payload: a local key is used
    //   as an index to this dictionary.
    //  During package editing the author sets a mutable field to produce data from one of these sources in order to provide
    //   information from one chainNode to another.

    [JsonObject( MemberSerialization = MemberSerialization.OptIn )]
    public class MutableField <T> : IMutableField
    {
        public string LastKey
        {
            get { return AbsoluteKey.Split('.').LastOrDefault(); }
            set
            {
                AbsoluteKey = AbsoluteKey.TrimEnd(LastKey.ToCharArray()) + value;
            }
        }

        public List<string> IntermediateKeys
        {
            get { return !AbsoluteKey.Contains( "." )?new List< string >() : AbsoluteKey.Split('.').WithoutLast().ToList(); }
            set
            {
                string newKey = value.Aggregate("", (current, str) => current + (str + "."));
                newKey += LastKey;
                AbsoluteKey = newKey;
            }
        }
        public string IntermediateKeyString
        {
            get { return AbsoluteKey.TrimEnd(("." + LastKey).ToCharArray()); }
            set { AbsoluteKey = value + (value != "" ? "." : "") + LastKey; }
        }

        public List< string > IntermediateKeysWithoutParent
        {
            get
            {
                return SchemaParent == null
                    ? IntermediateKeys
                    : IntermediateKeys.GetRange( SchemaParent.NumberOfIntermediates+1,
                        NumberOfIntermediates - (SchemaParent.NumberOfIntermediates+1) );
            }
        }

        public int NumberOfIntermediates { get { return IntermediateKeys.Count; } }

        public event Action ValueChanged = delegate { };

        /// //////////////
        
        #region Schema Source
        private SchemaSource m_SchemaSource = SchemaSource.Mutable;

        [JsonProperty (Order=1)]
        public SchemaSource SchemaSource
        {
            get { return m_SchemaSource; }
            set
            {
                m_SchemaSource = value;
            }
        }
        #endregion

        #region Literal Value

        public bool SetLiteralValueAsObject( object value )
        {
            // Will this work?
            if ( value is T )
            {
                LiteralValue = (T) value;
                return true;
            }

            return false;
        }

        public string GetLiteralValueAsString()
        {
            if(LiteralValue == null) return "";
            return LiteralValue.ToString();
        }

        private T m_LiteralValue = default( T );
        [JsonProperty]
        public T LiteralValue
        {
            get
            {
                return m_LiteralValue;
            }
            set
            {
                m_AbsoluteKey = "";

                bool valueChanged = ( !( m_LiteralValue != null && m_LiteralValue.Equals( value ) ) || !UseLiteralValue );

                m_LiteralValue = value;
                UseLiteralValue = true;

                if (valueChanged)
                    ValueChanged();
            }
        }

        public bool UseLiteralValue
        {
            get
            {
                return SchemaSource == SchemaSource.Literal;
            }
            private set
            {
                if ( value==UseLiteralValue || !value )
                    return;
                SchemaSource = SchemaSource.Literal;
                ValueChanged();
            }
        }

        #endregion

        #region Keying and Type Specification

        private string m_AbsoluteKey = "";
        [JsonProperty]
        public string AbsoluteKey
        {
            get { return m_AbsoluteKey; }
            set
            {
                if ( m_AbsoluteKey == value )
                    return;

                m_AbsoluteKey = value;
                InvalidateParentDependencies();
                AbsoluteKeyChanged();
                ValueChanged();
            }
        }

        
        public bool UseGlobalParameter
        {
            get { return SchemaSource == SchemaSource.Global; }
            private set
            {
                if (value == UseGlobalParameter) return;
                SchemaSource = SchemaSource.Global;
                ValueChanged();
            }
        }

        public bool UseCachedData
        {
            get { return SchemaSource == SchemaSource.Cached; }
            set
            {
                if ( value == UseCachedData ) return;
                SchemaSource = value?SchemaSource.Cached:SchemaSource.Mutable;
                ValueChanged();
            }
        }

        public bool UseMutableData
        {
            get { return SchemaSource == SchemaSource.Mutable; }
            set
            {
                if ( value == UseMutableData ) return;
                SchemaSource = value ? SchemaSource.Mutable : SchemaSource.Cached;
                ValueChanged();
            }
        }

        [JsonProperty]
        public string GlobalParameterKey
        {
            get { return AbsoluteKey; }
            set
            {
                var valueChanged = GlobalParameterKey != value || !UseGlobalParameter;

                AbsoluteKey = value;
                UseGlobalParameter = true;

                if (valueChanged)
                    ValueChanged();
            }
        }

        #endregion

        // JSON.NET conditional serialization methods. Usage inferred by method name.
        public bool ShouldSerializeLiteralValue() { return UseLiteralValue; }
        public bool ShouldSerializeAbsoluteKey() { return (!UseLiteralValue&&!UseGlobalParameter); }
        public bool ShouldSerializeGlobalParameterKey() { return UseGlobalParameter; }

        private IAbsoluteKeyAssignable m_SchemaParent = null;
        public IAbsoluteKeyAssignable SchemaParent
        {
            get { return m_SchemaParent; }
            set
            {
                if (m_SchemaParent != null)
                    m_SchemaParent.AbsoluteKeyChanged -= InvalidateParentDependencies;

                m_SchemaParent = value;

                if ( m_SchemaParent != null )
                    m_SchemaParent.AbsoluteKeyChanged += InvalidateParentDependencies;

                InvalidateParentDependencies();
            }
        }

        public IAbsoluteKeyAssignable SchemaPattern { get; set; }

        private void InvalidateParentDependencies()
        {
            if ( !CheckParentAndPattern() )
                KeyValid = false;
        }

        private bool m_KeyDependenciesValid = false;
        private bool KeyDependenciesValid { get { return m_KeyDependenciesValid; }
            set
            {
                if (m_KeyDependenciesValid == value)
                    return;

                m_KeyDependenciesValid = value;
                //OnFieldValid(m_KeyValidated);
            } }

        //public event Action<bool> OnFieldValid = delegate { }; 

        public event Action AbsoluteKeyChanged = delegate { }; 
        

        private MutableObject SwitchToCachedMutable( MutableObject mutable )
        {
            return UseCachedData ? CachedMutableDataStore.DataStore : mutable;
        }

        //[Obsolete("GetValue with one mutable object is mostly deprecated.  Pass an arrity resolution list (List<MutableObject>) instead or use GetFirstValue if you expect just one resolution.")]
        public T GetLastKeyValue(MutableObject mutable)
        {
            if (UseLiteralValue)
                return LiteralValue;

            if ( UseGlobalParameter )
                return (T)(GlobalVariableDataStore.Instance.GetParameter(GlobalParameterKey));

            var localMutable = SwitchToCachedMutable(mutable);
            
            if (AbsoluteKey == "")
            {
                try
                {
                    return (T)(mutable as object);
                }
                catch (Exception)
                {
                    Debug.LogWarning("Warning: base level mutable is always a mutable object!");
                    return default(T);
                }
            }

            try
            {
                return (T)localMutable[LastKey];
            }
            catch (Exception)
            {
                Debug.LogWarning("Warning: Only the default object is available here.  Continuing...");
                return default (T);
            }
        }


        //  Mutable Fields are frequencly responsible for resolving GetValue calls from nonspecific arrity.  In order to do 
        //   this most cases combine the GetEntries and GetValue calls.  As an example, in order to get the highest score
        //   from a list of 7 teams (payload.teams.score) using a mutableField 'Score', a chainNode might include the 
        //   following:
        //
        //   float maxScore=0;
        //   foreach (var entry in Score.GetEntries(payload.data)){
        //      float localScore = Score.GetValue(entry);
        //      if (localScore>maxScore) maxScore=localScore;
        //   }
        //     
        //   The GetEntries call iterates through all possible ways to resolve its definition.  In this case the
        //    mutableField 'Score' has been assigned the absolute key 'teams.score'.  This key can be thought of as two 
        //    distinct keys: 'team' and 'score'.  In this case the payload contains a key-value pair with the key 'team'
        //    whose value is an enumerable of seven MutableObjects.  There are thus seven ways to resolve the 'team' key,
        //    each of which produces a mutableObject.  For every such object the mutableField will now attempt to resolve
        //    the 'Score' key, which should reference a float.  The GetEntries call will enumerate through each of these
        //    possibilities by producing a List<MutableObject> containing the mutableObjects at each level of the 
        //    resolution list, while the GetValue call will use that list to return the actual value the field should
        //    return from each one.
        //
        //   The arrity resolution lists produced by GetEntries have another useful property: they are not specific
        //    to the mutableField whose key was used to produce them, so they can be utilized by multiple mutable fields
        //    at the same time as long as they share an operating score.  One example of this would be a node that (for
        //    some reason) records the disassembly of the instruction with the highest EIP.  Such a node might ingest
        //    a payload that includes 'payload.instructions.EIP' and 'payload.instructions.disasm', and could operate
        //    as follows:
        //
        //   uint maxEIP=0;
        //   string foundDisasm;
        //   foreach (var entry in EIPField.GetEntries(payload.data)){
        //      uint localEIP = EIPField.GetValue(entry);
        //      if (localEIP>maxEIP){
        //          maxEIP = localEIP;
        //          foundDisasm = DisasmField.GetValue(entry);
        //      }
        //   }
        // 
        //   In this case every possible arrity resolution of EIPField contains all the information to also resolve
        //    DisasmField, so the same entry can be resolved to get information from either mutable field.

        // arrity-nonspecific getValue call
        public T GetValue(List<MutableObject> mutableData)
        {
            if (UseLiteralValue)
                return LiteralValue;

            if (UseGlobalParameter)
                return (T)(GlobalVariableDataStore.Instance.GetParameter(GlobalParameterKey));
            

            if ( AbsoluteKey == "" )
            {
                try
                {
                    return (T) (mutableData.First() as object);
                }
                catch ( Exception )
                {
                    Debug.LogWarning( "Warning: base level mutable is always a mutable object!" );
                    return default( T );
                }
            }

            try
            {
                var mutableIndex = Mathf.Min( mutableData.Count-1, IntermediateKeys.Count );

                var localMutable = mutableData[ mutableIndex];
                while ( mutableIndex < IntermediateKeys.Count )
                {
                    var nextLevel = localMutable[ IntermediateKeys[ mutableIndex ] ] as MutableObject;
                    if (nextLevel==null)
                        throw new Exception("Can't descend through this local object!");
                    localMutable = nextLevel;
                    mutableIndex++;
                }

                return (T) localMutable[LastKey];
            }
            catch (Exception e)
            {
                Debug.LogWarning("Warning: Only the default object is available in GetValue.  Continuing.  Exception: " + e);
                return default(T);
            }
        }

        public T GetFirstValueBelowArrity( List< MutableObject > arrityList )
        {
            if ( UseLiteralValue || UseGlobalParameter )
                return GetValue( arrityList );

            if (arrityList.Count==0)
                throw new Exception("Expected more than zero mutables...");

            if ( arrityList.Count > IntermediateKeys.Count )
            {
                int i=0;
                for(; i<IntermediateKeys.Count; i++)
                {
                    if ( !arrityList[ i ].ContainsKey( IntermediateKeys[ i ] ) )
                        break;
                }


                if ( arrityList[i].ContainsKey( LastKey ) )
                {
                    try
                    {
                        return (T)arrityList[ IntermediateKeys.Count ][ LastKey ];
                    }
                    catch ( Exception )
                    {
                        Debug.LogWarning( "Warning: Only the default object is available here.  Continuing..." );
                        return default ( T );
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        Debug.LogError("An arrity resolution list that doesn't contain the bottom level object cannot possibly resolve " + AbsoluteKey);
                        return default(T);
                    }

                    return GetValue(GetEntriesInternal(arrityList.Skip( i-1 ).First(), arrityList.Take(i-1).ToList()).First());
                }
            }

            var lastElement = arrityList[ arrityList.Count - 1 ];
            
            return GetValue( GetEntriesInternal( lastElement, arrityList.WithoutLast().ToList() ).First() );
        }

        //  Some chain nodes are only coded to operate with one set of values.  In these cases the node is configured to
        //   use GetFirstValue as a shorthand to return the first value that would be produced by the arrity-nonspecific
        //   approach described above (GetEntries and GetValue).
        public T GetFirstValue(MutableObject mutable)
        {
            return GetValue(GetEntries(mutable).First());
        }

        public IEnumerable<List<MutableObject>> GetEntries(MutableObject mutable)
        {
            var localMutable = SwitchToCachedMutable( mutable );

            if ( UseGlobalParameter || UseLiteralValue)
                return new List<List<MutableObject>> { new List<MutableObject> { localMutable } };
            return GetEntriesInternal(localMutable, new List<MutableObject>());
        }

        public IEnumerable<List<MutableObject>> GetEntries(IEnumerable<MutableObject> mutables)
        {
            if (UseGlobalParameter || UseLiteralValue)
                return new List<List<MutableObject>> { mutables.ToList() };
            var mutablesList = mutables.ToList();
            return GetEntriesInternal(mutablesList.Last(), mutablesList.WithoutLast().ToList());
        }

        private IEnumerable<List<MutableObject>> GetEntriesInternal(MutableObject mutable, List<MutableObject> arrityList)
        {
            arrityList.Add(mutable);

            if (arrityList.Count == IntermediateKeys.Count+1)
            {
                yield return arrityList;
                arrityList.RemoveAt(arrityList.Count-1);
                yield break;
            }

            if (!mutable.ContainsKey( IntermediateKeys[arrityList.Count-1] ))
                Debug.Log( "Things!" );

            var nextLevel = mutable[IntermediateKeys[arrityList.Count-1]];

            if (nextLevel is MutableObject)
            {
                foreach (var entry in GetEntriesInternal(nextLevel as MutableObject, arrityList))
                    yield return entry;
            }
            else
            {
                var resolutionList = nextLevel as IEnumerable<MutableObject>;
                foreach (var resolution in resolutionList)
                    foreach (var entry in GetEntriesInternal(resolution, arrityList))
                        yield return entry;
            }

            arrityList.RemoveAt(arrityList.Count-1);
        }
        
        #region Key Validation

        public static bool CouldFieldResolveOnEntry( string absoluteKey, List< MutableObject > entry )
        {
            var keyTokens = absoluteKey.Split( '.' );

            var keyIndex = 0;

            foreach ( var token in keyTokens )
            {
                if (keyIndex >= entry.Count)
                    return false;
                if ( !entry[ keyIndex ].ContainsKey( token ) )
                    return false;
                keyIndex++;
            }

            return true;
        }

        private bool CheckKeyStepsResolvable(MutableObject mutable)
        {
            if (IntermediateKeys == null)
                return true;

            if (UseGlobalParameter)
                return GlobalVariableDataStore.Instance.IsGlobalKeyValid( GlobalParameterKey );

            var current = mutable;

            foreach (var key in IntermediateKeys)
            {
                // check to see if this level contains the intermediate key
                if (!current.ContainsKey(key))
                    return false;

                // check to ensure this intermediate is actually a mutableObject
                var listCurrent = current[key] as MutableObject;
                if (listCurrent == null)
                {
                    var foundList = current[key] as IEnumerable<MutableObject>;
                    if (foundList==null || !foundList.Any())
                        return false;
                    listCurrent = foundList.First();
                }

                // set this intermediate level for the next intermediary
                current = listCurrent;
            }

            if ( LastKey == "" )
                return mutable is T;

            if ( current == null )
            {
                if ( Input.GetKey( KeyCode.F10 ) )
                    return false;
                
                //throw new Exception( "What is happening?" );
                return false;
            }

            if (!current.ContainsKey(LastKey))
                return false;

            var foundResult = current[LastKey] is T;

            return foundResult;
        }

        private bool CheckKeyStepsResolvable(List<MutableObject> entryList)
        {
            if (IntermediateKeys == null)
                return true;

            if ( UseGlobalParameter )
                return GlobalVariableDataStore.Instance.VariableKeys().Contains( GlobalParameterKey );

            var currentLevel = entryList.GetEnumerator();

            foreach (var key in IntermediateKeys)
            {
                if ( !currentLevel.MoveNext() )
                    return false;

                // check to see if this level contains the intermediate key
                if (!currentLevel.Current.ContainsKey(key))
                    return false;

                // check to ensure this intermediate is actually a mutableObject
                var listCurrent = currentLevel.Current[key] as MutableObject;
                if (listCurrent == null)
                {
                    var foundList = currentLevel.Current[key] as IEnumerable<MutableObject>;
                    if (foundList == null)
                        return false;
                    listCurrent = foundList.First();
                }
            }

            if ( !currentLevel.MoveNext() )
                return false;

            if (LastKey == "")
                return currentLevel is T;

            if (!currentLevel.Current.ContainsKey(LastKey))
                return false;

            var foundResult = currentLevel.Current[LastKey] is T;

            return foundResult;
        }


        //private MutableObject ResolveIntermediates(MutableObject mutable)
        //{
        //    if (IntermediateKeysWithoutParent == null || IntermediateKeysWithoutParent.Count==0)
        //        return mutable;
        //    return IntermediateKeysWithoutParent.Aggregate(mutable, ( current, nextKey ) =>
        //    {
        //        var foundMutable = current[ nextKey ] as MutableObject;
        //        if ( foundMutable != null )
        //            return foundMutable;
        //        return ( current[ nextKey ] as IEnumerable< MutableObject > ).First();
        //    } );
        //}

        public bool ValidateKey( MutableObject mutable )
        {
            // check schema source and switch incoming mutable here

            var localMutable = SwitchToCachedMutable( mutable );
            
            KeyValid = ValidateKeyInternal() && AbsoluteResolvable(localMutable);

            return KeyValid;
        }

        public bool ValidateKey(List<MutableObject> entryList)
        {   
            KeyValid = ValidateKeyInternal() && AbsoluteResolvable(entryList);

            return KeyValid;
        }

        public bool IsFieldResolvable( MutableObject mutableToTest )
        {
            var localMutable = SwitchToCachedMutable(mutableToTest);

            return ValidateKeyInternal( ) && AbsoluteResolvable( localMutable );
        }

        private bool ValidateKeyInternal()
        {
            if (UseLiteralValue)
            {
                return true;
            }

            if (!CheckParentAndPattern())
                return false;

            if (UseGlobalParameter)
            {
                //return GlobalVariableDataStore.Instance.VariableKeys().Contains(GlobalParameterKey);
                return true;
            }

            return true;
        }

        private bool CheckParentAndPattern()
        {
            if (SchemaPattern != null)
                if (!PatternResolvable(SchemaPattern))
                    return false;

            if (SchemaParent != null)
                if (!ParentResolvable(SchemaParent))
                    return false;

            return true;
        }

        protected bool AbsoluteResolvable(MutableObject mutable)
        {
            return CouldResolve( mutable );
            //ParentField.ResolveViaParent(scopedMutable));
        }

        protected bool AbsoluteResolvable( List< MutableObject > entryList )
        {
            return CouldResolve(entryList);
        }

        private bool m_KeyValid;

        public event Action< bool > KeyValidChanged = delegate { }; 

        public bool KeyValid
        {
            get
            {
                return m_KeyValid;
            }
            private set
            {
                if ( m_KeyValid == value )
                    return;
                m_KeyValid = value;
                KeyValidChanged(m_KeyValid);
            }
        }

        
        public bool CouldResolve(MutableObject mutable)
        {
            if (UseLiteralValue)
                return true;

            return CheckKeyStepsResolvable(mutable);
        }

        public bool CouldResolve( List< MutableObject > entryList )
        {
            if (UseLiteralValue)
                return true;

            return CheckKeyStepsResolvable(entryList);
        }

        private bool PatternResolvable(IAbsoluteKeyAssignable schemaPattern)
        {
            if ( SchemaSource == SchemaSource.Global || SchemaSource == SchemaSource.Literal )
                return true;

            if ((schemaPattern.SchemaSource != SchemaSource)
                && (schemaPattern.SchemaSource != SchemaSource.Global
                && schemaPattern.SchemaSource != SchemaSource.Literal))
                return false;

            if (IntermediateKeys.Count == 0)
                return true;

            if ( UseGlobalParameter )
                return true;


            string ownIntermediateSchema = IntermediateKeyString;
                //AbsoluteKey.Split('.').WithoutLast().Aggregate(
                //(a, b) => a + "." + b);
            string patternIntermediateSchema = schemaPattern.AbsoluteKey.TrimEnd(("." + LastKey).ToCharArray());
                //schemaPattern.AbsoluteKey.Split('.').WithoutLast().Aggregate(
                //(a, b) => a + "." + b);

            if (ownIntermediateSchema.Length > patternIntermediateSchema.Length)
                return false;

            if (patternIntermediateSchema.Substring(0, ownIntermediateSchema.Length) != ownIntermediateSchema)
                return false;

            return true;
        }

        private bool ParentResolvable( IAbsoluteKeyAssignable schemaParent )
        {
            if ( SchemaSource == SchemaSource.Global || SchemaSource == SchemaSource.Literal )
                return true;

            if ((schemaParent.SchemaSource != SchemaSource)
                && (schemaParent.SchemaSource != SchemaSource.Global
                || schemaParent.SchemaSource != SchemaSource.Literal))
                return false;

            if (IntermediateKeys.Count == 0)
                return true;

            string ownIntermediateSchema = IntermediateKeys.Aggregate( (a,b)=>a+"."+b );

            string patternIntermediateSchema =schemaParent.AbsoluteKey.Contains( '.' )?
                schemaParent.AbsoluteKey.Substring( 0, schemaParent.AbsoluteKey.LastIndexOf( '.' ))
                :"";

            if (ownIntermediateSchema.Length < patternIntermediateSchema.Length)
                return false;

            if (ownIntermediateSchema.Substring(0, patternIntermediateSchema.Length) != patternIntermediateSchema)
                return false;

            return true;
        }

#endregion
        
    }
}
