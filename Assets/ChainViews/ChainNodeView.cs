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
using ChainViews.Elements;
using Chains;
using Controllables;
using JetBrains.Annotations;
using Mutation;
using Newtonsoft.Json;
using Ui;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility;
using Utility.Undo;

namespace ChainViews
{
    public partial class ChainNodeView : MonoBehaviour, IControllableMemberGeneratable, IBoundsChanger, IPointerClickHandler
    {
        public event Action BoundsChanged = delegate { };

        public event Action< ChainNodeView, bool > DuplicationRequested = delegate { };

        public RectTransform BoundsRectTransform { get { return RectTransform; } }

        private float knownLargestHeight = 0f;
        private Rect m_rect;
        public Rect rect
        {
            get
            {
                if(RectTransform.rect.height > knownLargestHeight)
                {
                    knownLargestHeight = RectTransform.rect.height;
                    m_rect = new Rect(transform.position.x - 80, transform.position.y - knownLargestHeight - 80,
                        RectTransform.rect.width + 160,
                        knownLargestHeight + 160);
                }
                    
                return m_rect;
            }
        }

        public bool IsVisible
        {
            get
            {
                return ChainView.Instance.CamRect.Overlaps(rect);
            }
        }


        #region Reference Properties

        [Header("Component References")]
        
        [SerializeField]
        private GameObject m_ContentsPanelComponent = null;
        private GameObject ContentsPanelComponent { get { return m_ContentsPanelComponent; } }

        public RectTransform ControllableUiItemRoot { get { return ContentsPanelComponent.GetComponent<RectTransform>(); } }

        [SerializeField]
        private Image m_BackgroundImage = null;
        private Image BackgroundImage { get { return m_BackgroundImage; } }

        [SerializeField]
        private Text m_SelectedIndicationComponent = null;
        private Text SelectedIndicationComponent { get { return m_SelectedIndicationComponent; } }

        [SerializeField]
        private RectTransform m_LineInputTransform = null;
        public RectTransform LineInputTransform { get { return m_LineInputTransform; } }

        [SerializeField]
        private RectTransform m_LineOutputTransform = null;
        public RectTransform LineOutputTransform { get { return m_LineOutputTransform; } }

        [SerializeField]
        private InputField m_CommentInputField = null;
        private InputField CommentInputField { get { return m_CommentInputField; } }

        [SerializeField]
        private Text m_TypeTextComponent = null;
        private Text TypeTextComponent { get { return m_TypeTextComponent; } }
        
        [SerializeField]
        private ErrorBorderBehaviour m_ErrorBorderComponent = null;
        private ErrorBorderBehaviour ErrorBorderComponent { get { return m_ErrorBorderComponent; } }

        [SerializeField]
        private Image m_SimplifiedDisplayPanel = null;
        private Image SimplifiedDisplayPanel { get { return m_SimplifiedDisplayPanel; } }

        [SerializeField]
        private RectTransform m_IndexTextRoot = null;
        private RectTransform IndexTextRoot { get { return m_IndexTextRoot; } }

        [SerializeField]
        private Text m_IndexTextComponent = null;
        private Text IndexTextComponent { get { return m_IndexTextComponent; } }

        [SerializeField]
        private Graphic m_DisableButtonForegroundComponent = null;
        private Graphic DisableButtonForegroundComponent { get { return m_DisableButtonForegroundComponent; } }

        [SerializeField]
        private Graphic m_DisableButtonBackgroundComponent = null;
        private Graphic DisableButtonBackgroundComponent { get { return m_DisableButtonBackgroundComponent; } }

        [SerializeField]
        private Graphic m_DisabledOverlayGraphicComponent = null;
        private Graphic DisabledOverlayGraphicComponent { get { return m_DisabledOverlayGraphicComponent; } }

        [SerializeField]
        private Draggable m_Draggable = null;

        public Draggable Draggable { get { return m_Draggable; } }


        [Header( "Prefab References" )]

        [SerializeField]
        private GameObject m_StateRouterUiPrefab = null;
        public GameObject StateRouterUiPrefab { get { return m_StateRouterUiPrefab; } }

        #endregion


        [Header("Configuration")]

        [SerializeField]
        private Color m_DisableButtonLightColor = Color.white;
        private Color DisableButtonLightColor { get { return m_DisableButtonLightColor; } }

        [SerializeField]
        private Color m_DisableButtonDarkColor = Color.gray;
        private Color DisableButtonDarkColor { get { return m_DisableButtonDarkColor; } }

        [SerializeField]
        private ColorMapDefinition m_ColorMap = new ColorMapDefinition();
        private ColorMapDefinition ColorMap { get { return m_ColorMap; } }



