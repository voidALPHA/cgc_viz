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
using Adapters.GlobalParameters;
using JetBrains.Annotations;
using Mutation.Mutators;
using Newtonsoft.Json;
using Visualizers;

namespace Chains
{
    [JsonObject( MemberSerialization.OptIn )]
    public class Chain
    {
        public event Action< bool > HasErrorChanged = delegate { };

        public event Action< ChainGroup > RootGroupChanged = delegate { };

        //
        // OBSOLETE Keep for patching up packages.
        //

        //[Obsolete]
        //[JsonProperty, UsedImplicitly]
        //private List< ChainNode > Nodes
        //{
        //    set
        //    {
        //        RootGroup.SetNodes( value );
        //    }
        //}

        //
        //
        //

        private ChainGroup m_RootGroup;
        [JsonProperty, UsedImplicitly]
        public ChainGroup RootGroup
        {
            get { return m_RootGroup; }
            private set
            {
                if ( m_RootGroup == value )
                    return;

                if ( m_RootGroup != null )
                {
                    m_RootGroup.HasErrorChanged -= HandleRootGroupHasErrorChanged;

                    m_RootGroup.IsRootGroup = false;
                }

                m_RootGroup = value;

                if ( m_RootGroup != null )
                {
                    m_RootGroup.HasErrorChanged += HandleRootGroupHasErrorChanged;

                    m_RootGroup.IsRootGroup = true;
                }

                RootGroupChanged( m_RootGroup );
            }
        }


        #region Group and Node Event Handlers

        private void HandleRootGroupHasErrorChanged( bool hasError)
        {
            HasError = hasError;
        }
        
        #endregion



        public Chain()
        {
            RootGroup = new ChainGroup();
        }

        public Chain( ChainGroup rootGroup )
        {
            RootGroup = rootGroup;
        }




        private bool m_HasError;
        public bool HasError
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


        #region Root Node Evaluation and Unloading

        private IEnumerable<ChainNode> NodesEnumerable
        {
            get { return RootGroup.RecursiveNodesEnumerable; }
        }

        public IEnumerable<ChainNode> RootNodes
        {
            get
            {
                var targetedNodes = NodesEnumerable.SelectMany( n => n.Router.UniqueTargets ).Distinct();

                var untargetedNodes =
                    NodesEnumerable.Where( n => !targetedNodes.Contains( n ) )
                        .OrderByDescending( n => n is CommandLineArgumentAdapter || n is EarlyExecutionNode);

                return untargetedNodes;
            }
        }

        public IEnumerable< ChainNode > NodesEnumerableByRouterTraversal
        {
            get
            {
                foreach ( var rootNode in RootNodes )
                    foreach ( var child in rootNode.NodesEnumerableByRouterTraversal )
                        yield return child;
            }
        }

        public IEnumerator EvaluateRootNodes()
        {
            Unload();

            RootBoundingBoxes = new List< BoundingBox >();

            foreach ( var node in RootNodes )
            {
                var iterator = node.BeginRootNodeEvaluation( RootBoundingBoxes );

                while ( iterator.MoveNext() )
                    yield return null;
            }
        }

        public void InitializeSchema()
        {
            foreach ( var node in RootNodes )
            {
                node.InitializeSchema();
            }
        }

        private List<BoundingBox> m_RootBoundingBoxes = new List<BoundingBox>();
        public List<BoundingBox> RootBoundingBoxes
        {
            get { return m_RootBoundingBoxes; }
            private set { m_RootBoundingBoxes = value; }
        }

        public void Unload()
        {
            RootGroup.Unload();

            foreach ( var bound in RootBoundingBoxes )
            {
                UnityEngine.Object.Destroy( bound.gameObject );
            }

            GlobalVariableDataStore.Instance.Unload();

            RootBoundingBoxes.Clear();
        }

        #endregion
    }
}
