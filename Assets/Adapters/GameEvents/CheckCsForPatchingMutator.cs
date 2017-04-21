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
using System.Runtime.InteropServices;
using Adapters.GameEvents.Model;
using Mutation;
using Mutation.Mutators;
using Visualizers;

namespace Adapters.GameEvents
{
    public class CheckCsForPatchingMutator : Mutator
    {
        private MutableScope m_ScoreEntries = new MutableScope() {AbsoluteKey = "Scores"};
        [Controllable(LabelText = "Scores Scope")]
        public MutableScope ScoreEntries { get { return m_ScoreEntries; } }

        private MutableField<int> m_ScoreTeamId = new MutableField<int>() 
        { AbsoluteKey = "Scores.Team Id" };
        [Controllable(LabelText = "Score Team Id")]
        public MutableField<int> ScoreTeamId { get { return m_ScoreTeamId; } }

        private MutableField<int> m_ScoreCsId = new MutableField<int>() 
        { AbsoluteKey= "Scores.Challenge Set Id" };
        [Controllable(LabelText = "Score Cs Id")]
        public MutableField<int> ScoreCsId { get { return m_ScoreCsId; } }

        private MutableTarget m_RcsPatchingTarget = new MutableTarget() 
        { AbsoluteKey = "Scores.RcsPatching" };
        [Controllable(LabelText = "Rcs Patching Target")]
        public MutableTarget RcsPatchingTarget { get { return m_RcsPatchingTarget; } }

        private MutableTarget m_NdsPatchingTarget = new MutableTarget() 
        { AbsoluteKey = "Scores.NdsPatching" };
        [Controllable(LabelText = "Nds Patching Target")]
        public MutableTarget NdsPatchingTarget { get { return m_NdsPatchingTarget; } }


        private MutableScope m_CsOfflineScope = new MutableScope() {AbsoluteKey = "CsOfflines"};
        [Controllable(LabelText = "CsOffline Scope")]
        public MutableScope CsOfflineScope { get { return m_CsOfflineScope; } }

        private MutableField<CsOfflineReason> m_OfflineReason = new MutableField<CsOfflineReason>() 
        { AbsoluteKey = "CsOfflines.OfflineReason" };
        [Controllable(LabelText = "Offline Reason")]
        public MutableField<CsOfflineReason> OfflineReason { get { return m_OfflineReason; } }

        private MutableField< int > m_OfflineTeamId = new MutableField< int >()
        { AbsoluteKey = "CsOfflines.Team Id" };
        [Controllable(LabelText = "Offline Team Id")]
        public MutableField<int> OfflineTeamId { get { return m_OfflineTeamId; } }
        
        private MutableField<int> m_OfflineCsId = new MutableField<int>() 
        { AbsoluteKey= "CsOfflines.Cs Id" };
        [Controllable(LabelText = "Offline Cs Id")]
        public MutableField<int> OfflineCsId { get { return m_OfflineCsId; } }

        public CheckCsForPatchingMutator()
        {
            ScoreTeamId.SchemaParent = ScoreEntries;
            ScoreCsId.SchemaParent = ScoreEntries;
            RcsPatchingTarget.SchemaParent = ScoreEntries;
            NdsPatchingTarget.SchemaParent = ScoreEntries;

            OfflineReason.SchemaParent = CsOfflineScope;
            OfflineTeamId.SchemaParent = CsOfflineScope;
            OfflineCsId.SchemaParent = CsOfflineScope;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            foreach ( var entry in ScoreEntries.GetEntries( newSchema ) )
            {
                RcsPatchingTarget.SetValue( true, entry );
                NdsPatchingTarget.SetValue( false, entry );
            }

            base.OnProcessOutputSchema( newSchema );
        }

        private class IntPair
        {
            public IntPair( int csId, int teamId )
            {
                CsId = csId;
                TeamId = teamId;
            }

            public int CsId { get; set; }
            public int TeamId { get; set; }

            public override bool Equals( object obj )
            {
                if (!(obj is IntPair))
                    return false;

                var otherObj = obj as IntPair;
                return otherObj.CsId==CsId && otherObj.TeamId==TeamId;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ( CsId * 397 ) ^ TeamId;
                }
            }
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            // construct offlines dictionary
            var offlines = new Dictionary<IntPair, CsOfflineReason>();
            foreach ( var entry in CsOfflineScope.GetEntries( payload.Data ) )
            {
                var teamId = OfflineTeamId.GetValue( entry );
                var csId = OfflineCsId.GetValue( entry );
                var offlineReason = OfflineReason.GetValue( entry );

                offlines[ new IntPair( csId, teamId ) ] = offlineReason;
            }

            // check each score entry against the offlines dictionary
            foreach ( var entry in ScoreEntries.GetEntries( payload.Data ) )
            {
                var teamId = ScoreTeamId.GetValue( entry );
                var csId = ScoreCsId.GetValue( entry );
                var idPair = new IntPair( csId, teamId );

                bool patchingRcs = false;
                bool patchingNds = false;
                if ( offlines.ContainsKey( idPair ) )
                {
                    var reason = offlines[ idPair ];
                    patchingRcs = reason == CsOfflineReason.Rcs || reason == CsOfflineReason.Both;
                    patchingNds = reason == CsOfflineReason.Ids || reason == CsOfflineReason.Both;
                }
                RcsPatchingTarget.SetValue(patchingRcs, entry);
                NdsPatchingTarget.SetValue(patchingNds, entry);
            }

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
