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
using Choreography.Views;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Scripts.Utility.Misc;
using Ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility;
using Utility.Undo;

namespace ChainViews
{
    public class ChainGroupView : MonoBehaviour, IBoundsChanger
    { 

        public event Action BoundsChanged = delegate { };

        public RectTransform BoundsRectTransform { get { return BackgroundPanel.RectTransform; } }

        public bool IsVisible
        {
            get
            {
                var myRect = BoundsRectTransform.rect;
                myRect.x += BoundsRectTransform.position.x;
                myRect.y += BoundsRectTransform.position.y;

                //Debug.DrawLine(new Vector3(myRect.x, myRect.y), new Vector3(myRect.x + myRect.width, myRect.y), Color.green, 5f);
                //Debug.DrawLine(new Vector3(myRect.x + myRect.width, myRect.y), new Vector3(myRect.x + myRect.width, myRect.y + myRect.height), Color.green, 5f);
                //Debug.DrawLine(new Vector3(myRect.x, myRect.y), new Vector3(myRect.x, myRect.y + myRect.height), Color.green, 5f);
                //Debug.DrawLine(new Vector3(myRect.x, myRect.y + myRect.height), new Vector3(myRect.x + myRect.width, myRect.y + myRect.height), Color.green, 5f);
                return myRect.Overlaps(ChainView.Instance.CamRect);
            }
        }

        //public bool IsVisible
        //{
        //    get { return ChildAttachmentPoint.gameObject.activeSelf; }

        //    private set
        //    {
        //        if (ChildAttachmentPoint.gameObject.activeSelf != value )
        //        {
        //            ChainView.Instance.TargetsDirty = true;
        //            ChildAttachmentPoint.gameObject.SetActive( value );
        //        }
        //    }
        //}

        #region Lazy Init Component Properties

        private RectTransform m_RectTransform;
        public RectTransform RectTransform
        {
            get { return m_RectTransform ?? ( m_RectTransform = GetComponent<RectTransform>() ); }
        }

        #endregion

        #region Inspectable Properties


        [Header("Component References")]

        [SerializeField]
        private Image m_NewNodePositionIndicator = null;
        private Image NewNodePositionIndicator { get { return m_NewNodePositionIndicator; } }

        [SerializeField]
        private RectTransform m_ChildAttachmentPoint = null;
        public RectTransform ChildAttachmentPoint { get { return m_ChildAttachmentPoint; } }

        [SerializeField]
        private RectTransform m_GroupAttachmentPoint = null;
        private RectTransform GroupAttachmentPoint { get { return m_GroupAttachmentPoint; } }

        [SerializeField]
        private RectTransform m_NodeAttachmentPoint = null;
        private RectTransform NodeAttachmentPoint { get { return m_NodeAttachmentPoint; } }

        [SerializeField]
        private ChainGroupViewBackgroundPanelBehaviour m_BackgroundPanel = null;
        public ChainGroupViewBackgroundPanelBehaviour BackgroundPanel { get { return m_BackgroundPanel; } }

        [SerializeField]
        private Button m_DeleteButton = null;
        private Button DeleteButton { get { return m_DeleteButton; } }

        [SerializeField]
        private Text m_FilenameTextComponent = null;
        private Text FilenameTextComponent { get { return m_FilenameTextComponent; } }

        [SerializeField]
        private Graphic m_DirtyIndicatorComponent = null;
        private Graphic DirtyIndicatorComponent { get { return m_DirtyIndicatorComponent; } }

        [SerializeField]
        private RectTransform m_TitleBarComponent = null;
        private RectTransform TitleBarComponent { get { return m_TitleBarComponent; } }

        [SerializeField]
        private InputField m_CommentInputField = null;
        private InputField CommentInputField { get { return m_CommentInputField; } }


        [Header( "Prefab References" )]

        [SerializeField]
        private GameObject m_NodeViewPrefab = null;
        private GameObject NodeViewPrefab { get { return m_NodeViewPrefab; } }


        [Header( "Configuration" )]

        [SerializeField]
        private float m_MaxMouseDistanceToAcceptClick = 0.0f;
        private float MaxMouseDistanceToAcceptClick { get { return m_MaxMouseDistanceToAcceptClick; } }

        [SerializeField]
        private float m_BackgroundPadding = 20;
        private float BackgroundPadding { get { return m_BackgroundPadding; } }

