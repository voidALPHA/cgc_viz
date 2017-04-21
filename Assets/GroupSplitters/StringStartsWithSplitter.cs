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
using System.Globalization;
using System.Linq;
using Mutation;
using UnityEngine;
using Visualizers;

namespace GroupSplitters
{
    public class StringStartsWithSplitter : GroupSplitter
    {
        private MutableField<string> m_StringProperty = new MutableField<string>() { AbsoluteKey = "Prefix" };
        [Controllable( LabelText = "String from each entry" )]
        public MutableField<string> StringProperty { get { return m_StringProperty; } }

        private MutableField<string> m_StringArgument = new MutableField<string>() { LiteralValue = "Transmit ,Send " };
        [Controllable( LabelText = "Comma Sep'd Strings" )]
        public MutableField<string> StringArgument { get { return m_StringArgument; } }

        //private MutableField<bool> m_TruncateMatchField = new MutableField<bool>() { LiteralValue = false };
        //[Controllable( LabelText = "Truncate Matched Text" )]
        //public MutableField<bool> TruncateMatchField{ get { return m_TruncateMatchField; } }


        public StringStartsWithSplitter()
        {
            StringArgument.SchemaPattern = EntryField;
            StringProperty.SchemaParent = EntryField;
        }

        protected override void SelectGroups( List<MutableObject> entry )
        {
            SelectedList = new List<MutableObject>();
            UnSelectedList = new List<MutableObject>();

            var stringArg = StringArgument.GetValue( entry );

            var stringTokens = stringArg.Split( ',' );

            foreach ( var subEntry in EntryField.GetEntries( entry ) )
            {
                if ( IsSelected( subEntry, stringTokens ) )
                    SelectedList.Add(subEntry.Last());
                else
                    UnSelectedList.Add(subEntry.Last());
            }

            //Debug.LogFormat( "Selected {0}, did not select {1}", SelectedList.Count, UnSelectedList.Count );
        }

        private bool IsSelected( List<MutableObject> entry, IEnumerable< string > tokens  )
        {
            if ( !StringArgument.CouldResolve( entry ) )
                return false;

            foreach ( var t in tokens )
            {
                var str = StringProperty.GetValue( entry );

                if ( str.StartsWith( t, true, CultureInfo.InvariantCulture ) )
                    return true;
            }

            return false;
        }
    }
}