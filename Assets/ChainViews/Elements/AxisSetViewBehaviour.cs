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

namespace ChainViews.Elements
{
    public class AxisSetViewBehaviour : MonoBehaviour, IControllableMemberGeneratable, IBoundsChanger
    {
        public event Action BoundsChanged = delegate { };

        public RectTransform BoundsRectTransform { get { return GetComponent< RectTransform >(); } }

        [SerializeField]
        private RectTransform m_AttachmentPoint = null;
        private RectTransform AttachmentPoint { get { return m_AttachmentPoint; } }


        private readonly List<MonoBehaviour> m_ControllableUiItems = new List<MonoBehaviour>();
        public List<MonoBehaviour> ControllableUiItems { get { return m_ControllableUiItems; } }

        public ISchemaProvider SchemaProvider { get; set; }

        public RectTransform ControllableUiItemRoot { get { return AttachmentPoint; } }

        public System.Object Model
        {
            get { return AxisLabelSet; }
        }


        private AxisLabelSet m_AxisLabelSet;
        public AxisLabelSet AxisLabelSet
        {
            get
            {
                return m_AxisLabelSet;
            }
            set
            {
                m_AxisLabelSet = value;

                ControllableFactory.GenerateControllableUiElements( this );

                foreach ( var boundsChanger in ControllableUiItems.Where( e => e is IBoundsChanger ).Cast< IBoundsChanger >() )
                    boundsChanger.BoundsChanged += () => BoundsChanged();
            }
        }

        [UsedImplicitly]
        public void HandleExpandCollapseButtonPressed()
        {
            AttachmentPoint.gameObject.SetActive( !AttachmentPoint.gameObject.activeSelf );

            BoundsChanged();
        }

        [UsedImplicitly]
        public void HandleRemoveButtonPressed()
        {
            AxisLabelSet.RequestDeletion();
        }
    }
}