        [SerializeField]
        private Color m_DefaultBackgroundColor = Color.cyan;
        private Color DefaultBackgroundColor { get { return m_DefaultBackgroundColor; } }

        [SerializeField]
        private Color m_OffscreenBackgroundColor = Color.red;
        private Color OffscreenBackgroundColor { get { return m_OffscreenBackgroundColor; } }


        #endregion


        //private Color BackgroundColor { set { ImageComponent.color = value; } }

        // Should this be separate from (but parallel to) the model's IsRootGroup?
        private bool IsRootGroupView { get { return Group != null && Group.IsRootGroup; } }



        private readonly List<ChainGroupView> m_GroupViews = new List<ChainGroupView>();
        private List<ChainGroupView> GroupViews
        {
            get { return m_GroupViews; }
        }

        public IEnumerable<ChainGroupView> RecursiveGroupViewsEnumerable
        {
            get
            {
                yield return this;

                foreach ( var groupView in GroupViews )
                {
                    var iterator = groupView.RecursiveGroupViewsEnumerable.GetEnumerator();
                    while ( iterator.MoveNext() )
                        yield return iterator.Current;
                }
            }
        }



        private readonly List<ChainNodeView> m_NodeViews = new List<ChainNodeView>();
        private List<ChainNodeView> NodeViews
        {
            get { return m_NodeViews; }
        }

        public IEnumerable<ChainNodeView> NodeViewsEnumerable
        {
            get { return NodeViews.AsReadOnly(); }
        }

        public IEnumerable< ChainNodeView > RecursiveNodeViewsEnumerable
        {
            get { return RecursiveGroupViewsEnumerable.SelectMany( gv => gv.NodeViews ); }
        }




        #region Model

        private ChainGroup m_Group;
        public ChainGroup Group
        {
            get { return m_Group; }
            set
            {
                if ( m_Group != null )
                {
                    m_Group.HasErrorChanged -= HandleGroupHasErrorChanged;
                    m_Group.GroupAdded -= HandleGroupGroupAdded;
                    m_Group.GroupRemoved -= HandleGroupGroupRemoved;
                    m_Group.NodeAdded -= HandleGroupNodeAdded;
                    m_Group.NodeRemoved -= HandleGroupNodeRemoved;
                    m_Group.CommentChanged -= HandleChainGroupCommentChanged;

                    DestroyChildViews();
                }

                //RealtimeLogger.Log( "Setting group in view" );

                m_Group = value;

                if ( m_Group != null )
                {
                    m_Group.HasErrorChanged += HandleGroupHasErrorChanged;
                    m_Group.GroupAdded += HandleGroupGroupAdded;
                    m_Group.GroupRemoved += HandleGroupGroupRemoved;
                    m_Group.NodeAdded += HandleGroupNodeAdded;
                    m_Group.NodeRemoved += HandleGroupNodeRemoved;
                    m_Group.CommentChanged += HandleChainGroupCommentChanged;

                    GenerateChildViews();

                    if(m_Group.IsRootGroup)
                    {
                        TitleBarComponent.gameObject.SetActive(false);
                        Destroy(transform.FindChild("BackgroundPanel").GetComponent<Draggable>());
                    }

                    CommentInputField.text = Group.Comment;
                }
            }
        }

        #endregion


        public void Destroy()
        {
            Group = null;

            GraphicRaycasterVA.Instance.RemoveCanvas(GetComponent<Canvas>());
            DestroyImmediate( gameObject );
        }


        private void HandleGroupHasErrorChanged( bool hasError )
        {
            // TODO: Visually indicate error?
        }


        #region Child Views


        private void GenerateChildViews()
        {
            foreach ( var group in Group.Groups )
            {
                AddChainGroupView( group );
            }

            foreach ( var node in Group.Nodes )
            {
                AddChainNodeView( node );
            }
        }

        private void DestroyChildViews()
        {
            foreach ( var nv in NodeViews )
                nv.Destroy();

            foreach ( var gv in GroupViews )
                gv.Destroy();
        }


        #region Child Group Lifecycle


        private void HandleGroupGroupAdded( ChainGroup group, bool isTransfer )
        {
            // If it's a transfer, we'll be getting the existing view sent over soon.
            if ( isTransfer )
                return;

            AddChainGroupView( group );
        }


