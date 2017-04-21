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
using Assets.PayloadSelection.UI;
using Assets.Visualizers;
using ChainViews;
using JetBrains.Annotations;
using Mutation;
using Newtonsoft.Json;
using UnityEngine;
using Visualizers;
using VoidAlpha.Utilities;

namespace PayloadSelection
{
    [JsonObject( MemberSerialization.OptIn )]
    public class PayloadExpression
    {
        public event Action<CriteriaGroup> CriteriaGroupAdded = delegate { };

        public event Action<CriteriaGroup> CriteriaGroupRemoved = delegate { };

        public event Action CriteriaGroupsCleared = delegate { }; 


        private List<CriteriaGroup> m_CriteriaGroups = new List<CriteriaGroup>();
        [JsonProperty( ObjectCreationHandling = ObjectCreationHandling.Replace )]
        [UsedImplicitly]
        private List< CriteriaGroup > CriteriaGroups
        {
            get { return m_CriteriaGroups; }
            set
            {
                ClearCriteriaGroups();

                foreach ( var criteriaGroup in value )
                    AddCriteriaGroup( criteriaGroup );
            }
        }


        public IEnumerable<CriteriaGroup> CriteriaGroupsEnumerable { get { return CriteriaGroups.AsReadOnly(); } }


        public CriteriaGroup AddCriteriaGroup()
        {
            BoundsClickSelectionOperation.Instance.StartSelectionOperation(ChainView.Instance.Chain.RootBoundingBoxes);

            return AddCriteriaGroup( new CriteriaGroup() );
        }


        // TODO: Privatize this overload if/when only done through UI and not code-built expressions.
        public CriteriaGroup AddCriteriaGroup( CriteriaGroup criteriaGroup )
        {
            criteriaGroup.DeletionRequested += HandleCriteriaGroupDeletionRequested;

            CriteriaGroups.Add( criteriaGroup );

            CriteriaGroupAdded( criteriaGroup );

            return criteriaGroup;
        }

        private void ClearCriteriaGroups()
        {
            foreach ( var criteriaGroup in CriteriaGroups )
            {
                criteriaGroup.DeletionRequested -= HandleCriteriaGroupDeletionRequested;
            }

            CriteriaGroups.Clear();

            CriteriaGroupsCleared();
        }

        public void RemoveCriteriaGroup( CriteriaGroup criteriaGroup )
        {
            if ( !CriteriaGroups.Contains( criteriaGroup ) )
                throw new Exception("Cannot remove criteria group as it does not exist in the collection.");

            criteriaGroup.DeletionRequested -= HandleCriteriaGroupDeletionRequested;

            CriteriaGroups.Remove( criteriaGroup );

            CriteriaGroupRemoved( criteriaGroup );
        }

        private void HandleCriteriaGroupDeletionRequested( CriteriaGroup criteriaGroup )
        {
            RemoveCriteriaGroup( criteriaGroup );
        }

        public List< BoundingBox > ResolveExpression( IEnumerable< BoundingBox > topLevelBounds )
        {
            List<BoundingBox> currentBounds = topLevelBounds.ToList();

            var criteriaEnumerator = CriteriaGroups.GetEnumerator();


            while (criteriaEnumerator.MoveNext())
            {
                currentBounds = ResolveGroup( criteriaEnumerator.Current, 
                    BoundingBox.DescendThroughNondiscriminatedBounds( currentBounds ) );
            }

            return currentBounds;
        }

        private List<BoundingBox> ResolveGroup( CriteriaGroup currentGroup, List< BoundingBox > currentBounds)
        {
            if ( currentBounds.Count == 0 )
                return new List< BoundingBox >();

            return currentGroup.ResolveStep( currentBounds );
        }

        public void BeginBoundSelection(IEnumerable<BoundingBox> topLevelBounds)
        {
            BoundsClickSelectionOperation.Instance.StartSelectionOperation(topLevelBounds);
        }

        public List<MutableObject> ComputeSchemaForGroups( IEnumerable< BoundingBox > topLevelBounds )
        {
            List<MutableObject> schemaList = new List< MutableObject >();

            List<BoundingBox> currentBounds = topLevelBounds.ToList();

            var criteriaEnumerator = CriteriaGroups.GetEnumerator();

            while (criteriaEnumerator.MoveNext())
            {
                currentBounds = BoundingBox.DescendThroughNondiscriminatedBounds( currentBounds );

                schemaList.Add( ComputeSchemaFromBounds( currentBounds ));

                currentBounds = ResolveGroup(criteriaEnumerator.Current, currentBounds);
            }

            schemaList.Add(ComputeSchemaFromBounds(currentBounds));

            return schemaList;
        }

        private MutableObject ComputeSchemaFromBounds(IEnumerable< BoundingBox > bounds)
        {
            List<MutableObject> payloadList = bounds.Select(bound => bound.Data).ToList();

            var schemaList = MutableObject.GetUniqueSchemas( payloadList );

            return MutableObject.UnionSchemas(schemaList);
        }

        /*
        public bool MatchesExpression( IEnumerable< MutableObject > mutablePath )
        {
            var mutableEnumerator = mutablePath.GetEnumerator();
            var criteriaEnumerator = Criteria.GetEnumerator();

            // have to keep these separate so both moves are always considered independently
            bool moreLevels = mutableEnumerator.MoveNext();
            bool moreCriteria = criteriaEnumerator.MoveNext();
            while ( moreLevels && moreCriteria )
            {
                if ( criteriaEnumerator.Current.Any( c => !c.MeetsCriterion( mutableEnumerator.Current ) ) )
                {
                    return false;
                }

                moreCriteria = criteriaEnumerator.MoveNext();
                moreLevels = mutableEnumerator.MoveNext();
            }

            // either we haven't reached the end of the criteria list, 
            //  or we haven't reached the last object specified
            if ( moreCriteria || moreLevels )
                return false;

            return true;
        }
         */
    }
}
