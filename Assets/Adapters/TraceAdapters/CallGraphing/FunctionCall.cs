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

using System.Collections.Generic;
using Adapters.TraceAdapters.Common;

namespace Adapters.TraceAdapters.CallGraphing
{
    public class FunctionCall : IDataElement
    {
        public FunctionDescriptor Descriptor { get; private set; }

        public double CallingTime { get; private set; }

        public double ReturnTime { get; private set; }

        public List<FunctionCall> FunctionCalls { get; private set; }

        public FunctionCall( FunctionDescriptor descriptor, double callingTime )
        {
            FunctionCalls = new List < FunctionCall >();

            Descriptor = descriptor;

            CallingTime = callingTime;
        }

        public void CallFunction( FunctionCall call )
        {
            FunctionCalls.Add( call );
        }

        public void Return( double returnTime )
        {
            ReturnTime = returnTime;
        }
    }
}
