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
using Mutation;
using Newtonsoft.Json;
using UnityEngine;
using Visualizers;

namespace GroupSplitters
{
    public class SelectEntriesAroundIndexSplitter : GroupSplitter
    {
        private MutableField<int> m_CenterIndex = new MutableField<int>() 
        { LiteralValue = 0 };
        [Controllable(LabelText = "Center Index to Select")]
        private MutableField<int> CenterIndex { get { return m_CenterIndex; } }


        private MutableField<int> m_SelectionWidth = new MutableField<int>() 
        { LiteralValue = 3 };
        [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
        private MutableField<int> SelectionWidth
        {
            get { return m_SelectionWidth; }
            set
            {
                m_SelectionWidth = value;

                switch ( value.SchemaSource )
                {
                    case SchemaSource.Global:
                        SelectBeforeCount.GlobalParameterKey = value.GlobalParameterKey;
                        SelectAfterCount.GlobalParameterKey = value.GlobalParameterKey;
                        break;
                    case SchemaSource.Literal:
                        SelectBeforeCount.LiteralValue = value.LiteralValue;
                        SelectAfterCount.LiteralValue = value.LiteralValue;
                        break;
                    case SchemaSource.Cached:
                    case SchemaSource.Mutable:
                        SelectBeforeCount.AbsoluteKey = value.AbsoluteKey;
                        SelectBeforeCount.SchemaSource = value.SchemaSource;

                        SelectAfterCount.AbsoluteKey = value.AbsoluteKey;
                        SelectAfterCount.SchemaSource = value.SchemaSource;
                        break;
                }
                SelectBeforeCount.AbsoluteKey = value.AbsoluteKey;
                SelectBeforeCount.LiteralValue = value.LiteralValue;
            }
        }

        private MutableField< int > m_SelectBeforeCount = new MutableField< int >()
        { LiteralValue = 3 };
        [Controllable( LabelText = "SelectBeforeCount" )]
        private MutableField< int > SelectBeforeCount { get { return m_SelectBeforeCount; } }

        private MutableField< int > m_SelectAfterCount = new MutableField< int >()
        { LiteralValue = 3 };
        [Controllable( LabelText = "SelectAfterCount" )]
        private MutableField< int > SelectAfterCount { get { return m_SelectAfterCount; } }


        private MutableField< bool > m_MaintainCountAtEdgesField = new MutableField<bool> { LiteralValue = false };
        [Controllable(LabelText = "MaintainCountAtEdges")]
        private MutableField< bool > MaintainCountAtEdgesField { get { return m_MaintainCountAtEdgesField; } }

        private MutableField<bool> m_SelectedListOnly = new MutableField<bool>()
        { LiteralValue = false };
        [Controllable(LabelText = "Selected List Only")]
        public MutableField<bool> SelectedListOnly { get { return m_SelectedListOnly; } }


        protected override void SelectGroups( List<MutableObject> entry )
        {
            SelectedList.Clear();
            UnSelectedList.Clear();

            var maintainCountAtEdges = MaintainCountAtEdgesField.GetLastKeyValue( entry.Last() );

            var entries = EntryField.GetEntries( entry );
            var entriesCount = entries.Count();
            
            if ( entriesCount == 0 )
                Debug.LogError( "What?  Empty entries?" );

            var beforeWidth = SelectBeforeCount.GetLastKeyValue( entry.Last() );
            var afterWidth = SelectAfterCount.GetLastKeyValue( entry.Last() );

            var lowerBound = maintainCountAtEdges?Mathf.Min(beforeWidth, entriesCount-1):0;
            var upperBound = maintainCountAtEdges ? Mathf.Max(entriesCount - afterWidth - 1,0) : entriesCount - 1;

            if ( lowerBound >= upperBound )
            {
                // use the lower of the two values for both bounds to avoid overlap
                lowerBound = Mathf.Min( lowerBound, upperBound );
                upperBound = Mathf.Min(lowerBound, upperBound);
            }

            var index = Mathf.Clamp( CenterIndex.GetLastKeyValue( entry.Last() ),
                lowerBound, upperBound);
            
            var minIndex = Mathf.Max( index - beforeWidth, 0 );
            var maxIndex = Mathf.Min( index + afterWidth, entriesCount - 1 );

            //var i = 0;
            //foreach ( var entry in entries )
            //{
            //    if ( i < minIndex )
            //    UnSelectedList.
            //}

            bool selectedListOnly = SelectedListOnly.GetValue( entry );

            int currentIndex = 0;

            if ( selectedListOnly )
            {
                foreach ( var subEntry in EntryField.GetEntries( entry ) )
                {
                    if (currentIndex > maxIndex)
                        break;
                    if (currentIndex >= minIndex)
                        SelectedList.Add(subEntry.Last() );
                    currentIndex++;
                }
            }
            else
            {
                foreach ( var subEntry in EntryField.GetEntries( entry ) )
                {
                    if ( currentIndex < minIndex || currentIndex > maxIndex )
                        UnSelectedList.Add( subEntry.Last() );
                    else
                        SelectedList.Add( subEntry.Last() );

                    currentIndex++;
                }
            }
        }
    }
}
