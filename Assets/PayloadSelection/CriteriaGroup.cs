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
using PayloadSelection.CriterionStatements;
using Visualizers;

namespace PayloadSelection
{
    [JsonObject( MemberSerialization.OptIn )]
    public class CriteriaGroup : IEnumerable<CriterionStatement>
    {
        public event Action< CriteriaGroup > DeletionRequested = delegate { };

        public event Action< CriterionStatement > CriterionAdded = delegate { };

        public event Action< CriterionStatement > CriterionRemoved = delegate { };

        public event Action< CriteriaConjunction > ConjuctionChanged = delegate { };

        public event Action<CriterionStatement> CriteriaChanged = delegate { };


        private List<CriterionStatement> m_Criteria = new List< CriterionStatement >();

        [JsonProperty( ObjectCreationHandling = ObjectCreationHandling.Replace )]
        [UsedImplicitly( ImplicitUseKindFlags.Assign )]
        private List< CriterionStatement > Criteria
        {
            get { return m_Criteria; }
            set
            {
                foreach ( var criterionStatement in value )
                    AddCriterion( criterionStatement );
            }
        }

        public IEnumerable<CriterionStatement> CriteriaEnumerable { get { return Criteria.AsReadOnly(); } }

        public void AddCriterion( CriterionStatement criterion )
        {
            if ( Criteria.Contains( criterion ))
                throw new InvalidOperationException("Cannot add criterion, it already exists in criteria.");

            Criteria.Add( criterion );

            criterion.CriterionChanged += CriteriaChanged;

            criterion.RemovalRequested += HandleCriterionRemovalRequested;

            CriterionAdded( criterion );
        }

        private void RemoveCriterion( CriterionStatement criterion )
        {
            if ( !Criteria.Contains( criterion ) )
                throw new InvalidOperationException( "Cannot remove criterion, it does not exist in criteria." );

            criterion.RemovalRequested -= HandleCriterionRemovalRequested;

            Criteria.Remove( criterion );

            CriterionRemoved( criterion );
        }

        private void HandleCriterionRemovalRequested( CriterionStatement criterion )
        {
            RemoveCriterion( criterion );
        }

        private CriteriaConjunction m_Conjunction = CriteriaConjunction.And;
        [JsonProperty]
        public CriteriaConjunction Conjunction
        {
            get { return m_Conjunction; }
            set
            {
                if ( m_Conjunction == value )
                    return;

                m_Conjunction = value;

                ConjuctionChanged( m_Conjunction );
            }
        }

        public List< BoundingBox > ResolveStep( List< BoundingBox > bounds )
        {
            List< BoundingBox > selectedBounds = Conjunction==CriteriaConjunction.And?bounds.ToList():new List< BoundingBox >();

            foreach ( var criterion in Criteria )
            {
                selectedBounds = 
                    Conjunction == CriteriaConjunction.And ? 
                    selectedBounds.Intersect( criterion.GetMatchingBounds( bounds ) ).ToList() 
                    : selectedBounds.Union(criterion.GetMatchingBounds(bounds)).ToList();
            }

            return selectedBounds;
        }

        public void RequestDeletion()
        {
            DeletionRequested( this );
        }



        public IEnumerator<CriterionStatement> GetEnumerator()
        {
            foreach ( var criteria in Criteria )
                yield return criteria;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public enum CriteriaConjunction
    {
        And,
        Or,
    }
}
