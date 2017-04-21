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
using Visualizers;

namespace Choreography.Steps.BoundsProviderSteps
{
    public class BoundsFromPayloadExpressionStep : Step, IBoundsProvider
    {
        #region Bounds stuff

        [Controllable, UsedImplicitly]
        public string BoundsProviderKey { get; set; }

        private IEnumerable< BoundingBox > m_Bounds;
        public IEnumerable< BoundingBox > Bounds
        {
            get
            {
                if (m_Bounds==null)
                    throw new Exception("Bounds retrieved before evaluation!");
                return m_Bounds;
            }
            private set { m_Bounds = value; }
        }

        #endregion



        private const string CompleteEventName = "Complete";

        [Controllable, UsedImplicitly]
        public PayloadExpression Expression { get; set; }

        public BoundsFromPayloadExpressionStep()
        {
            BoundsProviderKey = "BoundsFromPayExprStep";

            Bounds = new List<BoundingBox>();

            Expression = new PayloadExpression();

            Router.AddEvent( CompleteEventName );
        }

        protected override IEnumerator ExecuteStep()
        {
            Bounds = Expression.ResolveExpression( ChainView.Instance.Chain.RootBoundingBoxes );

            Router.FireEvent( CompleteEventName );

            yield return null;
        }
    }
}
