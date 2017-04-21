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
using System.Reflection.Emit;
using Adapters.TraceAdapters.Commands;
using Chains;
using Mutation;
using Visualizers;
using Visualizers.CsView.Texturing;

namespace Adapters.ChallengeSets
{
    public class CsStatsAdapter : Adapter
    {
        private MutableField<int> m_CsIndex = new MutableField<int>() 
        { LiteralValue = 10 };
        [Controllable(LabelText = "CsIndex")]
        public MutableField<int> CsIndex { get { return m_CsIndex; } }

        private MutableTarget m_CsStatsTarget = new MutableTarget() 
        { AbsoluteKey = "Cs Stats" };
        [Controllable(LabelText = "Cs Stats Target")]
        public MutableTarget CsStatsTarget { get { return m_CsStatsTarget; } }

        private MutableTarget m_BinaryStatsTarget = new MutableTarget() 
        { AbsoluteKey = "Binary Stats" };
        [Controllable(LabelText = "Binary Stats Target")]
        public MutableTarget BinaryStatsTarget { get { return m_BinaryStatsTarget; } }

        //private MutableTarget m_LocalBinaryIndexTarget = new MutableTarget() 
        //{ AbsoluteKey = "Binary Index" };
        //[Controllable(LabelText = "Local Binary Index")]
        //public MutableTarget LocalBinaryIndexTarget { get { return m_LocalBinaryIndexTarget; } }

        private SelectionState PerCsState { get { return Router["Per CS"]; } }

        private SelectionState PerBinaryState { get { return Router[ "Per Binary" ]; } }


        public CsStatsAdapter()
        {
            Router.AddSelectionState( "Per CS" );
            Router.AddSelectionState( "Per Binary" );
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            BinaryStatsTarget.SetValue(PopulateBinaryStats(new List< BinaryStats >() { DefaultBinaryStats }), newSchema);
            CsStatsTarget.SetValue(PopulateCsStats(DefaultChallengeSetStats), newSchema);
            //LocalBinaryIndexTarget.SetValue( (int)0, newSchema );

            Router.TransmitAllSchema(newSchema);
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var csIndex = (uint)CsIndex.GetFirstValue( payload.Data );

            //var csInfoCommand = new getcs

            #region Data query
            var csStatsCommand = new GetCsInfoCommand(csIndex);

            var commandIterator = CommandProcessor.Execute(csStatsCommand);
            while (commandIterator.MoveNext())
                yield return null;


            CsStatsTarget.SetValue(PopulateCsStats(csStatsCommand.CsInfo), payload.Data);
            #endregion

            #region Use default information

            //CsStatsTarget.SetValue( PopulateCsStats( DefaultChallengeSetStats ), payload.Data );

            #endregion


            var binsetCommand = new GetBinsetCommand( csStatsCommand.CsInfo.Binary );

            commandIterator = CommandProcessor.Execute( binsetCommand );
            while ( commandIterator.MoveNext() )
                yield return null;


            var binList = new List< BinaryStats >();

            foreach ( var binId in binsetCommand.BinIds )
            {
                var binInfoCommand = new GetBinaryStatsCommand( binId );

                commandIterator = CommandProcessor.Execute(binInfoCommand);
                while (commandIterator.MoveNext())
                    yield return null;

                binList.Add( binInfoCommand.BinStats );
            }
            
            BinaryStatsTarget.SetValue(PopulateBinaryStats(
                binList), payload.Data);

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }

        public static MutableObject PopulateCsStats(ChallengeSetInfo csInfo)
        {
            var stats = new MutableObject();

            stats[ "Lines of Code" ] = csInfo.LinesOfCode;
            //stats[ "Binary Set" ] = csInfo.BinarySet;
            stats[ "Binary Id" ] = csInfo.Binary;

            var filesMutable = csInfo.Files.Select( file => new MutableObject { { "File name", csInfo.Files } }.ToList());
            stats[ "Files" ] = filesMutable;

            var cweMutable = csInfo.CWEs.Select( cwe => new MutableObject { { "CWE Name", cwe } } ).ToList();
            stats[ "CWEs" ] = cweMutable;

            var tagMutable = csInfo.Tags.Select( tag => new MutableObject { { "Tag", tag } } ).ToList();
            stats[ "Tags" ] = tagMutable;

            stats[ "Cs Name" ] = csInfo.Name;
            stats[ "Readme" ] = csInfo.Readme;
            stats[ "Shortname" ] = csInfo.Shortname;

            stats["Description"] = csInfo.Description;

            stats[ "Visual Position" ] = csInfo.Position;

            return stats;
        }

