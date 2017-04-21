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
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Utility.SystemStats
{
    public class SystemStatsMemory : ISystemStats
    {
        public GameObject UnityObject { get; set; }

        public Text TextComponent { get; set; }

        public string StatString { get; set; }

        private uint prevMonoHeapSize = 0;
        private uint prevMonoUsedSize = 0;
        private uint prevUsedHeapSize = 0;
        private uint prevTotalAllocatedMemory = 0;
        private uint prevTotalReservedMemory = 0;
        private uint prevTotalUnusedReservedMemory = 0;

        public void Update()
        {
            uint monoHeapSize = Profiler.GetMonoHeapSize();
            uint monoUsedSize = Profiler.GetMonoUsedSize();
            uint usedHeapSize = Profiler.usedHeapSize;
            uint totalAllocatedMemory = Profiler.GetTotalAllocatedMemory();
            uint totalReservedMemory = Profiler.GetTotalReservedMemory();
            uint totalUnusedReservedMemory = Profiler.GetTotalUnusedReservedMemory();

            int deltaMonoHeapSize = (int) ( monoHeapSize - prevMonoHeapSize );
            int deltaMonoUsedSize = (int) ( monoUsedSize - prevMonoUsedSize );
            int deltaUsedHeapSize = (int) ( usedHeapSize - prevUsedHeapSize );
            int deltaTotalAllocatedMemory = (int) ( totalAllocatedMemory - prevTotalAllocatedMemory );
            int deltaTotalReservedMemory = (int) ( totalReservedMemory - prevTotalReservedMemory );
            int deltaTotalUnusedReservedMemory = (int) ( totalUnusedReservedMemory - prevTotalUnusedReservedMemory );

            prevMonoHeapSize = monoHeapSize;
            prevMonoUsedSize = monoUsedSize;
            prevUsedHeapSize = usedHeapSize;
            prevTotalAllocatedMemory = totalAllocatedMemory;
            prevTotalReservedMemory = totalReservedMemory;
            prevTotalUnusedReservedMemory = totalUnusedReservedMemory;

            // Note that we're actually creating TWO lines here
            StatString = String.Format("Memory:  MHS:{0,13:N0}  MUS:{1,13:N0}  UHS:{2,13:N0}  TAM:{3,13:N0}  TRM:{4,13:N0}  URM:{5,13:N0}\nDeltas:  MHS:{6,13:N0}  MUS:{7,13:N0}  UHS:{8,13:N0}  TAM:{9,13:N0}  TRM:{10,13:N0}  URM:{11,13:N0}",
                monoHeapSize, monoUsedSize, usedHeapSize, totalAllocatedMemory, totalReservedMemory, totalUnusedReservedMemory,
                deltaMonoHeapSize, deltaMonoUsedSize, deltaUsedHeapSize, deltaTotalAllocatedMemory, deltaTotalReservedMemory, deltaTotalUnusedReservedMemory);
        }
    }
}
