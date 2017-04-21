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

using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.MemorySpaceManagement
{
    public class FreeAnnotationToAddress : DataMutator
    {

        private MutableField<string> m_AnnotationText = new MutableField<string>()
        { AbsoluteKey = "Annotation Text" };
        [Controllable(LabelText = "Annotation Text")]
        public MutableField<string> AnnotationText { get { return m_AnnotationText; } }

        private MutableTarget m_AddressTarget = new MutableTarget()
        { AbsoluteKey = "Free Address" };
        [Controllable(LabelText = "Address Target")]
        public MutableTarget AddressTarget { get { return m_AddressTarget; } }
        
        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            foreach (var entry in AnnotationText.GetEntries(newSchema))
            {
                AddressTarget.SetValue((uint)0, entry);
            }

            Router.TransmitAllSchema(newSchema);
        }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            foreach (var entry in AnnotationText.GetEntries(mutable))
            {
                var annotationString = AnnotationText.GetValue(entry);

                var stringParts = annotationString.Split(' ');

                if (stringParts[0].CompareTo("Free") != 0)
                {
                    Debug.Log("The string " + annotationString + " isn't a free!");
                    AddressTarget.SetValue((uint)0, entry);
                    continue;
                }
                
                AddressTarget.SetValue(uint.Parse(stringParts[4].Split('x')[1], System.Globalization.NumberStyles.HexNumber), entry);

            }

            return mutable;
        }
    }
}
