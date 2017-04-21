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
using Visualizers;

namespace Mutation.Mutators.ArithmeticOperators
{
    public class GetOtherIndicesMutator : DataMutator
    {
        private MutableScope m_Scope = new MutableScope();
        [Controllable(LabelText = "Scope")]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableField< int > m_OwnIndex = new MutableField< int >()
        { LiteralValue = 3 };
        [Controllable( LabelText = "Own Index" )]
        public MutableField< int > OwnIndex
        {
            get { return m_OwnIndex; }
        }

        private MutableField< int > m_NumberOfIndeces = new MutableField< int >()
        { LiteralValue = 7 };

        [Controllable( LabelText = "NumberOfIndeces" )]
        public MutableField< int > NumberOfIndeces
        {
            get { return m_NumberOfIndeces; }
        }

        private MutableTarget m_IndecesTarget = new MutableTarget() 
        { AbsoluteKey = "Other Indeces" };
        [Controllable(LabelText = "Indeces Target")]
        public MutableTarget IndecesTarget { get { return m_IndecesTarget; } }

        public GetOtherIndicesMutator()
        {
            OwnIndex.SchemaParent = Scope;
            NumberOfIndeces.SchemaParent = Scope;
            IndecesTarget.SchemaParent = Scope;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {
                var otherIndeces = new List< MutableObject >();

                var ownIndex = OwnIndex.GetFirstValueBelowArrity( entry );

                for (int i = 1; i <= NumberOfIndeces.GetFirstValueBelowArrity(entry); i++)
                {
                    if ( i != ownIndex) 
                        otherIndeces.Add( new MutableObject(){{"Index", i}} );
                }

                IndecesTarget.SetValue( otherIndeces, entry );
            }

            return mutable;
        }
    }
}
