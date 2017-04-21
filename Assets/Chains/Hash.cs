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

using System.Collections.Generic;
using System.Linq;
using ChainViews;
using Newtonsoft.Json;
using UnityEngine;
using Utility;

namespace Chains
{
    public class Hash
    {
        public const int ARBITRARY_LARGE_NUMBER = 999999999;

        private class StateHashPair
        {
            public StateHashPair(Hash hash, string stateName)
            {
                Hash = hash;
                StateName = stateName;
            }

            public Hash Hash { get; set; }
            public string StateName { get; set; }
        }
        private string TypeHash { get; set; }

        private string PositionHash { get; set; }

        private List<StateHashPair> m_RelativesHashes = new List<StateHashPair>();
        private List<StateHashPair> RelativesHashes { get { return m_RelativesHashes; } set { m_RelativesHashes = value; } }

        private const int RELATIVES_DEPTH = 4;

        //public Hash(ChainNodeView nodeView)
        //{
        //    LocalHash(nodeView);

        //    //DescendentHash = HashingUtil.StringToMd5Hash(  )
        //}

        private void LocalHash(ChainNodeView nodeView)
        {
            TypeHash = HashingUtil.StringToMd5Hash(nodeView.ChainNode.GetType().ToString());

            PositionHash = HashingUtil.StringToMd5Hash(nodeView.ViewModel.Position.ToString());
        }

        private static int DirectHashMatch(Hash hash1, Hash hash2)
        {
            return (hash1.PositionHash == hash2.PositionHash ? 0 : 1) + (hash1.TypeHash == hash2.TypeHash ? 0 : 1);
        }

        public Hash(ChainNodeView nodeView, Dictionary<ChainNode, ChainNodeView> nodesToViews, int remainingDepth = RELATIVES_DEPTH, bool descendNodes = true)
        {
            LocalHash(nodeView);

            if(remainingDepth <= 0)
                return;

            if(descendNodes)
                foreach(
                    var subState in (from s in nodeView.ChainNode.Router.SelectionStatesEnumerable select s))
                {
                    foreach ( var childNode in subState )
                    {
                        if ( !nodesToViews.ContainsKey( childNode ) )
                            continue;
                        //Debug.LogError( "Node not found in node to nodeView correlative dictionary." );

                        RelativesHashes.Add(
                            new StateHashPair( new Hash( nodesToViews[ childNode ], nodesToViews, remainingDepth - 1,
                                descendNodes ), subState.Name ) );
                    }
                }
            else
            {
                if(StateRouter.NodeParents.ContainsKey(nodeView.ChainNode)
                    && nodesToViews.ContainsKey(StateRouter.NodeParents[nodeView.ChainNode].Node))
                    RelativesHashes.Add(new StateHashPair(
                        new Hash(
                            nodesToViews[StateRouter.NodeParents[nodeView.ChainNode].Node],
                            nodesToViews,
                            remainingDepth - 1,
                            descendNodes),
                        "parent"));
            }
        }

        public static int HashMatchingLevel(Hash hash1, Hash hash2, int differenceScale=50000, int differenceReduction=10)
        {
            //     Compare the two base levels  AND    all of their children
            //                vv                 v             vv
            return DirectHashMatch(hash1, hash2)* differenceScale + NestedHashMatch(hash1, hash2, differenceScale/differenceReduction, differenceReduction);
        }

        private class HashPair
        {
            public Hash Hash1 { get; set; }
            public Hash Hash2 { get; set; }

            public HashPair(Hash hash1, Hash hash2)
            {
                Hash1 = hash1;
                Hash2 = hash2;
            }

            public override int GetHashCode()
            {
                return Hash1.GetHashCode() ^ Hash2.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if(!(obj is HashPair))
                    return false;
                var otherPair = obj as HashPair;
                return (Hash1 == otherPair.Hash1) && (Hash2 == otherPair.Hash2);
            }
        }

