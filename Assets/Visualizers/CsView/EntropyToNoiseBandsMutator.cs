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
using System.Linq;
using Mutation;
using Mutation.Mutators;

namespace Visualizers.CsView
{
    public class EntropyToNoiseBandsMutator : DataMutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<float> m_Entropy = new MutableField<float>() 
        { AbsoluteKey= "Entropy" };
        [Controllable(LabelText = "Entropy")]
        public MutableField<float> Entropy { get { return m_Entropy; } }

        private MutableTarget m_LowerBandTarget = new MutableTarget() 
        { AbsoluteKey = "Lower Band" };
        [Controllable(LabelText = "LowerBandTarget")]
        public MutableTarget LowerBandTarget { get { return m_LowerBandTarget; } }

        private MutableTarget m_UpperBandTarget = new MutableTarget() 
        { AbsoluteKey = "Upper Band" };
        [Controllable(LabelText = "UpperBandTarget")]
        public MutableTarget UpperBandTarget { get { return m_UpperBandTarget; } }

        private MutableField<string> m_EntropySteps = new MutableField<string>()
        { LiteralValue = ".2,.2,.3,.6,1" };
        [Controllable(LabelText = "Entropy Steps")]
        public MutableField<string> EntropySteps { get { return m_EntropySteps; } }

        private MutableField<int> m_BandSpacing = new MutableField<int>() 
        { LiteralValue = 1 };
        [Controllable(LabelText = "Band Spacing")]
        public MutableField<int> BandSpacing { get { return m_BandSpacing; } }
        
        private MutableField<int> m_MinimumBandSize = new MutableField<int>() 
        { LiteralValue = 3 };
        [Controllable(LabelText = "Minimum Band Size")]
        public MutableField<int> MinimumBandSize { get { return m_MinimumBandSize; } }


        public EntropyToNoiseBandsMutator()
        {
            Entropy.SchemaParent = Scope;
            LowerBandTarget.SchemaParent = Scope;
            UpperBandTarget.SchemaParent = Scope;
            EntropySteps.SchemaParent = Scope;
            BandSpacing.SchemaParent = Scope;
            MinimumBandSize.SchemaParent = Scope;
        }


        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {

                var entropy = Entropy.GetValue(entry);

                var bandSpacing = BandSpacing.GetValue(entry);

                var entropySteps =
                    ( from step in EntropySteps.GetValue(entry).Split( ',' ) select float.Parse( step.Trim() ) )
                        .ToArray();

                var entropyLevel = 0;
                while ( entropyLevel < entropySteps.Length && entropy > entropySteps[ entropyLevel ] )
                    entropyLevel++;

                var minBands = MinimumBandSize.GetValue(entry) + entropyLevel;

                LowerBandTarget.SetValue( minBands, entry);
                UpperBandTarget.SetValue( bandSpacing + minBands, entry);
            }

            return mutable;

        }
    }

}