        private void AddChainGroupView( ChainGroup group )
        {
            var viewGo = Instantiate( ChainView.GroupViewPrefab );
            var view = viewGo.GetComponent<ChainGroupView>();

            view.RectTransform.position = Vector3.zero;

            view.Group = group;

            AddChainGroupView( view );

            if(!group.SuppressUndo)
            {
                UndoLord.Instance.Push(new GroupAdd(view, this, NewNodePosition));
            }
        }

        private void AddChainGroupView( ChainGroupView view, bool isTransfer = false )
        {
            GroupViews.Add( view );
            
            view.transform.SetParent( GroupAttachmentPoint, isTransfer );

            if ( !isTransfer )
            {
                view.transform.localPosition = Vector2.zero;
                view.SetVisiblePosition( NewNodePosition, local: false );
            }

            view.BoundsChanged += HandleChildViewBoundsChanged;
            view.TransferRequested += HandleChildGroupViewTransferRequested;

            BoundsDirty = true;

            UpdateDeleteButtonState();
        }

        private void HandleGroupGroupRemoved( ChainGroup group, bool isTransfer )
        {
            if ( isTransfer )
                return;

            RemoveChainGroupView( group, true );
        }

        private void RemoveChainGroupView( ChainGroup group, bool destroy )
        {
            var view = GroupViews.FirstOrDefault( nv => nv.Group == group );
            if ( view == null )
                return;

            RemoveChainGroupView( view, destroy );
        }

        private void RemoveChainGroupView( ChainGroupView view, bool destroy )
        {
            view.BoundsChanged -= HandleChildViewBoundsChanged;
            view.TransferRequested -= HandleChildGroupViewTransferRequested;

            GroupViews.Remove( view );



            UpdateWorkspaceSize();

            EnsureWorkspaceIsVisible();

            if ( destroy )
                Destroy( view.gameObject );

            BoundsDirty = true;

            UpdateDeleteButtonState();
        }

        #endregion




        #region Child Node Lifecycle

        private void HandleGroupNodeAdded( ChainNode node, bool isTransfer )
        {
            if ( isTransfer )
                return;

            AddChainNodeView( node );

            if(!node.SuppressUndos)
            {
                UndoLord.Instance.Push(new NodeAdd(node, this, NewNodePosition));
            }
            
        }

        private void HandleGroupNodeRemoved( ChainNode node, bool isTransfer )
        {
            if ( isTransfer )
                return;

            RemoveChainNodeView( node, true );
        }

        private void AddChainNodeView( ChainNode chainNode )
        {
            var viewGo = Instantiate( NodeViewPrefab, NodeAttachmentPoint );
            var view = viewGo.GetComponent<ChainNodeView>();
            
            view.ChainNode = chainNode;

            //view.Expanded = true;

            AddChainNodeView( view );

        }

        private void AddChainNodeView( ChainNodeView view, bool isTransfer = false )
        {
            if(isTransfer)
                view.transform.SetParent( NodeAttachmentPoint, true );

            NodeViews.Add( view );

            if ( !isTransfer )
                view.transform.position = NewNodePosition;


            view.BoundsChanged += HandleChildViewBoundsChanged;
            view.TransferRequested += HandleChildNodeViewTransferRequested;
            view.DuplicationRequested += HandleNodeViewDuplicationRequested;

            BoundsDirty = true;

            UpdateDeleteButtonState();
        }

        

        private void RemoveChainNodeView( ChainNode node, bool destroy )
        {
            if ( ChainView.SelectedChainNode == node )
                ChainView.SelectedChainNode = null;

            if ( node.Router.SelectionStatesEnumerable.Contains( ChainView.SelectedSelectionState ) )
                ChainView.SelectedSelectionState = null;

            var view = NodeViews.FirstOrDefault( nv => nv.ChainNode == node );
            if ( view == null )
                return;

            RemoveChainNodeView( view, destroy );
        }

        private void RemoveChainNodeView( ChainNodeView view, bool destroy )
        {
            view.BoundsChanged -= HandleChildViewBoundsChanged;
            view.TransferRequested -= HandleChildNodeViewTransferRequested;
            view.DuplicationRequested -= HandleNodeViewDuplicationRequested;

            NodeViews.Remove( view );


            if ( destroy )
                Destroy( view.gameObject );


            BoundsDirty = true;

            UpdateDeleteButtonState();
        }

