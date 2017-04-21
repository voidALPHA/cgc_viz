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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Visualizers;
using ColorUtility = Assets.Utility.ColorUtility;

namespace Mutation.Mutators.MemorySpaceManagement
{
    
    public class AllocateMemorySpaceMutator : Mutator
    {
        private MutableScope m_TraceScope = new MutableScope();
        [Controllable(LabelText = "Scope")]
        public MutableScope TraceScope { get { return m_TraceScope; } }

        #region Allocate, read, and write scopes and fields

        private MutableScope m_AllocateScope = new MutableScope() {AbsoluteKey = "Allocates"};
        [Controllable(LabelText = "Allocate Scope")]
        public MutableScope AllocateScope { get { return m_AllocateScope; } }

        private MutableField<uint> m_AllocateAddress = new MutableField<uint>() 
        { AbsoluteKey= "Allocates.Address" };
        [Controllable(LabelText = "Allocate Address")]
        public MutableField<uint> AllocateAddress { get { return m_AllocateAddress; } }

        private MutableField<int> m_AllocateSize = new MutableField<int>() 
        { AbsoluteKey = "Allocates.Size"};
        [Controllable(LabelText = "Allocate Size")]
        public MutableField<int> AllocateSize { get { return m_AllocateSize; } }

        private MutableField<int> m_AllocateIndex = new MutableField<int>() 
        { AbsoluteKey= "Allocates.Processed Index" };
        [Controllable(LabelText = "Allocate Index")]
        public MutableField<int> AllocateIndex { get { return m_AllocateIndex; } }


        private MutableTarget m_AllocatePositionTarget = new MutableTarget() 
        { AbsoluteKey = "Allocates.VisualPosition" };
        [Controllable(LabelText = "Allocate Position Target")]
        public MutableTarget AllocatePositionTarget { get { return m_AllocatePositionTarget; } }

        private MutableTarget m_AllocateSizeTarget = new MutableTarget() 
        { AbsoluteKey = "Allocates.VisualSize" };
        [Controllable(LabelText = "Allocate Size Target")]
        public MutableTarget AllocateSizeTarget { get { return m_AllocateSizeTarget; } }

        private MutableTarget m_AllocateEndIndexTarget = new MutableTarget() 
        { AbsoluteKey = "Allocates.End Index" };
        [Controllable(LabelText = "AllocateEndIndexTarget")]
        public MutableTarget AllocateEndIndexTarget { get { return m_AllocateEndIndexTarget; } }

        private MutableTarget m_AllocateColorTarget = new MutableTarget() 
        { AbsoluteKey = "Allocates.Color" };
        [Controllable(LabelText = "Allocate Color Target")]
        public MutableTarget AllocateColorTarget { get { return m_AllocateColorTarget; } }


        private MutableScope m_FreeScope = new MutableScope() { AbsoluteKey = "Frees" };
        [Controllable(LabelText = "Free Scope")]
        public MutableScope FreeScope { get { return m_FreeScope; } }

        private MutableField<uint> m_FreeAddress = new MutableField<uint>()
        { AbsoluteKey = "Frees.Address" };
        [Controllable(LabelText = "Free Address")]
        public MutableField<uint> FreeAddress { get { return m_FreeAddress; } }

        private MutableField<int> m_FreeIndex = new MutableField<int>() 
        { AbsoluteKey = "Frees.Processed Index" };
        [Controllable(LabelText = "FreeIndex")]
        public MutableField<int> FreeIndex { get { return m_FreeIndex; } }




        private MutableScope m_ReadScope = new MutableScope() {AbsoluteKey = "Memory Reads"};
        [Controllable(LabelText = "Read Scope")]
        public MutableScope ReadScope { get { return m_ReadScope; } }

        private MutableField<uint> m_ReadAddress = new MutableField<uint>() 
        { AbsoluteKey = "Memory Reads.Read Address" };
        [Controllable(LabelText = "ReadAddress")]
        public MutableField<uint> ReadAddress { get { return m_ReadAddress; } }

        private MutableField<int> m_ReadSize = new MutableField<int>() 
        { AbsoluteKey = "Memory Reads.Read Size" };
        [Controllable(LabelText = "ReadSize")]
        public MutableField<int> ReadSize { get { return m_ReadSize; } }

        private MutableField<int> m_ReadIndex = new MutableField<int>() 
        { AbsoluteKey= "Memory Reads.Processed Index" };
        [Controllable(LabelText = "Read Index")]
        public MutableField<int> ReadIndex { get { return m_ReadIndex; } }



