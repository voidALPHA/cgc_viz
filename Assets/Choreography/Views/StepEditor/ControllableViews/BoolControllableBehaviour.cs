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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Visualizers;

namespace Choreography.Views.StepEditor.ControllableViews
{
    public class BoolControllableBehaviour : MonoBehaviour
    {

        [Header("Component References")]

        [SerializeField]
        private Text m_LabelTextComponent = null;
        private Text LabelTextComponent { get { return m_LabelTextComponent; } }

        [SerializeField]
        private Toggle m_ToggleComponent = null;
        private Toggle ToggleComponent { get { return m_ToggleComponent; } }





        private PropertyInfo PropertyInfo { get; set; }

        private System.Object PropertyResolutionObject { get; set; }



        private ControllableCondition m_ControllableCondition;
        private ControllableCondition ControllableCondition
        {
            get { return m_ControllableCondition; }
            set
            {
                if ( m_ControllableCondition != null )
                    m_ControllableCondition.Changed -= HandleControllableChanged;

                m_ControllableCondition = value;

                if ( m_ControllableCondition != null )
                    m_ControllableCondition.Changed += HandleControllableChanged;
            }
        }

        private void HandleControllableChanged( bool conditionValid )
        {
            ToggleComponent.isOn = conditionValid;
        }



        private bool PropertyValue
        {
            get
            {
                if ( ControllableCondition != null )
                    return ControllableCondition.ConditionValid;
                else
                    return (bool)PropertyInfo.GetValue( PropertyResolutionObject, null );
            }
            set
            {
                if ( ControllableCondition != null  )
                    ControllableCondition.ConditionValid = value;
                else
                    PropertyInfo.SetValue( PropertyResolutionObject, value, null );
            }
        }


        public void Initialize( PropertyInfo propertyInfo, System.Object propertyResolutionObject )
        {
            PropertyInfo = propertyInfo;

            PropertyResolutionObject = propertyResolutionObject;

            LabelTextComponent.text = propertyInfo.Name;

            if ( PropertyInfo.PropertyType == typeof( ControllableCondition ) )
                ControllableCondition = (ControllableCondition)PropertyInfo.GetValue( PropertyResolutionObject, null );

            ToggleComponent.isOn = PropertyValue;
        }

        [UsedImplicitly]
        public void Start()
        {
        }

        [UsedImplicitly]
        public void OnDestroy()
        {
            ControllableCondition = null;
        }


        [UsedImplicitly]
        public void HandleToggleChanged()
        {
            PropertyValue = ToggleComponent.isOn;
        }
    }
}
