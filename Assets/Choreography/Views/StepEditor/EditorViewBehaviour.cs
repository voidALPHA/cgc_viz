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
using System.Linq;
using System.Reflection;
using Adapters.GlobalParameters;
using Bounds;
using Choreography.Steps;
using Choreography.Views.StepEditor.ControllableViews;
using Choreography.Views.StepEditor.CuratedChoreography;
using JetBrains.Annotations;
using Mutation;
using PayloadSelection;
using UnityEngine;
using UnityEngine.UI;
using Visualizers;

namespace Choreography.Views.StepEditor
{
    public class EditorViewBehaviour : MonoBehaviour
    {
        public event Action<Step> TargetClicked = delegate { };

        [Header("Component References")]

        [SerializeField]
        private RectTransform m_NoneSelectedRoot = null;
        private RectTransform NoneSelectedRoot { get { return m_NoneSelectedRoot; } }

        [SerializeField]
        private RectTransform m_SelectedRoot = null;
        private RectTransform SelectedRoot { get { return m_SelectedRoot; } }


        [SerializeField]
        private Image m_TitleBarComponent = null;
        private Image TitleBarComponent { get { return m_TitleBarComponent; } }

        [SerializeField]
        private Text m_StepNameTextComponent = null;
        private Text StepNameTextComponent { get { return m_StepNameTextComponent; } }

        [SerializeField]
        private InputField m_StepNoteInputFieldComponent = null;
        private InputField StepNoteInputFieldComponent { get { return m_StepNoteInputFieldComponent; } }

        [SerializeField]
        private RectTransform m_OptionsRootTransform = null;
        private RectTransform OptionsRootTransform { get { return m_OptionsRootTransform; } }

        [SerializeField]
        private EditorRouterViewBehaviour m_RouterViewComponent = null;
        private EditorRouterViewBehaviour RouterViewComponent { get { return m_RouterViewComponent; } }


        [Header("Prefab References")]

        [SerializeField]
        private GameObject m_OptionViewPrefab = null;
        private GameObject OptionViewPrefab { get { return m_OptionViewPrefab; } }

        [SerializeField]
        private GameObject m_ControllableButtonPrefab = null;
        private GameObject ControllableButtonPrefab { get { return m_ControllableButtonPrefab; } }

        [SerializeField]
        private GameObject m_PayloadExpressionControllablePrefab = null;
        private GameObject PayloadExpressionControllablePrefab { get { return m_PayloadExpressionControllablePrefab; } }

        [SerializeField]
        private GameObject m_BoundsProviderControllablePrefab = null;
        private GameObject BoundsProviderControllablePrefab { get { return m_BoundsProviderControllablePrefab; } }

        [SerializeField]
        private GameObject m_SubStepListViewPrefab = null;
        private GameObject SubStepListViewPrefab { get { return m_SubStepListViewPrefab; } }

        [SerializeField]
        private GameObject m_EnumControllablePrefab = null;
        private GameObject EnumControllablePrefab { get { return m_EnumControllablePrefab; } }

        [SerializeField]
        private GameObject m_BoolControllablePrefab = null;
        private GameObject BoolControllablePrefab { get { return m_BoolControllablePrefab; } }



        private Step m_Step;
        public Step Step
        {
            get
            {
                return m_Step;
            }
            set
            {
                if ( m_Step != null )
                {
                    m_Step.NameChanged -= HandleStepNameChanged;

                    m_Step.NoteChanged -= HandleStepNoteChanged;

                    m_Step.DelayChanged -= HandleStepDelayChanged;

                    DestroyOptionsItems();
                }

                m_Step = value;

                SelectedRoot.gameObject.SetActive( m_Step != null );
                NoneSelectedRoot.gameObject.SetActive( m_Step == null );


                if ( m_Step != null )
                {

                    StepNameTextComponent.text = m_Step.Name;

                    StepNoteInputFieldComponent.text = m_Step.Note;

                    // Set delay text

                    //TitleBarComponent.color = TimelineColorManager.GetStepColor( m_Step );


                    RouterViewComponent.Router = m_Step.Router;


                    m_Step.NameChanged += HandleStepNameChanged;

                    m_Step.NoteChanged += HandleStepNoteChanged;

                    m_Step.DelayChanged += HandleStepDelayChanged;


                    GenerateControllableButtons();

                    GenerateControllableProperties();
                }
            }
        }


        #region Controllable Options

        private readonly List<MonoBehaviour> m_ControllableViews = new List<MonoBehaviour>();
        private List<MonoBehaviour> ControllableViews { get { return m_ControllableViews; } }