        private RectTransform m_RectTransform;
        public RectTransform RectTransform
        {
            get
            {
                return m_RectTransform ?? ( m_RectTransform = GetComponent<RectTransform>() );
            }
        }



        private readonly List<MonoBehaviour> m_ControllableUiItems = new List<MonoBehaviour>();
        public List< MonoBehaviour > ControllableUiItems
        {
            get
            {
                return m_ControllableUiItems;
            }
        }

        //public List< MonoBehaviour > ControllableUiItems
        //{
        //    get
        //    {
        //        return GetType()
        //            .GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
        //            .Where(p => Attribute.IsDefined( p, typeof(ControllableAttribute))
        //            && typeof(MonoBehaviour).IsAssignableFrom(p.PropertyType))
        //            .Select(p => p.GetValue(this, null)).Cast<MonoBehaviour>().ToList();
        //    }
        //}

        public System.Object Model
        {
            get { return ChainNode; }
        }

        //private bool m_Expanded;
        //public bool Expanded
        //{
        //    get { return m_Expanded; }
        //    set
        //    {
        //        m_Expanded = value;

        //        ContentsPanelComponent.SetActive( value );

        //        LineOutputTransform.gameObject.SetActive( !value );

        //        ChainView.Instance.TargetsDirty = true;

        //        BoundsChanged();
        //    }
        //}

        //public bool Simplified
        //{
        //    set
        //    {
        //        //var size = ContentsPanelComponent.GetComponent<RectTransform>().sizeDelta;
        //        var size = RectTransform.sizeDelta;
        //        SimplifiedDisplayPanel.rectTransform.sizeDelta = size;

        //        ContentsPanelComponent.gameObject.SetActive( value ? false : Expanded );
        //        SimplifiedDisplayPanel.gameObject.SetActive( value );
        //    }
        //}



        public bool Selected
        {
            set
            {
                SelectedIndicationComponent.gameObject.SetActive( value );
            }
        }


        private ChainNode m_ChainNode;
        public ChainNode ChainNode
        {
            get { return m_ChainNode; }
            set
            {
                if ( m_ChainNode == value )
                    return;

                if ( m_ChainNode != null )
                {
                    m_ChainNode.HasErrorChanged -= HandleChainNodeHasErrorChanged;
                    m_ChainNode.TargetsDirty -= HandleChainNodeTargetsDirty;
                    m_ChainNode.DisabledChanged -= HandleChainNodeDisabledChanged;
                    m_ChainNode.CommentChanged -= HandleChainNodeCommentChanged;

                    DestroyControllableViews();
                }

                m_ChainNode = value;

                if ( m_ChainNode != null )
                {
                    QueueGenerateControllableViews();
                    //GenerateControllableViews();
                    
                    gameObject.name = ChainNode.Name;
                    TypeTextComponent.text = ChainNode.Name;
                    CommentInputField.text = ChainNode.Comment;

                    m_ChainNode.HasErrorChanged += HandleChainNodeHasErrorChanged;
                    m_ChainNode.TargetsDirty += HandleChainNodeTargetsDirty;
                    m_ChainNode.DisabledChanged += HandleChainNodeDisabledChanged;
                    m_ChainNode.CommentChanged += HandleChainNodeCommentChanged;

                    if(m_ChainNode.JsonId > NextJsonId)
                    {
                        NextJsonId = m_ChainNode.JsonId + 1;
                    }
                    IndexTextComponent.text = m_ChainNode.JsonId.ToString();

                    IndicateError();

                    IndicateDisabled();
                }
            }
        }

       

        public void Destroy()
        {
            ChainNode = null;

            Destroy( gameObject );
        }

        public ISchemaProvider SchemaProvider { get { return ChainNode; } }

        private void HandleChainNodeHasErrorChanged( bool error )
        {
            IndicateError();
        }

        private void IndicateError()
        {
            try
            {
                ErrorBorderComponent.gameObject.SetActive( ChainNode.HasError && !ChainNode.Disabled );
            }
            catch ( Exception ex )
            {
                Debug.LogError( "What? (" + ex.Message + ")", this );
            }
        }

        public bool DoShowIndex { set { IndexTextRoot.gameObject.SetActive( value ); } }


        private void HandleChainNodeTargetsDirty()
        {
            ChainView.Instance.TargetsDirty = true;
        }

        #region Transfering Between Groups

