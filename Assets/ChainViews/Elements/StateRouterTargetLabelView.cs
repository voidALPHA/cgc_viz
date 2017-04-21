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

using Chains;
using UnityEngine;
using UnityEngine.UI;

namespace ChainViews.Elements
{
    public class StateRouterTargetLabelView : MonoBehaviour
    {
        [Header("Component References")]

        [SerializeField]
        private Text m_TextComponent = null;
        private Text TextComponent { get { return m_TextComponent; } }
        
        [SerializeField]
        private RectTransform m_LineOutputTransform = null;
        public RectTransform LineOutputTransform { get { return m_LineOutputTransform; } }


        private string TargetName { set { TextComponent.text = value; } }

        private ChainNode m_Target;
        public ChainNode Target
        {
            get { return m_Target; }
            set
            {
                m_Target = value;

                TargetName = m_Target.Name;
            }
        }


        public int TargetIndex
        {
            set
            {
                var offset = value * 16.0f;
                
                var rectTransform = GetComponent< RectTransform >();

                rectTransform.offsetMax = new Vector2( 0.0f, -offset );
                rectTransform.offsetMin = new Vector2( 130 + offset, 0.0f );
            }
        }
    }
}
