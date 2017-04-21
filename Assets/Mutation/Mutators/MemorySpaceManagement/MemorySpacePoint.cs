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
using Adapters.TraceAdapters.Traces.Elements;
using UnityEngine;

namespace Mutation.Mutators.MemorySpaceManagement
{
    public class MemorySpacePoint
    {
        public MemorySpacePoint( uint address, bool isStart )
        {
            Address = address;
            IsStart = isStart;
        }
        
        public bool IsStart { get; set; }
        public bool IsEnd { get { return !IsStart; } set { IsStart = !value; } }
        public uint Address { get; private set; }
        public MemorySpacePoint RelatedPoint { get; set; }

        public MemorySpacePoint StartPoint { get { return IsStart ? this : RelatedPoint; } }
        public MemorySpacePoint EndPoint { get { return IsEnd ? this : RelatedPoint; } }

        public int StartIndex { get; set; }
        public int EndIndex { get; set; }

        public MemorySpacePoint ContainingAllocation { get; set; }

        public float VisualPoint { get; set; }
        public float VisualStart { get { return StartPoint.VisualPoint; } set { StartPoint.VisualPoint = value; } }
        public float VisualEnd { get { return EndPoint.VisualPoint; } set { EndPoint.VisualPoint = value; } }
        public Color VisualColor { get; set; }

        public bool IsOutOfBounds { get; set; }

        public int AllIndices { set { StartIndex = value;
            EndIndex = value;
            RelatedPoint.StartIndex = value;
            RelatedPoint.EndIndex = value;
        } }

        public static MemorySpacePoint GeneratePoint( uint address, int size )
        {
            var newPoint = new MemorySpacePoint( address, true );
            var endPoint = new MemorySpacePoint(address + (uint)size, false);
            newPoint.RelatedPoint = endPoint;
            endPoint.RelatedPoint = newPoint;

            return newPoint;
        }

        public void ComputeVisualPositionsFromContainer()
        {
            VisualStart = Mathf.Lerp(ContainingAllocation.VisualStart, ContainingAllocation.VisualEnd,
                (StartPoint.Address - ContainingAllocation.StartPoint.Address) / (float)(ContainingAllocation.EndPoint.Address - ContainingAllocation.StartPoint.Address));
            
            VisualEnd = Mathf.Lerp(ContainingAllocation.VisualStart, ContainingAllocation.VisualEnd,
                (EndPoint.Address - ContainingAllocation.StartPoint.Address) / (float)(ContainingAllocation.EndPoint.Address - ContainingAllocation.StartPoint.Address));
        }
    }

    public class MemoryVisualSpace
    {
        public MemoryVisualSpace( float startPoint, float endPoint )
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
        }

