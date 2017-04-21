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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mutation.Mutators.Axes.AxialTransformers
{
    public class AxialTransformer<T>
    {
        public AxialTransformer(Func<T,T> func)
        {
            EvalFunc = func;
        } 

        public T Transform(T value)
        {
            return EvalFunc(value);
        }

        private Func<T, T> EvalFunc { get; set; }
    }

    public class AxialStack<T>
    {
        private List<AxialTransformer<T>> m_Transformers = new List<AxialTransformer<T>>();
        public List<AxialTransformer<T>> Transformers { get { return m_Transformers; } set { m_Transformers = value; } }

        public T TransformValue(T value)
        {
            return Transformers.Aggregate(value, (current, transformer) => transformer.Transform(current));
        }
    }
}
