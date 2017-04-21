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
    public class TextureToMaterialMutator : Mutator
    {
        private MutableField<Texture2D> m_Texture = new MutableField<Texture2D>() 
        { AbsoluteKey = "Texture" };
        [Controllable(LabelText = "Texture")]
        public MutableField<Texture2D> Texture { get { return m_Texture; } }

        private MutableTarget m_MaterialTarget = new MutableTarget() 
        { AbsoluteKey = "Material" };
        [Controllable(LabelText = "Material Target")]
        public MutableTarget MaterialTarget { get { return m_MaterialTarget; } }

        public TextureToMaterialMutator()
        {
            MaterialTarget.SchemaParent = Texture;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            MaterialTarget.SetValue( MaterialFactory.GetDefaultMaterial(), newSchema );

            base.OnProcessOutputSchema( newSchema );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            foreach ( var entry in Texture.GetEntries( payload.Data ) )
            {
                var newMaterial = Object.Instantiate(MaterialFactory.GetDefaultMaterial());

                newMaterial.mainTexture = Texture.GetValue( entry );

                MaterialTarget.SetValue( newMaterial, entry );
            }

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }
    }
}