        public float StartPoint { get; set; }
        public float EndPoint { get; set; }
    }

    public class MemorySpace
    {
        public List<MemorySpacePoint> RegisteredSpace { get; set; }

        public MemorySpace()
        {
            RegisteredSpace = new List< MemorySpacePoint >();
        }

        public MemorySpacePoint AddMemoryAllocation(uint address, int size)
        {
            var startPoint = MemorySpacePoint.GeneratePoint( address, size );
            
            AddSpace(startPoint, startPoint.RelatedPoint);

            return startPoint;
        }

        public MemorySpacePoint AddMemoryAllocation( uint address, int size, int startIndex, int endIndex )
        {
            var startPoint = MemorySpacePoint.GeneratePoint( address, size );

            startPoint.StartIndex = startIndex;
            startPoint.EndPoint.StartIndex = startIndex;

            startPoint.EndIndex = endIndex;
            startPoint.EndPoint.EndIndex = endIndex;

            // debug test
            if (RegisteredSpace.Any(s=>s.StartPoint.Address==address&&s.EndPoint.Address==address+size))
            {
                var duplicateSpace =
                    RegisteredSpace.First( s => s.StartPoint.Address == address && s.EndPoint.Address == address + size );

                Debug.Log( "Adding duplicate space!" );

                MemorySpacePoint container;

                var isContained = IsAccessContained( startPoint, startPoint.EndPoint, out container );
            }


            AddSpace( startPoint, startPoint.EndPoint);

            return startPoint;
        }
        
        public bool IsAccessContained( MemorySpacePoint accessStart, MemorySpacePoint accessEnd, out MemorySpacePoint containingSpace)
        {
            containingSpace = null;

            //List<MemorySpacePoint> containingRegions = new List< MemorySpacePoint >();

            int startRegions = BinarySearchIndex( accessStart.Address );
            if ( startRegions > RegisteredSpace.Count - 1 )
                return false;
            for ( int i = startRegions-1; i >= 0; i-- )
            {
                var testRegion = RegisteredSpace[ i ];
                if ( testRegion.IsEnd )
                    continue;
                if ( testRegion.EndPoint.Address < accessEnd.Address )
                    continue;
                if ( accessStart.StartIndex < testRegion.StartIndex )
                    continue;
                if ( accessStart.EndIndex > testRegion.EndIndex )
                    continue;
                containingSpace = testRegion;
                return true;
            }

            return false;
        }

        public void AddSpace( MemorySpacePoint startPoint, MemorySpacePoint endPoint )
        {
            if ( RegisteredSpace.Count == 0 )
            {
                RegisteredSpace.Add( startPoint );
                RegisteredSpace.Add( endPoint );
                return;
            }

            BinaryInsertPoint( startPoint );
            BinaryInsertPoint( endPoint );
        }

        public float AddressToVisualPosition( uint address )
        {
            var lowerIndex = BinarySearchIndex( address ) - 1;
            var upperIndex = lowerIndex + 1;

            if ( lowerIndex < 0 )
                return 0f;

            if ( upperIndex > RegisteredSpace.Count - 1 )
                return 1f;

            var lowerSpace = RegisteredSpace[lowerIndex];
            if ( lowerSpace.Address > address )
                return 0f;

            var upperSpace = RegisteredSpace[ upperIndex ];

            return Mathf.Lerp( lowerSpace.VisualPoint, upperSpace.VisualPoint,
                ( address - lowerSpace.Address ) / (float)( upperSpace.Address - lowerSpace.Address ) );
        }


        private void BinaryInsertPoint( MemorySpacePoint point )
        {
            RegisteredSpace.Insert( BinarySearchIndex( point.Address ), 
                point );
        }

        private int BinarySearchIndex( uint address )
        {
            if ( RegisteredSpace.Count == 0 )
                return 0;

            if (address < RegisteredSpace.First().Address)
            {
                return 0;
            }

            if (address > RegisteredSpace.Last().Address)
            {
                return RegisteredSpace.Count;
            }


            int lowerIndex = 0;
            int upperIndex = RegisteredSpace.Count - 1;

            while (upperIndex - lowerIndex > 1)
            {
                var midIndex = (lowerIndex + upperIndex) / 2;
                if (RegisteredSpace[midIndex].Address > address)
                    upperIndex = midIndex;
                else
                    lowerIndex = midIndex;
            }

            return lowerIndex+1;
        }

        public uint TotalSize{
            get
            {
                var total = (uint)0;
                var currentDepth = 0;
                var regionStart = (uint)0;
                foreach ( var point in RegisteredSpace )
                {
                    var depthChange = point.IsStart ? 1 : -1;

                    if ( depthChange > 0 && currentDepth == 0 )
                        regionStart = point.Address;

                    if ( depthChange < 0 && currentDepth == 1 )
                        total += point.Address - regionStart;

                    currentDepth += depthChange;
                }

                if (currentDepth !=0)
                    throw new Exception("Memory space should be balanced...");

                return total;
            }
        }

        public void SetVisualBounding()
        {
            var total = (uint)0;
            var currentDepth = 0;
            var regionStart = (uint)0;
            foreach (var point in RegisteredSpace)
            {
                var depthChange = point.IsStart ? 1 : -1;

                if (depthChange > 0 && currentDepth == 0)
                    regionStart = point.Address;

                if ( depthChange < 0 && currentDepth == 1 )
                {
                    total += point.Address - regionStart;
                    regionStart = point.Address;
                }

                point.VisualPoint = total + ( point.Address - regionStart );

                currentDepth += depthChange;
            }
            
            if (currentDepth != 0)
                throw new Exception("Memory space should be balanced...");

            foreach ( var point in RegisteredSpace )
            {
                point.VisualPoint /= total;
                //point.RelatedPoint.VisualEnd = ( point.VisualStart - point.RelatedPoint.VisualStart ) / total;
            }
        }

    }
}