        private void GenerateControllableProperties()
        {
            if ( Step == null )
                return;

            var controllableProperties =
                Step.GetType()
                    .GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
                    .Where( property => Attribute.IsDefined( property, typeof( ControllableAttribute ) ) );

            foreach ( var property in controllableProperties )
            {
                if ( property.PropertyType.IsEnum )
                {
                    var go = Instantiate( EnumControllablePrefab, OptionsRootTransform );
                    var view = go.GetComponent<EnumControllableBehaviour>();
                    
                    view.transform.SetAsLastSibling();

                    view.Initialize( property, Step );

                    ControllableViews.Add( view );
                }
                else if ( property.PropertyType == typeof( bool ) || property.PropertyType == typeof( ControllableCondition ) )
                {
                    var go = Instantiate( BoolControllablePrefab, OptionsRootTransform);
                    var view = go.GetComponent<BoolControllableBehaviour>();
                    
                    view.transform.SetAsLastSibling();

                    view.Initialize( property, Step );

                    ControllableViews.Add( view );
                }
                else if ( property.PropertyType == typeof( PayloadExpression ) )
                {
                    var expression = property.GetValue( Step, null ) as PayloadExpression;

                    var go = Instantiate( PayloadExpressionControllablePrefab, OptionsRootTransform);
                    var view = go.GetComponent< PayloadExpressionControllableViewBehaviour >();
                    
                    view.transform.SetAsLastSibling();

                    view.PropertyName = property.Name;
                    view.PayloadExpression = expression;

                    view.ExternalSchemaProvider = null;
                    //new List< ISchemaProvider >{ GlobalVariableDataStore.Instance};

                    ControllableViews.Add( view );
                }
                else if ( property.PropertyType == typeof( IBoundsProvider ) )
                {
                    var go = Instantiate( BoundsProviderControllablePrefab, OptionsRootTransform);
                    var view = go.GetComponent< BoundsProviderControllableBehaviour >();
                    
                    view.transform.SetAsLastSibling();

                    view.Initialize( property, Step );

                    ControllableViews.Add( view );
                }
                else
                {
                    var optionViewGo = Instantiate( OptionViewPrefab, OptionsRootTransform);
                    var optionView = optionViewGo.GetComponent< EditorOptionViewBehaviour >();
                    
                    optionView.transform.SetAsLastSibling();

                    optionView.SetProperty( Step, property );

                    ControllableViews.Add( optionView );
                }
            }

            var subStepContainers = Step.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where( property => Attribute.IsDefined( property, typeof(SubStepContaining) ) );
            foreach ( var property in subStepContainers )
            {
                if ( property.PropertyType == typeof( List< CameraSubStep > ) )
                {
                    var listViewObj = Instantiate( SubStepListViewPrefab, OptionsRootTransform);
                    var listview = listViewObj.GetComponent< CameraSubStepListView >();
                    
                    listview.transform.SetAsLastSibling();

                    listview.CameraSubSteps = property.GetValue(Step, null) as List<CameraSubStep>;

                    ControllableViews.Add( listview );
                }
            }
        }

        private void GenerateControllableButtons()
        {
            if ( Step == null )
                return;

            var controllableMethods =
                Step.GetType()
                    .GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
                    .Where( method => Attribute.IsDefined( method, typeof( ControllableAttribute ) ) );

            foreach ( var method in controllableMethods )
            {
                var buttonGo = Instantiate( ControllableButtonPrefab, OptionsRootTransform);
                var buttonView = buttonGo.GetComponent< EditorControllableButtonBehaviour >();
                
                buttonView.transform.SetAsLastSibling();

                buttonView.SetMethod( Step, method );

                ControllableViews.Add( buttonView );
            }
        }

        private void DestroyOptionsItems()
        {
            foreach ( var item in ControllableViews )
            {
                if ( item is EditorOptionViewBehaviour )
                    (item as EditorOptionViewBehaviour).CleanUp();

                Destroy( item.gameObject );
            }

            ControllableViews.Clear();
        }

        #endregion


        private void HandleStepNameChanged( string newName )
        {
            StepNameTextComponent.text = newName;
        }

        private void HandleStepNoteChanged( string note )
        {
            StepNoteInputFieldComponent.text = note;
        }
        
        private void HandleStepDelayChanged( float delay )
        {
            // Set delay text
        }


        [UsedImplicitly]
        private void Start()
        {
            RouterViewComponent.TargetClicked += HandleRouterTargetClicked;

            Step = null;
        }

        private void HandleRouterTargetClicked( Step target )
        {
            TargetClicked( target );
        }

        [UsedImplicitly]
        public void HandleNoteInputFieldTextChanged()
        {
            Step.Note = StepNoteInputFieldComponent.text;
        }
    }
}
