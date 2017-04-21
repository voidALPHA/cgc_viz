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
using UnityEngine;
using Visualizers.CsView.Texturing;

namespace Experimental
{
    public class ExperimentalOpcodeContainer : OpcodeContainer
    {
        [SerializeField]
        private List<String> m_OpcodeTypes;
        private List<String> OpcodeTypes { get { return m_OpcodeTypes; } }

        [SerializeField]
        private List<int> m_Frequencies;
        private List<int> Frequencies { get { return m_Frequencies; } }

        public override OpcodeHistogram Opcodes
        {
            get
            {
                var localCodes = new OpcodeHistogram();

                var freqIterator = Frequencies.GetEnumerator();

                foreach (var codeString in OpcodeTypes)
                {
                    // loop frequencies
                    if (!freqIterator.MoveNext())
                    {
                        freqIterator = Frequencies.GetEnumerator();
                        freqIterator.MoveNext();
                    }
                    var foundFreq = freqIterator.Current;

                    localCodes.Add(codeString, new OpcodePair(foundFreq, codeString));
                }

                return localCodes;
            }
        }

    }
}
