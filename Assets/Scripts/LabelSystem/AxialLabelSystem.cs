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
using JetBrains.Annotations;
using Mutation;
using Newtonsoft.Json;
using UnityEngine;
using Visualizers.MetaSelectors;

namespace LabelSystem
{
    // THIS IS THE "MODEL"
    [JsonObject( MemberSerialization.OptIn )]
    public class AxialLabelSystem : LabelSystem
    {
        public event Action<AxisLabelSet> AxisSetAdded = delegate { };
        public event Action<AxisLabelSet> AxisSetRemoved = delegate { };

        [SerializeField]
        private List<AxisLabelSet> m_Axes = new List<AxisLabelSet>();
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public List<AxisLabelSet> Axes
        {
            get { return m_Axes; }
            [UsedImplicitly]
            private set
            {
                foreach ( var axisLabelSet in value )
                    AddAxisSet( axisLabelSet );
            } 
        }

        #region UI stuff

        public override string LabelSystemUiHeader { get { return "Axial Label Settings"; } }

        #endregion


        public void AddAxisSet(AxisLabelSet axisLabelSet)
        {
            axisLabelSet.LabelSystem = this;
            axisLabelSet.ChainNode = ChainNode;

            axisLabelSet.DeletionRequested += () => RemoveAxisSet( axisLabelSet );

            Axes.Add(axisLabelSet);

            AxisSetAdded( axisLabelSet );
        }

        private void RemoveAxisSet(AxisLabelSet setToBeDeleted)
        {
            Axes.Remove(setToBeDeleted);

            AxisSetRemoved( setToBeDeleted );
        }

        public override void Render(VisualPayload payload, Transform labelRoot, IMetaSelectable selectable)
        {
            LabelRoot = labelRoot;

            RenderChartTitle(payload);

            foreach (var axis in Axes)
                axis.Render(payload, selectable);
        }
    }
}
