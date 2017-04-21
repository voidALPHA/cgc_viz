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
using System.Globalization;
using System.Linq;
using System.Text;
using Chains;
using LabelSystem;
using Mutation;
using Ui;
using UnityEngine;
using Utility;
using Utility.JobManagerSystem;

namespace Visualizers.LabelController
{
    public enum LateralJustificationOption
    {
        Center,
        Left,
        Right,
    }

    public enum VerticalJustificationOption
    {
        Center,
        Top,
        Bottom,
    }

    public class LabelVisualizer : Visualizer
    {
        [Header("Component References")]
        [SerializeField]
        private TextMesh m_TextComponent = null;
        public TextMesh TextComponent { get { return m_TextComponent; } }

        [SerializeField]
        private Transform m_Geometry;
        public Transform Geometry { get { return m_Geometry; } set { m_Geometry = value; } }

        [SerializeField]
        private Transform m_TextRoot;
        private Transform TextRoot { get { return m_TextRoot; } set { m_TextRoot = value; } }

        [SerializeField]
        private LabelClickSender m_LabelClickSender;
        protected LabelClickSender LabelClickSender { get { return m_LabelClickSender; } set { m_LabelClickSender = value; } }

        public int CharactersPerLine { get; set; }

        public int MaxLines { get; set; }

        private const float PressedLabelScaleMult = 0.90f;
        private Vector3 SavedScale { get; set; }

        private SelectionState OnClickState { get; set; }

        private LateralJustificationOption m_LateralJustification = LateralJustificationOption.Center;
        public LateralJustificationOption LateralJustification { get { return m_LateralJustification; } set { m_LateralJustification = value; } }

        private VerticalJustificationOption m_VerticalJustification = VerticalJustificationOption.Top;
        public VerticalJustificationOption VerticalJustification { get { return m_VerticalJustification;} set{m_VerticalJustification = value;} }


        private float m_MinWidth = -1;
        public float MinWidth { get { return m_MinWidth; } set { m_MinWidth = value; } }

        private float m_MaxWidth = -1;
        public float MaxWidth { get { return m_MaxWidth; } set { m_MaxWidth=value; } }

        private float m_MinHeight = .1f;
        public float MinHeight { get { return m_MinHeight; } set { m_MinHeight = value; } }

        private float m_MaxHeight = 1.2f;
        public float MaxHeight { get { return m_MaxHeight; } set { m_MaxHeight = value; } }

        private float m_BackgroundPadding = .1f;
        public float BackgroundPadding { get { return m_BackgroundPadding; } set { m_BackgroundPadding = value; } }

        private float m_BackgroundDepth = 1f;
        public float BackgroundDepth { get { return m_BackgroundDepth; } set { m_BackgroundDepth = value; } }

        
        private Vector3 m_ComponentScale = Vector3.one;
        private Vector3 ComponentScale { get { return m_ComponentScale; } set { m_ComponentScale = value; } }

        public LabelOrientation Orientation
        {
            set
            {
                if (!LabelBehaviour.OrientationToDegreesX.ContainsKey(value) || 
                    !LabelBehaviour.OrientationToDegreesZ.ContainsKey(value))
                    throw new NotImplementedException();

                Geometry.transform.localRotation = Quaternion.Euler(LabelBehaviour.OrientationToDegreesX[value], 0.0f, LabelBehaviour.OrientationToDegreesZ[value]);
                TextRoot.transform.localRotation = Quaternion.Euler(LabelBehaviour.OrientationToDegreesX[value], 0.0f, LabelBehaviour.OrientationToDegreesZ[value]);
            }
        }

        public bool RemoveBackground
        {
            set
            {
                if ( !value )
                    return;

                foreach (var rend in Geometry.GetComponentsInChildren<Renderer>())
                    Destroy( rend );
            }
        }

        public string Text
        {
            set
            {
                var lines = new List< string >() { value };
                if (CharactersPerLine>-1)
                    lines = WrapText( value, CharactersPerLine, MaxLines);

                string wrappedText = lines.Aggregate( ( a, b ) => a + "\n" + b );
                
                TextComponent.text = wrappedText;

                Resize();
            }
        }

