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

using Visualizers;

namespace Mutation.Mutators
{
    public class RemoveDataMutator : DataMutator
    {
        private MutableField<object> m_DataToRemove = new MutableField<object>() 
        { AbsoluteKey = "Data" };
        [Controllable(LabelText = "DataToRemove")]
        public MutableField<object> DataToRemove { get { return m_DataToRemove; } }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            mutable.RecursiveRemove( DataToRemove.AbsoluteKey.Split( '.' ) );

            return mutable;
        }
    }
}
