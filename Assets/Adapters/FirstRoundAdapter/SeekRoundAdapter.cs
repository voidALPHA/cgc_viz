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

namespace Adapters.FirstRoundAdapter
{
    public class SeekRoundAdapter : Adapter
    {
        private MutableField<int> m_SeekRoundNumber = new MutableField<int>() 
        { LiteralValue = -1 };
        [Controllable(LabelText = "Seek Round Number")]
        public MutableField<int> SeekRoundNumber { get { return m_SeekRoundNumber; } }
        
        private SelectionState Default { get { return Router["Default"]; } }

        private MutableTarget m_NextRoundNumber = new MutableTarget() 
        { AbsoluteKey = "Next Round" };
        [Controllable(LabelText = "Next Round Number")]
        public MutableTarget NextRoundNumber { get { return m_NextRoundNumber; } }

        public SeekRoundAdapter()
        {
            NextRoundNumber.SchemaParent = SeekRoundNumber;

            Router.AddSelectionState("Default");
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            NextRoundNumber.SetValue( 1, newSchema );

            Router.TransmitAllSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach ( var entry in SeekRoundNumber.GetEntries( payload.Data ) )
            {
                var seekNum = SeekRoundNumber.GetValue( entry );

                GetNextRoundCommand roundCommand = new GetNextRoundCommand(seekNum);

                var commandIterator = CommandProcessor.Execute( roundCommand );
                while (commandIterator.MoveNext())
                    yield return null;

                NextRoundNumber.SetValue( roundCommand.NextRoundNumber, entry );
            }

            var iterator = Router.TransmitAll(payload);
            while (iterator.MoveNext())
                yield return null;
        }
    }
}
