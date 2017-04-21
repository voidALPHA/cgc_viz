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
using Visualizers;

namespace Mutation.Mutators
{
    public class EnumerableWrapperMutator : DataMutator
    {
        private string m_EntryFieldName = "Entries";
        [Controllable(LabelText = "Entry Field Name")]
        private string EntryFieldName { get { return m_EntryFieldName; } set { m_EntryFieldName = value; } }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            return new MutableObject()
            { new KeyValuePair<string, object>(EntryFieldName, new List<MutableObject>() { mutable }
                )};
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            Router.TransmitAllSchema(new MutableObject()
            {
                new KeyValuePair<string, object>(EntryFieldName, new List<MutableObject>()
                {
                    newSchema
                })
            });
        }
    }
}
