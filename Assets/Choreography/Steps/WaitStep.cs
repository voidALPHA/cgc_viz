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
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencers;
using Visualizers;

namespace Choreography.Steps
{
    [UsedImplicitly]
    public class WaitStep : Step
    {
        private static Dictionary<string, List<Action>> s_WaitingTransmits = new Dictionary< string, List<Action>>();
        public static Dictionary<string, List<Action>> WaitingTransmits { get {return s_WaitingTransmits;} set
            {
            s_WaitingTransmits = value;
        } }

        private const string WaitEventName = "Unwait";

        private string m_GroupId = "";
        [Controllable(LabelText = "Group ID to Wait for")]
        public string GroupId
        {
            get
            {
                return m_GroupId;
            }
            set { m_GroupId = value; }
        }

        public bool AwaitingConclusion { get; set; }
        

        public WaitStep()
        {
            Router.AddEvent(WaitEventName);
        }

        public static void UnwaitGroupId( string groupId )
        {
            if (!WaitingTransmits.ContainsKey( groupId ))
                return;

            var incoming = WaitingTransmits[ groupId ];
            foreach ( var act in incoming )
                act();

            WaitingTransmits.Remove( groupId );
        }

        protected override IEnumerator ExecuteStep()
        {
            AwaitingConclusion = true;

            if ( !WaitingTransmits.ContainsKey( GroupId ) )
                WaitingTransmits[ GroupId ] = new List< Action >();

            WaitingTransmits[GroupId].Add( ()=>AwaitingConclusion=false);
            
            while (AwaitingConclusion)
                yield return null;

            Router.FireEvent( WaitEventName );
        }

        protected override void OnTimelineStopped()
        {
            WaitingTransmits.Clear();
        }
    }
}
