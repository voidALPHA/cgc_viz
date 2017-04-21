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
using Newtonsoft.Json;
using UnityEngine;
using Visualizers.CsView.Texturing;

namespace Adapters.TraceAdapters.Commands
{

    public class ChallengeSetInfo
    {
        public string Name { get; set; }

        public uint Binary { get; set; }

        public List<string> Files { get; set; }

        public int LinesOfCode { get; set; }

        public string Shortname { get; set; }

        private string m_Description = "No Description";
        public string Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        public List<string> CWEs { get; set; }

        public List<string> Tags { get; set; }

        public string Readme { get; set; }

        public int Position { get; set; }
    }

    public class ChallengeSetStats
    {
        public string Name { get; set; }

        public uint BinarySet { get; set; }

        public int LinesOfCode { get; set; }

        public List<string> CWEs { get; set; }

        public int Position { get; set; }
    }

    public class BinaryStats
    {
        public uint Binary { get; set; }

        public string File { get; set; }

        public float Entropy { get; set; }

        public List<int> ByteHistogram { get; set; }

        public string SetByteHistogram
        {
            set
            {
                var foundStrings = from str in value.Split(',') select 
                                   str.Replace( "\"","" ).Trim(new char[] { '{', '}' });
                
                ByteHistogram = new List< int >();

                foreach ( var foundString in foundStrings )
                {
                    ByteHistogram.Add( int.Parse(foundString) );
                }
            }
        }

        public List< BinarySection> Sections { get; set; }

        public string SetSections
        {
            set
            {
                Sections = new List< BinarySection >();

                var sectionStrings = value.Split( '}' );
                foreach ( var sectionString in sectionStrings )
                {
                    if ( !sectionString.Contains( '{' ) )
                        continue;

                    var sectionPortion = sectionString.Split( '{' ).Last();
                    var divisions = (from portion in sectionPortion.Split( ',' ) select portion.Replace( "\\\"", "" )).ToList();
                    

                    var newSection = new BinarySection();

                    try
                    {
                        newSection.BinaryFlag = ParseBinarySectionFlag(
                                divisions.First(
                                    sec => sec.StartsWith( "flag", StringComparison.InvariantCultureIgnoreCase ) )
                                    .Split( ':' )
                                    .Last() );

                        newSection.FileSize = int.Parse(
                            divisions.First(
                                sec => sec.StartsWith( "file_size", StringComparison.InvariantCultureIgnoreCase ) )
                                .Split( ':' )
                                .Last() );

                        newSection.BinaryType = (BinarySectionTypes)
                            Enum.Parse( typeof( BinarySectionTypes ),
                                divisions.First(
                                    sec => sec.StartsWith( "type", StringComparison.InvariantCultureIgnoreCase ) )
                                    .Split( ':' )
                                    .Last() );

                        newSection.MemorySize = int.Parse(
                            divisions.First(
                                sec => sec.StartsWith( "memory_size", StringComparison.InvariantCultureIgnoreCase ) )
                                .Split( ':' )
                                .Last() );

                    }
                    catch ( Exception e )
                    {
                        var errorString = "Can't parse sections! Exception: " + e;

                        Debug.Log( errorString );
                    }

                    Sections.Add( newSection );
                }
            }
        }

        public OpcodeHistogram OpcodeHistogram { get; set; }

        public string SetOpcodeHistogram
        {
            set
            {
                var foundStrings = value.Trim(new [] {'{', '}'}).Split( ',' );
                
                OpcodeHistogram = new OpcodeHistogram();

                foreach ( var foundString in foundStrings )
                {
                    var localString = foundString.Trim( new [] { '\"','\\'} );

                    var opcodeParts = localString.TrimEnd( '"' )//.Trim(new char[] { '[', ']' })
                        .Split( ':' );

                    var newOpcode = new OpcodePair( int.Parse( opcodeParts[1] ), opcodeParts[0].Trim(new[] { '\"', '\\' }) );

                    OpcodeHistogram.Add(newOpcode.Opcode, newOpcode);
                }
            }
        }

