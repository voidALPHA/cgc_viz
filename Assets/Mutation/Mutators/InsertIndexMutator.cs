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
using System.Text;
using Visualizers;

namespace Mutation.Mutators
{
    public class InsertIndexMutator : DataMutator
    {
        private MutableTarget m_IndexTarget = new MutableTarget() 
        { AbsoluteKey = "Index" };
        [Controllable(LabelText = "Index Target")]
        public MutableTarget IndexTarget { get { return m_IndexTarget; } }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            int counter = 0;
            foreach (var element in IndexTarget.GetEntries(mutable))
                IndexTarget.SetValue(counter++, element);

            return mutable;
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            try
            {
                foreach (var entry in IndexTarget.GetEntries(newSchema))
                {
                    IndexTarget.SetValue( 0, entry );
                }

                Router.TransmitAllSchema( newSchema );
            }
            catch ( Exception e )
            {
                throw e;
            }
        }
    }
}
