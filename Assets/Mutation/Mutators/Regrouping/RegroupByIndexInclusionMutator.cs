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
using Visualizers;

namespace Mutation.Mutators.Regrouping
{
    public class RegroupByIndexInclusionMutator : Mutator
    {
        private MutableScope m_MetaScope = new MutableScope() { AbsoluteKey = "" };
        [Controllable(LabelText = "Scope")]
        public MutableScope MetaScope { get { return m_MetaScope; } }


        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Element List Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField<int> m_Discriminant = new MutableField<int>()
        { AbsoluteKey = "Scores.TeamID" };
        [Controllable(LabelText = "Discriminant")]
        public MutableField<int> Discriminant { get { return m_Discriminant; } }

        private MutableTarget m_EntriesTarget = new MutableTarget()
        { AbsoluteKey = "TeamScores" };
        [Controllable(LabelText = "Entries Target")]
        public MutableTarget EntriesTarget { get { return m_EntriesTarget; } }

        private MutableField<string> m_CsvRangeOrKeyword = new MutableField<string>() 
        { LiteralValue = "All" };
        [Controllable(LabelText = "Csv, Range, Or All")]
        public MutableField<string> CsvRangeOrKeyword { get { return m_CsvRangeOrKeyword; } }

        public RegroupByIndexInclusionMutator()
        {
            Scope.SchemaParent = MetaScope;
            Discriminant.SchemaParent = Scope;
            EntriesTarget.SchemaParent = MetaScope;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            if (MutableObject.KeyAContainsB(EntriesTarget.AbsoluteKey, Scope.AbsoluteKey))
                throw new Exception("This scope combination will result in an infinitely nested mutable object.  Keys are " + EntriesTarget.AbsoluteKey + " and " + Scope.AbsoluteKey);

            foreach (var entry in MetaScope.GetEntries(newSchema))
            {
                EntriesTarget.SetValue(
                    new List<MutableObject> { Scope.GetEntries(entry).First().Last()}, entry);
            }

            Router.TransmitAllSchema(newSchema);
        }


        public static Func< int, bool > InclusionFunc(string inclusionString)
        {
            int csvFirst = 0;

            if (inclusionString.ToLower() == "all")
            {
                // inclusionFunc = all
            }
            else if (inclusionString.ToLower() == "none")
            {
                return index => false;
            }
            else if (int.TryParse(inclusionString, out csvFirst))
            {
                return index => index == csvFirst;
            }
            else if (inclusionString.Contains(','))
            {
                var localCsv = (from str in inclusionString.Split(',') select int.Parse(str.Trim())).ToList();
                return index => IndexInCsv(index, localCsv);
            }
            else if (inclusionString.Contains('.'))
            {
                var splitPosition = inclusionString.IndexOf("..", StringComparison.InvariantCultureIgnoreCase);

                var startIndex = int.Parse(inclusionString.Substring(0, splitPosition).Trim());
                var endIndex = int.Parse(inclusionString.Substring(splitPosition + 2).Trim());

                return index => IndexInRange(index, startIndex, endIndex);
            }
            else
            {
                throw new Exception("Cannot understand parsing string " + inclusionString + "!");
            }

            return index => true;
        }

        private List<MutableObject> DiscriminatedList(List<MutableObject> mutable)
        {
            var topLevelEntryListing = new List<MutableObject>();

            var inclusionString = CsvRangeOrKeyword.GetValue( mutable ).Trim();

            var inclusionFunc = InclusionFunc( inclusionString );

            foreach (var entry in Scope.GetEntries(mutable))
            {
                var discriminant = Discriminant.GetFirstValueBelowArrity(entry);

                if (inclusionFunc(discriminant))
                    topLevelEntryListing.Add( entry.Last() );
            }

            return topLevelEntryListing;
        }



        private static bool IndexInRange( int index, int min, int max )
        {
            return index >= min && index <= max;
        }

        private static bool IndexInCsv( int index, List< int > csv )
        {
            return csv.Contains( index );
        }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            if (MutableObject.KeyAContainsB(EntriesTarget.AbsoluteKey, Scope.AbsoluteKey))
                throw new Exception("This scope combination will result in an infinitely nested mutable object.  Keys are " + EntriesTarget.AbsoluteKey + " and " + Scope.AbsoluteKey);

            foreach (var entry in MetaScope.GetEntries(payload.Data))
                EntriesTarget.SetValue(DiscriminatedList(entry), entry);

            var iterator = Router.TransmitAll(payload);
            while (iterator.MoveNext())
                yield return null;
        }
    }
}
