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

using System.Collections;
using System.Collections.Generic;
using Chains;
using FocusCamera;
using LabelSystem;
using Mutation;
using UnityEngine;
using Visualizers.MetaSelectors;

namespace Visualizers.IsoGrid
{
    public class IsoGridBehaviour : Visualizer, IFocusableTarget
    {
        [Header("Component References")]
        [SerializeField]
        private List< Transform > m_HorizontallyScaledComponents = null;
        private List<Transform> HorizontallyScaledComponents
        {
            get { return m_HorizontallyScaledComponents; }
        }

        // NOTE:  Nothing actually changes the vertical scale, currently.
        [SerializeField]
        private List<Transform> m_VerticallyScaledComponents = null;
        private List<Transform> VerticallyScaledComponents
        {
            get { return m_VerticallyScaledComponents; }
        }

        [SerializeField]
        private List<Transform> m_DepthScaledComponents = null;
        private List<Transform> DepthScaledComponents
        {
            get { return m_DepthScaledComponents; }
        }

        [SerializeField]
        private GameObject m_Apex = null;
        private GameObject Apex { get { return m_Apex; } }

        [SerializeField]
        private BackdropClickSender m_SlicerBackdrop;
        private BackdropClickSender SlicerBackdrop { get { return m_SlicerBackdrop; } set { m_SlicerBackdrop = value; } }

        [SerializeField]
        private GameObject m_SliceObject = null;
        private GameObject SliceObject { get { return m_SliceObject; } set { m_SliceObject = value; } }

        [SerializeField]
        private bool m_AllowSlicer;
        [Controllable]
        public bool AllowSlicer { get { return m_AllowSlicer; } set { m_AllowSlicer = value; } }    // todo: If set to false, turn off the slicer 
        

        [Header("Configuration")]
        [SerializeField]
        private Color m_DefaultColor = new Color( 0.0f, 0.85f, 1.0f );
        private Color DefaultColor { get { return m_DefaultColor; } }

        [SerializeField]
        private bool m_ExpandHeight = false;
        private bool ExpandHeight { get { return m_ExpandHeight; } set { m_ExpandHeight = value; } }

        [SerializeField]
        private bool m_DrawBackgrounds = true;
        public bool DrawBackgrounds { get { return m_DrawBackgrounds; } set { m_DrawBackgrounds = value; } }


        private List<VisualPayload> m_Bars = new List<VisualPayload>();
        private List<VisualPayload> Bars
        {
            get { return m_Bars; }
        }

        #region Child Roots

        private Transform m_BarRoot;
        private Transform BarRoot
        {
            get
            {
                if ( m_BarRoot == null )
                {
                    m_BarRoot = new GameObject( "Bar Root" ).transform;
                    m_BarRoot.gameObject.AddComponent< BoundingBox >();
                    m_BarRoot.SetParent( transform );
                    m_BarRoot.transform.localPosition = Vector3.zero;
                    m_BarRoot.transform.localRotation = Quaternion.identity;
                }

                return m_BarRoot;
            }
        }

        #endregion


        #region Selection Graph States and Maintenance
        public SelectionState NormalState { get; set; }
        public SelectionState SelectedState { get; set; }
        public SelectionState NoneSelectedState { get; set; }
        public SelectionState NormalStateMulti { get; set; }
        public SelectionState SelectedStateMulti { get; set; }
        public SelectionState NoneSelectedStateMulti { get; set; }

        public MetaSelectionSet SelectionManager { get; set; }
        #endregion



        #region Initialization

        private void Start()
        {
            SlicerBackdrop.onBackdropClicked += OnBackdropClicked;
            SlicerBackdrop.onBackdropDragged += OnBackdropDragged;
        }

        public void InitializeIsoGrid(BoundingBox bound)
        {
            Apex.GetComponent<Renderer>( ).material.color = DefaultColor;

            InitializeIsoGrid( );

            //AxialLabelSystem.Initialize( );   // May need to intialize the 'view' (visualizer) here?

            HideBackgrounds( !DrawBackgrounds );
        }

        private void InitializeIsoGrid()
        {
            // May need to come up with a better fix for this, as this isn't the best fix for our memory leak
            Resources.UnloadUnusedAssets();    // Needed to unload unused materials (which are assets); in our case these are the isoGrid materials

            foreach (var bar in Bars)
                Destroy(bar.VisualData.Bound.gameObject);
            Bars.Clear();
        }

        private void HideBackgrounds(bool hideBackgrounds)
        {
            if (!hideBackgrounds)
                return;

            foreach ( var obj in BackgroundComponents() )
            {
                foreach (var rend in obj.GetComponentsInChildren<Renderer>())
                    rend.enabled = false;
                foreach (var col in obj.GetComponentsInChildren<Collider>())
                    col.enabled = false;
            }
        }

