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
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.VisualModifiers
{
    public class RecolorMaterialMutator : Mutator
    {
        private MutableField<Color> m_NewColor = new MutableField<Color>() 
        { LiteralValue = Color.magenta };
        [Controllable(LabelText = "New Color")]
        public MutableField<Color> NewColor { get { return m_NewColor; } }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var newColor = NewColor.GetFirstValue( payload.Data );

            foreach ( var renderer in payload.VisualData.Bound.GetRenderers() )
            {
                //var newMaterial = GameObject.Instantiate( renderer.material );
                //newMaterial.color = newColor;
                //renderer.material = newMaterial;
                renderer.material.color = newColor;
            }

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
