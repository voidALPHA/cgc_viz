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

using Mutation.Mutators.Axes.AxialTransformers;

namespace Mutation.Mutators.Axes
{
    public abstract class AxisMutator : DataMutator
    {
    }

    public abstract class Axis<tIn, tOut> : AxisMutator
    {
        private AxialStack<tIn> m_InputStack = new AxialStack<tIn>();
        public AxialStack<tIn> InputStack { get { return m_InputStack; } set { m_InputStack = value; } }
                    
        private AxialStack<tOut> m_OutputStack = new AxialStack<tOut>(); 
        public AxialStack<tOut> OutputStack { get { return m_OutputStack; } set { m_OutputStack = value; } }
    }
}
