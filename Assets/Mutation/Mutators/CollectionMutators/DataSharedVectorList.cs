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
using Chains;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.CollectionMutators
{
    public class WriteDataSharedVectorList : DataMutator
    {
        public static NodeDataShare<Dictionary<int, Vector3> > DataShare { get; set; }

        private MutableField<string> m_GroupId = new MutableField<string>() 
        { LiteralValue = "Group Id" };
        [Controllable(LabelText = "Group Id")]
        public MutableField<string> GroupId { get { return m_GroupId; } }

        private MutableField<int> m_EntryIndex = new MutableField<int>() 
        { AbsoluteKey = "Entries.Index" };
        [Controllable(LabelText = "Entry Index")]
        public MutableField<int> EntryIndex { get { return m_EntryIndex; } }
        
        private MutableField<Vector3> m_EntryPosition = new MutableField<Vector3>() 
        { AbsoluteKey = "Entries.Position"};
        [Controllable(LabelText = "Entry Position")]
        public MutableField<Vector3> EntryPosition { get { return m_EntryPosition; } }

        public WriteDataSharedVectorList()
        {
            //EntryPosition.SchemaParent = GroupId;
            //EntryPosition.SchemaParent = EntryIndex;
            EntryIndex.SchemaParent = EntryPosition;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            if (DataShare==null)
                DataShare = new NodeDataShare< Dictionary< int, Vector3 > >();
            
            var groupId = GroupId.GetFirstValue( mutable );
                
            if ( !DataShare.ContainsKey( groupId ) )
                DataShare[groupId] = new Dictionary< int, Vector3 >();
            var vectors = DataShare[ groupId ];

            //var vectors = new Dictionary<int, Vector3>();
            foreach ( var subEntry in EntryPosition.GetEntries(mutable) )
            {
                var index = EntryIndex.GetValue( subEntry );
                var position = EntryPosition.GetValue( subEntry );

                vectors[ index ] = position;
            }
            return mutable;
        }
    }

    public class ReadDataSharedVectorList : DataMutator
    {
        private MutableField<string> m_GroupId = new MutableField<string>() 
        { LiteralValue = "Group Id" };
        [Controllable(LabelText = "Group Id")]
        public MutableField<string> GroupId { get { return m_GroupId; } }

        private MutableField<int> m_Index = new MutableField<int>() 
        { AbsoluteKey = "Entries.Index" };
        [Controllable(LabelText = "Index")]
        public MutableField<int> Index { get { return m_Index; } }
        
        private MutableTarget m_VectorTarget = new MutableTarget() 
        { AbsoluteKey = "Entries.Target" };
        [Controllable(LabelText = "Vector Target")]
        public MutableTarget VectorTarget { get { return m_VectorTarget; } }

        public ReadDataSharedVectorList()
        {
            //Index.SchemaParent = GroupId;
            VectorTarget.SchemaParent = Index;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            var groupId = GroupId.GetFirstValue( mutable );
            
            var storeExists = WriteDataSharedVectorList.DataShare != null && WriteDataSharedVectorList.DataShare.ContainsKey(groupId);

            if (!storeExists)
            {
                foreach (var subEntry in Index.GetEntries(mutable))
                    VectorTarget.SetValue(Vector3.zero, subEntry);
                return mutable;
            }
            

            var foundStore = WriteDataSharedVectorList.DataShare[ groupId ];

            foreach ( var subEntry in Index.GetEntries( mutable ) )
            {
                var index = Index.GetValue( subEntry );
                VectorTarget.SetValue( 
                    foundStore.ContainsKey( index ) ? foundStore[ index ] : Vector3.zero,
                    subEntry );
            }

            return mutable;
        }
    }
}
