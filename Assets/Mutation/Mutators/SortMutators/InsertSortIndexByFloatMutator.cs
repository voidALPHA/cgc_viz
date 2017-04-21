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
using Visualizers;

namespace Mutation.Mutators.SortMutators
{
    public class InsertSortIndexByFloatMutator : InsertSortIndexMutator
    {

        private MutableField<float> m_SortDiscriminant = new MutableField<float>()
        { AbsoluteKey = "TotalScore" };
        [Controllable(LabelText = "Sort Discriminant")]
        public MutableField<float> SortDiscriminant { get { return m_SortDiscriminant; } }

        private MutableField<int> m_SecondarySortDiscriminant = new MutableField<int>() 
        { LiteralValue = 1 };
        [Controllable(LabelText = "Secondary Discriminant")]
        public MutableField<int> SecondarySortDiscriminant { get { return m_SecondarySortDiscriminant; } }


        private MutableField<bool> m_SortDescending = new MutableField<bool>()
        { LiteralValue = false };
        [Controllable(LabelText = "Sort Descending")]
        public MutableField<bool> SortDescending { get { return m_SortDescending; } }

        public InsertSortIndexByFloatMutator() : base()
        {
            SortDiscriminant.SchemaParent = Scope;
            SecondarySortDiscriminant.SchemaParent = Scope;
            SortDescending.SchemaPattern = Scope;
        }
        
        protected override void IndexEntries( MutableObject mutable )
        {
            var sortList = new List< List< MutableObject > >();

            foreach (var entry in Scope.GetEntries(mutable))
                sortList.Add( entry.ToList() );

            sortList.Sort(
                ( mut1, mut2 ) =>
                {
                    var sortMult = SortDescending.GetFirstValue( mutable ) ? -1 : 1;
                    var sortResult = SortDiscriminant.GetValue( mut1 ).CompareTo( SortDiscriminant.GetValue( mut2 ) );
                    if ( sortResult == 0 )
                        sortResult =
                            SecondarySortDiscriminant.GetValue( mut1 )
                                .CompareTo( SecondarySortDiscriminant.GetValue( mut2 ) );
                    return sortResult * sortMult;
                } );
            
            int i = 0;
            foreach ( var element in sortList)
            {
                SortIndexTarget.SetValue( i++, element);
            }
        }
    }
}
