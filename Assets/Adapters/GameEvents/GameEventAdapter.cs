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
using System.IO;
using System.Linq;
using Adapters.GameEvents.Model;
using Adapters.TraceAdapters.Commands;
using Mutation;
using UnityEngine;
using Visualizers;

namespace Adapters.GameEvents
{
    public abstract class GameEventAdapterBase : Adapter
    {
        private MutableTarget m_GameEventsTarget = new MutableTarget() { AbsoluteKey = "Game Events" };
        [Controllable( LabelText = "Game Events Target" )]
        public MutableTarget GameEventsTarget { get { return m_GameEventsTarget; } }
        

        protected GameEventContainer EventContainer { get; set; }


        protected GameEventAdapterBase()
        {
            Router.AddSelectionState( "All" );
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            var roundsList = new List< Round >()
            {
                Round.GetTestRound()
            };

            foreach ( var entry in GameEventsTarget.GetEntries( newSchema ) )
            {
                GameEventsTarget.SetValue(EventContainerToMutableObject( roundsList ), entry);
            }

            Router.TransmitAllSchema( newSchema );
        }


        protected abstract IEnumerator PopulateGameEventContainer( VisualPayload payload );


        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var gameEventIterator = PopulateGameEventContainer( payload );
            while ( gameEventIterator.MoveNext() )
                yield return null;

            if ( EventContainer == null )
            {
                Debug.LogError( "Aborting; EventContainer was null." );
                yield break;
            }
            //var eventContainerMutableObject = MutableObject.FromObject( eventContainer );



            var eventContainerMutableObject = EventContainerToMutableObject(EventContainer.Rounds);

            GameEventsTarget.SetValue( eventContainerMutableObject, payload.Data );

            var iterator = Router.TransmitAll( payload);
            while ( iterator.MoveNext() )
                yield return null;
        }

