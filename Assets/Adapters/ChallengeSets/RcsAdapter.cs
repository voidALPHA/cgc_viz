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
using Chains;
using Mutation;
using Visualizers;

namespace Adapters.ChallengeSets
{
    public class RcsAdapter : Adapter
    {
        private MutableField<int> m_RcsId = new MutableField<int>() 
        { LiteralValue = 8 };
        [Controllable(LabelText = "Rcs Id")]
        public MutableField<int> RcsId { get { return m_RcsId; } }
        
        private MutableTarget m_RcsDataTarget = new MutableTarget() 
        { AbsoluteKey = "Rcs Data" };
        [Controllable(LabelText = "Rcs Data Target")]
        public MutableTarget RcsDataTarget { get { return m_RcsDataTarget; } }

        private SelectionState Default { get { return Router[ "Default" ]; } }

        public RcsAdapter()
        {
            Router.AddSelectionState( "Default" );
        }

        private MutableObject ConstructRcsMutable( RcsToCsInfo rcsInfo )
        {
            var mutable = new MutableObject();

            mutable.Add( "Team", (int)rcsInfo.Team );
            mutable.Add( "Cs ID", (int)rcsInfo.CsId);
            mutable.Add( "Round", (int)rcsInfo.Round );
            mutable.Add( "Bs Id", (int)rcsInfo.BsId );

            return mutable;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            RcsDataTarget.SetValue( ConstructRcsMutable( new RcsToCsInfo() ), newSchema );

            base.OnProcessOutputSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            GetRcsToCsCommand rcsCommand = new GetRcsToCsCommand( 
                (uint)RcsId.GetFirstValue( payload.Data ));


            var iterator = CommandProcessor.Execute( rcsCommand );
            while ( iterator.MoveNext() )
                yield return null;

            RcsDataTarget.SetValue( ConstructRcsMutable( rcsCommand.RcsInfo ), payload.Data );

            iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
