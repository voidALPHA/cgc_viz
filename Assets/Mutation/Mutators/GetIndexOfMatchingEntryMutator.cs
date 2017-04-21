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
using JetBrains.Annotations;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators
{
    [UsedImplicitly]
    public class GetIndexOfMatchingEntryMutator : DataMutator
    {

        #region Comparison Type


        private enum ComparisonType
        {
            //
            // Looking to steal this for another class? IfCompareMutator might be a better candidate (omits LastLessThanOrEqual).
            //

            // Firsts
            Equal,
            NotEqual,
            GreaterThan,
            LessThan,
            GreaterThanEqual,
            LessThanEqual,

            // Lasts
            LastLessThanOrEqual
        }


        private MutableField< ComparisonType > m_ComparisonTypeField = new MutableField< ComparisonType > { LiteralValue = ComparisonType.Equal };
        [Controllable( LabelText = "Comparison Type" )]
        private MutableField< ComparisonType > ComparisonTypeField { get { return m_ComparisonTypeField; } }

        private readonly Dictionary< ComparisonType, Func< int, int, bool > > m_ComparisonPredicates = new Dictionary< ComparisonType, Func< int, int, bool > >
        {
            { ComparisonType.Equal, (a, b) => a == b },
            { ComparisonType.NotEqual, (a, b) => a != b },
            { ComparisonType.GreaterThan, (a, b) => a > b },
            { ComparisonType.LessThan, (a, b) => a < b },
            { ComparisonType.GreaterThanEqual, (a, b) => a >= b },
            { ComparisonType.LessThanEqual, (a, b) => a <= b },
        };
        private Dictionary<ComparisonType, Func<int, int, bool>> ComparisonPredicates { get { return m_ComparisonPredicates; } }

        #endregion


        private MutableScope m_Scope = new MutableScope();
        [Controllable( LabelText = "Array Parent Scope" )]
        private MutableScope Scope { get { return m_Scope; } }


        private MutableField<int> m_IntToMatch = new MutableField<int>() { LiteralValue = 0 };
        [Controllable( LabelText = "Int To Match" )]
        private MutableField<int> IntToMatch { get { return m_IntToMatch; } }


        private MutableField<int> m_PerEntryInt = new MutableField<int>() { AbsoluteKey = "Instructions.InstructionIndex" };
        [Controllable( LabelText = "Per Entry Int" )]
        private MutableField<int> PerEntryInt { get { return m_PerEntryInt; } }

        private MutableTarget m_FoundIndexTarget = new MutableTarget() { AbsoluteKey = "MatchingIndex" };
        [Controllable( LabelText = "Matching Index Target" )]
        private MutableTarget FoundIndexTarget { get { return m_FoundIndexTarget; } }


        public GetIndexOfMatchingEntryMutator()
        {
            FoundIndexTarget.SchemaParent = Scope;

            IntToMatch.SchemaParent = Scope;

            PerEntryInt.SchemaParent = Scope;

            ComparisonTypeField.SchemaParent = Scope;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            FoundIndexTarget.SetValue( 0, newSchema );

            Router.TransmitAllSchema( newSchema );
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {
                var valueToMatch = IntToMatch.GetValue( entry );
                var found = false;
                var indexOfMatchingElement = 0;
                var comparisonType = ComparisonTypeField.GetValue( entry );

                // Process "Lasts"
                if ( comparisonType == ComparisonType.LastLessThanOrEqual )
                {
                    var exceeded = false;

                    foreach ( var subEntry in PerEntryInt.GetEntries( entry ) )
                    {
                        var valueWeFound = PerEntryInt.GetValue( subEntry );

                        if ( valueWeFound > valueToMatch )
                        {
                            exceeded = true;

                            indexOfMatchingElement--;

                            if ( indexOfMatchingElement > -1 )
                                found = true;

                            break;
                        }

                        indexOfMatchingElement++;
                    }

                    if ( !exceeded )
                        indexOfMatchingElement--;
                }
                
                // Process "Firsts"
                else
                {

                    foreach ( var subEntry in PerEntryInt.GetEntries( entry ) )
                    {
                        var valueWeFound = PerEntryInt.GetValue( subEntry );

                        found = ComparisonPredicates[ comparisonType ]( valueWeFound, valueToMatch );

                        if ( found )
                            break;

                        indexOfMatchingElement++;
                    }
                }

                if ( !found )
                    indexOfMatchingElement = -1;

                FoundIndexTarget.SetValue( indexOfMatchingElement, entry );

                //Debug.LogFormat( "Found index of element: {0}", indexOfMatchingElement );

            }

            return mutable;
        }
    }
}