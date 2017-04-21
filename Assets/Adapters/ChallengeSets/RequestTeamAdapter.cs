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
using Adapters.TraceAdapters.Commands;
using Adapters.TraceAdapters.Traces;
using JetBrains.Annotations;
using Mutation;
using Visualizers;

namespace Adapters.ChallengeSets
{
    [UsedImplicitly]
    public class RequestTeamAdapter : Adapter
    {
        private MutableField< RequestNature > m_RequestNatureField = new MutableField< RequestNature > { LiteralValue = TraceAdapters.Traces.RequestNature.Pov };
        [Controllable( LabelText = "Request Nature" )]
        private MutableField< RequestNature > RequestNatureField { get { return m_RequestNatureField; } }

        private MutableField< int > m_RequestIdField = new MutableField< int > { LiteralValue = 1 };
        [Controllable( LabelText = "Request Id" )]
        private MutableField< int > RequestIdField { get { return m_RequestIdField; } }


        private MutableTarget m_TeamTarget = new MutableTarget() { AbsoluteKey = "Request Team" };
        [Controllable( LabelText = "Pov Info Target" )]
        private MutableTarget TeamTarget { get { return m_TeamTarget; } }

        private MutableField<bool> m_SpoofData = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "Spoof Data")]
        public MutableField<bool> SpoofData { get { return m_SpoofData; } }


        public RequestTeamAdapter()
        {
            Router.AddSelectionState( "Default" );
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            //var info = new MutableObject {
            //    {"Team", 0}
            //};

            TeamTarget.SetValue( 0, newSchema );

            base.OnProcessOutputSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            if ( !SpoofData.GetFirstValue( payload.Data ) )
            {

                var requestNature = RequestNatureField.GetFirstValue( payload.Data );
                var requestId = RequestIdField.GetFirstValue( payload.Data );

                var command = new GetPovInfoCommand( requestId );
                var team = 0;

                if ( requestNature == RequestNature.Pov && requestId>0 )
                {
                    var commandIterator = CommandProcessor.Execute( command );
                    while ( commandIterator.MoveNext() )
                        yield return null;

                    team = command.TeamId;
                }

                //var info = new MutableObject();
                //
                //if ( requestNature == RequestNature.Pov )
                //{
                //    var commandIterator = CommandProcessor.Execute( command );
                //    while ( commandIterator.MoveNext() )
                //        yield return null;

                //    info[ "Team" ] = command.Team;
                //    info[ "CsId" ] = command.CsId;
                //    info[ "File" ] = command.File;
                //    info[ "Submissions" ] = new List< MutableObject > {
                //        new MutableObject {
                //            { "Id", command.Submissions[0].Id },
                //            { "Round", command.Submissions[0].Round },
                //            { "Target", command.Submissions[0].Target },
                //            { "ThrowCount", command.Submissions[0].ThrowCount },
                //        }
                //    };
                //}
                //else
                //{
                //    // These are so incorrect, I have simplified this entire class to just return the requesting team id.
                //
                //    info["Team"] = 8;
                //    info["CsId"] = 0;
                //    info["File"] = "";
                //    info["Submissions"] = new List<MutableObject> {
                //        new MutableObject {
                //            { "Id", 0 },
                //            { "Round", 0 },
                //            { "Target", 0 },
                //            { "ThrowCount", 0 },
                //        }
                //    };
                //}

                //var info = new MutableObject {
                //    {"Team", team}
                //};

                TeamTarget.SetValue( team, payload.Data );
            }
            else
            {
                TeamTarget.SetValue( 3, payload.Data );
            }

            var routerIterator = Router.TransmitAll( payload );
            while ( routerIterator.MoveNext() )
                yield return null;
        }
    }
}