        /// <summary>
        /// Returns the amount of difference between two Hashes
        /// </summary>
        private static int NestedHashMatch(Hash hash1, Hash hash2, int differenceScale = 50000, int differenceReduction = 10)
        {
            //Dictionary<HashPair, int> optionConfigurations = new Dictionary<HashPair, int>();
            //foreach(var relativePair in hash1.RelativesHashes)
            //    foreach(var otherPair in hash2.RelativesHashes)
            //    {
            //        optionConfigurations.Add(new HashPair(relativePair.Hash, otherPair.Hash), 9999);
            //    }
            
            // How large is the total difference between the two hashes, including children?
            var totalScore = 0;  
            // Dictionary of all previously made matches with their scores, for comparison for later matches
            var prevMatches = new Dictionary<HashPair, int>();
            // List of all the unmatched pairs that still need to be made
            var unmatchedPairs = new List<StateHashPair>(hash1.RelativesHashes);
            
            #region Temporary variables to reduce garbage production
            StateHashPair relativePair, bestOption;
            int relativeFitness = 0, bestFitness = ARBITRARY_LARGE_NUMBER;
            KeyValuePair<HashPair, int> prevMatch;
            #endregion 

            // Go through each relative of the replaced node
            while(unmatchedPairs.Count > 0)
            {
                relativePair = unmatchedPairs[0];

                // Figure out who the best fit is
                bestFitness = ARBITRARY_LARGE_NUMBER;
                bestOption = null;
                
                // Skim each of the children in the candidate replacement node
                foreach (
                    var otherRelativePair in
                        from h in hash2.RelativesHashes where h.StateName == relativePair.StateName select h)
                {
                    // Check difference between next level hashes
                    relativeFitness = differenceScale*DirectHashMatch(relativePair.Hash, otherRelativePair.Hash);

                    if(relativeFitness == 0)
                    {
                        // Second level is exact match, don't bother checking down the rest of the chain
                        // or any other child of candidate replacement node
                        bestFitness = 0;
                        bestOption = relativePair;
                        
                        if(prevMatches.Any(p => p.Key.Hash2 == otherRelativePair.Hash))
                        {
                            // ...except this option was used previously.
                            prevMatch = prevMatches.First(p => p.Key.Hash2 == otherRelativePair.Hash);

                            // Return old match to the calculation queue.
                            unmatchedPairs.Add(hash1.RelativesHashes.First(pair => pair.Hash == prevMatch.Key.Hash1));

                            // And remove the stale match from the previous matches table.
                            prevMatches.Remove(prevMatch.Key);
                        }

                        break;
                    }

                    // Wasn't quite an exact match.  Continue down the chain, get that score too
                    relativeFitness += NestedHashMatch(relativePair.Hash, otherRelativePair.Hash, differenceScale/differenceReduction, differenceReduction);

                    // This match is better than what we found before
                    if(relativeFitness < bestFitness)
                    {
                        if(prevMatches.Any(p => p.Key.Hash2 == otherRelativePair.Hash))
                        {
                            // ...except this option was used previously.
                            prevMatch = prevMatches.First(p => p.Key.Hash2 == otherRelativePair.Hash);

                            // Was the previous match better?
                            if(prevMatch.Value < bestFitness)
                            {
                                // Previous match was better.  Skip this potential match.
                                continue;
                            }

                            // This match is better.  Return old match to the calculation queue.
                            unmatchedPairs.Add(hash1.RelativesHashes.First(pair => pair.Hash == prevMatch.Key.Hash1));

                            // And remove the stale match from the previous matches table.
                            prevMatches.Remove(prevMatch.Key);
                        }

                        // Record this match as the "best so far"
                        bestFitness = relativeFitness;
                        bestOption = otherRelativePair;
                    }
                }

                if(bestOption == null)
                {
                    // This was worst case scenario; there was no found match for this node.  Tank the score.
                    //totalScore += 9999;
                }
                else
                {
                    // Accumulate the best fitness
                    totalScore += bestFitness;
                    // Record this node with its best fitness
                    prevMatches[new HashPair(relativePair.Hash, bestOption.Hash)] = bestFitness;
                }

                // Either a match has been found or there is no match to find.  Remove from the list.
                unmatchedPairs.RemoveAt(0);
            }

            // account for differences in the number of relatives in these hashes
            totalScore += differenceScale*Mathf.Abs( hash1.RelativesHashes.Count - hash2.RelativesHashes.Count ) * 2;

            return totalScore;
        }

        public override string ToString()
        {
            return "Hash: {T:" + TypeHash + ",P:" + PositionHash + "}";
        }
    }
}
