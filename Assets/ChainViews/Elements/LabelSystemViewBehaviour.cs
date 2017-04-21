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
using Controllables;
using JetBrains.Annotations;
using LabelSystem;
using Mutation;
using UnityEngine;
using UnityEngine.UI;

namespace ChainViews.Elements
{
    public class LabelSystemViewBehaviour : MonoBehaviour, IControllableMemberGeneratable, IBoundsChanger
    {
        public event Action BoundsChanged = delegate { };

        public RectTransform BoundsRectTransform { get { return GetComponent<RectTransform>(); } }

        private readonly List<MonoBehaviour> m_ControllableUiItems = new List<MonoBehaviour>();
        public List<MonoBehaviour> ControllableUiItems { get { return m_ControllableUiItems; } }

        public RectTransform ControllableUiItemRoot { get { return AxisSetAttachmentPoint; } }

        public ISchemaProvider SchemaProvider { get; set; }

        public System.Object Model
        {
            get { return LabelSystem; }
        }


        [Header("Component References")]
        [SerializeField]
        private Text m_SettingsUiTextComponent;
        private Text SettingsUiTextComponent { get { return m_SettingsUiTextComponent; } }

        [SerializeField]
        private RectTransform m_AxisSetAttachmentPoint;
        private RectTransform AxisSetAttachmentPoint { get { return m_AxisSetAttachmentPoint; } }

        [Header("Prefab References")]
        [SerializeField]
        private GameObject m_AxisSetViewPrefab;
        private GameObject AxisSetViewPrefab { get { return m_AxisSetViewPrefab; } set { m_AxisSetViewPrefab = value; } }

        private List<AxisSetViewBehaviour> m_AxisSetViews = new List<AxisSetViewBehaviour>();
        private List<AxisSetViewBehaviour> AxisSetViews { get { return m_AxisSetViews; } }


        private LabelSystem.LabelSystem m_LabelSystem;
        public LabelSystem.LabelSystem LabelSystem
        {
            get
            {
                return m_LabelSystem;
            }
            set
            {
                m_LabelSystem = value;

                SettingsUiTextComponent.text = m_LabelSystem.LabelSystemUiHeader;

                if (m_LabelSystem is AxialLabelSystem)
                {
                    var axialLabelSystem = m_LabelSystem as AxialLabelSystem;

                    ControllableFactory.GenerateControllableUiElements( this );

                    foreach ( var boundsChanger in ControllableUiItems.Where( e => e is IBoundsChanger ).Cast<IBoundsChanger>() )
                        boundsChanger.BoundsChanged += () => BoundsChanged();

                    // Init any axes that came in at init time (now)
                    foreach ( var axisSet in axialLabelSystem.Axes )
                        HandleLabelSystemAxisSetAdded( axisSet );

                    axialLabelSystem.AxisSetAdded += HandleLabelSystemAxisSetAdded;
                    axialLabelSystem.AxisSetRemoved += HandleLabelSystemAxisSetRemoved;
                }
            }
        }

        [UsedImplicitly]
        public void HandleExpandCollapseButtonPressed()
        {
            AxisSetAttachmentPoint.gameObject.SetActive(!AxisSetAttachmentPoint.gameObject.activeSelf);

            BoundsChanged( );
        }

        [UsedImplicitly]
        public void HandleAddButtonPressed()
        {
            AddAxisSet();
        }

        private void AddAxisSet()
        {
            // may need a better system for this in terms of genericism
            if (LabelSystem is AxialLabelSystem)
            {
                var axialLabelSystem = LabelSystem as AxialLabelSystem;

                axialLabelSystem.AddAxisSet( new AxisLabelSet() );
            }
        }

        private void HandleLabelSystemAxisSetAdded(AxisLabelSet axisLabelSet)
        {
            var axisSetViewGo = Instantiate( AxisSetViewPrefab );
            var axisSetView = axisSetViewGo.GetComponent<AxisSetViewBehaviour>( );

            axisSetView.SchemaProvider = SchemaProvider;
            axisSetView.AxisLabelSet = axisLabelSet;

            var headerText = axisSetViewGo.GetComponentInChildren<Text>( );
            headerText.text = "Axis Settings:";

            
            axisSetView.BoundsChanged += HandleAxisSetViewBoundsChanged;


            axisSetViewGo.transform.SetParent(AxisSetAttachmentPoint.transform, false);


            AxisSetViews.Add(axisSetView);

            
            BoundsChanged();
        }


        private void HandleLabelSystemAxisSetRemoved(AxisLabelSet axisLabelSet)
        {
            var axisSetView = AxisSetViews.FirstOrDefault(e => e.AxisLabelSet == axisLabelSet);

            if (axisSetView == null)
                return;


            axisSetView.BoundsChanged -= HandleAxisSetViewBoundsChanged;


            Destroy(axisSetView.gameObject);


            AxisSetViews.Remove(axisSetView);


            BoundsChanged();
        }


        private void HandleAxisSetViewBoundsChanged()
        {
            BoundsChanged();
        }
    }
}