        private void Resize()
        {
            transform.localScale = ComponentScale;

            var textRenderer = TextComponent.GetComponent<Renderer>();
            var size = textRenderer.GetLocallyAlignedBounds();

            var geomScale = Vector3.one;

            float localPadding = BackgroundPadding * Mathf.Min( size.x, size.y ) * 2f;

            geomScale.x = size.x + localPadding;// * (TextComponent.fontSize / 64f) + (padding * 2.0f);
            geomScale.y = size.y + localPadding;
            geomScale.z = localPadding*BackgroundDepth;
            //geomScale.y *= (TextComponent.fontSize / 64f) + (padding * 2.0f);

            if (float.IsNaN(size.x) || float.IsNaN(size.y) || float.IsNaN(size.z))
                Debug.LogError("Vector scale not valid!", this);

            Geometry.SetWorldScale( geomScale);

            //TextScale = size;
            
            var positionOffset =
                transform.right*(
                    LateralJustification == LateralJustificationOption.Left
                        ? 1f
                        : LateralJustification == LateralJustificationOption.Right ? -1f:0);

            var vertOffset =
                transform.up * (
                    VerticalJustification == VerticalJustificationOption.Top
                        ? -1f
                        : VerticalJustification == VerticalJustificationOption.Bottom ? 1f : 0 );

            positionOffset *= size.x / 2f;
            vertOffset *= size.y / 2f;

            TextRoot.transform.position = transform.position + positionOffset + vertOffset;
            Geometry.transform.position = transform.position + positionOffset + vertOffset;
        }

        private void Start()
        {
            LabelClickSender.onLabelMouseDown += OnLabelMouseDown;
            LabelClickSender.onLabelMouseUp += OnLabelMouseUp;
            LabelClickSender.onLabelMouseClick += OnLabelMouseClick;
        }

        private void Update()
        {
            var textScale = Vector3.one;

            var mainCamera = CameraLocaterSatellite.MasterCameraLocater.FoundCamera;

            var screenSpace = mainCamera.WorldToScreenPoint( transform.position );

            //Debug.Log( "Screenspace: " + screenSpace );

            var perspectiveDivisor = mainCamera.fieldOfView * .5f * Mathf.Deg2Rad;

            //var screenSpaceWidth = textScale.x / ( perspectiveDivisor * screenSpace.z );
            //screenSpaceWidth *= Screen.width;

            var screenSpaceHeight = textScale.y / (perspectiveDivisor * screenSpace.z);
            screenSpaceHeight *= Screen.height;

            //DebugSize = new Vector3(screenSpaceWidth, screenSpaceHeight, 1);


            //var minMult = float.MinValue;
            //var maxMult = float.MaxValue;

            var minSizeMult = MinHeight / screenSpaceHeight;
            var maxSizeMult = MaxHeight / screenSpaceHeight;

            //if ( minSizeMult > 1 )
            //    minMult = Math.Max( minMult, minSizeMult );
            //
            //if (maxSizeMult < 1)
            //    maxMult = Math.Max(maxMult, maxSizeMult);



            bool resetScale = true;

            var scalarOffset = 1f;

            if ( MinHeight > 0 )
                if (minSizeMult > 1)
                {
                    scalarOffset = minSizeMult;
                    resetScale = false;
                }

            if ( MaxHeight > 0 )
                if (maxSizeMult < 1)
                {
                    scalarOffset = Mathf.Min(maxSizeMult, scalarOffset);
                    resetScale = false;
                }

            if ( resetScale )
                ComponentScale = Vector3.one;
            else
                ComponentScale = Vector3.one*scalarOffset;

            Resize();
        }

        public void SetClickState( SelectionState clickState )
        {
            OnClickState = clickState;
        }

        private void OnLabelMouseDown()
        {
            SavedScale = transform.localScale;
            transform.localScale = new Vector3(SavedScale.x * PressedLabelScaleMult, SavedScale.y * PressedLabelScaleMult, SavedScale.z * PressedLabelScaleMult);
        }

        private void OnLabelMouseUp()
        {
            transform.localScale = SavedScale;
        }

        private void OnLabelMouseClick()
        {
            if (OnClickState!=null)
            JobManager.Instance.StartJob(
                OnClickState.Transmit( new VisualPayload(
                    Bound.Data, new VisualDescription( Bound ) ) ), jobName: "Label Click", startImmediately: true, maxExecutionsPerFrame: 1);
        }

        
        public static List<string> WrapText(string text, int charLimit, int maxLines = 0)
        {
            var lineSegments = text.Split( new[] { " " }, StringSplitOptions.None );

            var wrappedLines = new List<string>();

            var stringBuilder = new StringBuilder();
            

            foreach (var currentLineSegment in lineSegments)
            {
                var spaceLength = stringBuilder.Length == 0 ? 0 : 1;

                if (stringBuilder.Length + spaceLength + currentLineSegment.Length > charLimit )
                {
                    wrappedLines.Add( stringBuilder.ToString() );

                    stringBuilder.Length = 0;  // Clear string builder
                }

                if ( stringBuilder.Length > 0 )
                    stringBuilder.Append( " " );

                stringBuilder.Append( currentLineSegment );
            }

            if ( stringBuilder.Length > 0 )
                wrappedLines.Add( stringBuilder.ToString() );

            return maxLines > 0 ? wrappedLines.Take( maxLines ).ToList() : wrappedLines;
        }
    }
}
