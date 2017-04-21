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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Assets.Utility;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Utility;
using Visualizers;
using Object = System.Object;

namespace Choreography.Views.StepEditor.ControllableViews
{
    public class EditorOptionViewBehaviour : MonoBehaviour
    {

        [Header("Component References")]

        [SerializeField]
        private Text m_PropertyNameTextComponent = null;
        private Text PropertyNameTextComponent { get { return m_PropertyNameTextComponent; } }

        [SerializeField]
        private InputField m_ValueInputField = null;
        private InputField ValueInputField { get { return m_ValueInputField; } }



        private Object PropertyResolutionObject { get; set; }

        private PropertyInfo Property { get; set; }

        private ControllableAttribute ControllableAttribute { get; set; }

        public void SetProperty( Object propertyResolutionObject, PropertyInfo propertyInfo )
        {
            if ( Property != null )
                throw new InvalidOperationException( "Cannot reuse this view." );

            PropertyResolutionObject = propertyResolutionObject;

            Property = propertyInfo;

            ControllableAttribute = (ControllableAttribute)Property.GetCustomAttributes( typeof( ControllableAttribute ), true ).First();


            PropertyNameTextComponent.text = String.IsNullOrEmpty( ControllableAttribute.LabelText ) ? Property.Name : ControllableAttribute.LabelText;
            

            ValueInputField.text = Property.GetValue( PropertyResolutionObject, null ).ToString();


            ConfigureConditionality();

            ConfigureObservation();
        }

        public void CleanUp()
        {
            CleanUpConditionality();

            CleanUpObservation();
        }


        [UsedImplicitly]
        public void HandleInputFieldChanged()
        {
            var inputText = ValueInputField.text;

            Object value = null;
            bool success = false;

            if ( Property.PropertyType == typeof( ControllableCondition ) )
            {
                bool boolValue;

                success = bool.TryParse( inputText, out boolValue );

                if ( success )
                {
                    var controllableCondition = (ControllableCondition) Property.GetValue( PropertyResolutionObject, null );

                    controllableCondition.ConditionValid = boolValue;

                    // Ref type, don't re-assign. We've already set the value on the ref object.
                    success = false;
                }
            }
            else
            {
                success = inputText.StringToValueOfType(Property.PropertyType, ref value);
            }

            if ( success )
                Property.SetValue( PropertyResolutionObject, value, null );
        }


        #region Property Observation Handling

        EventInfo ObservingEvent { get; set; }

        private Delegate ChangeHandlerDelegate { get; set; }    // A formality of reflective event binding...

        private void ConfigureObservation()
        {
            if ( string.IsNullOrEmpty( ControllableAttribute.ObservableEventName ) )
                return;

            ObservingEvent = PropertyResolutionObject.GetType().GetEvent( ControllableAttribute.ObservableEventName );

            if ( ObservingEvent == null )
                throw new MissingMemberException( "Could not find change-observing event named " + ControllableAttribute.ObservableEventName );

            ChangeHandlerDelegate = Delegate.CreateDelegate( ObservingEvent.EventHandlerType, this, ((Action)HandleObservedPropertyChanged).Method );

            ObservingEvent.AddEventHandler( PropertyResolutionObject, ChangeHandlerDelegate );
        }

        private void HandleObservedPropertyChanged()
        {
            ValueInputField.text = Property.GetValue( PropertyResolutionObject, null ).ToString();
        }

        private void CleanUpObservation()
        {
            if ( ObservingEvent == null )
                return;

            ObservingEvent.RemoveEventHandler( PropertyResolutionObject, ChangeHandlerDelegate );
        }

        #endregion


        #region Conditional Handling

        ControllableCondition ControllableCondition { get; set; }

        private void ConfigureConditionality()
        {
            var conditionName = ControllableAttribute.ConditionalPropertyName;
            if ( string.IsNullOrEmpty( conditionName ) )
                return;

            // Check for raw string value of True or False

            if ( String.Compare( conditionName, "true", true, CultureInfo.InvariantCulture ) == 0 ||
                String.Compare( conditionName, "false", true, CultureInfo.InvariantCulture ) == 0)
            {
                ReflectCondition( bool.Parse( conditionName ) );
                return;
            }

            // Check for property by that name

            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var conditionProperty = PropertyResolutionObject.GetType().GetProperty( conditionName, flags );

            if ( conditionProperty == null )
                throw new MissingMemberException( "Could not find condition property named " + conditionName );

            if ( conditionProperty.PropertyType == typeof( ControllableCondition ) )
            {
                ControllableCondition = (ControllableCondition)conditionProperty.GetValue( PropertyResolutionObject, null );

                ControllableCondition.Changed += HandleConditionChanged;

                ReflectCondition( ControllableCondition.ConditionValid );
            }
            else if ( conditionProperty.PropertyType == typeof( bool ) )
            {
                var staticConditionValue = (bool)conditionProperty.GetValue( PropertyResolutionObject, null );

                ReflectCondition( staticConditionValue );
            }
            else
            {
                throw new MemberAccessException( "Property found with name, but not a type or value which can specify conditionality." );
            }
        }

        private void HandleConditionChanged( bool conditionValid )
        {
            ReflectCondition( conditionValid );
        }

        private void ReflectCondition( bool conditionValid )
        {
            if ( ControllableAttribute.InvertConditional )
                conditionValid = !conditionValid;

            if ( ControllableAttribute.ConditionalNotMetBehaviour == ControllableAttribute.ConditionBehaviour.ShowHide )
            {
                gameObject.SetActive( conditionValid );
            }
            else if ( ControllableAttribute.ConditionalNotMetBehaviour == ControllableAttribute.ConditionBehaviour.EnableDisable )
            {
                foreach ( var component in GetComponentsInChildren< Selectable >() )
                    component.interactable = conditionValid;
            }
            else throw new NotImplementedException("Handling of ConditionNotMetBehaviour " + ControllableAttribute.ConditionalNotMetBehaviour + " is not implemented." );
        }

        private void CleanUpConditionality()
        {
            if ( ControllableCondition != null )
            {
                ControllableCondition.Changed -= HandleConditionChanged;
            }
        }

        #endregion

    }
}
