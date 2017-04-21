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
using Mutation;
using Mutation.Mutators;
using UnityEngine;

namespace Visualizers.CsView
{
    public class StringToBitstreamMutator : Mutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<bool> m_RepeatByBytes = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "RepeatBytes")]
        public MutableField<bool> RepeatByBytes { get { return m_RepeatByBytes; } }

        private MutableField<string> m_StringInput = new MutableField<string>() 
        { LiteralValue = "TestString" };
        [Controllable(LabelText = "Input String")]
        public MutableField<string> StringInput { get { return m_StringInput; } }

        private MutableTarget m_BitStreamTarget = new MutableTarget() 
        { AbsoluteKey = "Bitstream" };
        [Controllable(LabelText = "Bitstream Target")]
        public MutableTarget BitStreamTarget { get { return m_BitStreamTarget; } }

        public StringToBitstreamMutator()
        {
            RepeatByBytes.SchemaParent = Scope;
            StringInput.SchemaParent = Scope;
            BitStreamTarget.SchemaParent = Scope;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            foreach (var entr in Scope.GetEntries( newSchema ))
                BitStreamTarget.SetValue( new InternalArrayBitstream( new List< bool >() ), entr );

            Router.TransmitAllSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach ( var entry in Scope.GetEntries( payload.Data ) )
            {
                try
                {
                    var repeatBytes = RepeatByBytes.GetValue( entry );
                    var stringData = StringInput.GetValue( entry );


                    var bitStream = InternalArrayBitstream.GenerateBitStreamFromLetterNumbers( stringData );
                    bitStream.AdvanceByBytes = repeatBytes;

                    BitStreamTarget.SetValue( bitStream, entry );
                }
                catch
                {
                    Debug.LogError( "Bitstream cannot be created!" );
                    var stringData2 = StringInput.GetValue(entry);
                }
            }
            
            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
