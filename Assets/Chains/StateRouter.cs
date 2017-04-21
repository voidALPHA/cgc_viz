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
using JetBrains.Annotations;
using Mutation;
using Newtonsoft.Json;
using UnityEngine;
using Utility.Undo;

namespace Chains
{
    [JsonObject( MemberSerialization.OptIn )]
    public class StateRouter
    {
        private static Dictionary<ChainNode, NodeSelectionStatesPair> m_NodeParents = new Dictionary<ChainNode, NodeSelectionStatesPair>(); 
        public static Dictionary<ChainNode, NodeSelectionStatesPair> NodeParents
        {
            get { return m_NodeParents; }
            private set { m_NodeParents=value; }
        }

        public ChainNode Owner { get; set; }

        public event Action<ChainNode> UniqueTargetAdded = delegate { };

        public event Action<ChainNode> UniqueTargetRemoved = delegate { };

        public event Action OutputSchemaRefreshRequested = delegate { };

        #region Selection States

        private List<SelectionState> m_SelectionStates = new List<SelectionState>();
        [JsonProperty( ObjectCreationHandling = ObjectCreationHandling.Replace )]
        private List<SelectionState> SelectionStates
        {
            get { return m_SelectionStates; }
            [UsedImplicitly]
            set
            {
                // Beware, there is a possible issue here: Adding states as we do does not raise UniqueTargetAdded/Removed when appopriate.
                //  It is not super straight-forward to implement... And the side effects are not immediately known. Test heavily if done! Saving, UI dupe, everything...

                foreach(var t in UniqueTargets)
                {
                    t.UntargetRequested -= HandleTargetUntargetRequested;
                    if(NodeParents.ContainsKey(t)) NodeParents.Remove(t);
                }
                    

                foreach ( var state in value )
                {
                    var foundStateIndex = m_SelectionStates.FindIndex( s => s.Name == state.Name );
                    if ( foundStateIndex != -1 )
                    {
                        UnBindFromState( m_SelectionStates[foundStateIndex] );

                        state.GroupId = m_SelectionStates[foundStateIndex].GroupId;

                        m_SelectionStates[foundStateIndex] = state;

                        BindToState( state );
                    }

                    // otherwise, this state we're trying to add is no longer part of this particular router, drop it.
                }

                foreach(var t in UniqueTargets)
                {
                    t.UntargetRequested += HandleTargetUntargetRequested;
                    NodeParents[t] = new NodeSelectionStatesPair
                    {
                        Node = Owner
                    };
                    NodeParents[t].States.AddRange(SelectionStates.Where(s => s.Contains(t)));
                }
                    
            }
        }


        private void HandleTargetUntargetRequested( ChainNode nodeToUntarget )
        {
            UntargetNode( nodeToUntarget );
        }


        public SelectionState this[string name]
        {
            get { return SelectionStates.FirstOrDefault( s => s.Name == name ); }
        }

        public IEnumerable< SelectionState > SelectionStatesEnumerable { get { return SelectionStates.AsReadOnly(); } }

        public void AddSelectionState( string name, string groupId = "" )
        {
            if ( SelectionStates.Any( s => s.Name == name ) )
                return;

            var state = new SelectionState( name );

            state.GroupId = groupId;

            BindToState( state );

            SelectionStates.Add( state );
        }

        #endregion


        public List<ChainNode> UniqueTargets
        {
            get { return SelectionStates.SelectMany( s => s ).Distinct().ToList(); }
        }

        public IEnumerator TransmitAll( VisualPayload payload )
        {
            foreach ( var state in SelectionStates )
            {
                var iterator = state.Transmit( payload );
                while ( iterator.MoveNext() )
                    yield return null;
            }
        }

        public void TransmitAllSchema( MutableObject schema )
        {
            foreach ( ChainNode node in SelectionStates
                .SelectMany( s => s.TargetsEnumerable ).Distinct() )
            {
                node.Schema = schema;
            }
        }

        public void TransmitSchemaToGroup( MutableObject schema, string groupId )
        {
            if ( groupId == "" )
            {
                Debug.LogError( "Can't transmit schema through states with no grouping, as they don't necessarily have the same schema..." );
                return;
            }

            foreach ( ChainNode node in SelectionStates.Where( s => s.GroupId == groupId )
                .SelectMany( s => s.TargetsEnumerable ) )
                node.Schema = schema;
        }

