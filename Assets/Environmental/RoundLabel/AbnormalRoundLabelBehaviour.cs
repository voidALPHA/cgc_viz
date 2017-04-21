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
    public class AbnormalRoundLabelBehaviour : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_EnableRoot = null;
        private GameObject EnableRoot { get { return m_EnableRoot; } }
        
        [SerializeField]
        private Text m_TextComponent = null;
        private Text TextComponent { get { return m_TextComponent; } }
        
        [SerializeField]
        private string m_UseLabelString = "BottomRight";
        private string UseLabelString { get { return m_UseLabelString; } }

        [UsedImplicitly]
        private void Start()
        {
            if ( !CommandLineArgs.IsPresent( UseLabelString ) )
            {
                EnableRoot.SetActive( false );
                return;
            }

            foreach ( var roundLabel in GameObject.FindObjectsOfType< RoundLabelBehaviour >() )
                roundLabel.DisabledByAbnormal = true;

            var text = string.Empty;

            if ( CommandLineArgs.IsPresent( "useRoundAsRoundLabel" ) )
                text = CommandLineArgs.GetArgumentValue( "round" );

            if ( string.IsNullOrEmpty( text ) )
                text = CommandLineArgs.GetArgumentValue( "roundLabel" );

            TextComponent.text = text ?? string.Empty;

            if ( string.IsNullOrEmpty( text ) )
                EnableRoot.SetActive( false );
        }
    }
}