        private MutableTarget m_ReadPositionTarget = new MutableTarget()
        { AbsoluteKey = "Memory Reads.VisualPosition" };
        [Controllable(LabelText = "Read Position Target")]
        public MutableTarget ReadPositionTarget { get { return m_ReadPositionTarget; } }

        private MutableTarget m_ReadSizeTarget = new MutableTarget()
        { AbsoluteKey = "Memory Reads.VisualSize" };
        [Controllable(LabelText = "Read Size Target")]
        public MutableTarget ReadSizeTarget { get { return m_ReadSizeTarget; } }

        private MutableTarget m_ReadColorTarget = new MutableTarget()
        { AbsoluteKey = "Memory Reads.Color" };
        [Controllable(LabelText = "Read Color Target")]
        public MutableTarget ReadColorTarget { get { return m_ReadColorTarget; } }



        private MutableScope m_WriteScope = new MutableScope() { AbsoluteKey = "Memory Writes" };
        [Controllable(LabelText = "Write Scope")]
        public MutableScope WriteScope { get { return m_WriteScope; } }

        private MutableField<uint> m_WriteAddress = new MutableField<uint>()
        { AbsoluteKey = "Memory Writes.Write Address" };
        [Controllable(LabelText = "WriteAddress")]
        public MutableField<uint> WriteAddress { get { return m_WriteAddress; } }

        private MutableField<int> m_WriteSize = new MutableField<int>()
        { AbsoluteKey = "Memory Writes.Write Size" };
        [Controllable(LabelText = "WriteSize")]
        public MutableField<int> WriteSize { get { return m_WriteSize; } }
        
        private MutableField<int> m_WriteIndex = new MutableField<int>()
        { AbsoluteKey = "Memory Writes.Processed Index" };
        [Controllable(LabelText = "Write Index")]
        public MutableField<int> WriteIndex { get { return m_WriteIndex; } }

        
        private MutableTarget m_WritePositionTarget = new MutableTarget()
        { AbsoluteKey = "Memory Writes.VisualPosition" };
        [Controllable(LabelText = "Write Position Target")]
        public MutableTarget WritePositionTarget { get { return m_WritePositionTarget; } }

        private MutableTarget m_WriteSizeTarget = new MutableTarget()
        { AbsoluteKey = "Memory Writes.VisualSize" };
        [Controllable(LabelText = "Write Size Target")]
        public MutableTarget WriteSizeTarget { get { return m_WriteSizeTarget; } }

        private MutableTarget m_WriteColorTarget = new MutableTarget()
        { AbsoluteKey = "Memory Writes.Color" };
        [Controllable(LabelText = "Write Color Target")]
        public MutableTarget WriteColorTarget { get { return m_WriteColorTarget; } }
        
        #endregion

        private MutableField<int> m_MaxIndex = new MutableField<int>() 
        { AbsoluteKey = "MaxInstCount" };
        [Controllable(LabelText = "Max Index")]
        public MutableField<int> MaxIndex { get { return m_MaxIndex; } }


        #region Color definition variables 
        //private MutableField<float> m_HueStart = new MutableField<float>() 
        //{ LiteralValue = .5f };
        //[Controllable(LabelText = "Hue Start")]
        //public MutableField<float> HueStart { get { return m_HueStart; } }

        //private MutableField<float> m_HueStepSize = new MutableField<float>() 
        //{ LiteralValue = .25f };
        //[Controllable(LabelText = "HueStepSize")]
        //public MutableField<float> HueStepSize { get { return m_HueStepSize; } }

        private MutableField<Color> m_AllocationColor = new MutableField<Color>() 
        { LiteralValue = Color.cyan };
        [Controllable(LabelText = "Allocation Color")]
        public MutableField<Color> AllocationColor { get { return m_AllocationColor; } }


        private MutableField<float> m_ValueStepSize = new MutableField<float>() 
        { LiteralValue = .4f };
        [Controllable(LabelText = "Value Step Size")]
        public MutableField<float> ValueStepSize { get { return m_ValueStepSize; } }
        
        //private MutableField<float> m_Saturation = new MutableField<float>() 
        //{ LiteralValue = .8f };
        //[Controllable(LabelText = "Saturation")]
        //public MutableField<float> Saturation { get { return m_Saturation; } }