        public void UntargetAllTargets()
        {
            foreach ( var state in SelectionStates )
                state.RemoveAllTargets();
        }

        public void UntargetNode( ChainNode nodeToUntarget )
        {
            foreach ( var state in SelectionStatesEnumerable )
            {
                state.RemoveTarget( nodeToUntarget, false );
            }
        }


        #region State Driven behaviour

        private void BindToState( SelectionState state )
        {
            state.TargetAdded += HandleStateTargetAdded;

            state.TargetRemoved += HandleStateTargetRemoved;
        }

        private void UnBindFromState( SelectionState state )
        {
            state.TargetAdded -= HandleStateTargetAdded;

            state.TargetRemoved -= HandleStateTargetRemoved;
        }

        private void HandleStateTargetAdded( SelectionState stateWithNewTarget, ChainNode newTarget, List<UndoItem> returnUndos )
        {
            // Cache this before the next step...
            bool isNewTargetForThisRouter = SelectionStates.SelectMany( s => s ).Count( t => t == newTarget ) == 1;

            // If a state adds a new target, all the other states in the router which are from a different group or no group, which contain the new target, remove that target.
            if(returnUndos != null)
                returnUndos.AddRange(SelectionStates.Where(s => s != stateWithNewTarget && (s.GroupId != stateWithNewTarget.GroupId || s.GroupId == "") && s.Contains(newTarget))
                                                    .Select(s => s.RemoveTarget(newTarget, true)));
            else
                foreach(var state in SelectionStates.Where(s => s != stateWithNewTarget && (s.GroupId != stateWithNewTarget.GroupId || s.GroupId == "") && s.Contains(newTarget)))
                {
                    state.RemoveTarget(newTarget, false);
                }

            if ( isNewTargetForThisRouter )
            {
                // This won't target this router, we're not listening to it yet...
                newTarget.RequestUntargeting();

                // Bind to this event AFTER requesting the untargeting...
                newTarget.UntargetRequested += HandleTargetUntargetRequested;

                UniqueTargetAdded( newTarget );

                if (NodeParents.ContainsKey(newTarget))
                {
                    Debug.LogWarning("Interesting... adding entry to NodeParents despite entry existing.  Clearing... ");
                    NodeParents.Remove(newTarget);
                }
                NodeParents[newTarget] = new NodeSelectionStatesPair() { Node = Owner };
                NodeParents[newTarget].States.Add(stateWithNewTarget);
            } else
            {
                if (!NodeParents.ContainsKey(newTarget))
                {
                    Debug.LogError("Uhm... modifying entry in NodeParents despite entry being missing.  Correcting... ");
                    NodeParents[newTarget] = new NodeSelectionStatesPair() { Node = Owner };
                }
                NodeParents[newTarget].States.Add(stateWithNewTarget);
            }

            if ( SelectionStates.SelectMany( s => s ).Count( t => t == newTarget ) == 1 )
                OutputSchemaRefreshRequested();
        }

        private void HandleStateTargetRemoved( SelectionState state, ChainNode target )
        {
            var isUniqueTarget = SelectionStates.SelectMany( s => s ).Count( t => t == target ) == 0;

            if ( isUniqueTarget )
            {
                target.UntargetRequested -= HandleTargetUntargetRequested;

                target.Schema = MutableObject.Empty;

                UniqueTargetRemoved( target );

                if(!NodeParents.ContainsKey(target))
                {
                    Debug.LogWarning("Odd... removing entry from NodeParents despite entry being missing.  Kay... ");
                } else
                {
                    NodeParents.Remove(target);
                }
            } else
            {
                if (!NodeParents.ContainsKey(target))
                {
                    Debug.LogError("Crap... modifying entry in NodeParents despite entry being missing.  That's a problem... ");
                } else
                {
                    NodeParents[target].States.Remove(state);
                }
            }
        }

        #endregion

    }

    //public class DontSerializeStateRouterConverter : JsonConverter
    //{
    //    public override bool CanConvert( Type objectType )
    //    {
    //        return objectType == typeof( StateRouter );
    //    }

    //    public override bool CanRead
    //    {
    //        get { return false; }
    //    }
        
    //    public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
    //    {
    //        throw new NotImplementedException();
    //    }

        
    //    public override bool CanWrite
    //    {
    //        get { return true; }
    //    }

    //    public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
    //    {
    //    }
    //}
}
