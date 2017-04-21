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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Visualizers;

namespace Mutation.Mutators.Regrouping
{
    public abstract class LeftJoinListsMutator<T> : Mutator
    {
        private MutableScope m_LeftListScope = new MutableScope() {AbsoluteKey = "Entries"};
        [Controllable(LabelText = "Left List Scope")]
        public MutableScope LeftListScope { get { return m_LeftListScope; } }

        private MutableField<T> m_LeftDiscriminant = new MutableField<T>() 
        { AbsoluteKey= "Entries.ID" };
        [Controllable(LabelText = "Left Discriminant")]
        public MutableField<T> LeftDiscriminant { get { return m_LeftDiscriminant; } }
        
        private MutableTarget m_JoinElementTarget = new MutableTarget() 
        { AbsoluteKey = "Entries.Element" };
        [Controllable(LabelText = "JoinElementTarget")]
        public MutableTarget JoinElementTarget { get { return m_JoinElementTarget; } }

        
        private MutableScope m_RightListEntries = new MutableScope() {AbsoluteKey = "Second List"};
        [Controllable(LabelText = "RightListEntries")]
        public MutableScope RightListEntries { get { return m_RightListEntries; } }

        private MutableField<T> m_RightDiscriminant = new MutableField<T>() 
        { AbsoluteKey = "Second List.ID" };
        [Controllable(LabelText = "Right Discriminant")]
        public MutableField<T> RightDiscriminant { get { return m_RightDiscriminant; } }


        public LeftJoinListsMutator()
        {
            JoinElementTarget.SchemaParent = LeftListScope;
            LeftDiscriminant.SchemaParent = LeftListScope;
            
            RightDiscriminant.SchemaParent = RightListEntries;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            foreach ( var entry in LeftListScope.GetEntries( newSchema ) )
                JoinElementTarget.SetValue( RightListEntries.GetEntries( newSchema ).First().Last(), entry );

            base.OnProcessOutputSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var rightListDictionary = new Dictionary<T, MutableObject>();
            foreach ( var entry in RightListEntries.GetEntries( payload.Data ) )
            {
                var discriminant = RightDiscriminant.GetValue( entry );
                var mutable = entry.Last();
                rightListDictionary.Add( discriminant, mutable );
            }

            foreach ( var entry in LeftListScope.GetEntries( payload.Data ) )
            {
                var discriminant = LeftDiscriminant.GetValue( entry );
                var foundMutable = new MutableObject();
                if ( rightListDictionary.ContainsKey( discriminant ) )
                    foundMutable = rightListDictionary[ discriminant ];
                JoinElementTarget.SetValue( foundMutable, entry );
            }

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