        #endregion


        #region Node Duplication

        private class NodeDupeSerializationWrapper
        {
            public ChainNode Node { get; set; }

            public List<ChainNodeView.ChainNodeViewModel> ViewModels { get; set; }

            [JsonConstructor, UsedImplicitly]
            public NodeDupeSerializationWrapper()
            {
            }

            public NodeDupeSerializationWrapper( ChainNode node, List<ChainNodeView.ChainNodeViewModel> viewModels )
            {
                Node = node;
                ViewModels = viewModels;
            }
        }

        private void HandleNodeViewDuplicationRequested( ChainNodeView nodeView, bool recurse )
        {
            var settings = HaxxisPackage.GetSerializationSettings( TypeNameHandling.All );

            var originalWrapper = new NodeDupeSerializationWrapper( nodeView.ChainNode, ChainView.Instance.GetNodeViewModels( nodeView.ChainNode, recurse ) );

            var wrapperJson = JsonConvert.SerializeObject( originalWrapper, Formatting.Indented, settings );

            var duplicateWrapper = JsonConvert.DeserializeObject< NodeDupeSerializationWrapper >( wrapperJson, settings );

            // Should this go in Group.AddNode?
            duplicateWrapper.Node.InitializeSchema();

            if(!recurse)
            {
                duplicateWrapper.Node.Router.UntargetAllTargets();
            }

            Group.AddNode( duplicateWrapper.Node, recurse: recurse, isTransfer: false );

            foreach ( var vm in duplicateWrapper.ViewModels )
            {
                vm.Position += new Vector3( 20.0f, -20.0f, 0.0f );
            }

            ChainView.Instance.SetNodeViewModels( duplicateWrapper.ViewModels );

            BoundsDirty = true;
        }

        #endregion


        public ChainNodeView FindChainNodeView( ChainNode node, bool recurse )
        {
            var foundNode = NodeViews.FirstOrDefault( view => view.ChainNode == node );
            if ( foundNode != null )
                return foundNode;

            if ( !recurse )
                return null;

            foreach ( var view in GroupViews )
            {
                foundNode = view.FindChainNodeView( node, true );

                if ( foundNode != null )
                    return foundNode;
            }

            return null;
        }


        #region Bounds Management

        private bool BoundsDirty { get; set; }

        private void HandleChildViewBoundsChanged()
        {
            //if ( ChainView.GroupTransferModeActive ) return;

            BoundsDirty = true;
        }

        public void RefreshLayout()
        {
            BoundsDirty = true;
        }

        [UsedImplicitly]
        private void LateUpdate()
        {
            if ( ChainView.Instance.Zooming || ChainView.Instance.Dragging ) return;

            if ( IsRootGroupView )
                UpdateBounds();
        }

        private void UpdateBounds()
        {
            //foreach ( var n in NodeViews )
            //    n.UpdateBounds();

            foreach ( var g in GroupViews )
                g.UpdateBounds();

            if ( !BoundsDirty )
                return;

            UpdateWorkspaceSize();

            EnsureWorkspaceIsVisible();

            BoundsDirty = false;
        }

        #endregion


        [UsedImplicitly]
        public void HandleDeleteButtonPressed()
        {
            if ( Group == null )
            {
                Destroy( gameObject );
                return;
            }

            UndoLord.Instance.Push(Group.RequestRemoval());
        }

        private void UpdateDeleteButtonState()
        {
            DeleteButton.interactable = !IsRootGroupView; //!( GroupViews.Any() || NodeViews.Any() || IsRootGroupView );
        }

        #endregion


