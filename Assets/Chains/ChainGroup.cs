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
using System.Runtime.Serialization;
using ChainViews;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using Utility.Undo;

namespace Chains
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ChainGroup
    {

        // Can Chain manage lifetime of Nodes and Groups? Chain should be the only thing that cares about actual creation/destruction, etc...


        public event Action<ChainNode, bool> NodeAdded = delegate { };

        public event Action<ChainNode, bool> NodeRemoved = delegate { };


        public event Action<ChainGroup, bool> GroupAdded = delegate { };

        public event Action<ChainGroup, bool> GroupRemoved = delegate { };


        //public event Action< ChainGroup > Destroying = delegate { };

        public event Action<ChainGroup> RemovalRequested = delegate { };

        public event Action<ChainGroup, HaxxisPackage> ReplacementRequested = delegate { };


        public event Action<bool> HasErrorChanged = delegate { };


        public bool IsRootGroup { get; set; }


        private List<ChainGroup> m_Groups = new List<ChainGroup>();
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace), UsedImplicitly]
        public List<ChainGroup> Groups
        {
            get { return m_Groups; }
            set
            {
                foreach(var g in m_Groups)
                {
                    UnbindFromGroup(g);
                }

                m_Groups = value;

                foreach(var g in m_Groups)
                {
                    BindToGroup(g);
                }
            }
        }

        private List<ChainNode> m_Nodes = new List<ChainNode>();
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace), UsedImplicitly]
        public List<ChainNode> Nodes
        {
            get { return m_Nodes; }
            private set
            {
                foreach(var n in m_Nodes)
                {
                    UnbindFromNode(n);
                }

                m_Nodes = value;

                foreach(var n in m_Nodes)
                {
                    BindToNode(n);
                }
            }
        }

        //
        // To be used for repairing packages; then kill it...
        //
        [Obsolete]
        public void SetNodes(IEnumerable<ChainNode> nodes)
        {
            Nodes = nodes.ToList();
        }




        public IEnumerable<ChainGroup> RecursiveGroupsEnumerable
        {
            get
            {
                yield return this;

                foreach(var immediateChild in Groups)
                {
                    var iterator = immediateChild.RecursiveGroupsEnumerable.GetEnumerator();
                    while(iterator.MoveNext())
                        yield return iterator.Current;
                }
            }
        }


        public IEnumerable<ChainNode> RecursiveNodesEnumerable
        {
            get
            {
                return RecursiveGroupsEnumerable.SelectMany(group => group.Nodes);
            }
        }


        public event Action<string> CommentChanged = delegate { };

        private string m_Comment = string.Empty;
        [JsonProperty]
        public string Comment
        {
            get { return m_Comment; }
            set
            {
                if(m_Comment == value) return;
                m_Comment = value;
                CommentChanged(m_Comment);
            }
        }



        private const int NO_MATCH_THRESHOLD = Hash.ARBITRARY_LARGE_NUMBER;



        #region Error Handling

        private bool m_HasError;
        private bool HasError
        {
            get { return m_HasError; }
            set
            {
                if(m_HasError == value)
                    return;

                m_HasError = value;

                HasErrorChanged(m_HasError);
            }
        }

        public static ChainGroup SerializingGroup { get; set; }
        public bool SuppressUndo { get; internal set; }

        private void HandleChildHasErrorChanged(bool nodeHasError)
        {
            UpdateHasError();
        }

        private void HandleNodeDisabledChanged(bool disabled)
        {
            UpdateHasError();
        }

        private void UpdateHasError()
        {
            HasError = Groups.Any(g => g.HasError) || Nodes.Any(n => n.HasError && !n.Disabled);
        }

        #endregion



        #region Node Creation and Destruction


        public void CreateAndAddNodeOfType(Type chainNodeType)
        {
            if(!typeof(ChainNode).IsAssignableFrom(chainNodeType))
                throw new Exception("chainNodeType must be assignable to type of ChainNode.");

            var newNode = Activator.CreateInstance(chainNodeType) as ChainNode;
            if(newNode == null)
                throw new ApplicationException("Activator could not create instance of " + chainNodeType.Name);

            newNode.InitializeSchema();
            newNode.JsonId = ChainNodeView.NextJsonId++;

            AddNode(newNode, true);
        }

        private void HandleNodeDestroying(ChainNode node)
        {
            RemoveNode(node, false);
        }

        #endregion


        #region Existing Node Add/Remove

        public void AddNode(ChainNode node, bool recurse, bool isTransfer = false)
        {

            BindToNode(node);


            if(recurse)
            {
                foreach(var target in node.Router.UniqueTargets)
                {
                    AddNode(target, true);
                }
            }

            Nodes.Add(node);

            UpdateHasError();

            NodeAdded(node, isTransfer);
        }

        private void RemoveNode(ChainNode node, bool recurse, bool isTransfer = false)
        {
            if(!isTransfer)
                node.RequestUntargeting();

            UnbindFromNode(node);

            if(recurse)
            {
                foreach(var target in node.Router.UniqueTargets)
                {
                    RemoveNode(target, true, isTransfer);
                }
            }

            if(!isTransfer)
                node.Router.UntargetAllTargets();

            Nodes.Remove(node);

            UpdateHasError();

            NodeRemoved(node, isTransfer);
        }

        private void BindToNode(ChainNode node)
        {
            node.TransferRequested += HandleNodeTransferRequested;
            node.HasErrorChanged += HandleChildHasErrorChanged;
            node.DisabledChanged += HandleNodeDisabledChanged;
            node.Destroying += HandleNodeDestroying;
        }


        private void UnbindFromNode(ChainNode node)
        {
            node.TransferRequested -= HandleNodeTransferRequested;
            node.HasErrorChanged -= HandleChildHasErrorChanged;
            node.DisabledChanged -= HandleNodeDisabledChanged;
            node.Destroying -= HandleNodeDestroying;
        }

        #endregion



        #region Group Creation and Destruction

        public void CreateGroup()
        {
            var group = new ChainGroup()
            {
                SuppressUndo = false
            };

            AddGroup(group);
        }


        public UndoItem RequestRemoval()
        {
            //if(!confirm && (Nodes.Any() || Groups.Any()))
            //{
            //    //Debug.Log("Cannot delete group with child groups or nodes in it.");
            //    ChainView.Instance.GroupDeleteDialog.Show(() => RequestRemoval(), null);
            //    return null;
            //}

            var undoItems = new List<UndoItem>();
            undoItems.Add(new GroupDelete(this));

            foreach(var group in new List<ChainGroup>(Groups))
            {
                undoItems.Add(group.RequestRemoval());
            }

            foreach(var node in new List<ChainNode>(Nodes))
            {
                undoItems.Add(node.Destroy(false));
            }

            RemovalRequested(this);

            return new UndoList(undoItems);
        }

        private void HandleGroupRemovalRequested(ChainGroup group)
        {
            RemoveGroup(group);
        }

        public void RequestReplacement(HaxxisPackage replacement)
        {
            ReplacementRequested(this, replacement);
        }

        private void HandleGroupReplacementRequested(ChainGroup original, HaxxisPackage replacement)
        {
            AddGroup(replacement.Chain.RootGroup, isTransfer: false);

            var newGroupView = ChainView.Instance.GetGroupViewForGroup(replacement.Chain.RootGroup);

            newGroupView.ViewModel = replacement.ChainViewModel.RootGroupViewModel;

            RelinkRouters(original, replacement.Chain.RootGroup);

            original.DestroyNodes();

            RemoveGroup(original);
        }

        private void RelinkRouters(ChainGroup original, ChainGroup replacement)
        {
            Debug.Log("Beginning RelinkRouters");
            
            // Create reference list of internal ChainNodes for the old and new groups
            HashSet<ChainNode> oldInternalNodes = new HashSet<ChainNode>(original.RecursiveNodesEnumerable), newInternalNodes = new HashSet<ChainNode>(replacement.RecursiveNodesEnumerable);

            var oldView = ChainView.Instance.GetGroupViewForGroup(original);
            var oldNodesToViews = oldView.GetNodesToViews();

            var newView = ChainView.Instance.GetGroupViewForGroup(replacement);
            var newNodesToViews = newView.GetNodesToViews();

            Dictionary<ChainNode, Hash> oldNodesToHashesDescending = new Dictionary<ChainNode, Hash>(), oldNodesToHashesAscending = new Dictionary<ChainNode, Hash>();
            Dictionary<ChainNode, Hash> newNodesToHashesDescending = new Dictionary<ChainNode, Hash>(), newNodesToHashesAscending = new Dictionary<ChainNode, Hash>();

            #region Temporary variables to reduce garbage production
            Hash oldHash;
            int bestScore, hashScore;
            ChainNode bestChainNode;
            StateRouter oldRouter, newRouter;
            NodeSelectionStatesPair parentSelectionStates;
            SelectionState newState;
            #endregion


            #region Search for nodes to replace

            // Skim through each ChainNode in the original group (incl child groups), figure out who references an external Node
            foreach(ChainNode oldNode in oldInternalNodes)
            {
                // Find the internal node in the StateRouter.NodeParents dictionary.  Doesn't exist = has no parent = has no chance at external input
                if(StateRouter.NodeParents.ContainsKey(oldNode))
                    // Is the parent node in the group as well?
                    if(!oldInternalNodes.Contains(StateRouter.NodeParents[oldNode].Node))
                    {
                        // Nope.  Found an external input.  Stash that away.
                        oldNodesToHashesDescending[oldNode] = new Hash(oldNodesToViews[oldNode], oldNodesToViews);
                    }

                // Skim through the Unique Targets for the Node's State Router.
                foreach(var uniqueTarget in oldNode.Router.UniqueTargets)
                {
                    // Is the target in the group as well?
                    if(!oldInternalNodes.Contains(uniqueTarget))
                    {
                        // Nope.  Found an external output.  Stash that away.
                        oldNodesToHashesAscending[oldNode] = new Hash(oldNodesToViews[oldNode], oldNodesToViews,
                            descendNodes: false);
                        break;
                    }
                }
            }

            #endregion

            #region Populate dictionaries as necessary

            // We have descending nodes to reconnect, populate descending dictionary.
            if(oldNodesToHashesDescending.Count > 0)
            {
                // Iterate through each Chain Node in the replacement group, figure out who doesn't have a parent.
                // No parent means it's possible for it to have an external input.
                foreach(ChainNode newNode in newInternalNodes)
                {
                 //   if(!StateRouter.NodeParents.ContainsKey(newNode))
                 //   {
                        // NodeParents doesn't contain newNode as a key, therefore newNode has no parent.
                        newNodesToHashesDescending[newNode] = new Hash(newNodesToViews[newNode], newNodesToViews);
                 //   }
                }
            }

            // We have ascending nodes to reconnect, populate ascending dictionary.
            if(oldNodesToHashesAscending.Count > 0)
            {
                foreach(ChainNode newNode in newInternalNodes)
                {
                    newNodesToHashesAscending[newNode] = new Hash(newNodesToViews[newNode], newNodesToViews,
                        descendNodes: false);
                }
            }

            #endregion

            #region Do the replacement thing

            foreach(var oldPair in oldNodesToHashesDescending)
            {
                oldHash = oldPair.Value;

                // Search for the best replacement Chain Node
                bestScore = NO_MATCH_THRESHOLD;
                bestChainNode = null;

                foreach(var newHash in newNodesToHashesDescending)
                {
                    hashScore = Hash.HashMatchingLevel(oldHash, newHash.Value);
                    Debug.Log("HashScore of " + hashScore + " between Old" + oldHash + " and New" +
                              newHash.Value);
                    if(hashScore < bestScore)
                    {
                        bestChainNode = newHash.Key;
                        bestScore = hashScore;
                    }
                    if(hashScore == 0) break;
                }

                // Found best replacement Node
                if(bestChainNode != null)
                {
                    parentSelectionStates = StateRouter.NodeParents[oldPair.Key];
                    //var extRouter = val.Node.Router;
                    //extRouter.UntargetNode(oldNode);
                    foreach(var state in parentSelectionStates.States)
                    {
                        state.AddTarget(bestChainNode, false);
                    }

                    if(bestScore == 0)
                    {
                        // Found perfect replacement, don't consider it as a candidate anymore
                        newNodesToHashesDescending.Remove(bestChainNode);
                    }
                }
            }

            foreach(var oldPair in oldNodesToHashesAscending)
            {
                oldHash = oldPair.Value;

                // Search for the best replacement Chain Node
                bestScore = NO_MATCH_THRESHOLD;
                bestChainNode = null;

                foreach(var newHash in newNodesToHashesAscending)
                {
                    hashScore = Hash.HashMatchingLevel(oldHash, newHash.Value);
                    Debug.Log("HashScore of " + hashScore + " between Old" + oldHash + " and New" +
                              newHash.Value);
                    if(hashScore < bestScore)
                    {
                        bestChainNode = newHash.Key;
                        bestScore = hashScore;
                    }
                    if(hashScore == 0) break;
                }

                // Found best replacement Node
                if(bestChainNode != null)
                {
                    oldRouter = oldPair.Key.Router;
                    newRouter = bestChainNode.Router;

                    foreach(var oldState in oldRouter.SelectionStatesEnumerable)
                    {
                        newState = newRouter[oldState.Name];
                        foreach(var alpha in new List<ChainNode>(oldState.TargetsEnumerable))
                        {
                            newState.AddTarget(alpha, false);
                        }
                    }

                    //break;
                }
            }

            #endregion

            // Don't forget to remove the old nodes from the dictionary...
            foreach(var oldNode in oldInternalNodes)
            {
                oldNode.RequestUntargeting();
            }

            Debug.Log("Ending RelinkRouters");
        }

        private void DestroyNodes()
        {
            foreach(var n in Nodes.ToList())
            {
                RemoveNode(n, recurse: false, isTransfer: false);
            }
        }

        //public void HandleGroupDestroying( ChainGroup group )
        //{
        //    RemoveGroup( group );
        //}

        //public void Destroy()
        //{
        //    Destroying( this );
        //}


        #endregion


        #region Existing Group Adding/Removing

        public void AddGroup(ChainGroup group, bool isTransfer = false)
        {
            group.IsRootGroup = false;

            BindToGroup(group);

            Groups.Add(group);

            GroupAdded(group, isTransfer);
        }

        private void RemoveGroup(ChainGroup group, bool isTransfer = false)
        {
            UnbindFromGroup(group);

            Groups.Remove(group);

            GroupRemoved(group, isTransfer);
        }

        private void BindToGroup(ChainGroup group)
        {
            group.TransferRequested += HandleGroupTransferRequested;
            group.HasErrorChanged += HandleChildHasErrorChanged;
            group.RemovalRequested += HandleGroupRemovalRequested;
            group.ReplacementRequested += HandleGroupReplacementRequested;
        }

        private void UnbindFromGroup(ChainGroup group)
        {
            group.TransferRequested -= HandleGroupTransferRequested;
            group.HasErrorChanged -= HandleChildHasErrorChanged;
            group.RemovalRequested -= HandleGroupRemovalRequested;
            group.ReplacementRequested -= HandleGroupReplacementRequested;
        }

        #endregion


        #region Child Transfer

        public void RequestTransfer(ChainGroup destinationGroup)
        {
            TransferRequested(this, destinationGroup);
        }

        public event Action<ChainGroup, ChainGroup> TransferRequested = delegate { };

        private void HandleGroupTransferRequested(ChainGroup groupToTransfer, ChainGroup destinationGroup)
        {
            RemoveGroup(groupToTransfer, true);

            destinationGroup.AddGroup(groupToTransfer, true);
        }

        private void HandleNodeTransferRequested(ChainNode nodeToTransfer, ChainGroup destinationGroup)
        {
            // TODO: Can make transfers recursive here? What does that mean if descendents are in another group already?

            RemoveNode(nodeToTransfer, recurse: false, isTransfer: true);

            destinationGroup.AddNode(nodeToTransfer, recurse: false, isTransfer: true);
        }

        #endregion


        public void Unload()
        {
            foreach(var node in Nodes)
            {
                node.Unload();
            }

            foreach(var group in Groups)
            {
                group.Unload();
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            foreach(var node in m_Nodes)
            {
                node.HasErrorChanged += HandleChildHasErrorChanged;
            }

            foreach(var group in m_Groups)
            {
                group.HasErrorChanged += HandleChildHasErrorChanged;
            }
        }
    }
}
