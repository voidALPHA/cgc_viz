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
using Ui.TypePicker;
using UnityEngine;
using Visualizers;

namespace Choreography.Steps
{
    [TypePickerIgnore]
    public class BoolTestStep : Step
    {
        private bool m_TestBool;
        [Controllable]
        private bool TestBool
        {
            get
            {
                return m_TestBool;
            }
            set
            {
                m_TestBool = value;

                Debug.Log( "TestBool set to " + m_TestBool );
            }
        }

        private ControllableCondition m_Condition = new ControllableCondition( false );
        [Controllable]
        private ControllableCondition Condition { get { return m_Condition; } set { m_Condition = value; } }

        private string m_Sometimes = "This string here...";
        [Controllable(ConditionalPropertyName="Condition")]
        private string Sometimes { get { return m_Sometimes; } set { m_Sometimes = value; } }

        protected override IEnumerator ExecuteStep()
        {
            Debug.Log( "TestBool step executing, set to " + TestBool );

            yield break;
        }
    }
}
