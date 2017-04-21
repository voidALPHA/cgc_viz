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
using Chains;
using Visualizers;

namespace Mutation.Mutators
{
    public class InsertDataSharedIndex : DataMutator
    {

        private MutableTarget m_IndexTarget = new MutableTarget() 
        { AbsoluteKey = "Index" };
        [Controllable(LabelText = "Index Field Target")]
        public MutableTarget IndexTarget { get { return m_IndexTarget; } }

        private MutableField<string> m_GroupId = new MutableField<string>() 
        { LiteralValue = "" };
        [Controllable(LabelText = "GroupId")]
        public MutableField<string> GroupId { get { return m_GroupId; } }


        private static NodeDataShare<int> m_DataShare
            = new NodeDataShare<int>();
        public static NodeDataShare<int> DataShare
        { get { return m_DataShare; } set { m_DataShare = value; } }

        public InsertDataSharedIndex() : base()
        {
            GroupId.SchemaPattern = IndexTarget;
        }


        public override void Unload()
        {
            DataShare.Clear();

            base.Unload();
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            foreach (var entry in IndexTarget.GetEntries( mutable )){

                var groupId = GroupId.GetValue(entry);

                var index = ( DataShare.ContainsKey( groupId )
                    ? DataShare[ groupId ]
                    : 0 );

                if ( groupId != "" )
                    DataShare[ groupId ] = index + 1;

                IndexTarget.SetValue(index, entry);
            }

            return mutable;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            IndexTarget.SetValue(0, newSchema);

            Router.TransmitAllSchema(newSchema);
        }
    }
}
