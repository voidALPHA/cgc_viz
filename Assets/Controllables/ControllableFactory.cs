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
using System.Linq;
using System.Reflection;
using ChainViews;
using ChainViews.Elements;
using Choreography.Views.StepEditor.ControllableViews;
using Mutation;
using PayloadSelection;
using UnityEngine;
using Utility;
using Visualizers;
using VoidAlpha.Utilities;

namespace Controllables
{
    public class ControllableFactory : MonoBehaviour
    {

        #region Static Interface

        private static ControllableFactory s_Instance;
        private static ControllableFactory Instance
        {
            get
            {
                if ( s_Instance == null )
                    s_Instance = FindObjectOfType< ControllableFactory >();

                return s_Instance;
            }
        }

        #endregion

        [Header( "Prefab References" )]

        [SerializeField]
        private GameObject m_ButtonPrefab = null;
        public static GameObject ButtonPrefab { get { return Instance.m_ButtonPrefab; } }

        [SerializeField]
        private GameObject m_LabelPrefab = null;
        public static GameObject LabelPrefab { get { return Instance.m_LabelPrefab; } }

        [SerializeField]
        private GameObject m_TextBoxPrefab = null;
        public static GameObject TextBoxPrefab { get { return Instance.m_TextBoxPrefab; } }

        [SerializeField]
        private GameObject m_MutableBoxPrefab = null;
        public static GameObject MutableBoxPrefab { get { return Instance.m_MutableBoxPrefab; } }

        [SerializeField]
        private GameObject m_MutableTargetViewPrefab = null;
        public static GameObject MutableTargetViewPrefab { get { return Instance.m_MutableTargetViewPrefab; } }

        [SerializeField]
        private GameObject m_MutableScopeViewPrefab = null;
        public static GameObject MutableScopeViewPrefab { get { return Instance.m_MutableScopeViewPrefab; } }

        [SerializeField]
        private GameObject m_LabelSystemViewPrefab = null;
        public static GameObject LabelSystemViewPrefab { get { return Instance.m_LabelSystemViewPrefab; } }


        [SerializeField]
        private GameObject m_PayloadExpressionControllablePrefab = null;
        public static GameObject PayloadExpressionControllablePrefab { get { return Instance.m_PayloadExpressionControllablePrefab; } }



        #region Controllable Members

        public static void DestroyControllableUiElements( IControllableMemberGeneratable view )
        {
            foreach (var c in view.ControllableUiItems)
            {
                DestroyImmediate(c.gameObject);
            }

            view.ControllableUiItems.Clear();
        }

