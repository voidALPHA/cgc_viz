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
using System.Text;
using Chains;
using FocusCamera;
using Mutation;
using UnityEngine;
using Visualizers.MetaSelectors;

namespace Visualizers.ScatterPlot
{
    public class ScatterPlotVisualizer : Visualizer
    {
        public override event Action OnVisualizerDestroyed = delegate { };

        private List<VisualPayload> ScatterPoints = new List<VisualPayload>();


        #region Selection Graph States and Maintenance
        private SelectionState m_NormalState = new SelectionState("Normal");
        private SelectionState m_SelectedState = new SelectionState("Selected");
        private SelectionState m_NoneSelectedState = new SelectionState("None Selected");
        private SelectionState m_NormalStateMulti = new SelectionState("Normal (Group)");
        private SelectionState m_SelectedStateMulti = new SelectionState("Selected (Group)");
        private SelectionState m_NoneSelectedStateMulti = new SelectionState("None Selected (Group)");


        public SelectionState NormalState { get { return m_NormalState; } set { m_NormalState = value; } }
        public SelectionState SelectedState { get { return m_SelectedState; } set { m_SelectedState = value; } }
        public SelectionState NoneSelectedState { get { return m_NoneSelectedState; } set { m_NoneSelectedState = value; } }
        public SelectionState NormalStateMulti { get { return m_NormalStateMulti; } set { m_NormalStateMulti = value; } }
        public SelectionState SelectedStateMulti { get { return m_SelectedStateMulti; } set { m_SelectedStateMulti = value; } }
        public SelectionState NoneSelectedStateMulti { get { return m_NoneSelectedStateMulti; } set { m_NoneSelectedStateMulti = value; } }

        private MetaSelectionSet SelectionManager { get; set; }

        #endregion

        private void Start()
        {

        }

        public void Destroy()
        {
            Destroy(gameObject);

            OnVisualizerDestroyed();
        }

        public void DrawPoint(Vector3 drawPoint, MutableObject mutable)
        {
            var localPoint = transform.localToWorldMatrix.MultiplyPoint(drawPoint);

            var newBound = BoundingBox.ConstructBoundingBox( GetType().Name, localPoint, transform.rotation, Vector3.one);

            newBound.transform.parent = transform;

            newBound.name = "Scatter point: " + drawPoint;

            ScatterPoints.Add(new VisualPayload(mutable, 
                new VisualDescription(newBound)));
        }

        public void ApplyPoints()
        {
            SelectionManager = MetaSelectionSet.ConstructPayloadSelectionSet(Bound, ScatterPoints,
                NormalState, SelectedState, NoneSelectedState, NormalStateMulti, SelectedStateMulti, NoneSelectedStateMulti);

            PayloadSelectorFactory.InstantiateSelectAll(SelectionManager);
            PayloadSelectorFactory.InstantiateFrustumSelect(SelectionManager);
            var clickSelector = PayloadSelectorFactory.InstantiateClickSelect(SelectionManager, gameObject);
            clickSelector.SelectionMode.OperationToPerform = SelectionOperation.Select;
        }

        #region FocusableTarget implementation

        public void UpdateInput()
        {
        }

        public Vector3 CameraLocation()
        {
            throw new NotImplementedException();
        }

        public List<CameraTarget> CameraTargets()
        {
            throw new NotImplementedException();
        }

        public void FocusTarget()
        {
            throw new NotImplementedException();
        }

        public void UnFocusTarget()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
