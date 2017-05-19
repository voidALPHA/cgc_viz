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
using System.Runtime.Serialization;
using Adapters.GlobalParameters;
using JetBrains.Annotations;
using Mutation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Utility.Undo;
using Visualizers;

namespace Chains
{
    [JsonObject( MemberSerialization.OptIn, IsReference = true )]
    [JsonConverter( typeof( ChainNodeConverter ))]
    public abstract class ChainNode : ISchemaProvider
    {
        public event Action< string > CommentChanged = delegate { };

        public event Action< bool > DisabledChanged = delegate { };

        public event Action<MutableObject> SchemaChanged = delegate{ };

        public event Action< bool > HasErrorChanged = delegate { };

        public event Action< ChainNode > Destroying = delegate { };

        public event Action TargetsDirty = delegate { };

        public event Action< ChainNode, ChainGroup > TransferRequested = delegate { };

        private string m_Comment = string.Empty;
        [JsonProperty]
        public string Comment
        {
            get { return m_Comment; }
            set
            {
                if ( m_Comment == value )
                    return;

                m_Comment = value;

                CommentChanged( m_Comment );
            }
        }

        public string Name { get { return GetType().Name; } }

        private bool m_Disabled;
        public bool Disabled
        {
            get { return m_Disabled; }
            private set
            {
                m_Disabled = value;

                foreach ( var target in Router.UniqueTargets )
                    target.ImplicitlyDisabled = value;

                DisabledChanged( m_Disabled );
            }
        }

        private void UpdateDisabled()
        {
            Disabled = ImplicitlyDisabled || ExplicitlyDisabled;
        }

        private bool m_ImplicitlyDisabled;
        private bool ImplicitlyDisabled
        {
            get { return m_ImplicitlyDisabled; }
            set
            {
                m_ImplicitlyDisabled = value;

                UpdateDisabled();
            }
        }


        private bool m_ExplicitlyDisabled;
        [JsonProperty( Order = 100 )]
        public bool ExplicitlyDisabled
        {
            get { return m_ExplicitlyDisabled; }
            set
            {
                m_ExplicitlyDisabled = value;

                UpdateDisabled();
            }
        }


        public string SchemaName { get { return "Local Payload"; } }

        private MutableObject m_Schema;
        public MutableObject Schema
        {
            get { return m_Schema; }
            set
            {
                m_Schema = value;

                CacheSchema();
                ValidateSchema();
                UnCacheSchema();

                if ( SchemaValid )
                    ProcessOutputSchema();

                SchemaChanged( m_Schema );
            }
        }

        public int JsonId { get; set; }

