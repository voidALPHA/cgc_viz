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
using Newtonsoft.Json.Utilities;
using Utility;
using Visualizers;

namespace Mutation.Mutators.Regrouping
{
    public class RegroupByDistinctIndexMutator : Mutator
    {
        private MutableScope m_MetaScope = new MutableScope() {AbsoluteKey = ""};
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

        public RegroupByDistinctIndexMutator() : base()
        {
            Scope.SchemaParent = MetaScope;
            Discriminant.SchemaParent = Scope;
            EntriesTarget.SchemaParent = MetaScope;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            if (MutableObject.KeyAContainsB(EntriesTarget.AbsoluteKey, Scope.AbsoluteKey))
                throw new Exception("This scope combination will result in an infinitely nested mutable object.  Keys are " + EntriesTarget.AbsoluteKey + " and " + Scope.AbsoluteKey);

            foreach ( var entry in MetaScope.GetEntries( newSchema ) )
            {
                EntriesTarget.SetValue( DiscriminatedList(entry), entry);
            }

            Router.TransmitAllSchema(newSchema);
        }

        private List< MutableObject > DiscriminatedList( List<MutableObject> mutable )
        {

            var topLevelEntryListing = new List<MutableObject>();

            var entryLists = new Dictionary<int, List<MutableObject>>();

            foreach (var entry in Scope.GetEntries(mutable))
            {
                var discriminant = Discriminant.GetFirstValueBelowArrity(entry);

                if (!entryLists.ContainsKey(discriminant))
                {
                    var newMutable = new MutableObject();

                    var newList = new List<MutableObject>();

                    newMutable.Add("Entries", newList);

                    topLevelEntryListing.Add(newMutable);

                    entryLists[discriminant] = newList;
                }

                var foundList = entryLists[discriminant];

                foundList.Add(entry.Last());
            }

            return topLevelEntryListing;
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            if (MutableObject.KeyAContainsB(EntriesTarget.AbsoluteKey, Scope.AbsoluteKey))
                throw new Exception("This scope combination will result in an infinitely nested mutable object.  Keys are " + EntriesTarget.AbsoluteKey + " and " + Scope.AbsoluteKey);

            foreach (var entry in MetaScope.GetEntries(payload.Data))
                EntriesTarget.SetValue(DiscriminatedList(entry), entry);

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
