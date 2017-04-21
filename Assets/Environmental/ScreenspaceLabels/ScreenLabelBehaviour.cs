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

namespace Environmental.ScreenspaceLabels
{
    public class ScreenLabelBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_EnableRoot = null;
        private GameObject EnableRoot { get { return m_EnableRoot; } }
        
        [SerializeField]
        private Text m_TextComponent = null;
        private Text TextComponent { get { return m_TextComponent; } }

        [SerializeField]
        private string m_ArgumentName = "TopLabel";
        private string ArgumentName { get { return m_ArgumentName; } }
        
        [UsedImplicitly]
        private void Start()
        {
            var text = string.Empty;

            if (CommandLineArgs.IsPresent(ArgumentName))
                text = CommandLineArgs.GetArgumentValue(ArgumentName);
            
            TextComponent.text = text ?? string.Empty;

            if (string.IsNullOrEmpty(text))
                EnableRoot.SetActive(false);
        }
    }
}