        [UsedImplicitly]
        public void Start()
        {
            BackgroundPanel.Draggable.MouseUp += () =>
            {
                if(!BackgroundPanel.Draggable.IsDragging) return;
                if(ChainView.GroupTransferModeActive)
                {
                    // TODO: Handle adding group transfer to undo stack
                }
                else
                {
                    UndoLord.Instance.Push(new ChainGroupMoved(BackgroundPanel.Draggable,
                        BackgroundPanel.transform.localPosition - BackgroundPanel.Draggable.OriginalPosition));
                }
            };
            BackgroundPanel.Draggable.DragEnded += HandleDragEnded;
            BackgroundPanel.Draggable.DragMoved += HandleDragMoved;   // Forwarding delegate, so don't use method group directly!

            BackgroundPanel.PreviewGroupViewDrop += HandlePreviewGroupViewDrop;
            BackgroundPanel.PreviewNodeViewDrop += HandlePreviewNodeViewDrop;
            BackgroundPanel.GroupViewDropped += HandleGroupViewDropped;
            BackgroundPanel.NodeViewDropped += HandleNodeViewDropped;

            BackgroundPanel.Clicked += HandleBackgroundClicked;

            BackgroundPanel.Color = DefaultBackgroundColor;

            //InvokeRepeating( "RefreshDirtyState", 0.1f, 3.0f );
            
            //GraphicRaycasterVA.Instance.AddCanvas(GetComponent<Canvas>());
        }

        private void HandleDragMoved()
        {
            if ( ChainView.Instance.Zooming )
                return;

            RefreshWorkspaceVisibilityIndicator();

            if ( !ChainView.GroupTransferModeActive )
                BoundsChanged();
        }

        private void HandleDragEnded()
        {
            if ( ChainView.Instance.Zooming || ChainView.Instance.Dragging )
                return;

            EnsureWorkspaceIsVisible();
            BoundsDirty = true;
        }

        #region Group Transferring Between Groups

        #region Group Transfer Receiver

        private void HandlePreviewGroupViewDrop( ChainGroupView groupView )
        {
            if ( !ChainView.GroupTransferModeActive )
                return;

            if ( groupView == null )
            {
                IndicateDropAcceptance( false );
                return;
            }

            if ( GroupViews.Contains( groupView ) )
            {
                IndicateDropAcceptance( false );
                return;
            }

            IndicateDropAcceptance( true );
        }

        private void HandleGroupViewDropped( ChainGroupView droppedGroupView )
        {
            if ( !ChainView.GroupTransferModeActive )
                return;

            Debug.Log( "group view dropped" );

            if ( GroupViews.Contains( droppedGroupView ) )
                return;

            droppedGroupView.RequestTransfer( this );
        }

        #endregion

        #region Transfered Group

        public event Action< ChainGroupView, ChainGroupView > TransferRequested = delegate { };

        private void RequestTransfer( ChainGroupView destinationView )
        {
            // Model
            Group.RequestTransfer( destinationView.Group );

            // View
            TransferRequested( this, destinationView );
        }

        #endregion

        #region Group Transfer Sender

        private void HandleChildGroupViewTransferRequested( ChainGroupView movedView, ChainGroupView destinationView)
        {
            RemoveChainGroupView( movedView, false );

            destinationView.AddChainGroupView( movedView, isTransfer: true );
        }

        #endregion

        #endregion


        #region Node Transferring Between Groups

        #region Node Transfer Receiver

        private void HandlePreviewNodeViewDrop( ChainNodeView nodeView )
        {
            if ( !ChainView.GroupTransferModeActive )
                return;

            if ( nodeView == null )
            {
                IndicateDropAcceptance( false );
                return;
            }

            if ( NodeViews.Contains( nodeView ) )
            {
                IndicateDropAcceptance( false );
                return;
            }

            IndicateDropAcceptance( true );
        }

        private void HandleNodeViewDropped( ChainNodeView droppedNodeView )
        {
            if ( !ChainView.GroupTransferModeActive )
                return;

            Debug.Log( "node view dropped" );

            if ( NodeViews.Contains( droppedNodeView ) )
                return;

            droppedNodeView.RequestTransfer( this );
        }

        #endregion

        #region Node Transfer Sender

        private void HandleChildNodeViewTransferRequested( ChainNodeView movedView, ChainGroupView destinationView )
        {
            RemoveChainNodeView( movedView, destroy: false );

            destinationView.AddChainNodeView( movedView, isTransfer: true );
        }

        #endregion

        #endregion

        private void IndicateDropAcceptance( bool willAccept )
        {
            BackgroundPanel.Color = willAccept ? new Color( 1.0f, 1.0f, 1.0f, 0.5f ) : DefaultBackgroundColor;
        }

        


        #region Staying on Screen

