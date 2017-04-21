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
using System.Linq;
using Mutation;
using UnityEngine;
using UnityEngine.UI;

namespace ChainViews.Elements
{
    public class MutableBoxMutableItemBehaviour : MonoBehaviour
    {
        [Header("Component References")]
        
        [SerializeField]
        private Text m_TextComponent = null;
        private Text TextComponent { get { return m_TextComponent; } }

        [SerializeField]
        private MutableBoxTypeIndicatorBehaviour m_TypeIndicatorComponent = null;
        private MutableBoxTypeIndicatorBehaviour TypeIndicatorComponent { get { return m_TypeIndicatorComponent; } }

        public event Action< MutableBoxMutableItemBehaviour > Selected = delegate { };

        public bool IsGlobalParameter
        {
            get { return SchemaSource == SchemaSource.Global; }
            set { SchemaSource = value ? SchemaSource.Global : SchemaSource.Mutable; }
        }

        public SchemaSource SchemaSource { get; set; }

        private string m_AbsoluteKey = string.Empty;
        public string AbsoluteKey
        {
            get
            {
                return m_AbsoluteKey;
            }
            set
            {
                m_AbsoluteKey = value;

                var relativeSplits = m_AbsoluteKey.Split( '.' );
                IndentLevel = AbsoluteKey==""?0:(Math.Max( relativeSplits.Count() - 1, 0 ) + (IsGlobalParameter?1:1));
                //RelativeKey = relativeSplits.Last() ?? m_AbsoluteKey;

                GenerateDisplayStrings();

                SetText();
            }
        }

        public Type Type
        {
            set { TypeIndicatorComponent.Type = value; }
        }

        private void SetText()
        {
            TextComponent.text = UserFacingLastKey;
        }

        private void GenerateDisplayStrings()
        {
            switch ( SchemaSource )
            {
                case SchemaSource.Literal:
                    UserFacingAbsoluteKey = AbsoluteKey;
                    UserFacingLastKey = AbsoluteKey==""?"":
                        AbsoluteKey.Split( '.' ).Last();
                    break;
                case SchemaSource.Mutable:
                    UserFacingAbsoluteKey = "Local Payload"+(AbsoluteKey!=""?".":"")+AbsoluteKey;
                    UserFacingLastKey = AbsoluteKey == "" ? "Local Payload" :
                        AbsoluteKey.Split('.').Last();
                    break;
                case SchemaSource.Cached:
                    UserFacingAbsoluteKey = "Cached Data" + (AbsoluteKey != "" ? "." : "") + AbsoluteKey;
                    UserFacingLastKey = AbsoluteKey == "" ? "Cached Data" :
                        AbsoluteKey.Split('.').Last();
                    break;
                case SchemaSource.Global:
                    UserFacingAbsoluteKey = "Global Data" + (AbsoluteKey != "" ? "." : "") + AbsoluteKey;
                    UserFacingLastKey = AbsoluteKey == "" ? "Global Payload" :
                        AbsoluteKey.Split('.').Last();
                    break;
            }
        }

        public string UserFacingLastKey { get; private set; }
        public string UserFacingAbsoluteKey { get; private set; }

        //public string UserfacingLastKey
        //{
        //    get { return AbsoluteKey == "" ? ( IsGlobalParameter ? "Global Payload" : "Local Payload" ) : RelativeKey; }
        //}
        //
        //public string UserFacingAbsoluteKey
        //{
        //    get { return AbsoluteKey == "" ? (IsGlobalParameter ? "Global Payload" : "Local Payload") : AbsoluteKey; }
        //}

        public string RelativeKey { get; private set; }

        private int IndentLevel
        {
            set
            {
                var rectTransform = TextComponent.GetComponent<RectTransform>();
                rectTransform.offsetMin = new Vector2( rectTransform.offsetMin.x + value * 10, rectTransform.offsetMin.y );
            }
        }

        private bool m_IsValidType = true;
        public bool IsValidType
        {
            get { return m_IsValidType; }
            set
            {
                m_IsValidType = value;
                
                var color = m_IsValidType ? Color.black : Color.gray;

                TextComponent.color = color;
            }
        }

        public void HandleClicked()
        {
            if ( IsValidType )
                Selected( this );
        }
    }
}
