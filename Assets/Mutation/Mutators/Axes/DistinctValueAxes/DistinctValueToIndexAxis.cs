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

namespace Mutation.Mutators.Axes.DistinctValueAxes 
{
    public abstract class DistinctValueToIndexAxis<T> : Axis<T, int>
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


        // This used to use ObjectCreationHandling.Replace; not sure why, but there was a reason!
        // Changing it because the base ChainNode class binds to the MutableTarget's AbsoluteKeyChanged event,
        //  then this target is replaced, losing the binding.  Now, the binding remains.
        //[Controllable( LabelText = "New Index Axis", ObjectCreationHandling = ObjectCreationHandling.Replace )]
        [Controllable( LabelText = "New Index Axis" )]
        public MutableTarget IndexAxis
        {
            get { return m_IndexAxis; }
            [UsedImplicitly]
            set { m_IndexAxis = value; }
        }


        //private String _mIndexFieldName = "Index Axis";
        //[Controllable(LabelText = "Name of new Index Axis")]
        //public String IndexFieldName
        //{
        //    get { return _mIndexFieldName; }
        //    set { _mIndexFieldName = value; }
        //}

        private MutableField<string> m_GroupId = new MutableField<string>()
        {LiteralValue = ""};
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
                IndexAxis.SetValue( 0, entry );

            Router.TransmitAllSchema(newSchema);
        }

        public override void Unload()
        {
            DataShare.Clear();

            base.Unload();
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            var entries = AxisKey.GetEntries(mutable);

            var foundStrings = new HashSet<T>();

            // identify the set of unique keys
            foreach (var entry in entries)
            {
                var axisKey = InputStack.TransformValue(AxisKey.GetValue(entry));

                if (!foundStrings.Contains(axisKey))
                    foundStrings.Add(axisKey);
            }

            // DO NOT sort the keys
            //var sortedKeys = foundStrings.ToList();
            //sortedKeys.Sort();

            // index the values
            var axisValues = GroupId.GetFirstValue(mutable) == "" ? 
                new Dictionary<T, int>()
                : DataShare.ContainsKey(GroupId.GetFirstValue(mutable)) ? 
                DataShare[GroupId.GetFirstValue(mutable)] 
                : new Dictionary<T, int>();

            int i = 0;
            foreach (var key in foundStrings) //sortedKeys
                axisValues[key] =  OutputStack.TransformValue(i++);

            // finally, write the new axis value into each entry
            entries = AxisKey.GetEntries(mutable);
            foreach (var entry in entries)
            {
                IndexAxis.SetValue(
                    axisValues[ InputStack.TransformValue( AxisKey.GetValue( entry ) ) ],
                    entry );
            }

            if (GroupId.GetFirstValue(mutable) != "")
                DataShare[GroupId.GetFirstValue(mutable)] = axisValues;

            return mutable;
        }
    }
}
