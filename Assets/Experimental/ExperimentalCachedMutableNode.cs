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
using System.Windows.Forms;
using Chains;
using ChainViews;
using JetBrains.Annotations;
using Mutation;
using Mutation.Mutators;
using PayloadSelection;
using PayloadSelection.CriterionStatements;
using UnityEngine;
using Visualizers;

namespace Experimental
{
    public class ExperimentalCachedMutableNode : Mutator
    {
        private MutableTarget m_CachedIntTarget = new MutableTarget() 
        { AbsoluteKey = "Cached Int" };
        [Controllable(LabelText = "Cached Int Target")]
        public MutableTarget CachedIntTarget { get { return m_CachedIntTarget; } }

        private MutableField<int> m_MutableFromCache = new MutableField<int>() 
        { LiteralValue = 9 };
        [Controllable(LabelText = "Mutable From Cache")]
        public MutableField<int> MutableFromCache { get { return m_MutableFromCache; } }

        private PayloadExpression m_Expression = new PayloadExpression();
        [Controllable, UsedImplicitly]
        public PayloadExpression Expression
        {
            get { return m_Expression; }
            set { m_Expression = value; }
        }

        private void SetUpTestExpression()
        {
            //Expression = new PayloadExpression();
            //var newGroup = new CriteriaGroup();
            //var newStatement = new IntGreaterThanCriterionStatement();
            //newStatement.Operand1.SchemaSource = SchemaSource.Cached;
            //newStatement.Operand1.AbsoluteKey = MutableFromCache.AbsoluteKey;
            //newStatement.Operand2.SchemaSource = SchemaSource.Literal;
            //newStatement.Operand2.LiteralValue = 5;
            //newGroup.AddCriterion( newStatement );
            //Expression.AddCriteriaGroup( newGroup );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            CacheSchema();

            var numberFromMutable = MutableFromCache.GetFirstValue( payload.Data );
            Debug.Log( "Mutable value is " + numberFromMutable );


            SetUpTestExpression();

            var boundsList = Expression.ResolveExpression( ChainView.Instance.Chain.RootBoundingBoxes );

            Debug.Log( "Payload expression located " + boundsList.Count );


            UnCacheSchema();

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }

        public override void CacheSchema()
        {
            var cachedData = new MutableObject()
            {
                { "Cached Four", 4 },
                { "Cached Twelve", 12 }
            };

            CachedMutableDataStore.DataStore = cachedData;
        }

        public override void UnCacheSchema()
        {
            CachedMutableDataStore.ClearDataCache();
        }
    }
}
