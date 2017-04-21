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

namespace Visualizers.CsView
{
    public class RandomlyInterspersedBitstream : ICsBitStream
    {
        private System.Random ByteSwitcher { get; set; }
        

        private ICsBitStream BitStream1 { get; set; }
        private ICsBitStream BitStream2 { get; set; }

        private float SecondStreamProportion { get; set; }

        public bool AdvanceByBytes { get; set; }

        public RandomlyInterspersedBitstream( ICsBitStream bitStream1, ICsBitStream bitStream2, float secondStreamProportion, int seed=1745 )
        {
            BitStream1 = bitStream1;
            BitStream2 = bitStream2;
            SecondStreamProportion = secondStreamProportion;

            ByteSwitcher = new System.Random(seed);
        }

        public bool ReadSequentialBit()
        {
            return ByteSwitcher.NextDouble() < SecondStreamProportion 
                ? BitStream2.ReadSequentialBit() 
                : BitStream1.ReadSequentialBit();
        }

        public IEnumerable< bool > ReadBits( int nBits )
        {
            for (int i = 0; i < nBits; i++)
            {
                if ( ByteSwitcher.NextDouble() < SecondStreamProportion )
                    yield return BitStream2.ReadSequentialBit();
                else
                    yield return BitStream1.ReadSequentialBit();
            }
            BitStream1.ToNextByte();
            BitStream2.ToNextByte();
        }

        public void AdvanceBitPointer( int positions )
        {
            for ( int i = 0; i < positions; i++ )
            {
                if (ByteSwitcher.NextDouble() < SecondStreamProportion)
                    BitStream2.AdvanceBitPointer(1);
                else
                    BitStream1.AdvanceBitPointer(1);
            }
            BitStream1.ToNextByte();
            BitStream2.ToNextByte();
        }

        public void ToNextByte()
        {
            BitStream1.ToNextByte();
            BitStream2.ToNextByte();
        }
    }

    public class CombineBitstreamsMutator : Mutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<ICsBitStream> m_FirstStream = new MutableField<ICsBitStream>() 
        { AbsoluteKey = "Bitstream 1" };
        [Controllable(LabelText = "First Stream")]
        public MutableField<ICsBitStream> FirstStream { get { return m_FirstStream; } }

        private MutableField<ICsBitStream> m_SecondStream = new MutableField<ICsBitStream>() 
        { AbsoluteKey = "Bitstream 2" };
        [Controllable(LabelText = "Second Stream")]
        public MutableField<ICsBitStream> SecondStream { get { return m_SecondStream; } }

        private MutableField<float> m_SecondStreamProportion = new MutableField<float>()
        { LiteralValue = .5f };
        [Controllable(LabelText = "Second Stream Proportion")]
        public MutableField<float> SecondStreamProportion { get { return m_SecondStreamProportion; } }

        private MutableField<int> m_RandomSeed = new MutableField<int>() 
        { LiteralValue = 1337 };
        [Controllable(LabelText = "Random Seed")]
        public MutableField<int> RandomSeed { get { return m_RandomSeed; } }
        

        private MutableField<bool> m_AdvanceByBytes = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "Advance Bitstream By Bytes")]
        public MutableField<bool> AdvanceByBytes { get { return m_AdvanceByBytes; } }
        
        private MutableTarget m_NewBitstreamTarget = new MutableTarget() 
        { AbsoluteKey = "New Bitstream" };
        [Controllable(LabelText = "Bitstream Target")]
        public MutableTarget NewBitstreamTarget { get { return m_NewBitstreamTarget; } }

        public CombineBitstreamsMutator()
        {
            FirstStream.SchemaPattern = Scope;
            SecondStream.SchemaPattern = Scope;
            RandomSeed.SchemaPattern = Scope;
            SecondStreamProportion.SchemaPattern = Scope;
            AdvanceByBytes.SchemaPattern = Scope;
            NewBitstreamTarget.SchemaParent = Scope;
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach ( var entry in Scope.GetEntries( payload.Data ) )
            {
                var bitstream1 = FirstStream.GetValue( entry );
                var bitstream2 = SecondStream.GetValue( entry );

                var secondStreamProportion = SecondStreamProportion.GetValue( entry );
                var randomSeed = RandomSeed.GetValue( entry );

                var combinedStream = new RandomlyInterspersedBitstream( bitstream1,
                    bitstream2, secondStreamProportion, randomSeed);

                combinedStream.AdvanceByBytes = AdvanceByBytes.GetValue( entry );

                NewBitstreamTarget.SetValue( combinedStream, entry );
            }

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            NewBitstreamTarget.SetValue( InternalArrayBitstream.GenerateBitStreamFromLetterNumbers( "Test" ), newSchema );

            Router.TransmitAllSchema( newSchema );
        }
    }
}
