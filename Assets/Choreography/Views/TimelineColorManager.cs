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

using System.Collections.Generic;
using Choreography.Steps;
using UnityEngine;

namespace Choreography.Views
{
    public class TimelineColorManager : MonoBehaviour
    {
        private static TimelineColorManager s_Instance;
        private static TimelineColorManager Instance
        {
            get { return s_Instance ?? ( s_Instance = FindObjectOfType< TimelineColorManager >() ); }
        }




        [SerializeField]
        private List< Color > m_StepColors = new List< Color >();
        private List< Color > StepColors { get { return m_StepColors; } }

        [SerializeField]
        private List< Color > m_EventColors = new List< Color >();
        private List< Color > EventColors { get { return m_EventColors; } }
        

        private static readonly Dictionary< Step, Color > m_StepsToColors = new Dictionary< Step, Color >();
        private static Dictionary< Step, Color > StepsToColors { get { return m_StepsToColors; } }

        private static readonly Dictionary< StepEvent, Color > m_EventsToColors = new Dictionary< StepEvent, Color >();
        private static Dictionary< StepEvent, Color > EventsToColors { get { return m_EventsToColors; } }


        private static int StepColorsUsed { get; set; }

        private static int EventColorsUsed { get; set; }


        public static Color GetStepColor( Step step )
        {
            if ( StepsToColors.ContainsKey( step ) )
                return StepsToColors[ step ];

            var color = Instance.StepColors[ StepColorsUsed++ % Instance.StepColors.Count ];

            StepsToColors.Add( step, color );

            return color;
        }

        public static Color GetEventColor( StepEvent @event )
        {
            if ( EventsToColors.ContainsKey( @event ) )
                return EventsToColors[ @event ];

            var color = Instance.EventColors[EventColorsUsed++ % Instance.EventColors.Count];

            EventsToColors.Add( @event, color );

            return color;
        }
    }
}

/*

        private class ColorPair
        {
            private Color Color { get; set; }
            private Object Object { get; set; }
        }
 * 
 *      private readonly List< ColorPair > m_StepsToColors = new List< ColorPair >();
        private List<ColorPair> StepsToColors { get {  return m_StepsToColors; } }

        private readonly List< ColorPair > m_EventsToColors = new List< ColorPair >();
        private List< ColorPair > EventsToColors { get { return m_EventsToColors; } }

 * 
*/