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
using System.Linq;
using ChainViews;
using JetBrains.Annotations;
using PayloadSelection;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.SpatialMutators
{
    public class GetPositionFromPayloadExpression : Mutator
    {
        private MutableField<Vector3> m_DefaultPosition = new MutableField<Vector3>() 
        { LiteralValue = Vector3.zero };
        [Controllable(LabelText = "Default Position")]
        public MutableField<Vector3> DefaultPosition { get { return m_DefaultPosition; } }

        private PayloadExpression m_Expression = new PayloadExpression();

        [Controllable, UsedImplicitly]
        public PayloadExpression Expression
        {
            get { return m_Expression; }
            set { Expression = value; }
        }

        private MutableTarget m_PositionTarget = new MutableTarget() 
        { AbsoluteKey = "Output Position" };
        [Controllable(LabelText = "PositionTarget")]
        public MutableTarget PositionTarget { get { return m_PositionTarget; } }

        private MutableObject PayloadData { get; set; }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            PayloadData = payload.Data;
            CacheSchema();

            var boundsList = Expression.ResolveExpression(ChainView.Instance.Chain.RootBoundingBoxes);

            PositionTarget.SetValue(
                !boundsList.Any()
                    ? DefaultPosition.GetFirstValue( payload.Data )
                    : boundsList.First().transform.position, payload.Data );

            UnCacheSchema();

            var iterator = Router.TransmitAll(payload);
            while (iterator.MoveNext())
                yield return null;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            PayloadData = newSchema;
            PositionTarget.SetValue(Vector3.zero, newSchema);

            Router.TransmitAllSchema(newSchema);
        }

        public override void CacheSchema()
        {
            CachedMutableDataStore.DataStore = PayloadData;
        }
    }
}
