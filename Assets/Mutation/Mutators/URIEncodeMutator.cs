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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Utility.NetworkSystem;
using Visualizers;

namespace Mutation.Mutators
{
    [UsedImplicitly]
    public class UriEncodeMutator : DataMutator
    {
        private MutableField<string> m_InputString = new MutableField<string>() { LiteralValue = "localhost:8003" };
        [Controllable(LabelText = "Value to Encode")]
        public MutableField<string> InputString { get { return m_InputString; } }

        private MutableTarget m_OutputUri = new MutableTarget() { AbsoluteKey = "EncodedURI" };
        [Controllable(LabelText = "Encoded URI")]
        public MutableTarget OutputUri { get { return m_OutputUri; } }

        public UriEncodeMutator()
        {
            OutputUri.SchemaParent = InputString;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            foreach(var entry in InputString.GetEntries(mutable))
            {
                OutputUri.SetValue(Uri.EscapeUriString(InputString.GetValue(entry)), entry);
            }
            return mutable;
        }
    }
}
