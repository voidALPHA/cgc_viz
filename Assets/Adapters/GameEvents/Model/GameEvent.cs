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
using System.Linq;
using System.Runtime.Serialization;

namespace Adapters.GameEvents.Model
{
    public class Round
    {
        public int RoundIndex { get; set; }

        public PreRoundEventCollection PreRoundEvents { get; set; }

        public GameEventCollection GameEvents { get; set; }
        
        //public IEnumerable<GameEvent> GetAllEventsEnumerator()
        //{
        //    foreach ( var poll in PreRoundEvents.GetAllEventsEnumerator() )
        //        yield return poll;

        //    foreach ( var povSubmission in GameEvents.GetAllEventsEnumerator() )
        //        yield return povSubmission;
        //}

        public static Round GetTestRound()
        {
            var newRound = new Round();

            newRound.GameEvents = new GameEventCollection();

            newRound.GameEvents.Polls = new List< PollEvent >(){new PollEvent() };
            newRound.GameEvents.PovSubmission = new List<PovSubmissionEvent>(){ new PovSubmissionEvent() };
            newRound.GameEvents.PovThrow = new List<PovThrowEvent>(){ new PovThrowEvent() };
            newRound.GameEvents.PovNotthrown = new List<PovNotThrownEvent>(){ new PovNotThrownEvent() };
            newRound.GameEvents.RcsSubmission = new List<RcsSubmissionEvent>(){ new RcsSubmissionEvent() };
            newRound.GameEvents.RcsDeployed = new List<RcsDeployedEvent>(){ new RcsDeployedEvent() };
            newRound.GameEvents.NetworkRuleSubmission = new List<NetworkRuleSubmissionEvent>(){ new NetworkRuleSubmissionEvent() };
            newRound.GameEvents.NetworkRuleDeployed = new List<NetworkRuleDeployedEvent>(){ new NetworkRuleDeployedEvent() };
            newRound.GameEvents.CsOffline = new List<CsOfflineEvent>(){ new CsOfflineEvent() };
            
            newRound.PreRoundEvents = new PreRoundEventCollection();
            newRound.PreRoundEvents.CsAdded = new List<CsAddedEvent>() { new CsAddedEvent() };
            newRound.PreRoundEvents.CsRemoved = new List<CsRemovedEvent>() { new CsRemovedEvent() };


            return newRound;
        }
    }

    public class PreRoundEventCollection
    {
        public List<CsAddedEvent> CsAdded { get; set; }

        public List<CsRemovedEvent> CsRemoved { get; set; }

        public IEnumerable<GameEvent> GetAllEventsEnumerator()
        {
            foreach ( var cs in CsAdded )
                yield return cs;

            foreach ( var cs in CsRemoved )
                yield return cs;
        }
    }

    public class GameEventCollection
    {
        public List<PollEvent> Polls { get; set; }

        public List<PovSubmissionEvent> PovSubmission { get; set; }

        public List<PovThrowEvent> PovThrow { get; set; }

        public List<PovNotThrownEvent> PovNotthrown { get; set; }

        public List<RcsSubmissionEvent> RcsSubmission { get; set; }

        public List<RcsDeployedEvent> RcsDeployed { get; set; }

        public List<NetworkRuleSubmissionEvent> NetworkRuleSubmission { get; set; }

        public List<NetworkRuleDeployedEvent> NetworkRuleDeployed { get; set; }

        public List<CsOfflineEvent> CsOffline { get; set; }
        
        public IEnumerable< GameEvent > GetAllEventsEnumerator()
        {
            foreach ( var poll in Polls )
                yield return poll;

            foreach ( var povSubmission in PovSubmission )
                yield return povSubmission;

            foreach ( var povThrow in PovThrow )
                yield return povThrow;

            foreach ( var povNotthrown in PovNotthrown )
                yield return povNotthrown;

            foreach ( var rcsSubmission in RcsSubmission )
                yield return rcsSubmission;

            foreach ( var rcsDeployed in RcsDeployed )
                yield return rcsDeployed;

            foreach ( var networkRuleSubmission in NetworkRuleSubmission )
                yield return networkRuleSubmission;

            foreach ( var networkRuleDeployed in NetworkRuleDeployed )
                yield return networkRuleDeployed;

            foreach ( var csOffline in CsOffline )
                yield return csOffline;
        }
    }


    public abstract class GameEvent
    {
        public double Timestamp { get; set; }
    }


    public abstract class PreRoundEvent : GameEvent
    {
    }


    public class CsAddedEvent : PreRoundEvent
    {
        public long Csid { get; set; }
    }

    public class CsRemovedEvent : PreRoundEvent
    {
        public long Csid { get; set; }
    }



    public class PollEvent : GameEvent
    {
        public float Duration { get; set; }

        public long Pollid { get; set; }

        public int TargetTeam { get; set; }

        public long TargetCsid { get; set; }

        public long TargetBsid { get; set; }

        public EventResult Result { get; set; }

        public EventNdsResult Nds { get; set; }
    }

    public class PovSubmissionEvent : GameEvent
    {
        public long Povid { get; set; }

        public int SourceTeam { get; set; }

        public int TargetTeam { get; set; }

        public long TargetCsid { get; set; }

        public long TargetBsid { get; set; }

        public int ThrowCount { get; set; }
    }

    public class PovThrowEvent : GameEvent
    {
        public float Duration { get; set; }

        public long Povid { get; set; }

        public int Submission { get; set; }

        public int SourceTeam { get; set; }

        public int TargetTeam { get; set; }

        public long TargetCsid { get; set; }

        public long TargetBsid { get; set; }

        public EventResult Result { get; set; }

        public EventNdsResult Nds { get; set; }

        public NegotiateType Negotiate { get; set; }

        // DELETE THIS THIS IS JUT TO TEST WITHOEUT ERROR this should not be in json but is.
        public int ThrowCount { get; set; }
    }

    public class PovNotThrownEvent : GameEvent
    {
        public long Povid { get; set; }

        public int Submission { get; set; }

        public int SourceTeam { get; set; }

        public int TargetTeam { get; set; }

        public int TargetCsid { get; set; }
    }

    public class RcsSubmissionEvent : GameEvent
    {
        public long Csid { get; set; }

        public int Team { get; set; }

        public long Rcsid { get; set; }

        public long Bsid { get; set; }
    }

    public class RcsDeployedEvent : GameEvent
    {
        public long Csid { get; set; }

        public int Team { get; set; }

        public long Bsid { get; set; }
    }

    public class NetworkRuleSubmissionEvent : GameEvent
    {
        public long Csid { get; set; }

        public int Team { get; set; }

        public long Ruleid { get; set; }
    }

    public class NetworkRuleDeployedEvent : GameEvent
    {
        public long Csid { get; set; }

        public int Team { get; set; }

        public long Ruleid { get; set; }
    }

    public class CsOfflineEvent : GameEvent
    {
        public long Csid { get; set; }

        public int Team { get; set; }

        public CsOfflineReason Reason { get; set; }
    }

    public enum EventResult
    {
        Fail,
        Succeed,
        Exception
    }

    public enum EventNdsResult
    {
        Pass,
        Block,
        Log
    }

    public enum NegotiateType
    {
        Type1,
        Type2,
        Fail
    }

// PREVIOUS:   [ EnumMember (Value = "network_rule")]  // Doesn't work without using data contract serialization?
    public enum CsOfflineReason
    {
        Ids,
        Rcs,
        Both
    }
}
