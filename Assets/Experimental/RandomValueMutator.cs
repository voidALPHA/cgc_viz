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
using Mutation;
using Mutation.Mutators;
using Visualizers;

namespace Experimental
{
    public class RandomValueMutator : DataMutator
    {
        private MutableField<float> m_RandMin = new MutableField<float>() 
        { LiteralValue = 0f };
        [Controllable(LabelText = "Random Minimum")]
        public MutableField<float> RandMin { get { return m_RandMin; } }

        private MutableField<float> m_RandMax = new MutableField<float>() 
        { LiteralValue = 1f };
        [Controllable(LabelText = "Random Maximum")]
        public MutableField<float> RandMax { get { return m_RandMax; } }


        private MutableTarget m_RandomTarget = new MutableTarget() 
        { AbsoluteKey = "Random Value" };
        [Controllable(LabelText = "Random Target")]
        public MutableTarget RandomTarget { get { return m_RandomTarget; } }

        private MutableField<int> m_Seed = new MutableField<int>() 
        { LiteralValue = 1337 };
        [Controllable(LabelText = "Seed")]
        public MutableField<int> Seed { get { return m_Seed; } }

        private static Random _rand;

        protected override MutableObject Mutate( MutableObject mutable )
        {
            if (_rand==null)
                _rand = new Random(Seed.GetFirstValue(mutable));

            foreach ( var entry in RandomTarget.GetEntries( mutable ) )
            {
                var max = RandMax.GetFirstValue(mutable);
                var min = RandMin.GetFirstValue(mutable);

                RandomTarget.SetValue((float)_rand.NextDouble() * (max-min+1)+min, 
                    entry);
            }

            return mutable;
        }
    }
}
