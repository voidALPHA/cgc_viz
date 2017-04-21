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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Adapters.GlobalParameters;
using Chains;
using Choreography;
using Choreography.Views;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Utilities;
using Scripts.Utility.Misc;
using Ui;
using Ui.TypePicker;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility;
using Utility.InputManagement;
using Utility.JobManagerSystem;
using Utility.Misc;
using Utility.Undo;
using Button = UnityEngine.UI.Button;
using Debug = UnityEngine.Debug;
using FileDialog = Packages.Plugins.Mono.FileDialog;
using Network = Utility.NetworkSystem.Network;

namespace ChainViews
{
    [JsonObject( MemberSerialization.OptIn )]
    public class ChainView : MonoBehaviour
    {
        

        #region Static Interface

        private static ChainView s_Instance;
        public static ChainView Instance
        {
            get { return s_Instance ?? (s_Instance = FindObjectOfType<ChainView>()); }
        }

        #endregion


        public event Action< Chain > ChainLoaded = delegate { };

        public static event Action< bool > IsBusyChanged = delegate { };

        public static event Action<bool> IsEvaluatingChanged = delegate { };

        public static event Action ErrorOccurred = delegate { };

        public static event Action AddGroupClicked = delegate { };


        #region Inspectable References

        [Header("Component References")]

        [SerializeField]
        private Canvas m_Canvas = null;
        public Canvas Canvas { get { return m_Canvas; } }

        [SerializeField]
        private RectTransform m_EvaluateButton = null;
        private RectTransform EvaluateButton { get { return m_EvaluateButton; } }

        [SerializeField]
        private RectTransform m_CancelButton = null;
        private RectTransform CancelButton { get { return m_CancelButton; } }

        [SerializeField]
        private RectTransform m_SaveButton = null;
        private RectTransform SaveButton { get { return m_SaveButton; }}

        [SerializeField]
        private RectTransform m_SaveAsButton = null;
        private RectTransform SaveAsButton { get { return m_SaveAsButton; } }

        [SerializeField]
        private RectTransform m_LoadButton = null;
        private RectTransform LoadButton { get { return m_LoadButton; } }

        [SerializeField]
        private RectTransform m_AddButton = null;
        private RectTransform AddButton { get { return m_AddButton; } }

        [SerializeField]
        private RectTransform m_NewButton = null;
        private RectTransform NewButton { get { return m_NewButton; } }

        [SerializeField]
        private KeyMnemonic[] m_MnemonicsToGloballyTest;
        private KeyMnemonic[] MnemonicsToTest { get { return m_MnemonicsToGloballyTest; } }


        [SerializeField]
        private RectTransform m_GroupAttachmentPoint = null;
        private RectTransform GroupAttachmentPoint { get { return m_GroupAttachmentPoint; } }


        [SerializeField]
        private LineManagerBehaviour m_LineManager = null;
        private LineManagerBehaviour LineManager { get { return m_LineManager; } }

        [SerializeField]
        private Text m_PackageNameTextComponent = null;
        private Text PackageNameTextComponent { get { return m_PackageNameTextComponent; } }

        [SerializeField]
        private PendingChangesDialogBehaviour m_PendingChangesDialog = null;
        private PendingChangesDialogBehaviour PendingChangesDialog { get { return m_PendingChangesDialog; } }

        [SerializeField]
        private Text m_PendingChangesAsterisk = null;
        private Text PendingChangesAsterisk { get { return m_PendingChangesAsterisk; } }

        [SerializeField]
        private RectTransform m_ZoomCenterIndicator = null;
        private RectTransform ZoomCenterIndicator { get { return m_ZoomCenterIndicator; } }

        [SerializeField]
        private Camera m_Camera = null;
        public Camera Camera { get { return m_Camera; } }

        private Rect m_camRect;
        public Rect CamRect { get { return m_camRect; } private set { m_camRect = value; } }

        private bool Panning { get; set; }

        #region Static Components

        [SerializeField]
        private TypePickerBehaviour m_NewNodePicker = null;
        public static TypePickerBehaviour NewNodePicker { get { return Instance.m_NewNodePicker; } }

        [SerializeField]
        private RectTransform m_TooltipCanvas = null;
        public static RectTransform TooltipCanvas { get { return Instance.m_TooltipCanvas; } }

        #endregion

        #endregion



        [Header( "Prefab References" )]
        
        [SerializeField]
        private GameObject m_GroupViewPrefab = null;
        public static GameObject GroupViewPrefab { get { return Instance.m_GroupViewPrefab; } }

        public bool Dragging { get { return Input.GetMouseButton(0) && Ui.Draggable.IsMouseDown; } } // Short circuit on "is left mouse button down?"

        private float ZoomTimer = -1f;
        public bool Zooming
        {
            set
            {
                if ( value )
                {
                    ZoomTimer = .05f;
                }
                else
                {
                    ZoomTimer = -1f;
                }
            }

            get
            {
                return ZoomTimer > Mathf.Epsilon;
            }
        }
        public bool AllowMouse { get; set; }

        public bool Visible
        {
            get { return Camera.gameObject.activeSelf; }
            set {
                Camera.gameObject.SetActive( value );
                Canvas.gameObject.SetActive( value );
                EventSystem.current.SetSelectedGameObject(null);
            }
        }


        private bool m_DoShowNodeIndices;
        public static bool DoShowNodeIndices
        {
            get { return Instance.m_DoShowNodeIndices; }
            set
            {
                Instance.m_DoShowNodeIndices = value;

                foreach ( var nv in Instance.RootGroupView.RecursiveNodeViewsEnumerable )
                    nv.DoShowIndex = Instance.m_DoShowNodeIndices;
            }
        }


        private Chain m_Chain;
        public Chain Chain
        {
            get { return m_Chain; }
            private set
            {
                if ( m_Chain != null )
                {
                    if ( HaxxisGlobalSettings.Instance.IsVgsJob == false && HaxxisGlobalSettings.Instance.DisableEditor == false )
                    {
                        Chain.RootGroupChanged -= HandleChainRootGroupChanged;
                        Chain.HasErrorChanged -= HandleChainHasErrorChanged;

                        DestroyRootGroupView();
                    }

                    m_Chain.Unload();
                }

                //RealtimeLogger.Log( "Setting chain" );

                m_Chain = value;

                TargetsDirty = true;

                if ( m_Chain != null )
                {
                    if ( HaxxisGlobalSettings.Instance.IsVgsJob == false && HaxxisGlobalSettings.Instance.DisableEditor == false)
                    {
                        Chain.RootGroupChanged += HandleChainRootGroupChanged;
                        Chain.HasErrorChanged += HandleChainHasErrorChanged;

                        CreateRootGroupView();
                    }

                    DumpInfo();

                    ChainLoaded( m_Chain );
                }
            }
        }

