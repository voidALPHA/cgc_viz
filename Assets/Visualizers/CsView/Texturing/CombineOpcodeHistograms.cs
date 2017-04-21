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

using Mutation;
using Mutation.Mutators;

namespace Visualizers.CsView.Texturing
{
    public class CombineOpcodeHistograms : DataMutator
    {
        private MutableField<OpcodeHistogram> m_InputHistograms = new MutableField<OpcodeHistogram>() 
        { AbsoluteKey = "Binaries.Opcodes" };
        [Controllable(LabelText = "Input Histograms")]
        public MutableField<OpcodeHistogram> InputHistograms { get { return m_InputHistograms; } }

        private MutableTarget m_CombinedTarget = new MutableTarget() 
        { AbsoluteKey = "Combined Histogram" };
        [Controllable(LabelText = "Combined Histogram")]
        public MutableTarget CombinedTarget { get { return m_CombinedTarget; } }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            var totalHistogram = new OpcodeHistogram();

            foreach ( var entry in InputHistograms.GetEntries( mutable ) )
            {
                var foundOpcodes = InputHistograms.GetValue( entry );
                foreach ( var kvp in foundOpcodes )
                {

                    if ( !totalHistogram.ContainsKey( kvp.Key ) )
                        totalHistogram.Add( kvp.Key, kvp.Value );

                    totalHistogram[ kvp.Key ].Frequency += kvp.Value.Frequency;
                }
            }

            CombinedTarget.SetValue( totalHistogram, mutable );

            return mutable;
        }
    }
}
