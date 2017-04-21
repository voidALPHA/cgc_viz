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
using System.Linq;
using Adapters.TraceAdapters.Commands;
using JetBrains.Annotations;
using Mutation;
using Visualizers;

namespace Adapters.ChallengeSets
{
    [UsedImplicitly]
    public class BinsetInfoAdapter : Adapter
    {
        private MutableField<int> m_BinsetIdField = new MutableField<int> { LiteralValue = 1 };
        [Controllable( LabelText = "Binset Id" )]
        private MutableField<int> BinsetIdField { get { return m_BinsetIdField; } }


        private MutableTarget m_InfoTarget = new MutableTarget { AbsoluteKey = "Binset Info" };
        [Controllable( LabelText = "Binset Info Target" )]
        private MutableTarget InfoTarget { get { return m_InfoTarget; } }

        private MutableField<bool> m_SpoofData = new MutableField<bool> { LiteralValue = false };
        [Controllable(LabelText = "Spoof Data")]
        public MutableField<bool> SpoofData { get { return m_SpoofData; } }


        public BinsetInfoAdapter()
        {
            Router.AddSelectionState( "Default" );
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            var mut = new MutableObject
            {
                {"Teams", new List< int >{ 0 } },
                {"CSID", 0}
            };

            InfoTarget.SetValue( mut, newSchema );

            base.OnProcessOutputSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            if ( !SpoofData.GetFirstValue( payload.Data ) )
            {

                var binsetId = BinsetIdField.GetFirstValue( payload.Data );

                int foundCsId;
                List<int> teams;

                if ( binsetId > 0 )
                {

                    var command = new GetBinsetInfoCommand( binsetId );

                    var commandIterator = CommandProcessor.Execute( command );
                    while ( commandIterator.MoveNext() )
                        yield return null;


                    switch ( command.Type )
                    {
                        case BinsetType.RefPatch:
                            teams = new List< int > { 8 };
                            break;
                        case BinsetType.Ref:
                            teams = new List< int > { 1, 2, 3, 4, 5, 6, 7, 8 };
                            break;
                        case BinsetType.Rcs:
                            teams = command.Submissions.Select( s => s.Team ).Distinct().OrderBy( v => v ).ToList();
                            break;
                        default:
                            throw new Exception( "Unknown binset type!" );
                    }

                    foundCsId = command.CsId;
                }
                else
                {
                    teams = new List<int> { 8 };
                    foundCsId = -1;
                }

                var mut = new MutableObject
                {
                    {"Teams", teams },
                    {"CSID", foundCsId }
                };

                InfoTarget.SetValue( mut, payload.Data );
            }
            else
            {
                InfoTarget.SetValue( new List<int> {8}, payload.Data );
            }

            var routerIterator = Router.TransmitAll( payload );
            while ( routerIterator.MoveNext() )
                yield return null;
        }
    }
}