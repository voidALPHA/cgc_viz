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
using Mutation;
using Visualizers;

namespace GroupSplitters
{
    public class IsCommsSplitter : GroupSplitter
    {
        private MutableField<string> m_AnnotationTypeProperty = new MutableField<string>() { AbsoluteKey = "Instructions.AnnotationType" };
        [Controllable( LabelText = "Annotation type string" )]
        public MutableField<string> AnnotationTypeProperty { get { return m_AnnotationTypeProperty; } }

        
        public IsCommsSplitter()
        {
            AnnotationTypeProperty.SchemaParent = EntryField;
        }

        protected override void SelectGroups( List<MutableObject> entry )
        {
            SelectedList = new List<MutableObject>();
            UnSelectedList = new List<MutableObject>();

            foreach ( var element in EntryField.GetEntries( entry ) )
            {
                if ( IsComms( element ) )
                    SelectedList.Add( element.Last() );
                else
                    UnSelectedList.Add(element.Last());
            }
        }

        protected bool IsComms( List<MutableObject> element )
        {
            if ( !AnnotationTypeProperty.CouldResolve( element ) )
                return false;

            var elementValue = AnnotationTypeProperty.GetValue( element );

            return elementValue.ToLower() == "transmit" || elementValue == "receive";
        }
    }
}