        private IEnumerable< Transform > BackgroundComponents()
        {
            foreach ( var obj in HorizontallyScaledComponents )
                yield return obj;
            foreach (var obj in VerticallyScaledComponents)
                yield return obj;
            foreach (var obj in DepthScaledComponents)
                yield return obj;
            yield return Apex.transform;
        }

        #endregion

        #region Runtime Functionality

        public void UpdateHorizontalScale( float horizontalSize )
        {
            foreach ( var component in HorizontallyScaledComponents )
            {
                var s = component.transform.localScale;
                component.transform.localScale = new Vector3(horizontalSize, s.y, s.z);
            }
        }

        public void UpdateDepthScale(float depthSize)
        {
            foreach (var component in DepthScaledComponents)
            {
                var s = component.transform.localScale;
                component.transform.localScale = new Vector3(s.x, s.y, depthSize);
            }
        }


        public IEnumerator Apply()
        {
            SelectionManager = MetaSelectionSet.ConstructPayloadSelectionSet(Bound, 
                Bars, NormalState, SelectedState, NoneSelectedState, NormalStateMulti, SelectedStateMulti, NoneSelectedStateMulti);

            var clickSelector = PayloadSelectorFactory.InstantiateClickSelect(SelectionManager, gameObject);
            clickSelector.SelectionMode.OperationToPerform = SelectionOperation.SelectOnly;

            var controlClickSelector = PayloadSelectorFactory.InstantiateClickSelect(SelectionManager, gameObject, new InputModifiers(){Control = true});
            controlClickSelector.SelectionMode.OperationToPerform = SelectionOperation.ToggleFullySelected;

            var rowSelect = PayloadSelectorFactory.InstantiateRowColumnSelect(SelectionManager);
            rowSelect.SelectionMode.OperationToPerform = SelectionOperation.SelectOnly;

            var controlRowSelect = PayloadSelectorFactory.InstantiateRowColumnSelect(SelectionManager);
            controlRowSelect.SelectionMode.OperationToPerform = SelectionOperation.ToggleFullySelected;

            PayloadSelectorFactory.InstantiateSelectAll(SelectionManager);

            var iterator = SelectionManager.TransmitAll();
            while (iterator.MoveNext( ))
                yield return null;
        }


        #region Runtime Child Creation

        public void AddEntry( int x, int z, MutableObject mutable)
        {
            var barGo = new GameObject("Iso sub-bound");

            var barBound = barGo.AddComponent<BoundingBox>();

            barGo.transform.SetParent(BarRoot);
            barGo.transform.localRotation = Quaternion.identity;
            barGo.transform.localPosition = new Vector3(x, 0, z);
            barGo.transform.localScale = Vector3.one;

            VisualPayload newPayload = new VisualPayload(mutable, 
                new VisualDescription(barBound));
            
            Bars.Add(newPayload);
        }

        #endregion


        private void SetSlicerFromBackdrop()
        {
            Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );

            Plane plane = new Plane( SlicerBackdrop.transform.up, SlicerBackdrop.transform.position );

            float dist;
            plane.Raycast( ray, out dist );

            float yPos = ray.GetPoint( dist ).y;

            SetSlicer(yPos);
        }

        private void SetSlicer(float yPos)
        {
            if (!AllowSlicer)
                return;

            SliceObject.transform.position = new Vector3(transform.position.x, yPos, transform.position.z);

            var renderers = SliceObject.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
                r.enabled = true;
        }

        private void OnBackdropClicked()
        {
            SetSlicerFromBackdrop();
        }

        private void OnBackdropDragged()
        {
            SetSlicerFromBackdrop();
        }

        private void OnBarClicked()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;
            Physics.Raycast( ray, out hitInfo );

            if (Vector3.Dot( hitInfo.normal, Vector3.up ) < 0.99f)
                return;

            var yPos = hitInfo.point.y - 0.001f; // Subtract a bit to prevent Z fighting, and make the top of the bar appear slightly above the slicer

            SetSlicer(yPos);
        }


        #endregion

        #region Focusable Target Implementation

        private Vector3 CurrentFocusPoint { get; set; }

        private Vector3 CameraOffset { get; set; }

        public void UpdateInput()
        {
            float keyboardInput = 0f;

            keyboardInput += Input.GetAxis("Horizontal") * 10f;

            CurrentFocusPoint += Payload.VisualData.Bound.transform.right * Time.deltaTime * keyboardInput;
        }

        public Vector3 CameraLocation()
        {
            return CurrentFocusPoint + CameraOffset;
        }

        public List<CameraTarget> CameraTargets()
        {
            return new List<CameraTarget>(){new CameraTarget(CurrentFocusPoint, 0f)};
        }

        public void FocusTarget()
        {
            CurrentFocusPoint = transform.position + Payload.VisualData.Bound.Size.z * transform.forward * .5f + transform.up * 7f;

            CameraOffset = (transform.right + transform.forward * -1f + transform.up) * 9f;
        }

        public void UnFocusTarget()
        {
        }

        #endregion     
    }
}
