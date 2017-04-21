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
using System.IO;
using System.Text;
using Adapters.GameEvents.Model;
using Adapters.ScoreAdapters;
using Adapters.TraceAdapters.Traces;
using Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adapters.TraceAdapters.Commands
{
    public abstract class Command
    {
        public abstract string RelativeUrl { get; }

        #region Results

        public bool Ok { get; set; }

        #endregion

        public virtual void HandleResponse( byte[] response )
        {
            var text = Encoding.ASCII.GetString( response );

            JsonConvert.PopulateObject( text, this );
        }
    }

    public abstract class PostCommand : Command
    {
        // TODO: Override ok here? Or is it just in scores/traces?

        public virtual string PostString { get { return string.Empty; } }

        public virtual Dictionary<string, string> Headers { get { return new Dictionary < string, string >(); } }
    }


    #region Scores/Events

    public class GetScoresForRoundCommand : Command
    {
        public override string RelativeUrl
        {
            get { return string.Format( "/score/{0}", RoundIndex ); }
        }

        #region Arguments

        private int RoundIndex { get; set; }

        public GetScoresForRoundCommand( int roundIndex )
        {
            RoundIndex = roundIndex;
        }

        #endregion

        #region Results

        public RoundScores Scores { get; private set; }

        // TODO: What does Ok property mean in this class??
        public override void HandleResponse( byte[] response )
        {
            var stream = new MemoryStream( response );

            Scores = RoundScores.FromStream( stream );
        }

        #endregion
    }

    public class GetEventsForRoundCommand : Command
    {
        public override string RelativeUrl
        {
            get { return string.Format( "/event/{0}", RoundIndex ); }
        }

        #region Arguments

        private int RoundIndex { get; set; }

        public GetEventsForRoundCommand( int roundIndex )
        {
            RoundIndex = roundIndex;
        }

        #endregion

        #region Results

        public GameEventContainer EventContainer { get; private set; }

        public override void HandleResponse( byte[] response )
        {
            var stream = new MemoryStream( response );

            EventContainer = GameEventContainer.Deserialize( stream );
        }

        #endregion
    }

    #endregion

    #region Ranked Score Command

    public class GetRankedScoresCommand : Command
    {
        public override string RelativeUrl
        {
            get { return string.Format( "/rank/{0}", RoundIndex ); }
        }

        #region Arguments

        private int RoundIndex { get; set; }

        public GetRankedScoresCommand(int roundIndex)
        {
            RoundIndex = roundIndex;
        }

        #endregion

        #region Results

        public RankedScoresContainer ScoresContainer { get; private set; }

        public override void HandleResponse(byte[] response)
        {
            var stream = new MemoryStream(response);

            ScoresContainer = RankedScoresContainer.Deserialize(stream);
        }

        #endregion
    }

    #endregion
    
    public class GetCsCollectionCommand : Command
    {
        public override string RelativeUrl { get { return "/cs"; } }

        #region Results

        [JsonProperty( "list" )]
        public List<ServiceIdentifier> ChallengeSets { get; set; }

        #endregion
    }

    public class GetCsInfoCommand : Command
    {
        public override string RelativeUrl { get { return string.Format( "/cs/{0}", CsId ); } }

        #region Arguments

        private uint CsId { get; set; }

        public GetCsInfoCommand( uint csId )
        {
            CsId = csId;
        }

        #endregion

        #region Results

        private ChallengeSetInfo m_CsInfo = new ChallengeSetInfo();
        public ChallengeSetInfo CsInfo { get { return m_CsInfo; } }

        [JsonProperty]
        private string Name { set { CsInfo.Name = value; } }

        [JsonProperty]
        private uint BsId { set { CsInfo.Binary = value; } }

        [JsonProperty]
        private int LinesOfCode { set { CsInfo.LinesOfCode = value; } }

        [JsonProperty]
        private List<string> CWE { set { CsInfo.CWEs = value; } }

        [JsonProperty]
        private List<string> Files { set { CsInfo.Files = value; } }

        [JsonProperty]
        private string Shortname { set { CsInfo.Shortname = value; } }

        [JsonProperty]
        private string Description { set { CsInfo.Description = value; } }

        [JsonProperty]
        private string Readme { set { CsInfo.Readme = value; } }

        [JsonProperty]
        private int Position { set { CsInfo.Position = value; } }

        [JsonProperty]
        private List<string> Tags { set { CsInfo.Tags = value; } }

        #endregion
    }

    public class GetCsRefPatchIdsCommand : Command
    {
        public override string RelativeUrl { get { return string.Format( "/cs/{0}/refpatch", CsId ); } }

        #region Arguments

        private uint CsId { get; set; }

        public GetCsRefPatchIdsCommand( uint csId )
        {
            CsId = csId;
        }

        #endregion

        #region Results

        [JsonProperty("Patches")]
        public List<uint> RefPatchIds { get; set; }

        #endregion
    }
    
    public class GetCsStatsCommand : Command
    {
        public override string RelativeUrl { get { return string.Format("/cs/{0}/stats", CsId); } }

        #region Arguments

        private uint CsId { get; set; }

        public GetCsStatsCommand(uint csId)
        {
            CsId = csId;
        }

        #endregion

        #region Results

        private ChallengeSetStats m_CsStats = new ChallengeSetStats();
        public ChallengeSetStats CsStats { get { return m_CsStats; } }

        [JsonProperty]
        private string Name { set { CsStats.Name = value; } }

        [JsonProperty]
        private uint Binary { set { CsStats.BinarySet = value; } }

        [JsonProperty]
        private int LinesOfCode { set { CsStats.LinesOfCode = value; } }
        
        [JsonProperty]
        private List<string> CWEs { set { CsStats.CWEs= value; } }

        [JsonProperty]
        private int Position { set { CsStats.Position = value; } }

        #endregion
    }

    public class GetBinaryStatsCommand : Command
    {
        public override string RelativeUrl { get { return string.Format("/bin/{0}/stats", BinId); } }

        #region Arguments

        private uint BinId { get; set; }

        public GetBinaryStatsCommand(uint binId)
        {
            BinId = binId;
        }

        #endregion

        #region Results
        
        private BinaryStats m_BinStats = new BinaryStats();
        public BinaryStats BinStats { get { return m_BinStats; } }

        

        [JsonProperty("binid")]
        private uint Binary { set { BinStats.Binary = value; } }

        [JsonProperty]
        private string File { set { BinStats.File = value; } }
        
        [JsonProperty]
        private float Entropy { set { BinStats.Entropy = value; } }
        
        [JsonProperty("byte_histogram")]
        private string ByteHistogram { set { BinStats.SetByteHistogram = value; } }
        
        [JsonProperty("Sections")]
        private string Sections { set { BinStats.SetSections = value; } }

        [JsonProperty("opcode_histogram")]
        private string OpcodeHistogram { set { BinStats.SetOpcodeHistogram = value; } }

        [JsonProperty]
        private uint File_Size { set { BinStats.FileSize = value; } }


        //[JsonProperty]
        //private List<FunctionListing> Functions { set { BinStats.FunctionListings = value; } }

        //[JsonProperty]
        //private List<BlockListing> Blocks { set { BinStats.BlockListings = value; } }

        #endregion
        
    }


    public class GetNextRoundCommand : Command
    {
        public override string RelativeUrl
        {
            get { return string.Format( "/complete/after/{0}", SeekRoundNumber ); }
        }

        #region Arguments

        private int SeekRoundNumber { get; set; }

        public GetNextRoundCommand( int seekRoundNumber )
        {
            SeekRoundNumber = seekRoundNumber;
        }

        #endregion

        #region Results

        public int NextRoundNumber { get; set; }

        [JsonProperty("round")]
        private int NextRound { set { NextRoundNumber = value; } }

#endregion
    }

    public class GetRcsToCsCommand : Command
    {
        public override string RelativeUrl
        {
            get { return String.Format("/rcs/{0}", RcsId); }
        }

        #region Arguments

        private uint RcsId { get; set; }

        public GetRcsToCsCommand(uint rcsId)
        {
            RcsId = rcsId;
            RcsInfo = new RcsToCsInfo();
        }

        #endregion

        #region Results

        public RcsToCsInfo RcsInfo { get; set; }

        
        [JsonProperty]
        private uint Team { set{ RcsInfo.Team = value;}  }

        [JsonProperty]
        private uint CsId { set{ RcsInfo.CsId = value;}  }

        [JsonProperty]
        private uint Round { set{ RcsInfo.Round = value;}  }

        [JsonProperty]
        private uint BsId { set{ RcsInfo.BsId = value;}  }


        #endregion
    }

    public class GetCsRcsCommand : Command
    {
        public override string RelativeUrl
        {
            get { return String.Format( "/cs/{0}/rcs", CsId ); }
        }

        #region Arguments

        private uint CsId { get; set; }

        public GetCsRcsCommand( uint csId )
        {
            CsId = csId;
        }

        #endregion

        #region Results

        public List<ReferenceChallengeSetInfo> ReplacementChallengeSets { get; set; }

        #endregion
    }

    public class GetCsPovsCommand : Command
    {
        public override string RelativeUrl
        {
            get { return string.Format( "/cs/{0}/pov", CsId ); }
        }

        #region Arguments

        private uint CsId { get; set; }

        public GetCsPovsCommand( uint csId )
        {
            CsId = csId;
        }

        #endregion

        #region Results

        [JsonProperty("pov")]
        public List<PovInfo> Povs { get; set; }

        #endregion
    }

    public class GetCsPollsCommand : Command
    {
        public override string RelativeUrl
        {
            get { return string.Format( "/cs/{0}/poll", CsId ); }
        }

        #region Arguments

        private uint CsId { get; set; }

        public GetCsPollsCommand( uint csId )
        {
            CsId = csId;
        }

        #endregion

        #region Results

        [JsonProperty( "poll" )]
        public List<PollInfo> Polls { get; set; }

        #endregion
    }

   

    public class GetPovInfoCommand : Command
    {
        public override string RelativeUrl
        {
            get { return string.Format( "/pov/{0}", PovId ); }
        }

        #region Arguments

        private int PovId { get; set; }

        public GetPovInfoCommand( int povId )
        {
            PovId = povId;
        }

        #endregion

        #region Results

        [JsonProperty("team")]
        private int? NullableTeam { get; set; }

        public int TeamId { get { return NullableTeam ?? 0; } }

        public int CsId { get; set; }

        public string File { get; set; }

        public List< PovSubmissionInfo > Submissions { get; set; }

        #endregion
    }

    public class GetBinsetCommand : Command
    {
        public override string RelativeUrl
        {
            get { return string.Format("/binset/{0}", BsId); }
        }

        #region Arguments

        private uint BsId { get; set; }

        public GetBinsetCommand( uint bsId )
        {
            BsId = bsId;
        }
        #endregion

        #region Results

        [JsonProperty("binId")]
        public List<uint> BinIds { get; set; }

        #endregion

    }

    public class GetBinsetInfoCommand : Command
    {
        public override string RelativeUrl
        {
            get { return string.Format( "/binset/{0}/info", BsId ); }
        }

        #region Arguments

        private int BsId { get; set; }

        public GetBinsetInfoCommand( int bsId )
        {
            BsId = bsId;

            RefPatchBsIds = new List< int >();

            Submissions = new List< BinsetSubmissionInfo >();
        }

        #endregion

        #region Results

        public BinsetType Type { get; set; }

        public int CsId { get; set; }

        [JsonProperty("cs_name")]
        public string CsName { get; set; }

        [JsonProperty("ref_bsid")]
        public int RefBsId { get; set; }

        [JsonProperty("ref_patch")]
        public List<int> RefPatchBsIds { get; set; }

        private List< BinsetSubmissionInfo > m_Submissions;
        public List< BinsetSubmissionInfo > Submissions
        {
            get { return m_Submissions; }
            set { m_Submissions = value; }
        }

        #endregion
    }


    public class GetExecutionIdCommand : Command
    {
        public override string RelativeUrl
        {
            get
            {
                var nature = RequestNature == RequestNature.Pov ? "pov" : "poll";

                return string.Format(
                    (IdsQuery?
                    "/{0}/{1}/idsresult/{2}/{3}/replay":
                    "/{0}/{1}/result/{2}/replay"), nature, RequestId, BinaryId, IdsId );
            }
        }

        #region Arguments

        public RequestNature RequestNature { get; private set; }

        public uint RequestId { get; private set; }

        public uint BinaryId { get; private set; }

        public uint? IdsId { get; private set; }
        private bool IdsQuery { get { return IdsId != null; } }

        public GetExecutionIdCommand( RequestNature requestNature, uint requestId, uint binaryId, int idsId=-1 )
        {
            RequestNature = requestNature;
            RequestId = requestId;
            BinaryId = binaryId;
            if ( idsId > -1 )
                IdsId = (uint)idsId;
            else
                IdsId = null;
        }

        #endregion

        #region Results

        [JsonProperty( "execution" )]
        public uint ExecutionId { get; set; }

        [JsonProperty( "pass" )]
        private bool Passed { get; set; }

        [JsonProperty( "vulnerable" )]
        private bool Vulnerable { get; set; }

        [JsonProperty( "pov_type" )]
        public int PovType { get; set; }

        public bool CsSuccess { get { return RequestNature == RequestNature.Pov ? !Vulnerable : Passed; } }

        #endregion
    }

    public class GetExecutionPerformanceCommand : Command
    {
        public override string RelativeUrl
        {
            get { return string.Format( "/exec/{0}/perf", ExecutionId ); }
        }

        #region Arguments

        private uint ExecutionId { get; set; }

        public GetExecutionPerformanceCommand( uint executionId )
        {
            ExecutionId = executionId;
        }

        #endregion


        #region Results

        [JsonProperty( "mem" )]
        public uint PagesTouched { get; set; }

        [JsonProperty( "cpu" )]
        public ulong InstructionCount { get; set; }

        #endregion
    }

    public class GetTraceFilenamesCommand : PostCommand
    {
        public override string RelativeUrl
        {
            get { return string.Format( "/analyze/request/{0}", ExecutionId ); }
        }

        public override string PostString
        {
            //get { return "{ \"config\" : [ \"itrace,regs,disasm\",\"branchtrace\" ] }"; } // Correct, but crashed api
            get { return "{ \"config\" : [ \"itrace,regs,disasm\" ] }"; }
        }

        public override Dictionary < string, string > Headers
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                };
            }
        }

        #region Arguments

        protected uint ExecutionId { get; set; }

        public GetTraceFilenamesCommand( uint executionId )
        {
            ExecutionId = executionId;
        }

        #endregion

        #region Results

        [JsonProperty("result")]
        public List< string > FileNames { get; set; }

        #endregion
    }

    public class GetMemoryTraceFilenamesCommand : GetTraceFilenamesCommand
    {
        public override string PostString
        {
            //get { return "{ \"config\" : [ \"itrace,regs,disasm\",\"branchtrace\" ] }"; } // Correct, but crashed api
            get { return "{ \"config\" : [ \"itrace,regs,disasm,memtrace\" ] }"; }
        }

        #region Arguments

        public GetMemoryTraceFilenamesCommand(uint executionId) : base(executionId)
        {
        }

        #endregion
    }

    public class GetVariableTraceFilenamesCommand : GetTraceFilenamesCommand
    {
        private string PostArgs { get; set; }

        public override string PostString
        {
            get { return PostArgs; }
        }


        #region Arguments

        public GetVariableTraceFilenamesCommand(uint executionId, 
            string postArgs= "{ \"config\" : [ \"itrace,regs,disasm,memtrace\" ] }"
            ) : base(executionId)
        {
            PostArgs = postArgs;
        }

        #endregion
    }


    public class GetTraceCommand : Command
    {
        public override string RelativeUrl
        {
            get { return string.Format( "/data/{0}", FileName ); }
        }

        #region Arguments

        private string FileName { get; set; }

        public GetTraceCommand( string fileName )
        {
            FileName = fileName;
        }

        #endregion

        #region Results

        public Trace Trace { get; private set; }

        // TODO: What does Ok property mean in this class??
        public override void HandleResponse( byte[] response )
        {
            //// diagnostic //
            ////var text = Encoding.ASCII.GetString(response);

            //var diagstream = new MemoryStream(response);

            //var reader = new BinaryReader( diagstream, Encoding.ASCII );

            //var text = reader.ReadChar()

            ////JsonConvert.PopulateObject(text, this);

            //Debug.Log(text);

            //throw new NotImplementedException();
            //// end diagnostic //

            var stream = new MemoryStream( response );
            
            Trace = Trace.LoadFromStream( stream );
        }

        #endregion
    }

    public class GetTraceFileCommand : Command
    {
        public override string RelativeUrl
        {
            get { return string.Format( "/data/{0}", FileName ); }
        }

        #region Arguments

        private string FileName { get; set; }

        public GetTraceFileCommand( string fileName )
        {
            FileName = fileName;
        }

        #endregion

        #region Results

        public byte[] Bytes { get; private set; }

        // TODO: What does Ok property mean in this class??
        public override void HandleResponse( byte[] response )
        {
            Bytes = response;
        }

        #endregion
    }

   
}