        public uint FileSize { get; set; }

        public List<FunctionListing> FunctionListings { get; set; }

        public List<BlockListing> BlockListings { get; set; }


        private BinarySectionFlags ParseBinarySectionFlag(string input)
        {
            if ( input.Contains( "READ" ) )
            {
                if (input.Contains( "WRITE" ))
                    return BinarySectionFlags.READ_WRITE;
                if (input.Contains( "EXEC" ))
                    return BinarySectionFlags.READ_EXEC;
                return BinarySectionFlags.READ;
            }
            if (input.Contains( "WRITE" ))
                return BinarySectionFlags.WRITE;
            return BinarySectionFlags.EXEC;
        }
    }


    public enum BinarySectionFlags
    {
        READ,
        WRITE,
        EXEC,
        READ_EXEC,
        READ_WRITE
    }

    public enum BinarySectionTypes
    {
        PT_NULL,
        PT_LOAD,
        PT_DYNAMIC,
        PT_INTERP,
        PT_NOTE,
        PT_SHLIB,
        PT_PHDR,
        PT_TLS
    }

    public class FunctionListing
    {
        public FunctionListing()
        {
        }

        public FunctionListing( uint startAddress, uint endAddress, int incomingEdges, int outgoingEdges )
        {
            StartAddress = startAddress;
            EndAddress = endAddress;
            IncomingEdges = incomingEdges;
            OutgoingEdges = outgoingEdges;
        }

        public uint StartAddress { get; set; }

        public uint EndAddress { get; set; }

        public int IncomingEdges { get; set; }

        public int OutgoingEdges { get; set; }
    }

    public class BlockListing
    {
        public BlockListing()
        {
        }

        public BlockListing( uint startAddress, uint endAddress )
        {
            StartAddress = startAddress;
            EndAddress = endAddress;
        }

        public uint StartAddress { get; set; }

        public uint EndAddress { get; set; }
    }
    

    public class BinarySection
    {
        public BinarySection()
        {
        }

        public BinarySection( int fileSize, int memorySize, BinarySectionFlags binaryFlag, BinarySectionTypes binaryType )
        {
            FileSize = fileSize;
            MemorySize = memorySize;
            BinaryFlag = binaryFlag;
            BinaryType = binaryType;
        }

        public int FileSize { get; set; }

        public int MemorySize { get; set; }

        public BinarySectionFlags BinaryFlag { get; set; }

        public BinarySectionTypes BinaryType { get; set; }
    }

    public class RcsToCsInfo
    {
        public uint Team { get; set; }

        public uint CsId { get; set; }

        public uint Round { get; set; }

        public uint BsId { get; set; }
    }

    public class ReferenceChallengeSetInfo
    {
        public uint Id { get; set; }

        public uint Team { get; set; }

        public uint Round { get; set; }

        public uint Bin { get; set; }

        public bool Pending { get; set; }
    }

    public enum BinsetType
    {
        Rcs,
        Ref,
        RefPatch
    }

    public class BinsetSubmissionInfo
    {
        public int Team { get; set; }

        [JsonProperty("name")]
        [Obsolete("This shouldn't be here; could get removed...")]
        public string TeamName { get; set; }

        public int Round { get; set; }

        public int RcsId { get; set; }
    }

    public class PovInfo
    {
        public uint Id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public uint Team { get; set; }

        public uint Round { get; set; }

        //public Object Target { get; set; }

        public string File { get; set; }
    }

    public class PovSubmissionInfo
    {
        public int Id { get; set; }

        public int Round { get; set; }

        public int Target { get; set; }

        [JsonProperty( "throw_count" )]
        public int ThrowCount { get; set; }
    }

    

    public class PollInfo
    {
        public uint Id { get; set; }

        public uint Round { get; set; }

        public string File { get; set; }
    }
    
}
