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
    public class SetMaterialsFromMutableMutator : Mutator
    {
        private MutableField<Material> m_MutableMaterial = new MutableField<Material>() 
        { AbsoluteKey = "Material" };
        [Controllable(LabelText = "Material to apply")]
        public MutableField<Material> MutableMaterial { get { return m_MutableMaterial; } }


        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            var newMaterial = MutableMaterial.GetFirstValue( payload.Data );

            foreach (var renderer in payload.VisualData.Bound.GetRenderers())
            {
                renderer.material = newMaterial;
            }

            var iterator = Router.TransmitAll(payload);
            while (iterator.MoveNext())
                yield return null;
        }
    }
}
