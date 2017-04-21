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
using ChainViews;
using Mutation;
using Mutation.Mutators.SimpleKineticMutators;
using PayloadSelection;
using PayloadSelection.CriterionStatements;
using UnityEngine;
using Visualizers;

namespace Choreography.Steps.TransformBounds
{
    public class RotateObjectStep : Step
    {

        [Controllable]
        public Vector3 RotationAxis { get; set; }

        [Controllable]
        public float RotationSpeed { get; set; }

        [Controllable]
        public bool AbsoluteRotation { get; set; }

        private const string EndEventName = "End";

        [Controllable]
        public PayloadExpression Expression { get; set; }

        public RotateObjectStep()
        {
            Router.AddEvent(EndEventName);
        }

        protected override IEnumerator ExecuteStep()
        {
            foreach ( var bound in Expression.ResolveExpression( ChainView.Instance.Chain.RootBoundingBoxes ) )
            {
                //var satellite = 
                    RotateOverTimeSatellite.CreateRotateOverTimeSatellite(
                    bound.gameObject, RotationAxis, RotationSpeed, !AbsoluteRotation );
            }

            Router.FireEvent(EndEventName);
            yield break;
        }
    }
}