        [UsedImplicitly]
        public void HandleInputConnectionClicked()
        {
            if ( !( Input.GetKey( KeyCode.LeftControl ) || Input.GetKey( KeyCode.RightControl ) ) )
                return;

            var parentNode = ChainView.Instance.Chain.NodesEnumerableByRouterTraversal.FirstOrDefault( n => n.Router.UniqueTargets.Contains( ChainNode ) );
            if ( parentNode == null )
                return;

            var parentGroup = ChainView.Instance.Chain.RootGroup.RecursiveGroupsEnumerable.FirstOrDefault( g => g.Nodes.Contains( parentNode ) );
            if ( parentGroup == null )
                return;

            var parentGroupView = ChainView.Instance.GetGroupViewForGroup( parentGroup );
            if ( parentGroupView == null )
                return;

            RequestTransfer( parentGroupView );
        }

        public event Action< ChainNodeView, ChainGroupView > TransferRequested = delegate { };

        public void RequestTransfer( ChainGroupView destinationGroupView )
        {
            ChainNode.RequestTransfer( destinationGroupView.Group );

            TransferRequested( this, destinationGroupView );
        }

        #endregion

        public IEnumerable< StateRouterTargetLabelView > TargetLabels
        {
            get
            {
                if ( RouterView == null )
                    return new List< StateRouterTargetLabelView >();
                
                return RouterView.TargetLabels;
            }
        }


        #region Controllable Views

        private bool GenerateControllableViewsQueued { get; set; }
        private bool GeneratingControllableViews { get; set; }

        private static int ActivelyGeneratingControllablesCount { get; set; }
        private static int ControllableGenerationQueueCount { get; set; }

        private void QueueGenerateControllableViews()
        {
            GenerateControllableViewsQueued = true;
            //ControllableGenerationQueueCount++;
            SatelliteUpdateLord.Enqueue(this);
        }

        public void UpdateQueuedControllableGeneration()
        {
            if ( !GenerateControllableViewsQueued )
                return;

            //if ( ActivelyGeneratingControllablesCount >= 4 )
            //    return;

            GeneratingControllableViews = true;
            //ActivelyGeneratingControllablesCount++;

            //Debug.Log( "Incremented Count to " + ActivelyGeneratingControllablesCount );

            GenerateControllableViews();

            GenerateControllableViewsQueued = false;
        }

        public void LateUpdateQueuedControllableGeneration()
        {
            if ( !GeneratingControllableViews )
                return;

            GeneratingControllableViews = false;
            //ActivelyGeneratingControllablesCount--;
            //ControllableGenerationQueueCount--;

            //if ( ControllableGenerationQueueCount == 0 )
            //    ChainView.Instance.TargetsDirty = true;
        }

        private void GenerateControllableViews()
        {
            ControllableFactory.GenerateControllableUiElements( this );

            foreach ( var boundsChanger in ControllableUiItems.Where( e => e is IBoundsChanger ).Cast< IBoundsChanger >() )
            {
                boundsChanger.BoundsChanged += () => BoundsChanged();
            }

            GenerateRouterView();
        }


        private void DestroyControllableViews()
        {
            foreach (var c in ControllableUiItems)
            {
                DestroyImmediate(c.gameObject);
            }
        }


        #endregion


        private StateRouterView RouterView { get; set; }

        private void GenerateRouterView()
        {
            RouterView = ControllableFactory.AddCustomElement< StateRouterView >( StateRouterUiPrefab, this );

            RouterView.Router = ChainNode.Router;

            // For whatever reason, a method group wasn't working here...
            RouterView.UniqueTargetsChanged += () => BoundsChanged();
        }

        

        [UsedImplicitly]
        public void HandleCloseButtonClicked()
        {
            var recurse = Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift );

            UndoLord.Instance.Push(ChainNode.Destroy( recurse ));
        }

        [UsedImplicitly]
        public void HandleDuplicateButtonClicked()
        {
            var recurse = Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift );

