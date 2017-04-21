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
using Utility;

namespace Environmental.RoundLabel
{
    public class RoundLabelBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_EnableRoot = null;
        private GameObject EnableRoot { get { return m_EnableRoot; } }
        
        [SerializeField]
        private Text m_TextComponent = null;
        private Text TextComponent { get { return m_TextComponent; } }
        
        [SerializeField]
        private bool m_DisabledByAbnormal = false;
        public bool DisabledByAbnormal
        {
            get
            {
                return m_DisabledByAbnormal;
            }
            set
            {
                if ( m_DisabledByAbnormal == value )
                    return;
                m_DisabledByAbnormal = value;
                EnableRoot.SetActive(!m_DisabledByAbnormal);
            }
        }

        [UsedImplicitly]
        private void Start()
        {
            UpdateRoundLabel();
        }

        public static void UpdateAllRoundLabels()
        {
            foreach (var roundLabel in GameObject.FindObjectsOfType<RoundLabelBehaviour>())
                roundLabel.UpdateRoundLabel();
        }

        private void UpdateRoundLabel()
        {

            if (DisabledByAbnormal)
            {
                EnableRoot.SetActive(false);
                return;
            }

            var text = string.Empty;

            if (CommandLineArgs.IsPresent("useRoundAsRoundLabel"))
                text = CommandLineArgs.GetArgumentValue("round");

            if (string.IsNullOrEmpty(text))
                text = CommandLineArgs.GetArgumentValue("roundLabel");

            TextComponent.text = text ?? string.Empty;
            
            EnableRoot.SetActive(!string.IsNullOrEmpty(text));
        }
    }
}