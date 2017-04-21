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

using System.Runtime.InteropServices;
using Mutation;
using Mutation.Mutators;

namespace Visualizers.CsView.CWEIcons
{
    public class CWEToMutableMutator : DataMutator
    {
        private MutableField<string> m_CWEName = new MutableField<string>() 
        { LiteralValue = "CWE-122" };
        [Controllable(LabelText = "CWE Name")]
        public MutableField<string> CWEName { get { return m_CWEName; } }

        private MutableTarget m_DescriptionTarget = new MutableTarget() 
        { AbsoluteKey = "Description" };
        [Controllable(LabelText = "Description Target")]
        public MutableTarget DescriptionTarget { get { return m_DescriptionTarget; } }

        private MutableTarget m_CWEMaterialTarget = new MutableTarget() 
        { AbsoluteKey = "CWE Material" };
        [Controllable(LabelText = "CWE Material Target")]
        public MutableTarget CWEMaterialTarget { get { return m_CWEMaterialTarget; } }

        public CWEToMutableMutator()
        {
            DescriptionTarget.SchemaParent = CWEName;
            CWEMaterialTarget.SchemaParent = CWEName;
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in CWEName.GetEntries( mutable ) )
            {
                var cweIcon = CWEIconFactory.GetIcon( CWEName.GetValue( entry ) );

                DescriptionTarget.SetValue( cweIcon.Description, entry );

                CWEMaterialTarget.SetValue( cweIcon.Material, entry );
            }

            return mutable;
        }
    }
}
