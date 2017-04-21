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

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.ExportToObj
{
    public class ExportToObjMutator : DataMutator
    {
        private MutableField<Vector3> m_Vertices = new MutableField<Vector3>() 
        { AbsoluteKey = "Vertex" };
        [Controllable(LabelText = "Vertices")]
        public MutableField<Vector3> Vertices { get { return m_Vertices; } }

        private MutableField<string> m_Filename = new MutableField<string>() 
        { LiteralValue = "Object.obj" };
        [Controllable(LabelText = "Filename to write to")]
        public MutableField<string> Filename { get { return m_Filename; } }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var fileEntry in Filename.GetEntries( mutable ) )
            {
                var fileName = Filename.GetValue( fileEntry );

                var outStream = new StreamWriter(fileName);

                outStream.WriteLine( "# Output from Haxxis obj exporter" );

                outStream.WriteLine( "o Object" );

                int nVerts = 0;

                foreach ( var vertex in Vertices.GetEntries( mutable ) )
                {
                    var position = Vertices.GetValue( vertex );

                    outStream.WriteLine(string.Format("v {0:F4} {1:F4} {2:F4}", position.x, position.y, position.z));

                    nVerts++;
                }

                outStream.WriteLine("s off");

                outStream.Write("l " );

                for (int i=1; i<=nVerts; i++)
                    outStream.Write( i + " " );

                outStream.Close();
            }

            return mutable;
        }
    }
}