        public static void GenerateControllableUiElements( IControllableMemberGeneratable view )
        {
            if ( view.ControllableUiItems.Any())
                throw new InvalidOperationException("Trying to add controllable elements to a view, but it already has controllable elements.");

            var controllableMembers =
                view.Model.GetType()
                    .GetMembers( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
                    .Where( m => Attribute.IsDefined( m, typeof( ControllableAttribute ) ) );
                    //.OrderBy( m => ( ((ControllableAttribute)m.GetCustomAttributes( typeof( ControllableAttribute ), true )[0]).Order ) );

            foreach ( var m in controllableMembers )
            {
                //var timer = DiagnosticTimer.Start( "AddPropertyorwhaweetetercure" );
                
                if ( m is PropertyInfo )
                {
                    AddProperty( m as PropertyInfo, view );
                }
                else if ( m is MethodInfo )
                {
                    AddMethod( m as MethodInfo, view );
                }
                else
                {
                    Debug.LogWarning( "Member found, not Property or Method but " + m.MemberType + ": " + m.Name );
                }

                //Debug.Log( timer.Stop().Conclusion );
            }
        }

        private static void AddProperty( PropertyInfo propertyInfo, IControllableMemberGeneratable view )
        {
            //try
            //{
                if ( MutableFieldTester.IsMutableField( propertyInfo.PropertyType ) )
                {
                    AddMutableBox( propertyInfo, view );
                }
                else if ( propertyInfo.PropertyType == typeof( MutableTarget ) )
                {
                    AddMutableTarget( propertyInfo, view );
                }
                else if ( propertyInfo.PropertyType == typeof( MutableScope ) )
                {
                    AddMutableScope( propertyInfo, view );
                }
                else if ( propertyInfo.PropertyType == typeof( LabelSystem.LabelSystem ) )
                {
                    AddLabelSystemView( propertyInfo, view );
                }
                else if ( propertyInfo.PropertyType == typeof( string ) )
                {
                    AddTextBox( propertyInfo, view );
                }
                else if ( propertyInfo.PropertyType == typeof( PayloadExpression ) )
                {
                    AddPayloadExpression( propertyInfo, view );
                }
                else
                {
                    Debug.LogErrorFormat( "Controllable attribute does not support type {0}. Use a mutable field.", propertyInfo.PropertyType );

                    AddReadOnlyObjectBox( propertyInfo, view );
                }
            //}
            //catch ( InvalidOperationException ex )
            //{
            //    Debug.LogError( "ERROR! " + ex );

            //    throw;
            //}
        }

        private static void AddMutableBox( PropertyInfo propertyInfo, IControllableMemberGeneratable view )
        {
            if ( view.SchemaProvider == null )
            {
                AddLabel( "SchemaProvider missing at init.", view );
                return;
            }


            var mutableBox = AddElement<MutableBoxBehaviour>( MutableBoxPrefab, view );

            mutableBox.Initialize( propertyInfo, view.Model, view.SchemaProvider );
        }

        private static void AddMutableTarget( PropertyInfo propertyInfo, IControllableMemberGeneratable view )
        {
            if ( view.SchemaProvider == null )
            {
                AddLabel( "SchemaProvider missing at init.", view );
                return;
            }

            var mutableTargetView = AddElement<MutableTargetViewBehaviour>( MutableTargetViewPrefab, view );

            mutableTargetView.Initialize( propertyInfo, view.Model, view.SchemaProvider );
        }

        private static void AddMutableScope( PropertyInfo propertyInfo, IControllableMemberGeneratable view )
        {
            if ( view.SchemaProvider == null )
            {
                AddLabel( "SchemaProvider missing at init.", view );
                return;
            }

            var mutableScopeView = AddElement<MutableScopeView>( MutableScopeViewPrefab, view );

            mutableScopeView.Initialize( propertyInfo, view.Model, view.SchemaProvider );
        }

        private static void AddLabelSystemView( PropertyInfo propertyInfo, IControllableMemberGeneratable view )
        {
            var labelSystemView = AddElement<LabelSystemViewBehaviour>( LabelSystemViewPrefab, view );

            labelSystemView.SchemaProvider = view.SchemaProvider;
            labelSystemView.LabelSystem = propertyInfo.GetValue( view.Model, null ) as LabelSystem.LabelSystem;
        }

        private static void AddTextBox( PropertyInfo propertyInfo, IControllableMemberGeneratable view )
        {
            var textBox = AddElement<TextBoxBehaviour>( TextBoxPrefab, view );

            textBox.TextSubmitted += s =>
            {
                propertyInfo.SetValue( view.Model, s, null );
            };

            textBox.Text = propertyInfo.GetValue( view.Model, null ) as string;

            textBox.LabelText = ControllableUtility.GetControllableLabel( propertyInfo );
        }

        private static void AddReadOnlyObjectBox( PropertyInfo propertyInfo, IControllableMemberGeneratable view )
        {
            var textBox = AddElement<TextBoxBehaviour>( TextBoxPrefab, view );

            textBox.Interactable = false;

            // object-type text boxes are read-only ( interactable is false)
            textBox.TextSubmitted += s =>
            {
            };

            textBox.Text = propertyInfo.GetValue( view.Model, null ).ToString();

            textBox.LabelText = ControllableUtility.GetControllableLabel( propertyInfo );
        }

        private static void AddLabel( string text, IControllableMemberGeneratable view )
        {
            var label = AddElement<LabelBehaviour>( LabelPrefab, view );

            label.Text = text;
        }

        private static void AddMethod( MethodInfo methodInfo, IControllableMemberGeneratable view )
        {
            var button = AddElement<ButtonBehaviour>( ButtonPrefab, view );

            button.LabelText = ControllableUtility.GetControllableLabel( methodInfo );

            button.Clicked += () => methodInfo.Invoke( view.Model, null );
        }

        private static void AddPayloadExpression(PropertyInfo propertyInfo, IControllableMemberGeneratable controllable)
        {
            // set up a payload expression ... 

            var foundObj = propertyInfo.GetValue(controllable.Model, null);
            var expression = foundObj as PayloadExpression;

            var view = AddElement<PayloadExpressionControllableViewBehaviour>(PayloadExpressionControllablePrefab, controllable);
            //var view = go.GetComponent<PayloadExpressionControllableViewBehaviour>();

            //view.transform.SetParent(OptionsRootTransform, false);
            //view.transform.SetAsLastSibling();

            view.PropertyName = ControllableUtility.GetControllableLabel(propertyInfo);
            view.PayloadExpression = expression;

            view.ExternalSchemaProvider = controllable.SchemaProvider;
        }

        private static T AddElement<T>( GameObject prefab, IControllableMemberGeneratable view ) where T : MonoBehaviour
        {
            var go = Instantiate( prefab, view.ControllableUiItemRoot.transform );
            go.transform.localScale = Vector3.one;

            // Universal element touching here

            //go.transform.SetParent( view.ControllableUiItemRoot.transform, false );
            

            var newView = go.GetComponent<T>();


            view.ControllableUiItems.Add( newView );


            return newView;
        }


        #endregion


        public static T AddCustomElement<T>( GameObject prefab, IControllableMemberGeneratable view ) where T : MonoBehaviour
        {
            return AddElement<T>( prefab, view );
        }
    }
}