        public void EnsureWorkspaceIsVisible()
        {
            if ( ChainView.Instance.Zooming || ChainView.Instance.Dragging ) return;

            //IsVisible = CheckWorkspaceIsVisible();
            
            if (IsRootGroupView)
                if ( !IsVisible )
                {
                    PositionWorkspaceAtZero();

                    //IsVisible = true;
                }
                else
                {
                    BackgroundPanel.Color = DefaultBackgroundColor;
                }

            //for(int i = 0; i < GroupViews.Count; ++i)
            //{
            //    GroupViews[i].EnsureWorkspaceIsVisible();
            //}

        }

        private bool CheckWorkspaceIsVisible()
        {
            var screenRect = new Rect( 0, 0, Screen.width, Screen.height - TimelineViewBehaviour.Instance.CurrentHeight );

            var minBuffer = Mathf.Max( BackgroundPanel.Draggable.SnapGridSize, 20.0f );

            var panelRect = RectUtility.RectTransformToScreenSpace( BackgroundPanel.RectTransform );
            panelRect = RectUtility.CreateExpandedRect( panelRect, -minBuffer );

            return panelRect.Overlaps( screenRect, true );
        }

        private void PositionWorkspaceAtZero()
        {
            var offset = -BackgroundPanel.RectTransform.localPosition;

            BackgroundPanel.RectTransform.localPosition = Vector3.zero;

            ChildAttachmentPoint.Translate( offset.PiecewiseMultiply( RectTransform.lossyScale ) );// -= new Vector3( -BackgroundPadding, BackgroundPadding, 0.0f );

            ChainView.Instance.Camera.transform.position = new Vector3(ChainView.Instance.Camera.aspect, -1f) * ChainView.Instance.Camera.orthographicSize + new Vector3(0f, 1080f, -5f);

            BackgroundPanel.Color = DefaultBackgroundColor;
        }

        private void RefreshWorkspaceVisibilityIndicator()
        {
            if ( ChainView.Instance.Zooming || ChainView.Instance.Dragging ) return;

            //IsVisible = CheckWorkspaceIsVisible();

            BackgroundPanel.Color = IsVisible ? DefaultBackgroundColor : OffscreenBackgroundColor;
        }

        #endregion


        
        #region Node/Group Picking

        public void HandleBackgroundClicked( PointerEventData eventData )
        {
            var pressToReleasePositionDelta = ( eventData.pressPosition - eventData.position ).magnitude;
            if ( pressToReleasePositionDelta > MaxMouseDistanceToAcceptClick )
                return;

            if ( eventData.button == PointerEventData.InputButton.Right && ChainView.Instance.Visible )
            {
                Vector3 indicatorPos = Vector3.zero;
                RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform, eventData.pressPosition, eventData.pressEventCamera, out indicatorPos);
                NewNodePosition = indicatorPos;

                BindToNodePicker();

                ShowNewNodePositionIndicator();

                ChainView.NewNodePicker.Show();
            }
        }

        

        private Vector3 NewNodePosition
        {
            get { return NewNodePositionIndicator.transform.position; }
            set { NewNodePositionIndicator.transform.position = value; }
        }

        private void HandleNewNodePickerTypeSelected( Type type )
        {
            UnbindFromNodePicker();

            if ( type == null )
                return;

            HideNewNodePositionIndicator();

            Group.CreateAndAddNodeOfType( type );
        }


        private void HandleNewNodePickerHidden()
        {
            UnbindFromNodePicker();

            NewNodePositionIndicator.gameObject.SetActive( false );
        }


        private void ShowNewNodePositionIndicator()
        {
            NewNodePositionIndicator.gameObject.SetActive( true );
        }

        private void HideNewNodePositionIndicator()
        {
            NewNodePositionIndicator.gameObject.SetActive( false );
        }

        private bool IsBoundToNodePicker { get; set; }

        private void BindToNodePicker()
        {
            if ( IsBoundToNodePicker )
                return;

            IsBoundToNodePicker = true;

            ChainView.NewNodePicker.TypeSelected += HandleNewNodePickerTypeSelected;
            ChainView.NewNodePicker.Hidden += HandleNewNodePickerHidden;

            ChainView.AddGroupClicked += HandleAddGroupClicked;
        }

        private void UnbindFromNodePicker()
        {
            ChainView.NewNodePicker.TypeSelected -= HandleNewNodePickerTypeSelected;
            ChainView.NewNodePicker.Hidden -= HandleNewNodePickerHidden;

            ChainView.AddGroupClicked -= HandleAddGroupClicked;

            IsBoundToNodePicker = false;
        }

