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

using System.Collections;
using Visualizers;

namespace Mutation.Mutators.DiscriminatingValues
{
    public class DiscriminateBoundMutator : Mutator
    {
        private MutableField<string> m_ValueName = new MutableField<string>() 
        { LiteralValue = "Value Key" };
        [Controllable(LabelText = "ValueName")]
        public MutableField<string> ValueName { get { return m_ValueName; } }

        private MutableField<object> m_DiscriminatingValue = new MutableField<object>() 
        { AbsoluteKey = "Discriminating Value"};
        [Controllable(LabelText = "Discriminating Value")]
        public MutableField<object> DiscriminatingValue { get { return m_DiscriminatingValue; } }


        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            if (payload.VisualData.Bound.Data==null)
                payload.VisualData.Bound.Data = new MutableObject();

            payload.VisualData.Bound.Data[ ValueName.GetFirstValue( payload.Data ) ] =
                DiscriminatingValue.GetFirstValue( payload.Data );

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