        public static List<MutableObject> PopulateBinaryStats(List<BinaryStats> binaries)
        {
            var binaryStatsList = new List<MutableObject>();

            foreach ( var binary in binaries )
            {
                var stats = new MutableObject();

                stats[ "Opcode Histogram" ] = binary.OpcodeHistogram;

                stats[ "Binary ID" ] = binary.Binary;


                //var blockList = new List< MutableObject >();
                //foreach ( var blockListing in binary.BlockListings )
                //{
                //    blockList.Add( new MutableObject
                //    {
                //        { "Block Start", blockListing.StartAddress },
                //        { "Block End", blockListing.EndAddress }
                //    } );
                //}
                //stats[ "Block Listings" ] = blockList;

                stats[ "Byte Histogram" ] = binary.ByteHistogram;

                stats[ "Entropy" ] = binary.Entropy;

                stats[ "File Hash" ] = binary.File;

                stats[ "File Size" ] = binary.FileSize;

                //stats[ "Function Listings" ] = binary.FunctionListings;

                var sectionListings = new List< MutableObject >();
                foreach ( var binarySection in binary.Sections )
                {
                    var section = new MutableObject();
                    section[ "File Size" ] = binarySection.FileSize;
                    section[ "Memory Size" ] = binarySection.MemorySize;
                    section[ "Binary Flag" ] = binarySection.BinaryFlag;
                    section[ "Binary Type" ] = binarySection.BinaryType;
                    sectionListings.Add( section );
                }
                stats[ "Section Listings" ] = sectionListings;

                binaryStatsList.Add( stats );
            }

            return binaryStatsList;
        }

        public static BinaryStats DefaultBinaryStats
        {
            get
            {
                var newStats = new BinaryStats();
                newStats.OpcodeHistogram = new OpcodeHistogram
                {
                    {"cmr", new OpcodePair( 45, "cmr" )},
                    {"tbd", new OpcodePair( 73, "tbd" )},
                    {"nas", new OpcodePair( 78, "nas" )},
                    {"cgc", new OpcodePair( 24, "cgc" )},
                    {"nds", new OpcodePair( 63, "nds" )},
                    {"ids", new OpcodePair( 12, "ids" )},
                    {"cs", new OpcodePair( 14, "cs" )},
                    {"rcs", new OpcodePair( 46, "rcs" )},
                    {"crs", new OpcodePair( 28, "crs" )}
                };
                newStats.Binary = 13;
                newStats.BlockListings = new List< BlockListing >
                {
                    new BlockListing(41, 462),
                    new BlockListing(485, 823),
                    new BlockListing(873, 1512)
                };
                newStats.ByteHistogram = new List< int >();
                var rand = new Random(1337);
                for ( int i = 0; i < 256; i++ )
                {
                    newStats.ByteHistogram.Add( rand.Next(50000) );
                }
                newStats.Entropy = (float)rand.NextDouble();
                newStats.File = "abcde";
                newStats.FileSize = 50303;
                //newStats.FunctionListings = new List< FunctionListing >
                //{
                //    new FunctionListing (41, 442, 2, 3 ),
                //    new FunctionListing (463, 842, 1, 1 ),
                //    new FunctionListing (921, 1532, 1, 1 )
                //};

                newStats.Sections = new List< BinarySection >
                {
                    new BinarySection(4283, 1421, BinarySectionFlags.READ, BinarySectionTypes.PT_LOAD),
                    new BinarySection(6544, 5325, BinarySectionFlags.READ_EXEC, BinarySectionTypes.PT_DYNAMIC),
                    new BinarySection(544, 525, BinarySectionFlags.READ_EXEC, BinarySectionTypes.PT_NOTE),
                    new BinarySection(3544, 2525, BinarySectionFlags.READ_EXEC, BinarySectionTypes.PT_NULL),
                };

                return newStats;
            }
        }

        private ChallengeSetInfo DefaultChallengeSetStats
        {
            get
            {
                var newStats = new ChallengeSetInfo();

                newStats.LinesOfCode = 4000;
                newStats.CWEs = new List< string >
                {
                    "CWE-121",
                    "CWE-129"
                };

                newStats.Tags = new List< string >
                {
                    "Snack Pack"
                };

                newStats.Name = "Unnamed";
                newStats.Readme = "Long readme text is long";
                newStats.Shortname = "Challenge set shortname";
                newStats.Description = "Description";

                newStats.Files = new List< string >
                {
                    "File A",
                    "File B"
                };

                newStats.Binary = 12;
                //newStats.BinarySet = 12;

                newStats.Position = 4;

                return newStats;
            }
        }
    }
}
