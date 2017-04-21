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
using System.Linq;
using Chains;
using JetBrains.Annotations;
using LabelSystem;
using Mutation;
using UnityEngine;
using Visualizers.IsoGrid;

namespace Visualizers.LabelController
{
    public class LabelController : VisualizerController
    {
        private MutableField<string> m_LabelText = new MutableField<string>() 
        { LiteralValue = "Label" };
        [Controllable(LabelText = "Label Text")]
        public MutableField<string> LabelText { get { return m_LabelText; } }

        private MutableField<bool> m_ShowBackground = new MutableField<bool>() 
        { LiteralValue = true };
        [Controllable(LabelText = "Show Background")]
        public MutableField<bool> ShowBackground { get { return m_ShowBackground; } }

        private MutableField<LabelOrientation> m_Orientation = new MutableField<LabelOrientation>() 
        { LiteralValue = LabelOrientation.Out };
        [Controllable(LabelText = "Label Orientation")]
        public MutableField<LabelOrientation> Orientation { get { return m_Orientation; } }

        private MutableField<int> m_FontSize = new MutableField<int>() 
        { LiteralValue = 64 };
        [Controllable(LabelText = "Font Size")]
        public MutableField<int> FontSize { get { return m_FontSize; } }

        private MutableField<Color> m_FontColor = new MutableField<Color>() 
        { LiteralValue = Color.black };
        [Controllable(LabelText = "Font Color")]
        public MutableField<Color> FontColor { get { return m_FontColor; } }

        private MutableField<string> m_FontName = new MutableField<string>() 
        { LiteralValue = "altdin" };
        [Controllable(LabelText = "Typeface", ValidValuesListName = "FontNames")]
        public MutableField<string> FontName { get { return m_FontName; } }

        [UsedImplicitly]
        private List<string> FontNames { get { return FontFactory.Instance.Fonts.Keys.ToList(); } }

        private MutableField<LateralJustificationOption> m_Justification = new MutableField<LateralJustificationOption>() { LiteralValue = LateralJustificationOption.Center };
        [Controllable(LabelText = "Lateral Justification")]
        public MutableField<LateralJustificationOption> Justification { get { return m_Justification; } }

        private MutableField<VerticalJustificationOption> m_VerticalJustification = new MutableField<VerticalJustificationOption>() { LiteralValue = VerticalJustificationOption.Center };
        [Controllable(LabelText = "Vertical Justification")]
        public MutableField<VerticalJustificationOption> VerticalJustification { get { return m_VerticalJustification; } }


        private MutableField<float> m_BackgroundPadding = new MutableField<float>() 
        { LiteralValue = .2f };
        [Controllable(LabelText = "Background Padding")]
        public MutableField<float> BackgroundPadding { get { return m_BackgroundPadding; } }

        private MutableField<float> m_BackgroundDepth = new MutableField<float>() 
        { LiteralValue = 1f };
        [Controllable(LabelText = "Background Depth")]
        public MutableField<float> BackgroundDepth { get { return m_BackgroundDepth; } }


        private MutableField<float> m_MinHeight = new MutableField<float>() 
        { LiteralValue = -1f };
        [Controllable(LabelText = "Min ScreenHeight")]
        public MutableField<float> MinHeight { get { return m_MinHeight; } }

        private MutableField<float> m_MaxHeight = new MutableField<float>() 
        { LiteralValue = -1f };
        [Controllable(LabelText = "Max ScreenHeight")]
        public MutableField<float> MaxHeight { get { return m_MaxHeight; } }

        private MutableField<int> m_CharactersPerLine = new MutableField<int>() 
        { LiteralValue = -1 };
        [Controllable(LabelText = "Characters Per Line")]
        public MutableField<int> CharactersPerLine { get { return m_CharactersPerLine; } }
        
        private MutableField<int> m_MaxLines = new MutableField<int>() 
        { LiteralValue = -1 };
        [Controllable(LabelText = "Maximum Lines to Render")]
        public MutableField<int> MaxLines { get { return m_MaxLines; } }



        public SelectionState DefaultState { get { return Router["Default"]; } }

        public SelectionState ClickState { get { return Router["On Click"]; } }

        public LabelController()
        {
            Router.AddSelectionState( "Default" );
            Router.AddSelectionState( "On Click" );
        }

        protected override IEnumerator ProcessPayload( VisualPayload payload )
        {
            var label = VisualizerFactory.InstantiateLabelVisualizerPrefab();

            label.Initialize( this, payload );


            var meshRenderer = label.TextComponent.GetComponent< MeshRenderer >();

            var targetFont = FontFactory.GetFontPair( FontName.GetFirstValue( payload.Data ) );

            var newMaterial = FontFactory.GenerateNewSpatialMaterial( targetFont.FontTexture );

            label.TextComponent.font = targetFont.Font;
            
            label.TextComponent.fontSize = Mathf.FloorToInt(FontSize.GetFirstValue( payload.Data ) * targetFont.FontScale);
            label.transform.localPosition = new Vector3(0f, label.TextComponent.fontSize * targetFont.VerticalOffset, 0f);

            label.CharactersPerLine = CharactersPerLine.GetFirstValue( payload.Data );

            label.MaxLines = MaxLines.GetFirstValue( payload.Data );

            newMaterial.color = FontColor.GetFirstValue( payload.Data );

            meshRenderer.material = newMaterial;


            label.BackgroundPadding = BackgroundPadding.GetFirstValue( payload.Data );
            label.BackgroundDepth = BackgroundDepth.GetFirstValue( payload.Data );

            label.LateralJustification = Justification.GetFirstValue(payload.Data);

            label.VerticalJustification = VerticalJustification.GetFirstValue( payload.Data );

            label.MinHeight = MinHeight.GetFirstValue( payload.Data );
            label.MaxHeight = MaxHeight.GetFirstValue( payload.Data );

            label.Orientation = Orientation.GetFirstValue(payload.Data);

            label.SetClickState( ClickState );

            label.RemoveBackground = !ShowBackground.GetFirstValue( payload.Data );

            label.Text = LabelText.GetFirstValue(payload.Data);

            var newPayload = new VisualPayload(payload.Data, new VisualDescription(label.Bound));

            var iterator = DefaultState.Transmit(newPayload);
            while (iterator.MoveNext())
                yield return null;
        }
    }
}