        [UsedImplicitly]
        public void OnApplicationQuit()
        {
            Chain = null;
        }

        public void DumpInfo()
        {
            var chainCount = Chain.RootNodes.Count();
            var groupBasedNodes = m_Chain.RootGroup.RecursiveNodesEnumerable;
            var groupBasedNodeCount = groupBasedNodes.Count();

            var groupCount = m_Chain.RootGroup.RecursiveGroupsEnumerable.Count();

            var routerBasedNodes = m_Chain.NodesEnumerableByRouterTraversal;
            var routerBasedNodeCount = routerBasedNodes.Count();

            Debug.Log("ChainView model has " + chainCount + " chains, " + groupBasedNodeCount + " nodes and " + groupCount + " groups. Router-based node count is " + routerBasedNodeCount + ".");

            if ( routerBasedNodeCount != groupBasedNodeCount )
            {
                Debug.LogErrorFormat( "Group-based node count differs from router-traversal node count. ({0} and {1})",
                    groupBasedNodeCount, routerBasedNodeCount );

                Debug.LogError( "Press F4 (configurable) to display node IDs." );
                
                FindUnmatchedNodes( groupBasedNodes, routerBasedNodes );
            }
        }

        private void FindUnmatchedNodes( IEnumerable< ChainNode > groupBasedNodes = null, IEnumerable< ChainNode > routerBasedNodes = null )
        {
            // Consider these enumerables are not order guaranteed. A node's error could be fixed, but its flag not unset until the next error is resolved.
            // This is unlikely to occur in the first place, because these errors are rare enough anyway, but even so, fixing all errors will eventually
            //  yield all nodes that had the errors as fixed.

            if ( groupBasedNodes == null )
                groupBasedNodes = m_Chain.RootGroup.RecursiveNodesEnumerable;

            if ( routerBasedNodes == null )
                routerBasedNodes = m_Chain.NodesEnumerableByRouterTraversal;
            

            var groupBasedNodeIds = groupBasedNodes.Select( n => n.JsonId ).ToList();
            foreach ( var node in routerBasedNodes )
            {
                foreach ( var target in node.Router.UniqueTargets )
                {
                    if ( !groupBasedNodeIds.Contains( target.JsonId ) )
                    {
                        Debug.LogError( "Node " + node.JsonId + " has a target which is not a part of a group." );

                        if ( node.HasBadRouterChildren == false )
                            node.TargetsDirty += HandleNodeWithBadRouterChildrenDirty;

                        node.HasBadRouterChildren = true;

                        return;
                    }
                }

                if ( node.HasBadRouterChildren )
                    node.TargetsDirty -= HandleNodeWithBadRouterChildrenDirty;

                node.HasBadRouterChildren = false;
            }
        }

        private void HandleNodeWithBadRouterChildrenDirty()
        {
            FindUnmatchedNodes();
        }


        private void HandleChainRootGroupChanged( ChainGroup group )
        {
            DestroyRootGroupView();

            CreateRootGroupView();
        }



        #region Selected SelectionState and ChainNode

        private ChainNode m_SelectedChainNode;
        public static ChainNode SelectedChainNode
        {
            get { return Instance.m_SelectedChainNode; }
            set
            {
                if ( Instance.m_SelectedChainNode != null )
                {
                    var nodeView = Instance.RootGroupView.FindChainNodeView( Instance.m_SelectedChainNode, true );
                    if ( nodeView != null )
                        nodeView.Selected = false;
                }

                // Check for match and early exit...
                if ( SelectedSelectionState != null )
                {
                    SelectedSelectionState.AddTarget( value );

                    SelectedSelectionState = null;

                    Instance.m_SelectedChainNode = null;

                    return;
                }


                Instance.m_SelectedChainNode = value;

                if ( Instance.m_SelectedChainNode != null )
                {
                    var nodeView = Instance.RootGroupView.FindChainNodeView( Instance.m_SelectedChainNode, true );
                    if ( nodeView != null )
                        nodeView.Selected = true;
                }
            }
        }

        private SelectionState m_SelectedSelectionState;
        public static SelectionState SelectedSelectionState
        {
            get { return Instance.m_SelectedSelectionState; }
            set
            {
                if ( Instance.m_SelectedSelectionState != null )
                {
                    Instance.m_SelectedSelectionState.Selected = false;
                }


                // Check for match and early exit...
                if ( SelectedChainNode != null )
                {
                    value.AddTarget( SelectedChainNode );

                    Instance.m_SelectedSelectionState = null;

                    SelectedChainNode = null;

                    return;
                }


                Instance.m_SelectedSelectionState = value;

                if ( Instance.m_SelectedSelectionState != null )
                {
                    Instance.m_SelectedSelectionState.Selected = true;
                }
            }
        }

        #endregion


        private bool m_IsBusy;
        public bool IsBusy
        {
            get { return m_IsBusy; }
            set
            {
                if ( value == m_IsBusy )
                    return;

                m_IsBusy = value;

                SetControlButtonEnabledStates();

                IsBusyChanged( m_IsBusy );
            }
        }

        private bool m_IsEvaluating;
        public bool IsEvaluating
        {
            get
            {
                return m_IsEvaluating;
            }
            set
            {
                m_IsEvaluating = value;

                SetControlButtonEnabledStates();

                IsEvaluatingChanged( m_IsEvaluating );
            }
        }

        private uint EvaluateJobId { get; set; }

        
        
        #region ViewModel

        public class ChainViewModel
        {
            [JsonProperty]
            public ChainGroupView.ChainGroupViewModel RootGroupViewModel { get; set; }

            [Obsolete( "This is temporary for backward compatibility of non-chaingroup chains." )]
            [JsonProperty]
            public List< ChainNodeView.ChainNodeViewModel > NodeViewModels { get; set; }

            [JsonConstructor, UsedImplicitly]
            private ChainViewModel()
            {
            }

            public ChainViewModel( ChainView chainView )
            {
                RootGroupViewModel = chainView.RootGroupView.ViewModel;
            }

            public ChainViewModel( ChainGroupView.ChainGroupViewModel rootGroupViewModel )
            {
                RootGroupViewModel = rootGroupViewModel;
            }
        }

        private ChainViewModel ViewModel
        {
            get
            {
                return new ChainViewModel( this );
            }
            set
            {
                if ( value == null )
                    return;

                // Backwards compatiblity...
                if ( value.RootGroupViewModel == null )
                {
                    value.RootGroupViewModel = new ChainGroupView.ChainGroupViewModel( RootGroupView, value.NodeViewModels );
                }

                RootGroupView.ViewModel = value.RootGroupViewModel;

                TargetsDirty = true;
            }
        }


        

        #endregion


        public static bool GroupTransferModeActive
        {
            get { return Input.GetKey( KeyCode.LeftControl ) || Input.GetKey( KeyCode.RightControl ); }
        }

        public ChainGroupView RootGroupView { get; set; }

