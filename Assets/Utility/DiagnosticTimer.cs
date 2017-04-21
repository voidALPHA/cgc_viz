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


using System.Diagnostics;

namespace VoidAlpha.Utilities
{
    public class DiagnosticTimer
    {
        public struct TimingReturn
        {
            public float Seconds;
            public string Conclusion;
        }

        public Stopwatch TimingWatch
        {
            get;
            private set;
        }


        private string m_ConclusionString = "";
        public string ConclusionString
        {
            get { return m_ConclusionString; }
            set { m_ConclusionString = value; }
        }

        public static DiagnosticTimer Start(string conclusionString)
        {
            return new DiagnosticTimer(conclusionString);
        }

        public DiagnosticTimer(string conclusionString = "")
        {
            TimingWatch = new Stopwatch();
            ConclusionString = conclusionString;

            TimingWatch.Start();
        }

        public TimingReturn Stop()
        {
            TimingWatch.Stop();
            TimingReturn endTiming;
            endTiming.Seconds = TimingWatch.ElapsedMilliseconds / 1000f;
            endTiming.Conclusion = ConclusionString + " " + endTiming.Seconds + " seconds.";
            return endTiming;
        }
    }
}
