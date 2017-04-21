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
using Chains;
using Mutation;
using UnityEngine;
using Visualizers;

namespace Kinetics
{
    public abstract class KineticNode : ChainNode
    {
        public SelectionState Started { get { return Router[ "Started" ]; } }
        public SelectionState Finished { get { return Router["Finished"]; } }

        private MutableField<string> m_KineticCurveName = new MutableField<string>()
        { LiteralValue = "Linear" };
        [Controllable(LabelText = "Kinetic Curve Name")]
        public MutableField<string> KineticCurveName { get { return m_KineticCurveName;} }

        protected KineticNode()
        {
            Router.AddSelectionState("Started");
            Router.AddSelectionState("Finished");
        }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var kineticCurve = CurveFactory.GetCurve(KineticCurveName.GetFirstValue(payload.Data));

            StartKinetic(payload, (timeProportion=>kineticCurve.Evaluate(timeProportion)));

            var iterator = Started.Transmit(payload);
            while (iterator.MoveNext())
                yield return null;
        }

        public abstract void StartKinetic(VisualPayload payload, Func<float, float> translateTimeToEffectProportion );

    }
}
