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

using System.Collections.Generic;
using JetBrains.Annotations;
using Mutation;
using PayloadSelection;
using PayloadSelection.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Choreography.Views.StepEditor.ControllableViews
{
    public class PayloadExpressionControllableViewBehaviour : MonoBehaviour
    {
    
        [Header( "Component References" )]

        [SerializeField]
        private Text m_LabelTextComponent = null;
        private Text LabelTextComponent { get { return m_LabelTextComponent; } }



        private PayloadExpression m_PayloadExpression;
        public PayloadExpression PayloadExpression
        {
            get { return m_PayloadExpression; }
            set { m_PayloadExpression = value; }
        }

        public string PropertyName { set { LabelTextComponent.text = value; } }
        
        public ISchemaProvider ExternalSchemaProvider { get; set; }


        [UsedImplicitly]
        public void HandleButtonPressed()
        {
            PayloadExpressionViewBehaviour.Instance.ExternalSchemaProvider = ExternalSchemaProvider;

            PayloadExpressionViewBehaviour.Instance.PayloadExpression = PayloadExpression;

            if (ExternalSchemaProvider!=null)
                ExternalSchemaProvider.CacheSchema();

            PayloadExpressionViewBehaviour.Instance.Show();

            if (ExternalSchemaProvider != null)
                ExternalSchemaProvider.UnCacheSchema();

        }
    }
}
