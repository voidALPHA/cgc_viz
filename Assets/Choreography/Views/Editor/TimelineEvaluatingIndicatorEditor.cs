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

using Choreography.Views;
using UnityEditor;
using UnityEngine;

namespace Choreography.Views.Editor
{
    [CustomEditor( typeof( TimelineIndicatorViewBehaviour ) )]
    public class TimelineEvaluatingIndicatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();

            if ( GUILayout.Button( "Turn On" ) )
            {
                ( target as TimelineIndicatorViewBehaviour ).IsTurnedOn = true;
                EditorUtility.SetDirty( target );
            }

            if ( GUILayout.Button( "Turn Off" ) )
            {
                ( target as TimelineIndicatorViewBehaviour ).IsTurnedOn = false;
                EditorUtility.SetDirty( target );
            }

            GUILayout.EndHorizontal();

            base.OnInspectorGUI();
        }
    }


}
