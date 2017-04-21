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
using System.Linq;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.MemorySpaceManagement
{
    public class AllocateAnnotationToAddress : DataMutator
    {
        private MutableField<string> m_AnnotationText = new MutableField<string>() 
        { AbsoluteKey = "Annotation Text" };
        [Controllable(LabelText = "Annotation Text")]
        public MutableField<string> AnnotationText { get { return m_AnnotationText; } }

        private MutableTarget m_AddressTarget = new MutableTarget() 
        { AbsoluteKey = "Alloc Address" };
        [Controllable(LabelText = "Address Target")]
        public MutableTarget AddressTarget { get { return m_AddressTarget; } }
        
        private MutableTarget m_SizeTarget = new MutableTarget() 
        { AbsoluteKey = "Alloc Size" };
        [Controllable(LabelText = "Size Target")]
        public MutableTarget SizeTarget { get { return m_SizeTarget; } }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            foreach ( var entry in AnnotationText.GetEntries( newSchema ) )
            {
                AddressTarget.SetValue( (uint)0, entry );
                SizeTarget.SetValue( (int)1, entry );
            }

            Router.TransmitAllSchema( newSchema );
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in AnnotationText.GetEntries( mutable ) )
            {
                var annotationString = AnnotationText.GetValue( entry );

                var stringParts = annotationString.Split( ' ' );

                if ( stringParts[ 0 ].ToLower().CompareTo( "allocate" ) != 0 )
                {
                    Debug.Log( "The string " + annotationString + " isn't an allocate!" );
                    AddressTarget.SetValue( (uint)0, entry );
                    SizeTarget.SetValue( 1, entry );
                    continue;
                }

                SizeTarget.SetValue( int.Parse(stringParts[1]), entry );
                try
                {
                    AddressTarget.SetValue( uint.Parse( stringParts[ 4 ].Split( 'x' )[1], System.Globalization.NumberStyles.HexNumber ),
                        entry );
                }
                catch
                {
                    Debug.Log( "What?" );
                }
            }

            return mutable;
        }
    }
}
