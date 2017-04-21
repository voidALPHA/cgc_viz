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
using Bounds;
using ChainViews;
using JetBrains.Annotations;
using PayloadSelection;
using UnityEngine;
using Visualizers;
using Visualizers.MetaSelectors;

namespace Choreography.Steps.SelectionSteps
{

    [UsedImplicitly]
    public class PerformMetaSelectionStep : Step, IBoundsProvider
    {

        #region Bounds Providing

        public void InvokeRemovedFromBoundsProviderRepo()
        {
            RemovedFromBoundsProviderRepo();
        }

        public event Action RemovedFromBoundsProviderRepo = delegate { };


        [Controllable, UsedImplicitly]
        public string BoundsProviderKey { get; set; }


        private List<BoundingBox> InternalBounds { get; set; }


        public IEnumerable<BoundingBox> Bounds
        {
            get
            {
                if (InternalBounds==null)
                    throw new Exception("Bounds retrieved before evaluation!");
                return InternalBounds;
            }
        }

        #endregion


        private const string EndEventName = "End";

        [Controllable, UsedImplicitly]
        public PayloadExpression TargetExpression { get; set; }

        

        //public IPayloadSelection Selection { get; set; }

        [Controllable, UsedImplicitly]
        public IBoundsProvider BoundsToSelect { get; set; }


        private SelectionOperation m_MetaSelectionOperation = SelectionOperation.DoNothing;

        [Controllable]
        public SelectionOperation MetaSelectionOperation { get { return m_MetaSelectionOperation; }
            set { m_MetaSelectionOperation = value; Debug.Log( "New MetaSelection operation" ); } 
        }

        public PerformMetaSelectionStep()
        {
            BoundsProviderKey = "PerfMetaSelStep";

            Router.AddEvent(EndEventName);

            TargetExpression = new PayloadExpression();

            InternalBounds = new List<BoundingBox>();
        }


        protected override IEnumerator ExecuteStep()
        {
            InternalBounds = new List<BoundingBox>();

            foreach ( var metaSelectionSet in 
                TargetExpression.ResolveExpression( ChainView.Instance.Chain.RootBoundingBoxes ) )
            {
                var foundSelectable = metaSelectionSet.GetComponent< MetaSelectionSet >();
                
                if ( foundSelectable == null )
                    continue;

                var selection = new MetaSelectionMode();
                selection.Selectable = foundSelectable;
                selection.OperationToPerform = MetaSelectionOperation;

                var iterator = selection.ApplyMode(
                    MetaSelectionSet.GetLocalSelectables( BoundsToSelect.Bounds, foundSelectable )
                    );

                while ( iterator.MoveNext() )
                    yield return null;
            }

            Router.FireEvent(EndEventName);
            yield return null;
        }
    }
}
