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

using System.Collections;
using System.Collections.Generic;
using ChainViews;
using JetBrains.Annotations;
using Mutation;
using PayloadSelection;
using PayloadSelection.CriterionStatements;
using PayloadSelection.PayloadSelectors;
using UnityEngine;
using Visualizers;
using Visualizers.MetaSelectors;

namespace Choreography.Steps.SelectionSteps
{
    [UsedImplicitly]
    public class SelectStep : Step
    {
        private const string EndEventName = "End";

        [Controllable]
        public PayloadExpression Expression { get; set; }

        public IPayloadSelection Selection { get; set; }

        private SelectionOperation m_MetaSelectionOperation = SelectionOperation.DoNothing;
        [Controllable]
        public SelectionOperation MetaSelectionOperation { get { return m_MetaSelectionOperation; }
            set { m_MetaSelectionOperation = value; Debug.Log( "New MetaSelection operation" ); } 
        }


        private List<BoundingBox> SelectedBounds { get; set; }

        public List<BoundingBox> ProvidedBounds
        {
            get { return SelectedBounds ?? new List< BoundingBox >(); }
        }

        public SelectStep()
        {
            Router.AddEvent(EndEventName);
        }

        protected override IEnumerator ExecuteStep()
        {
            SelectedBounds = new List<BoundingBox>();

            foreach ( var bound in Expression.ResolveExpression( ChainView.Instance.Chain.RootBoundingBoxes ) )
            {
                var foundSelectable = bound.GetComponent< MetaSelectionSet >();
                
                if ( foundSelectable == null )
                    continue;

                var selectedList = Selection.SelectPayloads(foundSelectable.SelectablePayloads);

                SelectedBounds.AddRange(selectedList.ConvertAll(vp => vp.VisualData.Bound));
                    
                var iterator = MetaSelectionMode.ApplySelectionOperation( MetaSelectionOperation, 
                    foundSelectable, selectedList);

                while ( iterator.MoveNext() )
                    yield return null;
            }

            Router.FireEvent(EndEventName);
            yield return null;
        }
    }
}