        //private MutableField<float> m_Value = new MutableField<float>() 
        //{ LiteralValue = .8f };
        //[Controllable(LabelText = "Value")]
        //public MutableField<float> Value { get { return m_Value; } }
        
        private MutableField<float> m_AllocationAlpha = new MutableField<float>() 
        { LiteralValue = .25f };
        [Controllable(LabelText = "Allocation Alpha")]
        public MutableField<float> AllocationAlpha { get { return m_AllocationAlpha; } }


        //private MutableField<float> m_ReadWriteDifferentiation = new MutableField<float>() 
        //{ LiteralValue = .3f };
        //[Controllable(LabelText = "Read Write Differentiation")]
        //public MutableField<float> ReadWriteDifferentiation { get { return m_ReadWriteDifferentiation; } }


        //private MutableField<Color> m_OutOfBoundsColor = new MutableField<Color>() 
        //{ LiteralValue = Color.red };
        //[Controllable(LabelText = "Out Of Bounds Color")]
        //public MutableField<Color> OutOfBoundsColor { get { return m_OutOfBoundsColor; } }
        
        private MutableField<Color> m_OutOfBoundsReadColor = new MutableField<Color>() 
        { LiteralValue = Color.green };
        [Controllable(LabelText = "OOB Read Color")]
        public MutableField<Color> OutOfBoundsReadColor { get { return m_OutOfBoundsReadColor; } }
        
        private MutableField<Color> m_OutOfBoundsWriteColor = new MutableField<Color>() 
        { LiteralValue = Color.red };
        [Controllable(LabelText = "OOB Write Color")]
        public MutableField<Color> OutOfBoundsWriteColor { get { return m_OutOfBoundsWriteColor; } }


        #endregion

        #region Gridline Targets

        private MutableTarget m_GridLinesTarget = new MutableTarget() 
        { AbsoluteKey = "GridLines" };
        [Controllable(LabelText = "Grid Lines Target")]
        public MutableTarget GridLinesTarget { get { return m_GridLinesTarget; } }
        
        #endregion

        public AllocateMemorySpaceMutator()
        {
            AllocateScope.SchemaParent = TraceScope;
            FreeScope.SchemaParent = TraceScope;
            ReadScope.SchemaParent = TraceScope;
            WriteScope.SchemaParent = TraceScope;

            MaxIndex.SchemaParent = TraceScope;

            AllocateAddress.SchemaParent = AllocateScope;
            AllocateSize.SchemaParent = AllocateScope;
            AllocateIndex.SchemaParent = AllocateScope;
            AllocatePositionTarget.SchemaParent = AllocateScope;
            AllocateColorTarget.SchemaParent = AllocateScope;
            AllocateSizeTarget.SchemaParent = AllocateScope;
            AllocateEndIndexTarget.SchemaParent = AllocateScope;

            FreeAddress.SchemaParent = FreeScope;
            FreeIndex.SchemaParent = FreeScope;

            ReadAddress.SchemaParent = ReadScope;
            ReadSize.SchemaParent = ReadScope;
            ReadIndex.SchemaParent = ReadScope;
            ReadPositionTarget.SchemaParent = ReadScope;
            ReadColorTarget.SchemaParent = ReadScope;
            ReadSizeTarget.SchemaParent = ReadScope;

            WriteAddress.SchemaParent = WriteScope;
            WriteSize.SchemaParent = WriteScope;
            WriteIndex.SchemaParent = WriteScope;
            WritePositionTarget.SchemaParent = WriteScope;
            WriteColorTarget.SchemaParent = WriteScope;
            WriteSizeTarget.SchemaParent = WriteScope;

            //HueStart.SchemaParent = TraceScope;
            //HueStepSize.SchemaParent = TraceScope;
            //Saturation.SchemaParent = TraceScope;
            //Value.SchemaParent = TraceScope;
            GridLinesTarget.SchemaParent = TraceScope;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            foreach ( var entry in TraceScope.GetEntries( newSchema ) )
            {
                foreach ( var subentry in AllocateScope.GetEntries( entry ) )
                {
                    AllocatePositionTarget.SetValue( 4f, subentry );
                    AllocateSizeTarget.SetValue( .4f, subentry );
                    AllocateColorTarget.SetValue( Color.magenta, subentry );
                    AllocateEndIndexTarget.SetValue( (int)12, subentry );
                }

                foreach (var subentry in ReadScope.GetEntries(entry))
                {
                    ReadPositionTarget.SetValue(4f, subentry);
                    ReadSizeTarget.SetValue(.4f, subentry);
                    ReadColorTarget.SetValue(Color.magenta, subentry);
                }

                foreach (var subentry in WriteScope.GetEntries(entry))
                {
                    WritePositionTarget.SetValue(4f, subentry);
                    WriteSizeTarget.SetValue(.4f, subentry);
                    WriteColorTarget.SetValue(Color.magenta, subentry);
                }
            }

            GridLinesTarget.SetValue( new List<MutableObject>
            {
                new MutableObject { { "Visual Position", .5f}, {"Visual Weight", .2f} },
                new MutableObject { { "Visual Position", 1f}, {"Visual Weight", .1f} }
            }, newSchema );

            Router.TransmitAllSchema( newSchema );
        }
        

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            //var outOfBoundsColor = OutOfBoundsColor.GetFirstValue( payload.Data );