        private void CreateRootGroupView()
        {
            var viewGo = Instantiate( GroupViewPrefab );
            RootGroupView = viewGo.GetComponent< ChainGroupView >();

            
            RootGroupView.transform.SetParent( GroupAttachmentPoint, false );

            RootGroupView.BoundsChanged += HandleRootGroupBoundsChanged;

            //RealtimeLogger.Log( "Setting group of root groupview" );
            RootGroupView.Group = Chain.RootGroup;
            //RealtimeLogger.Log( "Done setting group of rootgroup view" );

            TargetsDirty = true;
        }

        private void DestroyRootGroupView()
        {
            RootGroupView.BoundsChanged -= HandleRootGroupBoundsChanged;

            RootGroupView.Destroy();

            RootGroupView = null;

            TargetsDirty = true;
        }

        private void HandleRootGroupBoundsChanged()
        {
        }

        //public ChainNodeView GetChainNodeView( ChainNode node )
        //{
        //    return RootGroupView.GetChainNodeView( node, true );
        //}

        
        private void HandleChainHasErrorChanged( bool chainHasErrors )
        {
            SetControlButtonEnabledStates();
        }

        

        public static List< ChainNodeView > GetDescendentNodeViews( ChainNodeView chainNodeView )
        {
            var descendentNodes = new List<ChainNode>();

            GetDescendentsOfNode( chainNodeView.ChainNode, descendentNodes );

            return Instance.NodeViewsEnumerable.Where( nodeView => descendentNodes.Contains( nodeView.ChainNode ) ).ToList();
        }

        
        private static void GetDescendentsOfNode( ChainNode parentNode, List<ChainNode> descendents )
        {
            foreach ( var node in parentNode.Router.UniqueTargets )
            {
                descendents.Add( node );

                GetDescendentsOfNode( node, descendents );
            }
        }


        private IEnumerable< ChainNodeView > NodeViewsEnumerable
        {
            get { return RootGroupView.RecursiveNodeViewsEnumerable; }
        }

        private IEnumerable< ChainGroupView > GroupViewsEnumerable
        {
            get
            {
                if ( RootGroupView == null ) return null;

                var all = new List< ChainGroupView > { RootGroupView };
                    
                all.AddRange( RootGroupView.RecursiveGroupViewsEnumerable );

                return all;
            }
        }

        public ChainGroupView GetGroupViewForGroup( ChainGroup group )
        {
            return GroupViewsEnumerable.FirstOrDefault( g => g.Group == group );
        }

        public bool TargetsDirty { get; set; }

        private static void UpdateTargetLines()
        {
            if (!Instance.TargetsDirty
                || Instance.Dragging
                || Instance.Zooming
                || HaxxisGlobalSettings.Instance.IsVgsJob == true
                || HaxxisGlobalSettings.Instance.DisableEditor == true)
                return;

            if ( Instance.Chain == null )
            {
                Instance.LineManager.SetLines( null );
                return;
            }

            //Debug.Log( "Updating target lines at " + Time.realtimeSinceStartup );

            //Debug.Log( "Updating target lines." );

            var pairs = new List< LineManagerBehaviour.RectTransformPair >();

            //int i = 0;

            Dictionary<ChainNode, ChainNodeView> allChainNodeViews = new Dictionary<ChainNode, ChainNodeView>();
            foreach(var nodeView in Instance.NodeViewsEnumerable)
            {
                allChainNodeViews[nodeView.ChainNode] = nodeView;
            }

            foreach ( var nodeView in allChainNodeViews.Values )
                foreach ( var targetLabel in nodeView.TargetLabels )
                {
                    if(targetLabel.transform.parent.position.sqrMagnitude < 0.001f)
                    {
                        continue;
                    }
                    //var outputTransform = nodeView.Expanded ? targetLabel.LineOutputTransform : nodeView.LineOutputTransform;
                    var outputTransform = targetLabel.LineOutputTransform;
                        

                    if (allChainNodeViews.ContainsKey(targetLabel.Target))
                    {
                        var inputTransform = allChainNodeViews[targetLabel.Target].LineInputTransform;

                        pairs.Add( new LineManagerBehaviour.RectTransformPair( outputTransform, inputTransform ) );
                    }

                }

            //Debug.Log( "i is " + i );
            
            //return;

            Instance.LineManager.SetLines( pairs );

            Instance.TargetsDirty = false;
        }

        

        //private void UpdateLineManagerToMatchRootGroup()
        //{
        //    // TODO: Vet this...
        //    LineManager.RectTransform.localPosition = RootGroupView.RectTransform.localPosition;
        //    LineManager.RectTransform.sizeDelta = RootGroupView.RectTransform.sizeDelta;
        //}

        
        

        [UsedImplicitly]
        private void Start()
        {
            if ( Chain == null )
                Chain = new Chain();

            //InvokeRepeating( "RefreshDirtyState", 2.0f, 2.0f );

            ScaleLevel = ScaleFactors.IndexOf( 1.0f );

            AllowMouse = true;
            CamRect = new Rect(Camera.transform.position.x - Camera.orthographicSize * Camera.aspect,
                Camera.transform.position.y - Camera.orthographicSize, Camera.orthographicSize * 2f * Camera.aspect,
                Camera.orthographicSize * 2f);
        }



        private bool m_IsDirty;
        private bool IsDirty
        {
            get { return m_IsDirty; }
            set
            {
                m_IsDirty = value;
                PendingChangesAsterisk.gameObject.SetActive(m_IsDirty);
            }
        }

        private void RefreshDirtyState()
        {
            if ( IsBusy )
                return;

            if ( TimelineViewBehaviour.Instance == null )
                return;

            if ( TimelineViewBehaviour.Instance.Timeline.IsBusy )
                return;

            if ( HaxxisGlobalSettings.Instance.IsVgsJob == true )
                return;

            if ( HaxxisGlobalSettings.Instance.DisableEditor == true )
                return;

            if (IsDirty)
                return;

            var hp = new HaxxisPackage( Chain, ViewModel,
                    TimelineViewBehaviour.Instance.Timeline.IsEmpty ?
                    null : 
                    new ChoreographyPackage( TimelineViewBehaviour.Instance.Timeline ) );

            IsDirty = HaxxisPackage.IsChanged( hp, LoadedPackagePath );
        }


        [UsedImplicitly]
        private void Update()
        {
            if ( Zooming )
            {
                ZoomTimer -= Time.deltaTime;

                RootGroupView.EnsureWorkspaceIsVisible();

                return;
            }

            if ( Dragging || HaxxisGlobalSettings.Instance.IsVgsJob == true) return;

            if ( Input.GetButtonDown( "Toggle Chain Workspace" ) && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) )
            {
                Visible = !Visible;
            }

            if ( Input.GetButtonDown( "Toggle Node Debug" ) )
            {
                DoShowNodeIndices = !DoShowNodeIndices;
            }