            DuplicationRequested( this, recurse );
        }

        [UsedImplicitly]
        public void HandleEnableButtonClicked()
        {
            ChainNode.ExplicitlyDisabled = !ChainNode.ExplicitlyDisabled;
        }

        private void HandleChainNodeDisabledChanged( bool disabled )
        {
            IndicateDisabled();
        }

        private void IndicateDisabled()
        {
            //EnableButtonTextComponent.text = enabled ? "☒" : "☐"; // "○" : "⌀";

            var color = GetBackgroundColor();
            if ( ChainNode.Disabled )
            {
                color.r *= 0.7f;
                color.g *= 0.7f;
                color.b *= 0.7f;
                color.a = 0.9f;
            }
            BackgroundImage.color = color;

            DisabledOverlayGraphicComponent.enabled = ChainNode.Disabled;

            DisableButtonForegroundComponent.color = ChainNode.ExplicitlyDisabled ? DisableButtonLightColor : DisableButtonDarkColor;
            DisableButtonBackgroundComponent.color = ChainNode.ExplicitlyDisabled ? DisableButtonDarkColor : DisableButtonLightColor;

            IndicateError();
        }

        
        [UsedImplicitly]
        public void HandleCommentTextSubmitted()
        {
            UndoLord.Instance.Push(new NodeCommentChange(ChainNode, CommentInputField.text));
            ChainNode.Comment = CommentInputField.text;
            
            if (!EventSystem.current.alreadySelecting)
                EventSystem.current.SetSelectedGameObject( null );
        }

        private void HandleChainNodeCommentChanged( string text )
        {
            CommentInputField.text = text;
        }

        


        [UsedImplicitly]
        private void Start()
        {
            Draggable.MouseUp += () =>
            {
                if(!Draggable.IsDragging) return;
                if(ChainView.GroupTransferModeActive)
                {
                    // TODO: Handle adding node transfer to undo stack
                }
                else
                {
                    UndoLord.Instance.Push(new ChainNodeMoved(Draggable,
                        transform.localPosition - Draggable.OriginalPosition));
                }
            };
            Draggable.MouseDown += HandleDraggableMouseDown;
            Draggable.DragMoved += HandleDraggableDragMoved;
            Draggable.DragEnded += HandleDraggableDragEnded;

            DoShowIndex = ChainView.DoShowNodeIndices;
        }

        [UsedImplicitly]
        private void Update()
        {
            JustMoved = false;

            //UpdateQueuedControllableGeneration();
        }

        //private void LateUpdate()
        //{
        //    LateUpdateQueuedControllableGeneration();
        //}

        public void OnPointerClick( PointerEventData eventData )
        {
            var shiftDown = Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift );

            if ( shiftDown || ChainView.SelectedSelectionState != null )
                if ( !JustMoved )
                    ChainView.SelectedChainNode = ( ChainView.SelectedChainNode == ChainNode ) ? null : ChainNode;
            //else
            //    Expanded = !Expanded;
        }

        private bool IsRecursiveDrag { get; set; }

        private void HandleDraggableMouseDown()
        {
            if ( ChainView.Instance.Zooming ) return;

            IsRecursiveDrag = Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift );

            if ( IsRecursiveDrag )
            {
                var nodeViewsToDrag = ChainView.GetDescendentNodeViews( this );

                nodeViewsToDrag.Add( this );

                Draggable.Targets = nodeViewsToDrag.Select( d => d.transform ).ToList();
            }
            else
            {
                Draggable.Targets = new List< Transform >{ transform };
            }
        }

        private void HandleDraggableDragMoved()
        {
            if ( ChainView.Instance.Zooming ) return;

            BoundsChanged();
            
            if ( IsRecursiveDrag )
                foreach ( var nodeView in ChainView.GetDescendentNodeViews( this ) )
                    nodeView.BoundsChanged();
        }

        private bool JustMoved { get; set; }

        private void HandleDraggableDragEnded()
        {
            if ( ChainView.Instance.Zooming || ChainView.Instance.Dragging ) return;

            JustMoved = true;

            BoundsChanged();

            m_rect = new Rect(transform.position.x - 80, transform.position.y - knownLargestHeight - 80,
                RectTransform.rect.width + 160,
                knownLargestHeight + 160);

            if(IsRecursiveDrag)
            {
                foreach(var nodeView in ChainView.GetDescendentNodeViews(this))
                    nodeView.BoundsChanged();
            }
        }

        public void FireBoundsChanged()
        {
            BoundsChanged();

            m_rect = new Rect(transform.position.x - 80, transform.position.y - knownLargestHeight - 80,
                RectTransform.rect.width + 160,
                knownLargestHeight + 160);
        }

        private Color GetBackgroundColor()
        {
            return ColorMap.Resolve( ChainNode.GetType() );
        }


        #region ViewModel (Serialization of visual properties)

        public ChainNodeViewModel ViewModel
        {
            get { return new ChainNodeViewModel( this ); }
            set
            {
                if ( value.ChainNode != ChainNode )
                    throw new InvalidOperationException( "Cannot apply view model from different node's view." );

                RectTransform.localPosition = value.Position;
            }
        }

        public static int NextJsonId { get; set; }


        [JsonObject( MemberSerialization.OptOut )]
        public class ChainNodeViewModel
        {
            public ChainNode ChainNode { get; set; }

            public Vector3 Position { get; set; }

            //public Vector2 Size { get; set; }

            [JsonConstructor, UsedImplicitly]
            public ChainNodeViewModel()
            {
            }

            public ChainNodeViewModel( ChainNodeView chainNodeView )
            {
                ChainNode = chainNodeView.ChainNode;
                
                Position = chainNodeView.RectTransform.localPosition;
            }
        }

        #endregion

    }
}
