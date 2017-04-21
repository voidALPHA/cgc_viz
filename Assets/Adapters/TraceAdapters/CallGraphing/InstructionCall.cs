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


using Adapters.TraceAdapters.Common;

namespace Adapters.TraceAdapters.CallGraphing
{
    public class InstructionCall : IDataElement
    {
        public InstructionDescriptor Descriptor { get; private set; }

        public double CallingTime { get; private set; }

        // individual instructions don't really have a return time in the current data

        public InstructionCall(InstructionDescriptor descriptor, double callingTime)
        {
            Descriptor = descriptor;

            CallingTime = callingTime;
        }

    }
}
