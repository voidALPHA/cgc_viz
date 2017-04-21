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
using System.Diagnostics;
using System.Linq;
using System.Text;
using Chains;
using Mutation;
using Mutation.Mutators;
using Visualizers;

namespace Experimental
{
    public class ModifyNthInstructionMutator : DataMutator
    {
        private MutableTarget m_FieldToModify = new MutableTarget() 
        { AbsoluteKey = "Instructions.Eip" };
        [Controllable(LabelText = "FieldToModify")]
        public MutableTarget FieldToModify { get { return m_FieldToModify; } }

        private MutableField<int> m_ModNumber = new MutableField<int>() 
        { LiteralValue = 10 };
        [Controllable(LabelText = "Modify Each N-th Entry")]
        public MutableField<int> ModNumber { get { return m_ModNumber; } }

        private MutableField<bool> m_RandomizeInstruction = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "Randomize this Field on Nth Elements")]
        public MutableField<bool> RandomizeInstruction { get { return m_RandomizeInstruction; } }


        public ModifyNthInstructionMutator()
        {
            ModNumber.SchemaPattern = FieldToModify;
            RandomizeInstruction.SchemaPattern = FieldToModify;
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            var entries = FieldToModify.GetEntries(mutable);
            
            var nRand = new Random();

            int counter = 0;
            foreach (var entry in entries)
            {
                if (counter%ModNumber.GetValue(entry) == 0)
                {
                    counter = 1;

                    FieldToModify.SetValue(RandomizeInstruction.GetValue(entry) ? nRand.Next() : 0, entry);
                }
                else
                {
                    counter++;
                }
            }

            return mutable;
        }
    }
}
