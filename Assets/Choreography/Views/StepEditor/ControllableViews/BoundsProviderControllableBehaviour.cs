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
using System.Linq;
using System.Reflection;
using Bounds;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Choreography.Views.StepEditor.ControllableViews
{
    public class BoundsProviderControllableBehaviour : MonoBehaviour
    {

        [Header("Component References")]

        [SerializeField]
        private Text m_LabelTextComponent = null;
        private Text LabelTextComponent { get { return m_LabelTextComponent; } }

        [SerializeField]
        private RectTransform m_DropDownItemAttachmentPoint = null;
        private RectTransform DropDownItemAttachmentPoint { get { return m_DropDownItemAttachmentPoint; } }

        //[SerializeField]
        //private RectTransform m_DisabledDropDownHolder = null;
        //private RectTransform DisabledDropDownHolder { get { return m_DisabledDropDownHolder; } }

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

            CurrentProvider = PropertyInfo.GetValue( PropertyResolutionObject, null ) as IBoundsProvider;
        }

        private IBoundsProvider m_CurrentProvider;
        private IBoundsProvider CurrentProvider
        {
            get { return m_CurrentProvider; }
            set
            {
                m_CurrentProvider = value;

                CurrentlySelectedItemReadout.text = m_CurrentProvider != null ? value.BoundsProviderKey : "";
            }
        }


        [UsedImplicitly]
        public void Start()
        {
            BoundsProviderRepository.BoundsProviderAdded += HandleBoundsProviderAdded;

            BoundsProviderRepository.BoundsProviderRemoved += HandleBoundsProviderRemoved;

            GenerateDropDownItems();
        }


        [UsedImplicitly]
        public void OnDestroy()
        {
            BoundsProviderRepository.BoundsProviderAdded -= HandleBoundsProviderAdded;

            BoundsProviderRepository.BoundsProviderRemoved -= HandleBoundsProviderRemoved;

            DestroyDropDownItems();
        }


        private void HandleBoundsProviderAdded( IBoundsProvider provider )
        {
            AddDropDownItem( provider );
        }

        private void HandleBoundsProviderRemoved( IBoundsProvider provider )
        {
            if ( provider == CurrentProvider )
                CurrentProvider = null;

            RemoveDropDownItem( provider );
        }


        private void GenerateDropDownItems()
        {
            foreach ( var provider in BoundsProviderRepository.BoundsProvidersEnumerable )
                AddDropDownItem( provider );
        }

        private void AddDropDownItem( IBoundsProvider provider )
        {
            var go = Instantiate( DropDownItemPrefab );
            var item = go.GetComponent< ControllableDropDownItemBehaviour >();

            item.Initialize( provider.BoundsProviderKey, provider );

            item.transform.SetParent( DropDownItemAttachmentPoint, false );

            item.Clicked += HandleDropDownItemClicked;

            DropDownItems.Add( item );
        }


        private void DestroyDropDownItems()
        {
            foreach ( var provider in BoundsProviderRepository.BoundsProvidersEnumerable )
                RemoveDropDownItem( provider );
        }

        private void RemoveDropDownItem( IBoundsProvider provider )
        {
            var item = DropDownItems.FirstOrDefault( i => i.Model == provider );

            if ( item == null )
            {
                return;
            }

            item.Clicked -= HandleDropDownItemClicked;

            Destroy( item.gameObject );

            DropDownItems.Remove( item );
        }



        private void HandleDropDownItemClicked( ControllableDropDownItemBehaviour item )
        {
            PropertyInfo.SetValue( PropertyResolutionObject, item.Model, null );

            CurrentProvider = item.Model as IBoundsProvider;

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
