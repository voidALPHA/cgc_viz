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
using System.Collections.Generic;
using System.Linq;
using Adapters.TraceAdapters.Commands;
using Chains;
using Mutation;
using Visualizers;

namespace Adapters.ChallengeSets
{
    public class CsInfoAdapter : Adapter
    {
        private MutableField<int> m_CsIndex = new MutableField<int>()
        { LiteralValue = 10 };
        [Controllable(LabelText = "CsIndex")]
        public MutableField<int> CsIndex { get { return m_CsIndex; } }

        private MutableTarget m_CsInfoTarget = new MutableTarget() 
        { AbsoluteKey = "Cs Info" };
        [Controllable(LabelText = "Cs Info Target")]
        public MutableTarget CsInfoTarget { get { return m_CsInfoTarget; } }

        private SelectionState DefaultState { get { return Router[ "Default" ]; } }

        public CsInfoAdapter()
        {
            CsInfoTarget.SchemaParent = CsInfoTarget;

            Router.AddSelectionState( "Default" );
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            CsInfoTarget.SetValue( CsStatsAdapter.PopulateCsStats(DefaultChallengeSetInfo), newSchema);

            Router.TransmitAllSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach ( var entry in CsIndex.GetEntries( payload.Data ) )
            {
                ChallengeSetInfo csInfo;

                if ( CsIndex.GetValue( entry ) > 0 )
                {
                    var csIndex = (uint)CsIndex.GetValue( entry );

                    var csInfoCommand = new GetCsInfoCommand( csIndex );
                    var commandIterator = CommandProcessor.Execute( csInfoCommand );
                    while ( commandIterator.MoveNext() )
                        yield return null;

                    csInfo = csInfoCommand.CsInfo;
                }
                else
                {
                    csInfo = DefaultChallengeSetInfo;
                }



                CsInfoTarget.SetValue( CsStatsAdapter.PopulateCsStats( csInfo ), entry);
            }
            var iterator = Router.TransmitAll(payload);
            while (iterator.MoveNext())
                yield return null;
        }

        /*private MutableObject PopulateCsInfo(ChallengeSetInfo csInfo)
        {
            var stats = new MutableObject();

            stats["Lines of Code"] = csInfo.LinesOfCode;

            stats["Binary ID"] = csInfo.Binary;
            
            stats[ "CWEs" ] = csInfo.CWEs == null ? new List< MutableObject >(): csInfo.CWEs.Select(cwe => new MutableObject { { "CWE Name", cwe } }).ToList();

            stats[ "Tags" ] = csInfo.Tags == null
                ? new List< MutableObject >()
                : csInfo.Tags.Select( tag => new MutableObject() { { "Tag", tag } } ).ToList();

            stats["Cs Name"] = csInfo.Name ?? "DEFAULT NAME";

            stats[ "Shortname" ] = csInfo.Shortname ?? "DEFAULT SHORTNAME";

            stats[ "Readme" ] = csInfo.Readme??"DEFAULT README";

            stats["Visual Position"] = csInfo.Position;

            

            return stats;
        }*/

        private ChallengeSetInfo DefaultChallengeSetInfo
        {
            get
            {
                var newInfo = new ChallengeSetInfo();
                newInfo.Readme = "README";
                newInfo.Shortname = "Unknown";
                newInfo.Position = 12;
                newInfo.Files = new List< string > {"abcdefghi"};
                newInfo.LinesOfCode = 144;
                newInfo.Binary = 13;
                newInfo.Name = "Challenge Set";

                newInfo.CWEs = new List< string > {"CWE-120"};

                newInfo.Tags = new List< string > { "Snack Pack" };

                return newInfo;
            }
        }

    }
}
