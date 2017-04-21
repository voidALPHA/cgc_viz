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
using UnityEngine;
using UnityEngine.UI;

namespace Utility.DevCommand
{
    public class DevCommandConsoleOutputBehaviour : MonoBehaviour
    {
        private Text m_OutputText;
        private Text OutputText { get { return m_OutputText; } set { m_OutputText = value; } }

        private Scrollbar m_ScrollBar;
        private Scrollbar ScrollBar { get { return m_ScrollBar; } set { m_ScrollBar = value; } }

        private bool ResetScrollBar = false;
        private int MaxTextLength = 16000;  // 16200 crashes with 'String too long for TextMeshGenerator. Cutting off characters.'
        
        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }
        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private void Start()
        {
            OutputText = GetComponentInChildren<Text>();
            OutputText.text = "\n";
            ScrollBar = transform.parent.GetComponentInChildren<Scrollbar>();
        }

        private void Update()
        {
            if (ResetScrollBar)
            {
                ResetScrollBar = false;

                ScrollBar.value = 0;    // Keep scroll bar at bottom after text output
                ScrollBar.Rebuild(CanvasUpdate.Prelayout);
            }
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            //if (type == Log)  // We could filter on Error, Assert, Warning, Log, Exception
            
            if ( OutputText == null )
                return;

            var newText = OutputText.text += logString;

            if (!logString.EndsWith("\n"))
                newText += "\n";

            while (newText.Length > MaxTextLength)
            {
                var firstCRIndex = newText.IndexOf('\n');
                if (firstCRIndex < 0 || firstCRIndex == newText.Length - 1)
                {
                    newText = string.Empty;
                    break;
                }
                var newLen = ( newText.Length - firstCRIndex - 1 );
                newText = newText.Substring(firstCRIndex + 1, newLen);
            }

            OutputText.text = newText;

            ResetScrollBar = true;
        }
    }
}
