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
using JetBrains.Annotations;
using Visualizers;

namespace Mutation.Mutators.Axes.UniqueValueAxes
{
    public abstract class RepeatedValueToCountAxis<T> : Axis<T, int>
    {

        private MutableField<T> m_AxisKey = new MutableField<T> { AbsoluteKey = "Entries.Value" };
        [Controllable(LabelText = "Comparable Value")]
        public MutableField<T> AxisKey
        {
            get { return m_AxisKey; }
            set { m_AxisKey = value; }
        }

        private MutableTarget m_IndexAxis = new MutableTarget()
        { AbsoluteKey = "Index Axis" };
        [Controllable(LabelText = "New Index Axis")]
        public MutableTarget IndexAxis
        {
            get { return m_IndexAxis; }
            [UsedImplicitly]
            set { m_IndexAxis = value; }
        }
        private MutableField<string> m_GroupId = new MutableField<string>()
        { LiteralValue = "" };
        [Controllable(LabelText = "Shared Group Id")]
        public MutableField<string> GroupId
        {
            get { return m_GroupId; }
        }

        private static NodeDataShare<Dictionary<T, int>> s_DataShare
            = new NodeDataShare<Dictionary<T, int>>();
        public static NodeDataShare<Dictionary<T, int>> DataShare
        { get { return s_DataShare; } set { s_DataShare = value; } }

        public void Start()
        {
            IndexAxis.SchemaParent = AxisKey;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            var entryList = AxisKey.GetEntries(newSchema);

            foreach (var entry in entryList)
                IndexAxis.SetValue(0, entry);

            Router.TransmitAllSchema(newSchema);
        }

        public override void Unload()
        {
            DataShare.Clear();

            base.Unload();
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            var groupId = GroupId.GetFirstValue( mutable );

            Dictionary< T, int > localShare;

            if ( groupId != "" )
            {
                if ( !DataShare.ContainsKey( groupId ) )
                    DataShare.Add( groupId, new Dictionary< T, int >() );

                localShare = DataShare[ groupId ];
            }
            else
            {
                localShare = new Dictionary< T, int >();
            }

            foreach ( var entry in AxisKey.GetEntries( mutable ) )
            {
                var key = AxisKey.GetValue( entry );

                if ( !localShare.ContainsKey( key ) )
                    localShare[ key ] = 0;

                IndexAxis.SetValue( localShare[ key ], entry );
                localShare[ key ]++;
            }

            return mutable;
        }
    }
}
