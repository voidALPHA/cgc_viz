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
using Mutation;
using UnityEngine;

namespace Experimental
{
    [ExecuteInEditMode]
    public class ExperimentalUnionSchemaTest : MonoBehaviour
    {
        public bool Exec = false;

        public void Update()
        {
            if ( !Exec )
                return;
            Exec = false;

            MutableObject schema1 = new MutableObject
            {
            { "nested items", new MutableObject
                {
                    { "int item", 500 },
                    { "string item", "FooBarBaz" }
                }
            },
            { "Deep nested item", new MutableObject
                {
                    { "Deep at last", new MutableObject
                        {
                            { "The list", new List<MutableObject>
                            {
                                new MutableObject
                                {
                                    {"float points", 12f},
                                    {"string team", "ThisTeam"},
                                    {"ambiguous Score", "winner"}
                                }
                            } }
                        }
                    },
                }
            },
            { "something else", 4.2f }
            };


            MutableObject schema2 = new MutableObject
            {
            { "nested items", new MutableObject
                {
                    { "int item", 50000 },
                    { "float item", 45.5f }
                }
            },
            { "Deep nested item", new MutableObject
                {
                    { "Deep at last", new MutableObject
                        {
                            { "The list", new List<MutableObject>
                            {
                                new MutableObject
                                {
                                    {"float points", 11f},
                                    {"string team", "ThisTeam"},
                                    {"ambiguous Score", 12f}
                                }
                            } }
                        }
                    },
                }
            },
            { "something else", 4 }
            };

            //Debug.Log( "=======Schema1!=======" );
            //schema1.DebugMutable();
            //
            //Debug.Log("=======Schema2!=======");
            //schema2.DebugMutable();

            var outSchema = MutableObject.UnionSchemas( schema1, schema2 );

            Debug.Log("=======Schema union!=======");
            outSchema.DebugMutable();
        }

    }
}
