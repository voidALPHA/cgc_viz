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

using Chains;
using Visualizers;

namespace Mutation.Mutators.ArithmeticOperators
{
    public class DataSharedDiffEntries : DataMutator
    {

        private MutableField<float> m_DiffValue = new MutableField<float>() { AbsoluteKey = "Total Score" };
        [Controllable(LabelText = "Diff Value")]
        public MutableField<float> DiffValue { get { return m_DiffValue; } }

        private MutableTarget m_DiffTarget = new MutableTarget() 
        { AbsoluteKey = "Diff Result" };
        [Controllable(LabelText = "Diff Target")]
        public MutableTarget DiffTarget { get { return m_DiffTarget; } }


        private MutableField<string> m_GroupId = new MutableField<string>() { LiteralValue = "" };
        [Controllable(LabelText = "Shared Group Id")]
        public MutableField<string> GroupId
        {
            get { return m_GroupId; }
        }

        private static NodeDataShare<float> m_DataShare
            = new NodeDataShare<float>();
        public static NodeDataShare<float> DataShare
        { get { return m_DataShare; } set { m_DataShare = value; } }

        public DataSharedDiffEntries() : base()
        {
            DiffTarget.SchemaParent = DiffValue;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            var groupId = GroupId.GetFirstValue(mutable);

            if ( groupId == "" )
                DataShare.Remove(groupId);

            foreach ( var entry in DiffValue.GetEntries( mutable ) )
            {
                if ( DataShare.ContainsKey( groupId ) )
                {
                    DiffTarget.SetValue(
                        DiffValue.GetValue(entry) - DataShare[groupId], entry);
                    DataShare[groupId] = DiffValue.GetValue(entry);
                }
                else
                {
                    DiffTarget.SetValue(0f, entry);
                    DataShare[groupId] = DiffValue.GetValue(entry);
                }
            }

            return mutable;
        }


        public override void Unload()
        {
            DataShare.Clear();

            base.Unload();
        }

    }
}
