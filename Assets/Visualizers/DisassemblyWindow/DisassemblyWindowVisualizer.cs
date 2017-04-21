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
using System.Linq;
using Adapters.TraceAdapters.Traces;
using Mutation.Mutators.TeamSpecific;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace Visualizers.DisassemblyWindow
{
    public class DisassemblyWindowVisualizer : MonoBehaviour
    {
        [Header("Component References")]

        [SerializeField]
        private Text m_ElementCountTextComponent = null;
        private Text ElementCountTextComponent { get { return m_ElementCountTextComponent; } }

        [SerializeField]
        private Text m_RequestIdTextComponent = null;
        private Text RequestIdTextComponent { get { return m_RequestIdTextComponent; } }

        //[SerializeField]
        //private Text m_RequestNatureTextComponent = null;
        //private Text RequestNatureTextComponent { get { return m_RequestNatureTextComponent; } }

        [SerializeField]
        private Text m_BinaryIdTextComponent = null;
        private Text BinaryIdTextComponent { get { return m_BinaryIdTextComponent; } }
        
        [SerializeField]
        private Image m_CsSuccessIndicatorPanel = null;
        private Image CsSuccessIndicatorPanel { get { return m_CsSuccessIndicatorPanel; } }

        [SerializeField]
        private Image m_RequestSuccessIndicatorPanel = null;
        private Image RequestSuccessIndicatorPanel { get { return m_RequestSuccessIndicatorPanel; } }

        [SerializeField]
        private Image m_RequestTitlebarComponent = null;
        private Image RequestTitlebarComponent { get { return m_RequestTitlebarComponent; } }

        [SerializeField]
        private Image m_ChallengSetTitleBarComponent = null;
        private Image ChallengSetTitleBarComponent { get { return m_ChallengSetTitleBarComponent; } }

        [SerializeField]
        private Image m_DisassemblySectionTitleBar = null;
        private Image DisassemblySectionTitleBar { get { return m_DisassemblySectionTitleBar; } }

        [SerializeField]
        private Image m_CommsSectionTitleBar = null;
        private Image CommsSectionTitleBar { get { return m_CommsSectionTitleBar; } }

        [SerializeField]
        private Image m_FilamentColorComponent = null;
        private Image FilamentColorComponent { get { return m_FilamentColorComponent; } }

        [SerializeField]
        private Image m_RequestTeamLogoComponent = null;
        private Image RequestTeamLogoComponent { get { return m_RequestTeamLogoComponent; } }

        [SerializeField]
        private RectTransform m_CommsEntryAttachmentPoint = null;
        private RectTransform CommsEntryAttachmentPoint { get { return m_CommsEntryAttachmentPoint; } }

        [SerializeField]
        private RectTransform m_DisasmEntryAttachmentPoint = null;
        private RectTransform DisasmEntryAttachmentPoint { get { return m_DisasmEntryAttachmentPoint; } }
        


        //[SerializeField]
        //private GameObject m_CommsEntryPrefab = null;
        //private GameObject CommsEntryPrefab { get { return m_CommsEntryPrefab; } }

        [SerializeField]
        private GameObject m_DisassemblyRoot = null;
        private GameObject DisassemblyRoot { get { return m_DisassemblyRoot; } }

        [SerializeField]
        private GameObject m_CommsRoot = null;
        private GameObject CommsRoot { get { return m_CommsRoot; } }


        [SerializeField]
        private RectTransform m_BinsetTeamIndicatorAttachmentPoint = null;
        private RectTransform BinsetTeamIndicatorAttachmentPoint { get { return m_BinsetTeamIndicatorAttachmentPoint; } }

        [SerializeField]
        private Text m_BinsetShortNameTextComponent = null;
        private Text BinsetShortNameTextComponent { get { return m_BinsetShortNameTextComponent; } }




        [Header("Prefab References")]


        [SerializeField]
        private GameObject m_BinsetTeamIndicatorPrefab = null;
        private GameObject BinsetTeamIndicatorPrefab { get { return m_BinsetTeamIndicatorPrefab; } }


        [SerializeField]
        private Sprite m_FlagSprite = null;
        private Sprite FlagSprite { get { return m_FlagSprite; } }


        [Header("Configuration")]

        [SerializeField]
        private Color m_SuccessColor = Color.green;
        private Color SuccessColor { get { return m_SuccessColor; } }

        [SerializeField]
        private Color m_FailureColor = Color.red;
        private Color FailureColor { get { return m_FailureColor; } }

        #region Data Setters

        public Color RequestColor
        {
            set
            {
                //RequestTitlebarComponent.color = value;
            }
        }

        public Color FilamentColor
        {
            set
            {
                //ChallengSetTitleBarComponent.color = value;
                //DisassemblySectionTitleBar.color = value;
                //CommsSectionTitleBar.color = value;
                FilamentColorComponent.color = value;
            }
        }

        public List< int > BinsetTeamIndices
        {
            set
            {
                foreach ( var i in value )
                {
                    var color = TeamColorPalette.ColorFromIndex( i );

                    var teamIndicatorGo = Instantiate( BinsetTeamIndicatorPrefab, BinsetTeamIndicatorAttachmentPoint );
                    var teamIndicator = teamIndicatorGo.GetComponent< Graphic >();

                    teamIndicator.color = color;
                    
                    teamIndicator.transform.localPosition = Vector3.zero;
                }
            }
        }

        public string BinsetShortName
        {
            set
            {
                BinsetShortNameTextComponent.text = value;

                StartCoroutine( DelayScaleCsNameToFit() );
            }
        }

        private IEnumerator DelayScaleCsNameToFit()
        {
            yield return new WaitForEndOfFrame();

            var textSize = BinsetShortNameTextComponent.rectTransform.sizeDelta.x;

            var parentSize = ( BinsetShortNameTextComponent.rectTransform.parent as RectTransform ).sizeDelta.x;

            // Padding...
            parentSize -= 20;

            if ( textSize < parentSize )
                yield break;

            var scaleDiff = parentSize / textSize;

            BinsetShortNameTextComponent.rectTransform.localScale = new Vector3( scaleDiff, 1, 1 );

            //Debug.Log( "Binset name size is " + textSize );
            //Debug.Log( "parent size is " + parentSize );
        }

        public Material RequestTeamLogo
        {
            set { RequestTeamLogoComponent.material = value; }
        }

        public int BinaryId
        {
            set { BinaryIdTextComponent.text = string.Format( "Bin {0}", value ); }
        }

        private int m_RequestId;
        public int RequestId
        {
            get { return m_RequestId; }
            set
            {
                m_RequestId = value;

                UpdateRequestText();
            }
        }

        private RequestNature m_RequestNature;
        public RequestNature RequestNature
        {
            get { return m_RequestNature; }
            set
            {
                m_RequestNature = value;

                UpdateRequestText();
            }
        }

        private void UpdateRequestText()
        {
            RequestIdTextComponent.text = string.Format( "{0} {1}", RequestNature == RequestNature.Pov ? "PoV" : "Poll", RequestId );
        }

        public int ElementCount
        {
            set { ElementCountTextComponent.text = string.Format( "{0} Elements", value ); }
        }

        public bool Success
        {
            set
            {
                CsSuccessIndicatorPanel.color = value ? SuccessColor : FailureColor;

                if ( RequestNature == RequestNature.ServicePoll )
                    RequestSuccessIndicatorPanel.color = value ? SuccessColor : FailureColor;
                else
                    RequestSuccessIndicatorPanel.color = value ? FailureColor : SuccessColor;
            }
        }

        public int PovType
        {
            set
            {
                // One-time setter, cannot revert...

                if ( value == 2 )
                {
                    RequestSuccessIndicatorPanel.sprite = FlagSprite;
                    CsSuccessIndicatorPanel.sprite = FlagSprite;
                }
            }
        }

        private bool Embiggened { get; set; }
        public void Embiggen()
        {
            Embiggened = true;

            GetComponent< LayoutElement >().preferredWidth = -1;

            foreach ( var commsEntryVisualizer in CommsEntryVisualizers )
            {
                commsEntryVisualizer.Embiggen();
            }

            foreach ( var disasmEntryVisualizer in DisasmEntryVisualizers )
            {
                disasmEntryVisualizer.Embiggen();
            }
        }

        #endregion


        private void Awake()
        {
            CreateCommsEntryVisualizers();
            CreateDisasmEntryVisualizers();
        }

        public void Destroy()
        {
            Destroy( gameObject );
        }



        #region Comms and Disasm

        private static int CommsEntryVisualizerCount { get { return 16; } }
        private static int DisasmVisualizerCount { get { return 20; } }

        private List< CommsEntryVisualizer > m_CommsEntryVisualizers = new List< CommsEntryVisualizer >();
        private List< CommsEntryVisualizer > CommsEntryVisualizers
        {
            get { return m_CommsEntryVisualizers; }
            set { m_CommsEntryVisualizers = value; }
        }

        private List< DisasmEntryVisualizer > m_DisasmEntryVisualizers = new List< DisasmEntryVisualizer >();
        private List< DisasmEntryVisualizer > DisasmEntryVisualizers
        {
            get { return m_DisasmEntryVisualizers; }
            set { m_DisasmEntryVisualizers = value; }
        }

        public bool ShowDisassembly
        {
            set { DisassemblyRoot.SetActive( value ); }
        }

        public bool ShowComms
        {
            set { CommsRoot.SetActive( value ); }
        }

        private void CreateCommsEntryVisualizers()
        {
            for ( int i = 0; i < CommsEntryVisualizerCount; i++ )
            {
                var commsEntryVisualizerGo = VisualizerFactory.InstantiateCommsItem();
                var commsEntryVisualizer = commsEntryVisualizerGo.GetComponent< CommsEntryVisualizer >();

                commsEntryVisualizer.transform.SetParent( CommsEntryAttachmentPoint, false );
                commsEntryVisualizer.transform.SetAsFirstSibling();

                if ( Embiggened )
                    commsEntryVisualizer.Embiggen();

                commsEntryVisualizer.gameObject.SetActive( false );
                
                CommsEntryVisualizers.Add( commsEntryVisualizer );
            }
        }

        private void CreateDisasmEntryVisualizers()
        {
            for ( int i = 0; i < DisasmVisualizerCount; i++ )
            {
                var disasmEntryVisualizerGo = VisualizerFactory.InstantiateDisasmItem();
                var disasmEntryVisualizer = disasmEntryVisualizerGo.GetComponent< DisasmEntryVisualizer >();

                disasmEntryVisualizer.transform.SetParent( DisasmEntryAttachmentPoint );
                disasmEntryVisualizer.transform.SetAsFirstSibling();

                if ( Embiggened )
                    disasmEntryVisualizer.Embiggen();

                disasmEntryVisualizer.gameObject.SetActive( false );

                DisasmEntryVisualizers.Add( disasmEntryVisualizer );
            }
        }

        //public void SetCommsEntries( List<CommsEntryVisualizer> entryVisualizers )
        //{
        //    foreach ( var entryVisualizer in CommsEntryVisualizers )
        //    {
        //        Destroy( entryVisualizer.gameObject );
        //    }

        //    CommsEntryVisualizers = entryVisualizers;

        //    foreach ( var entryVisualizer in CommsEntryVisualizers )
        //    {
        //        entryVisualizer.transform.SetParent( CommsEntryAttachmentPoint, false );
        //    }
        //}

        private List< CommsEntryDescriptor > m_CommsEntries = new List< CommsEntryDescriptor >();
        public List< CommsEntryDescriptor > CommsEntries
        {
            get { return m_CommsEntries; }
            set
            {
                m_CommsEntries = value.ToList();    // Create our own copy...

                m_CommsEntries.Sort();
            }
        }

        private List<DisasmEntryDescriptor> m_DisasmEntries = new List< DisasmEntryDescriptor >();

        public List< DisasmEntryDescriptor > DisasmEntries
        {
            get { return m_DisasmEntries;}
            set
            {
                m_DisasmEntries = value.ToList();
                m_DisasmEntries.Sort();
            }
        }


        private int m_InstructionIndex = 0;
        public int InstructionIndex
        {
            get {  return m_InstructionIndex; }
            set
            {
                m_InstructionIndex = value;

                //Debug.LogFormat( "Disasm window getting index of {0}", m_InstructionIndex );

                RefreshEntryVisualizers();

                RefreshDisasmsVisualizers();
            }
        }

        private int m_LastLastIndexToShow = Int32.MaxValue;
        private int LastLastIndexToShow
        {
            get { return m_LastLastIndexToShow; }
            set { m_LastLastIndexToShow = value; }
        }
        
        private int m_LastDisasmsIndex = Int32.MaxValue;
        private int LastDisasmsIndex
        {
            get { return m_LastDisasmsIndex; }
            set { m_LastDisasmsIndex = value; }
        }

        private void RefreshDisasmsVisualizers()
        {
            var lastIndexToShow = DisasmEntries.BinarySearch(new DisasmEntryDescriptor(InstructionIndex));


            if (lastIndexToShow == LastDisasmsIndex)
                return;

            LastDisasmsIndex = lastIndexToShow;


            if (lastIndexToShow < 0)
            {
                lastIndexToShow = (~lastIndexToShow) - 1;
            }


            var currentIndex = lastIndexToShow;


            foreach (var entryVisualizer in DisasmEntryVisualizers)
            {
                if (currentIndex >= 0)
                {
                    var currentDisasmEntry = DisasmEntries[currentIndex];

                    entryVisualizer.Text = currentDisasmEntry.DisasmText;

                    entryVisualizer.gameObject.SetActive(true);
                }
                else
                {
                    entryVisualizer.gameObject.SetActive(false);
                }

                currentIndex--;
            }

        }

        private void RefreshEntryVisualizers()
        {
            var lastIndexToShow = CommsEntries.BinarySearch( new CommsEntryDescriptor( InstructionIndex ) );

            
            if ( lastIndexToShow == LastLastIndexToShow )
                return;

            LastLastIndexToShow = lastIndexToShow;


            if ( lastIndexToShow < 0 )
            {
                // If item not found, BinarySearch will return bitwise complement of insertion index.
                // Subtract one to get the index of the last item whose value is below the requested value.
                lastIndexToShow = ( ~lastIndexToShow ) - 1;
            }

            var currentIndex = lastIndexToShow;

            foreach (var entryVisualizer in CommsEntryVisualizers)
            {
                if (currentIndex >= 0)
                {
                    var currentCommsEntry = CommsEntries[currentIndex];

                    entryVisualizer.Color = currentCommsEntry.Color;
                    entryVisualizer.Text = currentCommsEntry.Message;
                    entryVisualizer.IsFromRequestSide = currentCommsEntry.IsFromRequestSide;

                    entryVisualizer.gameObject.SetActive(true);
                }
                else
                {
                    entryVisualizer.gameObject.SetActive(false);
                }

                currentIndex--;
            }

        }

        #endregion

    }
}