        private void HandleAddGroupClicked()
        {
            ChainView.NewNodePicker.Hide();
            
            UnbindFromNodePicker();

            Group.CreateGroup();
        }

        #endregion


        #region Sizing

        private void UpdateWorkspaceSize()
        {
            var padding = new Padding { Top = m_BackgroundPadding * 2, Bottom = BackgroundPadding, Left = BackgroundPadding, Right = BackgroundPadding };
            BackgroundPanel.RectTransform.ExpandToContain( ChildTransformsEnumerable, padding, new Vector3( 300, 64 ) );

            EnsureWorkspaceIsVisible();

            BoundsChanged();
        }

        private IEnumerable< RectTransform > ChildTransformsEnumerable
        {
            get
            {
                var nodeTransforms = NodeViews.Select( nv => nv.BoundsRectTransform );
                var groupTransforms = GroupViews.Select( gv => gv.BoundsRectTransform );

                return nodeTransforms.Concat( groupTransforms ).ToList();
            }
        }

        #endregion
        
        public Dictionary<ChainNode, ChainNodeView> GetNodesToViews()
        {
            return (from v in NodeViewsEnumerable
                    select new KeyValuePair<ChainNode, ChainNodeView>(v.ChainNode, v)).ToDictionary(k => k.Key, k => k.Value);
        }
        
        #region ViewModel (Serialization of visual properties)

        public ChainGroupViewModel ViewModel
        {
            get
            {
                return new ChainGroupViewModel( this );
            }
            set
            {
                if ( value.Group != Group )
                    throw new InvalidOperationException( "Cannot apply view model from different group's view." );

                SetVisiblePosition( value.Position, local: !IsRootGroupView );

                foreach ( var nodeViewModel in value.NodeViewModels )
                {
                    if ( nodeViewModel.ChainNode == null )
                        continue;

                    var view = NodeViews.FirstOrDefault( nv => nv.ChainNode == nodeViewModel.ChainNode );
                    if ( view == null)
                        continue;

                    view.ViewModel = nodeViewModel;
                }

                foreach ( var groupViewModel in value.GroupViewModels )
                {
                    if ( groupViewModel.Group == null )
                        continue;

                    var view = GroupViews.FirstOrDefault( gv => gv.Group == groupViewModel.Group );
                    if ( view == null )
                        continue;

                    view.ViewModel = groupViewModel;
                }

                LoadedPackagePath = HaxxisPackage.GetFullPath( value.RelativePath );

                UpdateWorkspaceSize();

                if ( IsRootGroupView )
                    PositionWorkspaceAtZero();
            }
        }

        public void SetVisiblePosition( Vector3 position, bool local )
        {
            if ( local )
                ChildAttachmentPoint.localPosition = position;
            else
                ChildAttachmentPoint.position = position;

            BackgroundPanel.RectTransform.position = ChildAttachmentPoint.position + new Vector3( -BackgroundPadding, BackgroundPadding, 0.0f );
        }

        public void TranslateVisiblePosition( Vector3 delta )
        {
            BackgroundPanel.RectTransform.transform.Translate( delta );
            ChildAttachmentPoint.Translate( delta );
        }


        [JsonObject( MemberSerialization.OptIn )]
        public class ChainGroupViewModel
        {
            [JsonProperty]
            public ChainGroup Group { get; set; }
            
            [JsonProperty]
            public Vector3 Position { get; set; }

            [JsonProperty]
            public string RelativePath { get; set; }

            [JsonProperty]
            public List< ChainNodeView.ChainNodeViewModel > NodeViewModels { get; private set; }

            [JsonProperty]
            public List<ChainGroupViewModel> GroupViewModels { get; private set; }
            
            [JsonConstructor, UsedImplicitly]
            private ChainGroupViewModel()
            {
            }

            [Obsolete( "This is only for backwards compatibility loading packages; will be removed!" )]
            public ChainGroupViewModel( ChainGroupView groupView, List< ChainNodeView.ChainNodeViewModel > nodeViewModels )
            {
                Group = groupView.Group;

                NodeViewModels = nodeViewModels;

                GroupViewModels = new List< ChainGroupViewModel >();
            }