            // Is handled by a KeyMnemonic on the Eval button, called below
            //if ( Input.GetButtonDown("Evaluate") && (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) && !IsBusy && !Chain.HasError )
            //{
            //    HandleEvaluateClicked();
            //}

            // Is handled by a KeyMnemonic on the Save button, called below
            //if (Input.GetButtonDown("Save") && !IsBusy)
            //{
            //    SaveCurrentPackage();
            //}

            foreach(var keyMnemonic in m_MnemonicsToGloballyTest)
            {
                keyMnemonic.Test();
            }

            if ( Input.GetButtonDown( "Dump Package Args" ) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                DumpCommandLineArgNodes();
            }

            if ( Input.GetButtonDown( "Refresh ChainView Layout") && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                RootGroupView.RefreshLayout();
            }

            // Temp debug measure...
            if ( Input.GetKeyDown( KeyCode.F1 ) )
            {
                if ( ( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) ) &&
                    ( Input.GetKey( KeyCode.LeftControl ) || Input.GetKey( KeyCode.RightControl ) ) )
                {
                    var allCanvases = FindObjectsOfType( typeof( Canvas ) ).Cast< Canvas >();
                    foreach ( var c in allCanvases )
                    {
                        var message = string.Format( "[CANVAS] Order: {0}. Path: {1}", c.sortingOrder, c.transform.GetHierarchyString() );

                        Debug.Log( message, c );
                    }
                }
            }

            if ( !Visible )
                return;

            bool refreshCamRect = false;

            if(!InputFocusManager.Instance.IsAnyInputFieldInFocus())
            {
                if (Input.GetButtonDown("ChainView Zoom In"))
                {
                    Zoom( 1, Input.mousePosition);

                    refreshCamRect = true;
                }
                if (Input.GetButtonDown("ChainView Zoom Out"))
                {
                    Zoom(-1, Input.mousePosition);

                    refreshCamRect = true;
                }

                var doScroll = false;
                var scrollX = 0.0f;
                var scrollY = 0.0f;
                if (Input.GetButtonDown("ChainView Scroll Up"))
                {
                    doScroll = true;
                    scrollY = 170.0f;
                }
                if (Input.GetButtonDown("ChainView Scroll Down"))
                {
                    doScroll = true;
                    scrollY = -170.0f;
                }
                if (Input.GetButtonDown("ChainView Scroll Right"))
                {
                    doScroll = true;
                    scrollX = 170.0f;
                }
                if (Input.GetButtonDown("ChainView Scroll Left"))
                {
                    doScroll = true;
                    scrollX = -170.0f;
                }
                if ( doScroll )
                {
                    if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    {
                        scrollX *= 2.0f;
                        scrollY *= 2.0f;
                    }

                    var pos = Camera.transform.position;
                    pos.x += scrollX;
                    pos.y += scrollY;
                    Camera.transform.position = pos;

                    RootGroupView.EnsureWorkspaceIsVisible();

                    refreshCamRect = true;
                }
            }

            var mousePos = Vector3.zero;
            if(RectTransformUtility.ScreenPointToWorldPointInRectangle(Canvas.transform as RectTransform,
                Input.mousePosition, Camera, out mousePos))
            {
                if(AllowMouse && !Panning && new Rect(0f, TimelineViewBehaviour.Instance.CurrentHeight, Camera.pixelWidth, Camera.pixelHeight - TimelineViewBehaviour.Instance.CurrentHeight).Contains(Input.mousePosition))
                {
                    if (Input.GetAxis("Mouse ScrollWheel") > Mathf.Epsilon)
                    {
                        Zoom( 1, Input.mousePosition);

                        refreshCamRect = true;
                    }
                    if (Input.GetAxis("Mouse ScrollWheel") < -Mathf.Epsilon)
                    {
                        Zoom(-1, Input.mousePosition);

                        refreshCamRect = true;
                    }

                    if(Input.GetMouseButtonDown(2))
                    {
                        startingPos = mousePos;
                        Panning = true;
                    }
                }

                if(Panning)
                {
                    if(Input.GetMouseButton(2))
                    {
                        Camera.transform.position += startingPos - mousePos;
                    }

                    if(Input.GetMouseButtonUp(2))
                    {
                        Panning = false;

                        refreshCamRect = true;
                    }
                }
            }

