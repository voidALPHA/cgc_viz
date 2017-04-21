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
using Adapters.TraceAdapters.Commands;
using Chains;
using Mutation;
using Visualizers;

namespace Adapters.ChallengeSets
{
    public class BinsetAdapter : Adapter
    {
        private MutableField<int> m_BinsetId = new MutableField<int>() 
        { AbsoluteKey = "Binset Id" };
        [Controllable(LabelText = "BinsetId")]
        public MutableField<int> BinsetId { get { return m_BinsetId; } }

        private MutableTarget m_BinaryStatsTarget = new MutableTarget() 
        { AbsoluteKey = "Binary Stats" };
        [Controllable(LabelText = "Binary Stats Target")]
        public MutableTarget BinaryStatsTarget { get { return m_BinaryStatsTarget; } }

        private SelectionState PerBinsetState { get { return Router[ "Per Binset" ]; } }

        public BinsetAdapter()
        {
            Router.AddSelectionState( "Per Binset" );
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            BinaryStatsTarget.SetValue( CsStatsAdapter.PopulateBinaryStats( new List<BinaryStats>() {CsStatsAdapter.DefaultBinaryStats} ), newSchema );

            Router.TransmitAllSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var binsetId = (uint)BinsetId.GetFirstValue( payload.Data );

            var binsetCommand = new GetBinsetCommand( binsetId );

            var commandIterator = CommandProcessor.Execute( binsetCommand );
            while ( commandIterator.MoveNext() )
                yield return null;

            var binList = new List< BinaryStats >();
            foreach ( var binId in binsetCommand.BinIds )
            {
                var binInfoCommand = new GetBinaryStatsCommand(binId);

                commandIterator = CommandProcessor.Execute(binInfoCommand);
                while (commandIterator.MoveNext())
                    yield return null;

                binList.Add(binInfoCommand.BinStats);
            }

            BinaryStatsTarget.SetValue(CsStatsAdapter.PopulateBinaryStats(
                binList), payload.Data);

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
