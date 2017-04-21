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

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    [RequireComponent(typeof( RectTransform ))]
    public class RectTransformDebugger : MonoBehaviour
    {
        #region Lazy Init Component Properties

        private RectTransform m_RectTransform;
        public RectTransform DebugRectTransform
        {
            get { return m_RectTransform ?? ( m_RectTransform = GetComponent<RectTransform>() ); }
        }

        #endregion

        private static int FontSize { get { return 9; } }
        private static string FontName { get { return "Consolas"; } }

        private RectTransform ElementRoot { get; set; }

        private Text m_TopLeftTextComponent;
        private Text TopLeftTextComponent { get { return m_TopLeftTextComponent; } set { m_TopLeftTextComponent = value; } }

        [UsedImplicitly]
        private void Start()
        {
            GenerateDisplayElements();
        }

        private void GenerateDisplayElements()
        {
            // Element Root

            var elementRootGo = new GameObject("|| RECT TRANSFORM DEBUGGER ||");
            ElementRoot = elementRootGo.AddComponent< RectTransform >();

            ElementRoot.SetParent( DebugRectTransform, false );
            ElementRoot.sizeDelta = new Vector2( 0.0f, 0.0f );
            ElementRoot.anchorMin = new Vector2( 0.0f, 0.0f );
            ElementRoot.anchorMax = new Vector2( 1.0f, 1.0f );

            //var elementRootImage = elementRootGo.AddComponent< Image >();
            //elementRootImage.sprite = null;
            //elementRootImage.color = new Color( 1.0f, 0.0f, 0.0f, 0.4f );
            
            // Top Left Text

            var topLeftTextGo = new GameObject( "Top Left Text" );
            TopLeftTextComponent = topLeftTextGo.AddComponent< Text >();

            TopLeftTextComponent.rectTransform.SetParent( ElementRoot );
            TopLeftTextComponent.rectTransform.anchorMin = new Vector2( 0.0f, 1.0f );
            TopLeftTextComponent.rectTransform.anchorMax = new Vector2( 0.0f, 1.0f );
            TopLeftTextComponent.rectTransform.pivot = new Vector2( 0.0f, 0.0f );
            TopLeftTextComponent.rectTransform.anchoredPosition = new Vector2( 0.0f, 0.0f );
            TopLeftTextComponent.rectTransform.sizeDelta = new Vector2(0.0f, 0.0f);
            
            TopLeftTextComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
            TopLeftTextComponent.verticalOverflow = VerticalWrapMode.Overflow;
            TopLeftTextComponent.alignment = TextAnchor.LowerRight;

            //Font.GetOSInstalledFontNames().ToList().ForEach( fontName => Debug.Log("Found font: " + fontName ));
            TopLeftTextComponent.font = Font.CreateDynamicFontFromOSFont( FontName, FontSize );
            TopLeftTextComponent.fontSize = FontSize;
        }

        [UsedImplicitly]
        private void Update()
        {
            var text = "<b>Position: </b>    " + DebugRectTransform.position + "\n" +
                       "<b>Local Pos: </b>   " + DebugRectTransform.localPosition + "\n" +
                       "<b>Anchored Pos: </b>" + DebugRectTransform.anchoredPosition + "\n" +
                       "<b>Size Delta: </b>  " + DebugRectTransform.sizeDelta + "\n" +
                       "<b>Rect Size: </b>   " + DebugRectTransform.rect.size + "\n" +
                       "<b>Pref Height: </b> " + LayoutUtility.GetPreferredHeight( DebugRectTransform );


            TopLeftTextComponent.text = text;
        }
    }
}
