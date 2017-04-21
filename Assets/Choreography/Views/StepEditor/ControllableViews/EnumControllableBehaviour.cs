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
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Choreography.Views.StepEditor.ControllableViews
{
    public class EnumControllableBehaviour : MonoBehaviour
    {

        [Header("Component References")]

        [SerializeField]
        private Text m_LabelTextComponent = null;
        private Text LabelTextComponent { get { return m_LabelTextComponent; } }

        [SerializeField]
        private RectTransform m_DropDownItemAttachmentPoint = null;
        private RectTransform DropDownItemAttachmentPoint { get { return m_DropDownItemAttachmentPoint; } }

        [SerializeField]
        private Text m_CurrentlySelectedItemReadout = null;
        private Text CurrentlySelectedItemReadout { get { return m_CurrentlySelectedItemReadout; } }


        [Header("Prefab References")]
        
        [SerializeField]
        private GameObject m_DropDownItemPrefab = null;
        private GameObject DropDownItemPrefab { get { return m_DropDownItemPrefab; } }




        private readonly List<ControllableDropDownItemBehaviour> m_DropDownItems = new List<ControllableDropDownItemBehaviour>();
        private List<ControllableDropDownItemBehaviour> DropDownItems
        {
            get { return m_DropDownItems; }
        }



        private PropertyInfo PropertyInfo { get; set; }

        private System.Object PropertyResolutionObject { get; set; }


        public void Initialize( PropertyInfo propertyInfo, System.Object propertyResolutionObject )
        {
            PropertyInfo = propertyInfo;

            PropertyResolutionObject = propertyResolutionObject;

            LabelTextComponent.text = propertyInfo.Name;

            CurrentlySelectedItemReadout.text = PropertyInfo.GetValue( PropertyResolutionObject, null ).ToString();
        }

        [UsedImplicitly]
        public void Start()
        {
            GenerateDropDownItems();
        }

        [UsedImplicitly]
        public void OnDestroy()
        {
            DestroyDropDownItems();
        }

        private void GenerateDropDownItems()
        {
            foreach ( var str in Enum.GetNames( PropertyInfo.PropertyType ) )
                AddDropDownItem( str );
        }

        private void AddDropDownItem( string str )
        {
            var go = Instantiate( DropDownItemPrefab );
            var item = go.GetComponent< ControllableDropDownItemBehaviour >();

            item.Initialize( str, str );

            item.transform.SetParent( DropDownItemAttachmentPoint, false );

            item.Clicked += HandleDropDownItemClicked;

            DropDownItems.Add( item );
        }


        private void DestroyDropDownItems()
        {
            foreach ( var str in Enum.GetNames( PropertyInfo.PropertyType ) )
                RemoveDropDownItem( str );
        }

        private void RemoveDropDownItem( string str )
        {
            var item = DropDownItems.FirstOrDefault( i => (string)i.Model == str );

            if ( item == null )
                return;

            item.Clicked -= HandleDropDownItemClicked;

            Destroy( item.gameObject );

            DropDownItems.Remove( item );
        }



        private void HandleDropDownItemClicked( ControllableDropDownItemBehaviour item )
        {
            PropertyInfo.SetValue( PropertyResolutionObject, Enum.Parse( PropertyInfo.PropertyType, (string)item.Model ), null );

            CurrentlySelectedItemReadout.text = (string)item.Model;

            HideDropDown();
        }



        [UsedImplicitly]
        public void HandleShowDropDownButtonClicked()
        {
            if ( ShowingDropDown )
                HideDropDown();
            else
                ShowDropDown();
        }

        
        
        private bool ShowingDropDown { get; set; }

        private void ShowDropDown()
        {
            ShowingDropDown = true;

            // move to host

            DropDownItemAttachmentPoint.gameObject.SetActive( true );
        }

        private void HideDropDown()
        {
            ShowingDropDown = false;

            // move back to original resting place

            DropDownItemAttachmentPoint.gameObject.SetActive( false );
        }
    }
}
