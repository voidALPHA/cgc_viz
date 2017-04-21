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
using ChainViews;
using Mutation.Mutators;
using UnityEngine;
using Visualizers;

namespace Mutation
{
    public class MutableToDebugMutator : DataMutator
    {

        private MutableField< int > m_MaxElementsField = new MutableField< int > { LiteralValue = 50 };
        [Controllable( LabelText = "Max Elements to Debug" )]
        public MutableField< int > MaxElementsField
        {
            get { return m_MaxElementsField; }
        }


        private int ElementCount { get; set; }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            ElementCount = 0;

            if (ChainView.Instance.IsEvaluating || !ChainView.Instance.IsBusy)    // Don't do this during load/save (during load, this method gets executed during schema propagation)
                DebugMutable(mutable, "");

            return mutable;
        }

        public void DebugMutable(MutableObject mutable, string depthString)
        {
            var maxElements = MaxElementsField.GetLastKeyValue( mutable );

            if (maxElements != -1 && ElementCount >= maxElements)
                return;
            foreach (var kvp in mutable)
            {
                Debug.Log(depthString+kvp.Key + ": " + kvp.Value);

                var subList = kvp.Value as IEnumerable<MutableObject>;

                if (subList != null)
                {
                    int subCounter = 0;
                    foreach (var subMutable in subList)
                    {
                        DebugMutable(subMutable, kvp.Key + "["+subCounter+"].");
                        subCounter++;
                    }
                }

                ElementCount++;
                if (maxElements != -1 && ElementCount >= maxElements)
                    return;
            }
        }

    }
}