        private MutableObject EventContainerToMutableObject(List<Round> eventRounds)
        {
            var eventContainerMutableObject = new MutableObject();
            var rounds = new List< MutableObject >();
            eventContainerMutableObject.Add( "Rounds", rounds );

            foreach (var round in eventRounds)
            {
                var roundMutable = new MutableObject();
                roundMutable.Add( "RoundIndex", round.RoundIndex );

                var events = new List< MutableObject >();
                roundMutable.Add( "Events", events );

                // DEBUG

                if (round.GameEvents.Polls==null)
                    round.GameEvents.Polls = new List< PollEvent >();

                // END DEBUG


                foreach ( var povSubmissionEvent in round.GameEvents.PovSubmission )
                {
                    var eventMutable = new MutableObject
                    {
                        { "Timestamp", povSubmissionEvent.Timestamp },
                        { "Pov ID", povSubmissionEvent.Povid },
                        { "Source Team", povSubmissionEvent.SourceTeam },
                        { "Target Team", povSubmissionEvent.TargetTeam },
                        { "Target Cs ID", povSubmissionEvent.TargetCsid },
                        { "Target Rcs ID", povSubmissionEvent.TargetBsid },
                        { "Result", EventResult.Succeed },
                        { "TypeString", "PovSubmission" },
                    };

                    events.Add( eventMutable );
                }

                foreach ( var pollEvent in round.GameEvents.Polls )
                {
                    var eventMutable = new MutableObject
                    {
                        { "Timestamp", pollEvent.Timestamp },
                        { "Poll ID", pollEvent.Pollid },
                        { "Target Team", pollEvent.TargetTeam },
                        { "Target Cs ID", pollEvent.TargetCsid },
                        { "Target Bs ID", pollEvent.TargetBsid },
                        { "Result", pollEvent.Result },
                        { "Ids", pollEvent.Nds },  
                        { "Duration", pollEvent.Duration },
                        { "TypeString", "Poll" },
                    };

                    events.Add( eventMutable );
                }

                foreach ( var gameEvent in round.GameEvents.PovThrow )
                {
                    var eventMutable = new MutableObject
                    {
                        { "Timestamp", gameEvent.Timestamp },
                        { "Duration", gameEvent.Duration },
                        { "Pov ID", gameEvent.Povid},
                        { "Source Team", gameEvent.SourceTeam},
                        { "Target Team", gameEvent.TargetTeam},
                        { "Target Cs ID", gameEvent.TargetCsid},
                        { "Target Bs ID", gameEvent.TargetBsid},
                        { "Result", gameEvent.Result},
                        { "Ids Result", gameEvent.Nds },
                        { "Negotiate", gameEvent.Negotiate},
                        { "TypeString", "PovThrow" },
                    };

                    events.Add( eventMutable );
                }


                foreach ( var gameEvent in round.GameEvents.PovNotthrown )
                {
                    var eventMutable = new MutableObject
                    {
                        { "Timestamp", gameEvent.Timestamp },
                        { "Pov ID", gameEvent.Povid},
                        { "Source Team", gameEvent.SourceTeam},
                        { "Target Team", gameEvent.TargetTeam},
                        { "Target Cs ID", gameEvent.TargetCsid},
                        { "TypeString", "PovNotThrown" },
                    };

                    events.Add( eventMutable );
                }

                foreach ( var gameEvent in round.GameEvents.RcsSubmission )
                {
                    var eventMutable = new MutableObject
                    {
                        { "Timestamp", gameEvent.Timestamp },
                        { "Cs ID", gameEvent.Csid},
                        { "Bs ID", gameEvent.Bsid},
                        { "Rcs ID", gameEvent.Rcsid},
                        { "Team", gameEvent.Team},
                        { "TypeString", "RcsSubmission" },
                    };

                    events.Add( eventMutable );
                }

                foreach ( var gameEvent in round.GameEvents.RcsDeployed )
                {
                    var eventMutable = new MutableObject
                    {
                        { "Timestamp", gameEvent.Timestamp },
                        { "Cs ID", gameEvent.Csid},
                        { "Team", gameEvent.Team},
                        { "Bs ID", gameEvent.Bsid},
                        { "TypeString", "RcsDeployed" },
                    };

                    events.Add( eventMutable );
                }

                foreach ( var gameEvent in round.GameEvents.NetworkRuleSubmission )
                {
                    var eventMutable = new MutableObject
                    {
                        { "Timestamp", gameEvent.Timestamp },
                        { "Cs ID", gameEvent.Csid},
                        { "Team", gameEvent.Team},
                        { "Rule ID", gameEvent.Ruleid},
                        { "TypeString", "NetworkRuleSubmission" },
                    };

                    events.Add( eventMutable );
                }

                foreach ( var gameEvent in round.GameEvents.NetworkRuleDeployed )
                {
                    var eventMutable = new MutableObject
                    {
                        { "Timestamp", gameEvent.Timestamp },
                        { "Cs ID", gameEvent.Csid},
                        { "Team", gameEvent.Team},
                        { "Rule ID", gameEvent.Ruleid},
                        { "TypeString", "NetworkRuleDeployed" },
                    };

                    events.Add( eventMutable );
                }

                foreach ( var gameEvent in round.GameEvents.CsOffline )
                {
                    var eventMutable = new MutableObject
                    {
                        { "Timestamp", gameEvent.Timestamp },
                        { "Cs ID", gameEvent.Csid},
                        { "Team", gameEvent.Team},
                        { "Offline Reason", gameEvent.Reason},
                        { "TypeString", "CsOffline" },
                    };

                    events.Add( eventMutable );
                }


                double lowestTimestamp = double.MaxValue;
                foreach ( var roundEvent in events.Where( roundEvent => (double)roundEvent[ "Timestamp" ] < lowestTimestamp &&
                                                                        (double)roundEvent[ "Timestamp" ] > .000001 ) )
                    lowestTimestamp = (double)roundEvent[ "Timestamp" ];


                foreach ( var roundEvent in events )
                    if ( (double)roundEvent[ "Timestamp" ] > .00001 )
                        roundEvent[ "Timestamp" ] = (double)roundEvent[ "Timestamp" ] - lowestTimestamp;

                var csAdded = new List<MutableObject>();
                foreach ( var csAdd in round.PreRoundEvents.CsAdded )
                {
                    var eventMutable = new MutableObject
                    {
                        {"Cs ID", csAdd.Csid }
                    };
                    csAdded.Add( eventMutable );
                }
                roundMutable[ "Cs Added" ] = csAdded;
                
                var csRemoved = new List<MutableObject>();
                foreach (var csRemove in round.PreRoundEvents.CsRemoved)
                {
                    var eventMutable = new MutableObject
                    {
                        {"Cs ID", csRemove.Csid }
                    };
                    csRemoved.Add(eventMutable);
                }
                roundMutable["Cs Removed"] = csRemoved;


                rounds.Add( roundMutable );
            }
            return eventContainerMutableObject;
        }
    }


    //public class GameEventFromFileAdapter : GameEventAdapterBase
    //{
    //    private MutableField<string> m_FilenameField = new MutableField<string> { LiteralValue = "48.txt" };
    //    [Controllable( LabelText = "Filename" )]
    //    private MutableField<string> FilenameField { get { return m_FilenameField; } }


    //    protected override IEnumerator PopulateGameEventContainer( VisualPayload payload )
    //    {
    //        var fullRoot = Path.Combine( Application.dataPath, "../EventData" );
    //        var fullPath = Path.Combine( fullRoot, FilenameField.GetFirstValue( payload.Data ) );

    //        var json = File.ReadAllText( fullPath );

    //        EventContainer = GameEventContainer.Deserialize( json );

    //        yield break;
    //    }
    //}


    public class GameEventFromTraceApiAdapter : GameEventAdapterBase
    {
        private MutableField< int > m_RoundIndexField = new MutableField< int > { LiteralValue = 1 };
        [Controllable( LabelText = "RoundIndex" )]
        private MutableField< int > RoundIndexField
        {
            get { return m_RoundIndexField; }
        }

        private MutableField<bool> m_SpoofData = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "Spoof Data")]
        public MutableField<bool> SpoofData { get { return m_SpoofData; } }


        protected override IEnumerator PopulateGameEventContainer( VisualPayload payload )
        {
            var spoofData = SpoofData.GetFirstValue( payload.Data );

            var roundNumber = RoundIndexField.GetFirstValue( payload.Data );

            var command = new GetEventsForRoundCommand( roundNumber );

            var iterator = CommandProcessor.Execute( command );
            while ( iterator.MoveNext() )
                yield return null;

            EventContainer = command.EventContainer;
        }
    }
}