            public ChainGroupViewModel( ChainGroupView groupView )
            {
                Group = groupView.Group;

                Position = groupView.IsRootGroupView ? Vector3.zero : groupView.ChildAttachmentPoint.localPosition;


                RelativePath = groupView.RelativePath;


                GroupViewModels = groupView.GroupViews.Select( gv => gv.ViewModel ).ToList();

                NodeViewModels = groupView.NodeViews.Select( nv => nv.ViewModel ).ToList();
            }
        }

        #endregion


        #region Save/Load

        private string m_LoadedPackagePath = string.Empty;
        public string LoadedPackagePath
        {
            get
            {
                return m_LoadedPackagePath;
            }
            set
            {
                //Debug.Log("Setting package path to " + value );

                m_LoadedPackagePath = value.Replace( '\\', '/' );

                UpdateNameText();
            }
        }

        public string RelativePath
        {
            get { return HaxxisPackage.GetRelativePath( LoadedPackagePath ); }
        }

        private void UpdateNameText()
        {
            var packageName = "unnamed";

            if ( !string.IsNullOrEmpty( LoadedPackagePath ) )
                packageName = HaxxisPackage.GetRelativePath( LoadedPackagePath );

            FilenameTextComponent.text = packageName;
        }

        [UsedImplicitly]
        public void RefreshDirtyState()
        {
            if ( ChainView.Instance.IsBusy )
                return;

            if ( TimelineViewBehaviour.Instance == null )
                return;

            if ( TimelineViewBehaviour.Instance.Timeline.IsBusy )
                return;

            if ( HaxxisGlobalSettings.Instance.IsVgsJob == true )
                return;

            if ( HaxxisGlobalSettings.Instance.DisableEditor == true )
                return;

            if (string.IsNullOrEmpty(LoadedPackagePath))
            {
                var hasChildren = GroupViews.Any() || NodeViews.Any();

                DirtyIndicatorComponent.gameObject.SetActive( hasChildren );
            
                return;
            }

            try
            {
                ChainGroup.SerializingGroup = Group;

                var hp = ChainView.GetHaxxisPackageForGroupView( this );

                var hasChanged = HaxxisPackage.IsChanged( hp, LoadedPackagePath );
                
                DirtyIndicatorComponent.gameObject.SetActive( hasChanged );
            }
            finally
            {
                ChainGroup.SerializingGroup = null;
            }
        }

        #endregion


        #region UI Handlers

        [UsedImplicitly]
        public void HandleDivorcePressed()
        {
            LoadedPackagePath = string.Empty;

            UpdateNameText();
        }

        [UsedImplicitly]
        public void HandleReloadPressed()
        {
            var request = new ChainView.PackageRequest( LoadedPackagePath, this);

            ChainView.Instance.LoadPackage( request );
        }

        [UsedImplicitly]
        public void HandleSavePressed()
        {
            var request = new ChainView.PackageRequest( LoadedPackagePath, this);

            ChainView.Instance.SavePackage( request );
        }

        [UsedImplicitly]
        public void HandleSaveAsPressed()
        {
            var request = new ChainView.PackageRequest( LoadedPackagePath, this );

            ChainView.Instance.SavePackageAs( request );
        }

        [UsedImplicitly]
        public void HandlePropagatePressed()
        {
            if(string.IsNullOrEmpty(LoadedPackagePath)) return;

            var request = new ChainView.PackageRequest(LoadedPackagePath, this);

            ChainView.PropagateGroup(request);
        }

        [UsedImplicitly]
        public void HandleCommentTextSubmitted()
        {
            // Not reactive, so...

            if(Group != null)
            {
                UndoLord.Instance.Push(new GroupCommentChange(this, CommentInputField.text));
                Group.Comment = CommentInputField.text;
            }
                
        }

        private void HandleChainGroupCommentChanged(string text)
        {
            CommentInputField.text = text;
        }

        private bool m_ShowChildren = true;
        private bool ShowChildren
        {
            get { return m_ShowChildren; }
            set { m_ShowChildren = value; }
        }

        [UsedImplicitly]
        public void HandleToggleShowNodesClicked()
        {
            ShowChildren = !ShowChildren;

            ChildAttachmentPoint.gameObject.SetActive( ShowChildren );
        }

        #endregion
        

    }
}
