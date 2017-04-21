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
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.PayloadSelection.UI
{
    public class AddCriterionStatementButtonBehaviour : MonoBehaviour
    {
        public event Action Clicked = delegate { };


        [Header("Component References")]

        //[SerializeField]
        //private Button m_Button = null;
        //private Button Button { get { return m_Button; } }

        [SerializeField]
        private Text m_SymbolTextComponent = null;
        private Text SymbolTextComponent { get { return m_SymbolTextComponent; } }


        private string m_Symbol = "?";

        public string Symbol
        {
            get
            {
                return m_Symbol;
            }
            set
            {
                m_Symbol = value;

                SymbolTextComponent.text = Symbol;
            }
        }


        [UsedImplicitly]
        public void HandleButtonClicked()
        {
            Clicked();
        }
    }
}
