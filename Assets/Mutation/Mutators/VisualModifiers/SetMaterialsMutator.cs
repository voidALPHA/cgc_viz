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

namespace Mutation.Mutators.VisualModifiers
{
    public class SetMaterialsMutator : Mutator
    {
        private MutableField<string> m_MaterialName = new MutableField<string>() 
        { LiteralValue = "DefaultMaterial" };
        [Controllable(LabelText = "Material Name")]
        public MutableField<string> MaterialName { get { return m_MaterialName; } }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var newMaterial = MaterialFactory.GetMaterial(MaterialName.GetFirstValue(payload.Data));

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
