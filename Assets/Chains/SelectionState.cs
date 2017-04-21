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
    public class SelectionState : IEnumerable< ChainNode >
    {
        public event Action< SelectionState, ChainNode, List<UndoItem>> TargetAdded = delegate { };
        public event Action< SelectionState, ChainNode> TargetRemoved = delegate { };

        public event Action< bool > SelectedChanged = delegate { };

        [JsonConstructor]
        private SelectionState()
        {
            GroupId = "";
        }
        
        public SelectionState(string name) : this()
        {
            Name = name;
        }


        [JsonProperty]
        public string Name { get; set; }

        public string GroupId { get; set; }

        public bool Selected { set { SelectedChanged( value ); } }

        private List<ChainNode> m_Targets = new List<ChainNode>();
        private List<ChainNode> Targets
        {
            get { return m_Targets; }
            set { m_Targets = value; }
        }

        [UsedImplicitly]
        [JsonProperty("Targets", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        private List< ChainNode > SerializedTargets
        {
            get
            {
                if ( ChainGroup.SerializingGroup == null )
                    return Targets;

                var targetsToAllow = ChainGroup.SerializingGroup.RecursiveNodesEnumerable.ToList();

                return Targets.Where( t => targetsToAllow.Contains( t ) ).ToList();
            }
            set { Targets = value; }
        }

        public IEnumerable< ChainNode > TargetsEnumerable
        {
            get { return Targets.AsReadOnly(); }
        }

        public void AddTarget(ChainNode target, bool registerUndo = true)
        {
            if ( target.HasDescendent( this ) )
            {
                Debug.LogWarning( "Not adding target because it would create a circular reference." );
                return;
            }

            if ( Targets.Contains( target ) )
                return;
            
            Targets.Add( target );


            if(registerUndo)
            {
                List<UndoItem> list = new List<UndoItem> { new RouterConnection(target, this, true) };
                TargetAdded(this, target, list);
                UndoLord.Instance.Push(new UndoList(list));
            }
            else
            {
                TargetAdded(this, target, null);
            }
                
        }


        public void RemoveTarget(ChainNode target)
        {
            var ui = RemoveTarget(target, true);
            if(ui != null)
                UndoLord.Instance.Push(ui);
        }

        public UndoItem RemoveTarget(ChainNode target, bool registerUndo)
        {
            if(!Targets.Contains(target))
                return null;

            Targets.Remove( target );

            TargetRemoved( this, target );

            if(registerUndo)
                return new RouterConnection(target, this, false);
            return null;
        }


        public IEnumerator Transmit(VisualPayload payload)
        {
            foreach (var target in Targets)
            {
                var iterator = target.StartReceivePayload(payload);
                while (iterator.MoveNext( ))
                    yield return null;
            }

            //    var jobId = JobManager.Instance.StartJobAndPause(target.ReceivePayload(payload), jobName: "Transmit", startImmediately: true);
            //    if (JobManager.Instance.IsJobRegistered(jobId)) // If the spawned job has not completed...
            //        yield return null;
        }

        public void TransmitSchema(MutableObject schema)
        {
            foreach (var target in Targets)
                target.Schema = schema;
        }

        public void RemoveAllTargets()
        {
            foreach ( var target in Targets.ToList() )  // ToList to clone, as this collection will be modified...
                RemoveTarget( target, false );
        }


        public bool Contains( ChainNode target )
        {
            return Targets.Contains( target );
        }


        #region IEnumerable Implementation

        public IEnumerator<ChainNode> GetEnumerator()
        {
            return Targets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}