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

namespace Visualizers.CsView
{
    public class SplitShortnameMutator : DataMutator
    {
        private MutableField<string> m_Shortname = new MutableField<string>() 
        { LiteralValue = "Shortname" };
        [Controllable(LabelText = "Shortname")]
        public MutableField<string> Shortname { get { return m_Shortname; } }

        private MutableTarget m_SplitString = new MutableTarget() 
        { AbsoluteKey = "Split Shortname" };
        [Controllable(LabelText = "Split String")]
        public MutableTarget SplitString { get { return m_SplitString; } }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Shortname.GetEntries( mutable ) )
            {
                var shortName = Shortname.GetValue( entry );
                /*
                if ( shortName == "" )
                    shortName = "NN";

                var outString = shortName;
                if ( shortName.Split( ' ' ).Length > 2 )
                    outString = shortName.Split( ' ' )[ 2 ];
                    */
                SplitString.SetValue( shortName, entry );
            }

            return mutable;
        }
    }
}