            if(refreshCamRect)
            {
                CamRect = new Rect(Camera.transform.position.x - Camera.orthographicSize * Camera.aspect,
                    Camera.transform.position.y - Camera.orthographicSize, Camera.orthographicSize * 2f * Camera.aspect,
                    Camera.orthographicSize * 2f);
                if(Input.GetMouseButtonUp(2))
                {
                    RootGroupView.EnsureWorkspaceIsVisible();
                    CamRect = new Rect(Camera.transform.position.x - Camera.orthographicSize * Camera.aspect,
                        Camera.transform.position.y - Camera.orthographicSize, Camera.orthographicSize * 2f * Camera.aspect,
                        Camera.orthographicSize * 2f);
                }
            }
                
        }

        private Vector3 startingPos = Vector3.zero;

        private void DumpCommandLineArgNodes()
        {
            if ( Chain == null )
                return;

            var demoStringBuilder = new StringBuilder();
            demoStringBuilder.Append( HaxxisPackage.GetRelativePath( LoadedPackagePath + "\n" ));

            var helpStringBuilder = new StringBuilder();
            helpStringBuilder.Append( HaxxisPackage.GetRelativePath( LoadedPackagePath + "\n" ) );

            foreach ( var node in Chain.NodesEnumerableByRouterTraversal
                .Where( n => n is CommandLineArgumentAdapter )
                .Cast< CommandLineArgumentAdapter >()
                .OrderBy( n => n.ParameterName.LiteralValue ) )
            {
                var curStringBuilder = new StringBuilder();

                curStringBuilder.Append( " -" );
                curStringBuilder.Append( node.ParameterName.LiteralValue );
                curStringBuilder.Append( "=" );
                curStringBuilder.Append( node.DefaultValue.LiteralValue );

                demoStringBuilder.Append( curStringBuilder );
                helpStringBuilder.Append( curStringBuilder );

                if ( !string.IsNullOrEmpty( node.Comment ))
                {
                    helpStringBuilder.Append( "    " );
                    helpStringBuilder.Append( node.Comment );
                }

                helpStringBuilder.Append( "\n" );
            }

            Debug.Log( demoStringBuilder.ToString() );
            Debug.Log( helpStringBuilder.ToString() );
        }

        [UsedImplicitly]
        private void LateUpdate()
        {
            UpdateTargetLines();
        }


        private void SetControlButtonEnabledStates()
        {
            EvaluateButton.GetComponent<Button>().interactable = !IsBusy && !Chain.HasError;

            CancelButton.GetComponent<Button>().interactable = IsEvaluating;

            SaveButton.GetComponent<Button>().interactable = !IsBusy;
            SaveAsButton.GetComponent<Button>().interactable = !IsBusy;
            LoadButton.GetComponent<Button>().interactable = !IsBusy;
            AddButton.GetComponent<Button>().interactable = !IsBusy;
            NewButton.GetComponent<Button>().interactable = !IsBusy;
        }


        #region Evaluation

        [UsedImplicitly]
        private void HandleEvaluateClicked()
        {
            if(!IsBusy && !Chain.HasError)
                EvaluateChain();
        }

        public void EvaluateChain()
        {
            if ( Chain == null )
                return;

            IsBusy = true;

            IsEvaluating = true;

            // Note that we are setting 'max executions per frame' to 1 here, so that we can see the 'busy' icon right away
            EvaluateJobId = JobManager.Instance.StartJob(EvaluateJob(), EvaluateCompleted, EvaluateCancelled, "Evaluate", maxExecutionsPerFrame: 1);
        }

        private void EvaluateCompleted(uint jobId)
        {
            IsEvaluating = false;   // Order is important here; clear this before clearing IsBusy

            IsBusy = false;
        }

        private void EvaluateCancelled(uint jobId, bool timedOut, Exception exception)
        {
            if (timedOut || exception != null)  // Don't consider it an error if the user just clicked the cancel button
                ErrorOccurred();    // Must raise this event before setting IsBusy

            HaxxisGlobalSettings.Instance.ReportVgsError( 7, "HP had evaluation errors" );

            IsBusy = false;

            IsEvaluating = false;

            TimelineViewBehaviour.Instance.Stop();

            //Chain.Unload();

            if ( exception == null )
                Debug.Log( "Evaluation was CANCELLED" );
            else
                Debug.Log( "Evaluation had an EXCEPTION" );
        }

        private IEnumerator EvaluateJob()
        {
            var sw = new Stopwatch();
            sw.Start( );

            yield return null;  // This allows us to skip a couple of frames to allow the 'busy' indicator to appear
            yield return null;

            var jobId = JobManager.Instance.StartJobAndPause(Chain.EvaluateRootNodes(), jobName: "EvalRootNodes", startImmediately: true);

            if (JobManager.Instance.IsJobRegistered(jobId)) // If the spawned job has not completed...
                yield return null;

            Debug.Log("EvaluateJob took " + sw.Elapsed + " seconds");
        }

        [UsedImplicitly]
        public void HandleCancelClicked()
        {
            if (IsBusy && IsEvaluating) // Note that the 'cancel' button is just meant for Evaluation, not load, save, etc.
            {
                JobManager.Instance.CancelJob( EvaluateJobId, true );
            }
        }


        private string m_LoadedPackagePath;
        public string LoadedPackagePath
        {
            get
            {
                return m_LoadedPackagePath;
            }
            private set
            {
                m_LoadedPackagePath = value;

                UpdateNameText();
            }
        }

        #endregion


        #region File management

        public class PackageRequest
        {
            public string Path { get; set; }

            private HaxxisPackage m_Package = null;

            public HaxxisPackage Package
            {
                get
                {
                    if(m_Package == null)
                    {
                        if(!ValidatePackagePath(Path))
                        {
                            return null;
                        }

                        var loadedPackage = HaxxisPackage.LoadJson(Path);

                        if(!ValidatePackage(loadedPackage))
                        {
                            return null;
                        }

                        m_Package = loadedPackage;
                    }

                    return m_Package;
                }
                set { m_Package = value; }
            }

            public ChainGroupView RequestingGroupView { get; set; }

            public PackageRequest( string path, ChainGroupView requestingGroupView )
            {
                Path = path;
                RequestingGroupView = requestingGroupView;
            }

            public PackageRequest(string path, HaxxisPackage package, ChainGroupView requestingGroupView)
            {
                Package = package;
                Path = path;
                RequestingGroupView = requestingGroupView;
            }
        }

        private static readonly FilePicker.FileFilter hpFilter = new FilePicker.FileFilter
        {
            FilterName = "Haxxis Package",
            FilterExtensions = new[] { "json" }
        };


        #region Save
        
        [UsedImplicitly]
        [Obsolete( "Don't use; this is a UI callback." )]
        public void HandleSaveAsClicked()
        {
            SavePackageAs( new PackageRequest( LoadedPackagePath, null ) );
        }

        public void SavePackageAs( PackageRequest packageRequest )
        {
            var res = FileDialog.ShowSaveDialog(packageRequest.Path, "Save Haxxis Package As...", new[] { hpFilter });

            if(!res.PickSuccess)
            {
                Debug.Log("User cancelled save as");
                return;
            }

            packageRequest.Path = res.FileLocation;

            SaveFile( packageRequest );
        }

        [UsedImplicitly]
        [Obsolete("Don't use; this is a UI callback.")]
        public void HandleSaveClicked()
        {
            SaveCurrentPackage();
        }

        private void SaveCurrentPackage()
        {
            if(!IsBusy)
                SavePackage( new PackageRequest( LoadedPackagePath, null ) );
        }

        // Public interface to saving
        public void SavePackage( PackageRequest packageRequest )
        {
            if ( string.IsNullOrEmpty( packageRequest.Path ) )
                SavePackageAs( packageRequest );
            else
                SaveFile( packageRequest );
        }

        private void SaveFile( PackageRequest packageRequest )
        {
            IsBusy = true;

            // Note that we are setting 'max executions per frame' to 1 here, so that we can see the 'busy' icon right away
            JobManager.Instance.StartJob(SaveFileJob(packageRequest), SaveFileCompleted, SaveFileCancelled, "Save", maxExecutionsPerFrame: 1);
        }

        private ChainGroupView LastSavedGroupView { get; set; }

        private IEnumerator SaveFileJob( PackageRequest packageRequest )
        {
            var sw = new Stopwatch();
            sw.Start();

            yield return null;  // This allow us to skip a couple of frames to allow the 'busy' indicator to appear
            yield return null;

            if ( packageRequest.RequestingGroupView == null )
            {
                var hp = new HaxxisPackage( Chain, ViewModel,
                    TimelineViewBehaviour.Instance.Timeline.IsEmpty ?
                    null :
                    new ChoreographyPackage( TimelineViewBehaviour.Instance.Timeline ) );

                HaxxisPackage.SaveJson( hp, packageRequest.Path );

                LoadedPackagePath = packageRequest.Path;
            }
            else
            {
                try
                {
                    // This indicates a group save is happening, for use by children as they are serialized.
                    ChainGroup.SerializingGroup = packageRequest.RequestingGroupView.Group;

                    packageRequest.RequestingGroupView.LoadedPackagePath = packageRequest.Path;

                    var hp = GetHaxxisPackageForGroupView( packageRequest.RequestingGroupView );

                    HaxxisPackage.SaveJson( hp, packageRequest.Path );

                    LastSavedGroupView = packageRequest.RequestingGroupView;
                }
                catch
                {
                    Debug.LogError( "Group save failed." );
                }
                finally
                {
                    ChainGroup.SerializingGroup = null;
                }
            }


            Debug.Log("Save took " + sw.Elapsed + " seconds");
        }

        

        private void SaveFileCompleted(uint jobId)
        {
            IsBusy = false;

            if ( LastSavedGroupView != null )
                LastSavedGroupView.RefreshDirtyState();
            else
                IsDirty = false;
        }

        private void SaveFileCancelled(uint jobId, bool timedOut, Exception exception)
        {
            ErrorOccurred();    // Must raise this event before setting IsBusy

            IsBusy = false;

            IsDirty = false;
            RefreshDirtyState();

            if (exception != null)
                Debug.Log("Save had an EXCEPTION");
        }

        #endregion

        #region Load

        [UsedImplicitly]
        [Obsolete( "Don't use; this is a UI callback." )]
        public void HandleLoadClicked()
        {
            if(IsBusy) return;

            RefreshDirtyState();
            
            //IsBusy = true;

            if ( IsDirty )
                PendingChangesDialog.Show( SelectFileToLoad, () => IsBusy = false );
            else
                SelectFileToLoad();
        }


        private void SelectFileToLoad()
        {
            var res = FileDialog.ShowOpenDialog("Load Haxxis Package", new []{hpFilter});
            if(res.PickSuccess)
                LoadPackage( new PackageRequest( res.FileLocation, null ) );
            else
                Debug.Log("User cancelled load");
        }

        public void LoadPackage( PackageRequest request )
        {
            IsBusy = true;

            // Note that we are setting 'max executions per frame' to 1 here, so that we can see the 'busy' icon right away
            JobManager.Instance.StartJob(LoadFileJob( request ), LoadFileCompleted, LoadFileCancelled, "Load", maxExecutionsPerFrame: 1);
        }


        private IEnumerator LoadFileJob( PackageRequest request )
        {
            yield return null;  // This allow us to skip a couple of frames to allow the 'busy' indicator to appear
            
            ChainNodeView.NextJsonId = 0;

            RealtimeLogger.Log("Starting load file job.");
            yield return null;

            SatelliteUpdateLord.Clear();

            var loadedPackage = request.Package;

            if(loadedPackage == null)
                yield break;

            yield return null;

            // Possible that this would be better in Chain setter; as a fix for old chain being in place when global
            //   value nodes in new chain impact global repo. This may cause the What? (error indicator border) message.
            loadedPackage.Chain.InitializeSchema();

            if ( request.RequestingGroupView == null )  // Null implies plain load; otherwise, it's a reload
            {
                // Remove all of the old nodes (by this point in time, the new nodes have already been added to the dictionary.)
                HashSet<ChainNode> newNodes = new HashSet<ChainNode>(loadedPackage.Chain.RootGroup.RecursiveNodesEnumerable);

                foreach(var pair in StateRouter.NodeParents.ToList())
                {
                    if(!newNodes.Contains(pair.Key)) StateRouter.NodeParents.Remove(pair.Key);
                }

                Chain = null;
                
                yield return null;

                Chain = loadedPackage.Chain;

                yield return null;

                if ( HaxxisGlobalSettings.Instance.IsVgsJob == false && HaxxisGlobalSettings.Instance.DisableEditor == false )
                {
                    ViewModel = loadedPackage.ChainViewModel;
                }


                if ( loadedPackage.Choreography == null )
                    TimelineViewBehaviour.Instance.Timeline = null;
                else if ( loadedPackage.Choreography.Timeline == null )
                    TimelineViewBehaviour.Instance.Timeline = null;
                else
                    TimelineViewBehaviour.Instance.Timeline = loadedPackage.Choreography.Timeline;
                    

                LoadedPackagePath = request.Path;
            }
            else
            {
                var originalViewPosition = request.RequestingGroupView.ViewModel.Position;

                loadedPackage.ChainViewModel.RootGroupViewModel.Position = originalViewPosition;

                //Debug.Log("Origin position is " + originalViewPosition );

                request.RequestingGroupView.Group.RequestReplacement( loadedPackage );

                var foundGroupView = GroupViewsEnumerable.FirstOrDefault(view => view.Group == loadedPackage.Chain.RootGroup);
                
                foundGroupView.LoadedPackagePath = request.Path;


                // Omit timeline...
            }
        }

        private void LoadFileCompleted( uint jobId )
        {
            IsBusy = false;

            IsDirty = false;

            RealtimeLogger.Log("Finished load file job.");

            UndoLord.Instance.DropStack();
            GraphicRaycasterVA.Instance.ForceReset();

            //Chain.Unload( );
        }

        private void LoadFileCancelled(uint jobId, bool timedOut, Exception exception)
        {
            ErrorOccurred();    // Must raise this event before setting IsBusy

            IsBusy = false;

            IsDirty = false;
            RefreshDirtyState();

            //Chain.Unload();

            if ( exception == null )
                Debug.Log( "Load was CANCELLED" );
            else
                Debug.Log("Load had an EXCEPTION");
        }

        private static bool ValidatePackagePath(string fullPath)
        {
            if ( string.IsNullOrEmpty( fullPath ) )
            {
                Debug.Log( "Path not set from load dialog." );
                
                return false;
            }

            if ( !HaxxisPackage.FileExists( fullPath ) )
            {
                Debug.Log( "File " + fullPath + " does not exist." );

                return false;
            }

            if(!fullPath.EndsWith(".json"))
            {
                Debug.Log("File is not a JSON file.");

                return false;
            }

            return true;
        }

        private static bool ValidatePackage( HaxxisPackage package )
        {
            if ( package == null )
            {
                Debug.Log( "Package is null." );
                
                return false;
            }

            return ValidateChain( package.Chain ) &&
                   ValidateViewModel( package.ChainViewModel );
        }

        private static bool ValidateChain( Chain chain )
        {
            if ( chain == null )
                Debug.LogError( "Could not load chain." );

            return chain != null;
        }

        private static bool ValidateViewModel( ChainViewModel viewModel )
        {
            if ( viewModel == null )
                Debug.LogError( "Could not load chain view model." );

            return viewModel != null;
        }

        #endregion

        #region Import

        [UsedImplicitly]
        public void HandleImportClicked()
        {
            if(IsBusy) return;

            var res = FileDialog.ShowOpenDialog("Import Haxxis Package", new[] { hpFilter });

            if(res.PickSuccess)
                ImportFile(res.FileLocation);
            else
                Debug.Log("User cancelled import");
        }

        public void ImportFile( string fullPath )
        {
            IsBusy = true;

            JobManager.Instance.StartJob( ImportFileJob( fullPath ), ImportFileCompleted, ImportFileCancelled, "Import", maxExecutionsPerFrame: 1 );
        }

        private IEnumerator ImportFileJob( string fullPath )
        {
            yield return null;
            yield return null;

            if (!ValidatePackagePath(fullPath))
                yield break;


            var loadedPackage = HaxxisPackage.LoadJson( fullPath );

            if ( !ValidatePackage( loadedPackage ) )
                Debug.LogWarning( "Package did not validate. Attempting to import each component individially."  );


            yield return null;

            if ( loadedPackage.Chain != null )
            {
                loadedPackage.Chain.InitializeSchema();

                Chain.RootGroup.AddGroup( loadedPackage.Chain.RootGroup );

                var foundGroupView =
                    GroupViewsEnumerable.FirstOrDefault( view => view.Group == loadedPackage.Chain.RootGroup );

                foundGroupView.ViewModel = loadedPackage.ChainViewModel.RootGroupViewModel;

                foundGroupView.LoadedPackagePath = fullPath;

                RootGroupView.EnsureWorkspaceIsVisible();
            }

            if ( loadedPackage.Choreography != null && loadedPackage.Choreography.Timeline != null)
                TimelineViewBehaviour.Instance.Timeline.ImportTimeline( loadedPackage.Choreography.Timeline );


            TargetsDirty = true;

        }

        private void ImportFileCompleted( uint jobId )
        {
            IsBusy = false;

            //Chain.Unload();
        }

        private void ImportFileCancelled(uint jobId, bool timedOut, Exception exception)
        {
            ErrorOccurred();    // Must raise this event before setting IsBusy

            IsBusy = false;

            //Chain.Unload();

            if (exception != null)
                Debug.Log("Import had an EXCEPTION");
        }

        #endregion

        #region New

        [UsedImplicitly]
        private void HandleNewClicked()
        {
            if(IsBusy) return;

            IsBusy = true;

            var hp = new HaxxisPackage( Chain, ViewModel,
                    TimelineViewBehaviour.Instance.Timeline.IsEmpty ?
                    null : 
                    new ChoreographyPackage( TimelineViewBehaviour.Instance.Timeline ) );

            var hasChanged = HaxxisPackage.IsChanged( hp, LoadedPackagePath );

            if ( hasChanged )
            {
                PendingChangesDialog.Show( LoadNewChain, () => IsBusy = false );
            }
            else
            {
                LoadNewChain();
            }
        }

        public void LoadNewChain()
        {
            Chain = new Chain();

            ViewModel = null;

            LoadedPackagePath = "";

            TimelineViewBehaviour.Instance.Timeline = new Timeline();

            UndoLord.Instance.DropStack();
            ChainNodeView.NextJsonId = 0;

            SatelliteUpdateLord.Clear();
            GraphicRaycasterVA.Instance.ForceReset();

            IsDirty = false;

            IsBusy = false;
        }

        #endregion

        #region Propagate
        public static void PropagateGroup(PackageRequest request)
        {
            Instance.IsBusy = true;

            // Note that we are setting 'max executions per frame' to 1 here, so that we can see the 'busy' icon right away
            JobManager.Instance.StartJob(PropagateGroupJob(request), PropagateGroupCompleted, PropagateGroupCancelled, "Propagate", maxExecutionsPerFrame: 1);
        }

        private static void PropagateGroupCancelled(uint id, bool timedOut, Exception exception)
        {
            ErrorOccurred();
            Instance.IsBusy = false;

            if(exception == null)
                Debug.Log("Propagation was CANCELLED");
            else
                Debug.Log("Propagation had an EXCEPTION");
        }

        private static void PropagateGroupCompleted(uint id)
        {
            Instance.IsBusy = false;

            RealtimeLogger.Log("Finished group propagation job.");

        }

        private static IEnumerator PropagateGroupJob(PackageRequest request)
        {
            yield return null;

            RealtimeLogger.Log("Starting group propagation job.");

            yield return null;

            if(!ValidatePackagePath(request.Path))
                yield break;
            
            uint jobID = JobManager.Instance.StartJobAndPause(Instance.SaveFileJob(request), jobName: "Propigate-SaveGroup", maxExecutionsPerFrame: 1, startImmediately: true);
            if(JobManager.Instance.IsJobRegistered(jobID))
                yield return null; // To pause this job

            PackageRequest originatingPackage = new PackageRequest(Instance.LoadedPackagePath, new HaxxisPackage(Instance.Chain, Instance.ViewModel,
                    TimelineViewBehaviour.Instance.Timeline.IsEmpty ?
                    null :
                    new ChoreographyPackage(TimelineViewBehaviour.Instance.Timeline)), null);

            Uri propigatingUri = new Uri(request.Path);
            var dir = new DirectoryInfo(HaxxisPackage.RootPath);
            var jsonFiles = dir.GetFiles("*.json", SearchOption.AllDirectories);
            var rootGroupPath = Instance.LoadedPackagePath;

            #region Temporary variables to reduce garbage production
            Uri relativeUri;
            StreamReader textStream;
            string line;
            ChainGroupView cgView;
            #endregion

            var filesToPatch = new List<string>();

            // Ensure we're only touching files that actually reference the propagating group
            foreach(var file in jsonFiles)
            {
                // Don't propagate to yourself
                if(file.ToString().Replace('\\', '/').Equals(request.Path)) continue;
                // Don't propagate to currently loaded package; that's what "reload" is for
                if(file.ToString().Replace('\\', '/').Equals(rootGroupPath)) continue;

                relativeUri = new Uri(file.ToString()).MakeRelativeUri(propigatingUri);

                using(textStream = file.OpenText())
                {
                    while(!textStream.EndOfStream)
                    {
                        line = textStream.ReadLine();
                        if(line.StartsWith("\"RelativePath\": ", StringComparison.InvariantCultureIgnoreCase) &&
                           line.Contains(relativeUri.ToString()))
                        {
                            filesToPatch.Add(file.ToString());
                            break;
                        }
                    }
                }
            }

            if(filesToPatch.Count == 0)
            {
                // There are no other files that use the group; stop here.
                yield break;
            }

            var groupCount = 0;
            HashSet<ChainGroup> groups;

            foreach(var file in filesToPatch)
            {
                groupCount = 0;
                
                jobID = JobManager.Instance.StartJobAndPause(Instance.LoadFileJob(new PackageRequest(file, null)), jobName: "Propigate-LoadNext", maxExecutionsPerFrame: 1, startImmediately: true);
                if(JobManager.Instance.IsJobRegistered(jobID))
                    yield return null; // To pause this job

                groups = new HashSet<ChainGroup>(Instance.Chain.RootGroup.RecursiveGroupsEnumerable);

                foreach(var group in groups)
                {
                    if(group == null) continue;

                    cgView = Instance.GetGroupViewForGroup(group);

                    if(cgView.LoadedPackagePath.Equals(request.Path))
                    {
                        jobID = JobManager.Instance.StartJobAndPause(Instance.LoadFileJob(new PackageRequest(request.Path, cgView)), jobName: "Propigate-Replace", maxExecutionsPerFrame: 1, startImmediately: true);
                        if(JobManager.Instance.IsJobRegistered(jobID))
                            yield return null; // To pause this job
                        groupCount++;
                    }
                }

                Debug.Log("Replaced " + groupCount + " group" + (groupCount == 1 ? "" : "s") + " in file " + file);

                // Save package as a whole
                jobID = JobManager.Instance.StartJobAndPause(Instance.SaveFileJob(new PackageRequest(file, null)), jobName: "Propigate-Save", maxExecutionsPerFrame: 1, startImmediately: true);
                if(JobManager.Instance.IsJobRegistered(jobID))
                    yield return null; // To pause this job
            }

            // Now need to return to the package the request originated from.
            jobID = JobManager.Instance.StartJobAndPause(Instance.LoadFileJob(originatingPackage), jobName: "Propigate-Reload", maxExecutionsPerFrame: 1, startImmediately: true);
            if(JobManager.Instance.IsJobRegistered(jobID))
                yield return null; // To pause this job
        }

        #endregion

        #endregion

        private string UseLocalPackageStorePrefsKey { get { return "UseLocalPackageStore"; } }
        public bool UseLocalPackageStore
        {
            get { return PlayerPrefs.GetInt( UseLocalPackageStorePrefsKey, 1 ) == 1; }
            private set
            {
                //PlayerPrefs.SetInt( UseLocalPackageStorePrefsKey, value ? 1 : 0 );
            }
        }


        #region Other UI Handlers 


        [UsedImplicitly]
        public void HandleAddGroupClicked()
        {
            AddGroupClicked();

            // Hide must come second...
            NewNodePicker.Hide();
        }

        [UsedImplicitly]
        public void HandleRefreshDirtyStateClicked()
        {
            RefreshDirtyState();
        }

        [UsedImplicitly]
        public void HandleMakeChoreoOnlyPressed()
        {
            if ( ( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) ) &&
                 ( Input.GetKey( KeyCode.LeftControl ) || Input.GetKey( KeyCode.RightControl ) ) )
            {
                Chain = new Chain();
            }
            else
            {
                Debug.Log( "Press Control and Shift then click this, to perform said action..." );
            }
        }

        #endregion



        private void UpdateNameText()
        {
            var packageName = string.IsNullOrEmpty( LoadedPackagePath )
                ? "untitled"
                : Path.GetFileName( LoadedPackagePath );

            PackageNameTextComponent.text = packageName;
        }

        public static void UnBusy(uint jobId)
        {
            Instance.IsBusy = false;
        }


        #region Scaling

        public int ScaleLevel { get; private set; }

        private readonly float[] m_ScaleFactors = { .05f, 0.1f, 0.25f, 0.5f, 0.75f, 1.0f, 1.5f };
        private float[] ScaleFactors { get { return m_ScaleFactors; } }

        public void Zoom( int direction, Vector2 mouseScreenPos )
        {
            if ( Dragging ) return;

            Vector3 worldPos = Vector3.zero;

            RectTransformUtility.ScreenPointToWorldPointInRectangle(Canvas.transform as RectTransform, mouseScreenPos, Camera, out worldPos);

            ZoomCenterIndicator.position = worldPos;

            // Find out how much to scale
            var newLevel = Mathf.Clamp(ScaleLevel + direction, 0, ScaleFactors.Length - 1);
            if(newLevel == ScaleLevel) return;
            ScaleLevel = newLevel;
            //Debug.Log("New Scale Level:" + ScaleLevel);
            var scaleFactor = ScaleFactors[ScaleLevel];

            // Actual scaling
            //var scaleVector = Vector3.one * scaleFactor;
            //var invScaleVector = Vector3.one * ( 1.0f / scaleFactor );

            Camera.orthographicSize = 540 * (1f/scaleFactor);
            //GroupAttachmentPoint.transform.localScale = scaleVector;
            //LineManager.RectTransform.localScale = invScaleVector;
            //RootGroupView.BackgroundPanel.RectTransform.localScale = scaleVector;

            // Re-center under mouse
            Vector3 delta = Vector3.zero;

            RectTransformUtility.ScreenPointToWorldPointInRectangle(Canvas.transform as RectTransform, mouseScreenPos, Camera, out delta);

            delta = ZoomCenterIndicator.position - delta;
            Camera.transform.position += delta;
            //RootGroupView.TranslateVisiblePosition( -delta );
            //Camera.transform.position = worldPos + (Vector3.back * 5);

            RootGroupView.EnsureWorkspaceIsVisible();

            Zooming = true;
        }

        #endregion

        #region Save/Load Helpers

        public static HaxxisPackage GetHaxxisPackageForGroupView( ChainGroupView groupView )
        {
            var chain = new Chain( groupView.Group );
            chain.RootGroup.IsRootGroup = false;    // Workaround for chain setting root group's IsRootGroup true... Hmm...

            var viewModel = new ChainViewModel( groupView.ViewModel );
            viewModel.RootGroupViewModel.Position = Vector3.zero;

            var hp = new HaxxisPackage( chain, viewModel, null );
            return hp;
        }

        public List< ChainNodeView.ChainNodeViewModel > GetNodeViewModels( ChainNode node, bool recurse )
        {
            var vms = new List< ChainNodeView.ChainNodeViewModel >();

            var firstView = NodeViewsEnumerable.FirstOrDefault( nv => nv.ChainNode == node );
            if ( firstView == null )
            {
                Debug.LogError( "Could not find view for node..." );
                return vms;
            }

            vms.Add( firstView.ViewModel );

            if ( !recurse )
                return vms;

            vms.AddRange( GetDescendentNodeViews( firstView ).Select( v => v.ViewModel ) );

            return vms;
        }

        public void SetNodeViewModels( List< ChainNodeView.ChainNodeViewModel > viewModels )
        {
            var views = NodeViewsEnumerable.ToList();

            foreach ( var vm in viewModels )
            {
                var foundView = views.FirstOrDefault( v => v.ChainNode == vm.ChainNode );

                if ( foundView == null )
                    continue;

                foundView.ViewModel = vm;
            }
        }

        private void OnDrawGizmos()
        {
            var rect = Instance.CamRect;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x + rect.width, rect.y));
            Gizmos.DrawLine(new Vector3(rect.x + rect.width, rect.y), new Vector3(rect.x + rect.width, rect.y + rect.height));
            Gizmos.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x, rect.y + rect.height));
            Gizmos.DrawLine(new Vector3(rect.x, rect.y + rect.height), new Vector3(rect.x + rect.width, rect.y + rect.height));
            Gizmos.color = Color.white;
        }

        #endregion


    }
}
