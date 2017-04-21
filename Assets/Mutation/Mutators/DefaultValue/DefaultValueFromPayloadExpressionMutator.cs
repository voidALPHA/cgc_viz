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
using JetBrains.Annotations;
using PayloadSelection;
using Visualizers;

namespace Mutation.Mutators.DefaultValue
{
    public abstract class DefaultValueFromPayloadExpressionMutator <T> : Mutator
    {
        private PayloadExpression m_Expression = new PayloadExpression();

        [Controllable, UsedImplicitly]
        public PayloadExpression Expression
        {
            get { return m_Expression; }
            set { Expression = value; }
        }

        private MutableField<string> m_PerElementAbsoluteKey = new MutableField<string>() { LiteralValue = "Variable Key" };
        [Controllable(LabelText = "Per Element Key")]
        public MutableField<string> PerElementAbsoluteKey { get { return m_PerElementAbsoluteKey; } }

        private MutableTarget m_DefaultableField = new MutableTarget() { AbsoluteKey = "Output Value" };
        [Controllable(LabelText = "Defaultable Target")]
        public MutableTarget DefaultableField { get { return m_DefaultableField; } }

        private MutableObject PayloadData { get; set; }

        protected abstract T GetDefaultValue( MutableObject mutable );

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            PayloadData = payload.Data;
            CacheSchema(); var extantDataField = new MutableField<T> { AbsoluteKey = PerElementAbsoluteKey.GetFirstValue(payload.Data) };

            bool valueAssigned = false;

            var boundsList = Expression.ResolveExpression(ChainView.Instance.Chain.RootBoundingBoxes);

            foreach (var bound in boundsList)
            {
                var useExtantValue = extantDataField.ValidateKey(bound.Data);

                if (useExtantValue)
                {
                    DefaultableField.SetValue(
                            extantDataField.GetLastKeyValue(bound.Data), payload.Data);
                    valueAssigned = true;
                }
                if (valueAssigned)
                    break;
            }

            if (!valueAssigned)
                DefaultableField.SetValue(GetDefaultValue(payload.Data), payload.Data);

            UnCacheSchema();

            var iterator = Router.TransmitAll(payload);
            while (iterator.MoveNext())
                yield return null;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            PayloadData = newSchema;
            DefaultableField.SetValue(GetDefaultValue( newSchema ), newSchema);

            Router.TransmitAllSchema(newSchema);
        }

        public override void CacheSchema()
        {
            CachedMutableDataStore.DataStore = PayloadData;
        }

    }
}