        private StateRouter m_Router;
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Reuse)]
        public StateRouter Router
        {
            get { return m_Router; }
            private set
            {
                if (m_Router != null)
                {
                    m_Router.Owner = null;

                    m_Router.OutputSchemaRefreshRequested -= HandleRouterOutputSchemaRefreshRequested;

                    m_Router.UniqueTargetAdded -= HandleRouterTargetAdded;
                    m_Router.UniqueTargetRemoved -= HandleRouterTargetRemoved;

                    Router.UniqueTargets.ForEach( t => t.ImplicitlyDisabled = false );
                }

                m_Router = value;

                if (m_Router != null)
                {
                    m_Router.Owner = this;

                    m_Router.OutputSchemaRefreshRequested += HandleRouterOutputSchemaRefreshRequested;

                    m_Router.UniqueTargetAdded += HandleRouterTargetAdded;
                    m_Router.UniqueTargetRemoved += HandleRouterTargetRemoved;

                    Router.UniqueTargets.ForEach( t => t.ImplicitlyDisabled = Disabled );
                }
            }
        }

        private void HandleRouterTargetAdded( ChainNode target )
        {
            target.ImplicitlyDisabled = Disabled;

            TargetsDirty();
        }

        private void HandleRouterTargetRemoved( ChainNode target )
        {
            if(target.ImplicitlyDisabled)
                target.ImplicitlyDisabled = false;

            TargetsDirty();
        }

        public void RequestUntargeting()
        {
            UntargetRequested( this );
        }

        public event Action< ChainNode > UntargetRequested = delegate { };


        public IEnumerable<ChainNode> NodesEnumerableByRouterTraversal
        {
            get
            {
                yield return this;

                foreach ( var target in Router.UniqueTargets )
                    foreach ( var targetNode in target.NodesEnumerableByRouterTraversal )
                        yield return targetNode;
            }
        }

        private bool m_SchemaValid;
        public bool SchemaValid
        {
            get { return m_SchemaValid; }
            private set
            {
                m_SchemaValid = value;

                UpdateHasError();
            }
        }

        // Errors should be set internally, this is in place to indicate a bugg condition that this class' internals can't detect, and will be removed when the bugg is fixed.
        private bool m_HasBadRouterChildren;
        public bool HasBadRouterChildren
        {
            get { return m_HasBadRouterChildren; }
            set
            {
                m_HasBadRouterChildren = value;

                if ( m_HasBadRouterChildren)
                    Debug.LogError( "Node marked as erroneous due to having phantom nodes in its router." );
                
                UpdateHasError();
            }
        }

        private void UpdateHasError()
        {
            HasError = m_SchemaValid == false || HasBadRouterChildren;
        }

        private bool m_HasError = true;
        public virtual bool HasError
        {
            get { return m_HasError; }
            private set
            {
                if ( m_HasError == value )
                    return;

                m_HasError = value;

                HasErrorChanged( m_HasError );
            }
        }


        protected ChainNode()
        {
            Router = new StateRouter();

            GlobalVariableDataStore.Instance.GlobalSchemaChanged += CheckGlobalSchemaFields;

            BindToMutableFields();

            BindToMutableTargets();

            BindToMutableScopes();

            ValidateSchema();

            SuppressUndos = false;
        }

        [OnDeserialized]
        private void OnDeserialized( StreamingContext context )
        {
        }

        private void BindToMutableFields()
        {
            foreach ( var mutableField in MutableFields )
                mutableField.ValueChanged += ProcessOutputSchema;
        }

        private void BindToMutableTargets()
        {
            foreach ( var mutableTarget in MutableTargets )
                mutableTarget.AbsoluteKeyChanged += ProcessOutputSchema;
        }

        private void BindToMutableScopes()
        {
            foreach ( var mutableScope in MutableScopes )
                mutableScope.AbsoluteKeyChanged += ProcessOutputSchema;
        }

        public void InitializeSchema()
        {
            if ( Schema != null )
            {
                //Debug.LogWarning( "Schema not null, cannot initialize." );

                return;
            }

            Schema = new MutableObject();
        }

        public IEnumerator BeginRootNodeEvaluation(List< BoundingBox > rootBoundingBoxes)
        {
            var boundingBox = BoundingBox.ConstructBoundingBox( GetType().Name );

            rootBoundingBoxes.Add(boundingBox);


            var iterator = StartReceivePayload( new VisualPayload( new MutableObject(), new VisualDescription( boundingBox ) ) );
            
            while ( iterator.MoveNext() )
                yield return null;
        }

        public IEnumerator StartReceivePayload( VisualPayload payload )
        {
            if ( Disabled )
                yield break;

            var iterator = ReceivePayload( payload );

            while ( iterator.MoveNext() )
                yield return null;
        }

        // TODO: Make this protected and/or rename as OnReceivePayload
        public abstract IEnumerator ReceivePayload(VisualPayload payload);

        private void CheckGlobalSchemaFields()
        {
            if ( MutableFields.Any( field => field.UseGlobalParameter ) )
            {
                ValidateSchema();

                SchemaChanged(Schema);
            }

            // not really...
            SchemaChanged(Schema);
        }

        private List<IMutableField> m_MutableFields;
        public List< IMutableField > MutableFields
        {
            get
            {
                return m_MutableFields ?? ( m_MutableFields = GetType()
                    .GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
                    .Where(p => typeof(IMutableField).IsAssignableFrom(p.PropertyType))
                    .Select(p => p.GetValue(this, null)).Cast<IMutableField>().ToList());
            }
        }

        private List<MutableTarget> m_MutableTargets;
        public List<MutableTarget> MutableTargets
        {
            get
            {
                return m_MutableTargets ?? (m_MutableTargets = GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => typeof(MutableTarget).IsAssignableFrom(p.PropertyType))
                    .Select(p => p.GetValue(this, null)).Cast<MutableTarget>().ToList());
            }
        }

        private List<MutableScope> m_MutableScopes;
        public List<MutableScope> MutableScopes
        {
            get
            {
                return m_MutableScopes ?? (m_MutableScopes = GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => typeof(MutableScope).IsAssignableFrom(p.PropertyType))
                    .Select(p => p.GetValue(this, null)).Cast<MutableScope>().ToList());
            }
        }
        
        public bool SuppressUndos { get; set; }

        private void ValidateSchema()
        {
            if ( Schema == null )
                return;
            
            foreach (var scope in MutableScopes)
            {
                scope.ValidateKey(Schema);

                if ( !scope.KeyValid )
                {
                    SchemaValid = false;
                    return;
                }
            }
            
            foreach ( IMutableField mf in MutableFields )
            {
                mf.ValidateKey( Schema );

                if ( !mf.KeyValid )
                {
                    SchemaValid = false;
                    return;
                }
            }
            
            foreach ( var target in MutableTargets )
            {
                target.ValidateKey( Schema );

                if ( !target.KeyValid )
                {
                    SchemaValid = false;
                    return;
                }
            }

            SchemaValid = true;

        }

        // Each ChainNode ingests and produces a schema object which is passed to child nodes.  This schema object informs
        //  later nodes of the form of the payload that will be passed through the chain during normal execution.  In the
        //  schema object each key that will be present is represented with an example of the object type they will refer
        //  to.  This object is then utilized by each mutableBoxBehaviour to filter the available keys by the type of the
        //  mutableField it represents.  For example, a FormatStringMutator includes a MutableField<string> used to
        //  determine the C#-style format string it will form its output from.  When the user opens the pull-down menu
        //  used to select a string from the data that will be accessible to the node it will filter the schema object by 
        //  checking each object to see if it is an instance of type string.  The dropdown will then be populated including
        //  all keys from the schema object, with only the string-typed fields available for selection.  During package
        //  editing any time a node is modified by the user it executs ProcessOutputSchema, a function overridden by each
        //  node type.  This function produces default values and uses the MutableTargets of the node to write them into the 
        //  schema object before passing it to later nodes.
        public void ProcessOutputSchema()
        {
            if ( Schema == null ) 
                return;

            if (!SchemaValid)
            {
                ValidateSchema();
                if (!SchemaValid)
                    return;
            }

            OnProcessOutputSchema( Schema.CloneKeys() );
        }

        protected virtual void OnProcessOutputSchema( MutableObject newSchema )
        {
            Router.TransmitAllSchema( newSchema );
        }


        private void HandleRouterOutputSchemaRefreshRequested()
        {
            ProcessOutputSchema();
        }

        public virtual void Unload()
        {
        }

        public void RequestTransfer( ChainGroup destinationGroup )
        {
            TransferRequested( this, destinationGroup );
        }

        public UndoItem Destroy( bool recurse )
        {
            var undoList = new List<UndoItem>();
            if ( recurse )
                foreach ( var target in Router.UniqueTargets )
                    undoList.Add(target.Destroy( recurse ));

            var ownItem = PrepareForDestruction(recurse);

            Destroying( this );

            if(!recurse) return ownItem;
            undoList.Add(ownItem);
            undoList.Reverse();
            return new UndoList(undoList);
        }

        public virtual NodeDelete PrepareForDestruction(bool recurse)
        {
            return new NodeDelete(this);

            // Anything targeting this should stop:
            //UntargetRequested( this );

            // And this should stop targeting anything else:
            //Router.UntargetAllTargets();
        }

        [UsedImplicitly]    // Recursive...
        public bool HasDescendent( ChainNode possibleDescendent )
        {
            if ( Router.UniqueTargets.Any( target => target.HasDescendent( possibleDescendent ) ) )
            {
                return true;
            }

            return this == possibleDescendent;
        }

        public bool HasDescendent( SelectionState possibleDescendent )
        {
            if ( Router.UniqueTargets.Any( target => target.HasDescendent( possibleDescendent ) ) )
            {
                return true;
            }

            return Router.SelectionStatesEnumerable.Contains( possibleDescendent );
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




    public class ChainNodeConverter : JsonConverter
    {
        public override bool CanRead { get { return true; } }

        public override bool CanWrite { get { return false; } }

        public override bool CanConvert( Type objectType ) { return typeof( ChainNode ).IsAssignableFrom( objectType ); }


        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            throw new NotImplementedException("If CanWrite is false; this won't be called.");
        }


        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            var jToken = JToken.ReadFrom( reader );

            var refString = jToken.Value< string >( "$ref" );
            var firstReference = refString == null;

            var nodeJsonId = -1;

            if ( firstReference )
            {
                var typeString = jToken.Value<string>( "$type" );

                if ( typeString != null )
                {
                    var type = Type.GetType( typeString );

                    if ( type == null )
                    {
                        var errorNode = new ErrorChainNode();

                        errorNode.JToken = jToken;

                        errorNode.FailedTypeString = typeString;

                        serializer.Populate( jToken.CreateReader(), errorNode );

                        return errorNode;
                    }
                }

                nodeJsonId = jToken.Value< int >( "$id" );
            }

            var nodeAsObject = serializer.Deserialize( jToken.CreateReader() );

            if ( nodeJsonId != -1 )
            {
                var node = nodeAsObject as ChainNode;
                node.JsonId = nodeJsonId;
            }

            return nodeAsObject;
        }
    }
}