            var outOfBoundsReadColor = OutOfBoundsReadColor.GetFirstValue( payload.Data );
            var outOfBoundsWriteColor = OutOfBoundsWriteColor.GetFirstValue( payload.Data );

            var allocatedAlpha = AllocationAlpha.GetFirstValue( payload.Data );

            var valueStepSize = ValueStepSize.GetFirstValue( payload.Data );
            var allocationColor = AllocationColor.GetFirstValue( payload.Data );

            float allocH, allocS, allocV;
            ColorUtility.RGBToHSV( allocationColor, out allocH, out allocS, out allocV );


            //var readWriteDifferentiation = ReadWriteDifferentiation.GetFirstValue( payload.Data );

            foreach ( var entry in TraceScope.GetEntries( payload.Data ) )
            {
                var memSpace = new MemorySpace();

                //var saturation = Saturation.GetValue( entry );
                //var hueStep = HueStepSize.GetValue( entry );
                //var value = Value.GetValue( entry );

                int maxIndex = MaxIndex.GetValue( entry );

                // match allocations and frees

                var allocations = new Dictionary<uint, MemorySpacePoint>();
                foreach ( var allocation in AllocateScope.GetEntries( entry ) )
                {
                    var address = AllocateAddress.GetValue( allocation );
                    var size = AllocateSize.GetValue( allocation );
                    var index = AllocateIndex.GetValue( allocation );

                    var newPoint = memSpace.AddMemoryAllocation( address, size );

                    newPoint.StartIndex = index;
                    newPoint.EndPoint.StartIndex = index;
                    newPoint.EndIndex = maxIndex;
                    newPoint.EndPoint.EndIndex = maxIndex;

                    allocations.Add( newPoint.Address, newPoint );
                }

                // assign the end indices of any allocates with matched frees
                foreach ( var free in FreeScope.GetEntries( entry ) )
                {
                    var index = FreeIndex.GetValue( free );
                    var address = FreeAddress.GetValue( free );

                    if ( allocations.ContainsKey( address ) )
                    {
                        var matchedAllocation = allocations[ address ];

                        matchedAllocation.EndIndex = index;
                        matchedAllocation.EndPoint.EndIndex = index;
                    }
                }


                // import reads to the memory space
                var readPoints = new Dictionary< int, MemorySpacePoint >();
                foreach ( var read in ReadScope.GetEntries( entry ) )
                {
                    var address = ReadAddress.GetValue( read );
                    var size = ReadSize.GetValue( read );
                    var index = ReadIndex.GetValue( read );

                    var readPoint = MemorySpacePoint.GeneratePoint(address, size);
                    readPoint.AllIndices = index;
                    

                    MemorySpacePoint containingPoint;

                    if ( memSpace.IsAccessContained( readPoint, readPoint.EndPoint, out containingPoint ) )
                    {
                        readPoint.ContainingAllocation = containingPoint;
                    }
                    else
                    {
                        var outOfBoundsAlloc = memSpace.AddMemoryAllocation( address, size, 0, maxIndex );
                        outOfBoundsAlloc.IsOutOfBounds = true;

                        readPoint.ContainingAllocation = outOfBoundsAlloc;
                    }

                    readPoints.Add( index, readPoint );
                }

                // import Writes to the memory space
                var writePoints = new Dictionary<int, MemorySpacePoint>();
                foreach (var write in WriteScope.GetEntries(entry))
                {
                    var address = WriteAddress.GetValue(write);
                    var size = WriteSize.GetValue(write);
                    var index = WriteIndex.GetValue(write);

                    var writePoint = MemorySpacePoint.GeneratePoint(address, size);
                    writePoint.AllIndices = index;

                    MemorySpacePoint containingPoint;

                    if (memSpace.IsAccessContained(writePoint, writePoint.EndPoint, out containingPoint))
                    {
                        writePoint.ContainingAllocation = containingPoint;
                    }
                    else
                    {
                        var outOfBoundsAlloc = memSpace.AddMemoryAllocation(address, size, 0, maxIndex);
                        outOfBoundsAlloc.IsOutOfBounds = true;

                        writePoint.ContainingAllocation = outOfBoundsAlloc;
                        
                    }

                    writePoints.Add(index, writePoint);
                }


                // generate visual space for memory allocations (include out of bounds allocations)
                memSpace.SetVisualBounding();


                // set colors for memory allocations (inclde out of bounds allocations with special color)
                //var hueRotor = HueStart.GetValue( entry );

                foreach ( var allocation in memSpace.RegisteredSpace.Where( s=>s.IsStart ) )
                {
                    Color nextColor = ColorUtility.HsvtoRgb( allocH, allocS, allocV );

                    allocV = .3f+(allocV+valueStepSize)%.7f;
                        /*(allocation.IsOutOfBounds)?
                        outOfBoundsColor:
                        ColorUtility.HsvtoRgb(hueRotor, saturation,
                        value);*/

                    nextColor.a = allocatedAlpha;

                    //if ( !allocation.IsOutOfBounds )
                    //    hueRotor += hueStep;

                    allocation.VisualColor = nextColor;
                }

                // set colors and visual locations for all reads
                foreach ( var read in readPoints )
                {
                    if (read.Value.ContainingAllocation==null)
                        Debug.LogError( "Danger will robinson!  Danger!" );
                    read.Value.VisualColor =
                        outOfBoundsReadColor;
             //           read.Value.ContainingAllocation.VisualColor == outOfBoundsColor?
             //           outOfBoundsReadColor:
             //           Color.Lerp(
             //           read.Value.ContainingAllocation.VisualColor,
             //       Color.white, readWriteDifferentiation);

                    //read.Value.VisualStart = memSpace.AddressToVisualPosition( read.Value.Address );
                    //read.Value.VisualEnd = memSpace.AddressToVisualPosition( read.Value.EndPoint.Address );
                    read.Value.ComputeVisualPositionsFromContainer();
                    
                }


                // set colors and visual locations for all writes
                foreach (var write in writePoints)
                {
                    if (write.Value.ContainingAllocation == null)
                        Debug.LogError("Danger will robinson!  Danger!");
                    write.Value.VisualColor =
                        outOfBoundsWriteColor;
                //       write.Value.ContainingAllocation.VisualColor == outOfBoundsColor?
                //       outOfBoundsWriteColor:
                //       Color.Lerp(
                //       write.Value.ContainingAllocation.VisualColor,
                //   Color.black, readWriteDifferentiation);

                    write.Value.ComputeVisualPositionsFromContainer();

                    //write.Value.VisualStart = memSpace.AddressToVisualPosition(write.Value.Address);
                    //write.Value.VisualEnd = memSpace.AddressToVisualPosition(write.Value.EndPoint.Address);
                }

                // FINALLY, write this information into the corresponding targets
                foreach ( var allocation in AllocateScope.GetEntries( entry ) )
                {
                    var targetAddress = AllocateAddress.GetValue( allocation );
                    //var targetIndex = AllocateIndex.GetValue( allocation );

                    if ( !allocations.ContainsKey( targetAddress ) )
                    {
                        throw new Exception( "Allocation address not found!" );
                    }
                    var foundAllocation = allocations[ targetAddress ];

                    AllocateColorTarget.SetValue( foundAllocation.VisualColor, allocation);
                    AllocatePositionTarget.SetValue( foundAllocation.VisualStart, allocation );
                    AllocateSizeTarget.SetValue( foundAllocation.VisualEnd-foundAllocation.VisualStart, allocation );
                    AllocateEndIndexTarget.SetValue( foundAllocation.EndIndex, allocation );
                }

                foreach ( var read in ReadScope.GetEntries( entry ) )
                {
                    var targetIndex = ReadIndex.GetValue( read );

                    if ( !readPoints.ContainsKey( targetIndex ) )
                    {
                        throw new Exception("Read index not found!");
                    }
                    var foundRead = readPoints[ targetIndex ];

                    ReadColorTarget.SetValue( foundRead.VisualColor, read );
                    ReadPositionTarget.SetValue( foundRead.VisualStart, read );
                    ReadSizeTarget.SetValue( foundRead.VisualEnd-foundRead.VisualStart, read );
                }


                foreach (var write in WriteScope.GetEntries(entry))
                {
                    var targetIndex = WriteIndex.GetValue(write);

                    if (!writePoints.ContainsKey(targetIndex))
                    {
                        throw new Exception("Write index not found!");
                        continue;
                    }
                    var foundwrite = writePoints[targetIndex];

                    WriteColorTarget.SetValue(foundwrite.VisualColor, write);
                    WritePositionTarget.SetValue(foundwrite.VisualStart, write);
                    WriteSizeTarget.SetValue(foundwrite.VisualEnd - foundwrite.VisualStart, write);
                }


                // write grid lines
                var gridLines = new List< MutableObject >();
                uint priorAddress = memSpace.RegisteredSpace.First().Address;


                gridLines.Add(new MutableObject
                {
                    {"Visual Position", 0f },
                    {"Visual Weight", 1000f }
                });

                foreach ( var space in memSpace.RegisteredSpace )
                {
                    if ( space.IsEnd )
                        continue;
                    var newGridLine = new MutableObject
                    {
                        {"Visual Position", space.VisualStart },
                        {"Visual Weight", (float)(Mathf.Max(0,priorAddress-space.StartPoint.Address)+1) }
                    };
                    priorAddress = space.EndPoint.Address;
                    gridLines.Add( newGridLine );
                }

                gridLines.Add(new MutableObject
                {
                    {"Visual Position", 1f },
                    {"Visual Weight", 1000f }
                });

                GridLinesTarget.SetValue( gridLines, entry );
            }


            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }



    public class SpaceRegion
    {
        public uint StartPoint { get; set; }
        public uint EndPoint { get; set; }
    }

    public class SpaceMarker
    {
        public SpaceMarker( uint point, bool isStart )
        {
            Point = point;
            IsStart = isStart;
        }

        public uint Point { get; set; }
        public bool IsStart { get; set; }
        public bool IsEnd { get { return !IsStart; } set { IsStart = !value; } }
    }

    public class MarkedSpace
    {
        public SortedList<uint, SpaceMarker> Markers { get; set; }

        public MarkedSpace()
        {
            Markers = new SortedList< uint, SpaceMarker >();
        }


    }
    /*
    public class SpaceGradient
    {
        // implicit list of pairs
        public List<uint> SpaceMarkersStartEnd { get; set; }

        public SpaceGradient()
        {
            SpaceMarkersStartEnd = new List< uint >();
        }

        public void AddSpace(uint address, int size)
        {
            uint newEndPoint = address + (uint)size;

            if (SpaceMarkersStartEnd.Count == 0)
            {
                SpaceMarkersStartEnd.Add(address);
                SpaceMarkersStartEnd.Add( newEndPoint );
                return;
            }

            int lowerIndex = 0;
            int upperIndex = SpaceMarkersStartEnd.Count - 1;

            while (upperIndex - lowerIndex > 1)
            {
                var midIndex = (lowerIndex + upperIndex) / 2;
                if (SpaceMarkersStartEnd[midIndex].StartPoint > address)
                    upperIndex = midIndex;
                else
                    lowerIndex = midIndex;
            }

            if (SpaceMarkersStartEnd[lowerIndex].StartPoint < address && SpaceMarkersStartEnd[lowerIndex].EndPoint > address)
            {
                // new address is contained, expand existing address
                SpaceMarkersStartEnd[lowerIndex].EndPoint = SpaceMarkersStartEnd[lowerIndex].EndPoint > newEndPoint ?
                    SpaceMarkersStartEnd[lowerIndex].EndPoint : newEndPoint;
                return;
            }
            if ( SpaceMarkersStartEnd[ lowerIndex ].StartPoint > newEndPoint )
            {
                SpaceMarkersStartEnd.Insert( lowerIndex, new SpaceRegion()
                {
                    StartPoint = address, EndPoint = newEndPoint
                });
            }

            SpaceMarkersStartEnd.Insert(lowerIndex + 1, new SpaceRegion()
            {
                StartPoint = address, EndPoint = newEndPoint
            });
        }

    }*/
    
}
