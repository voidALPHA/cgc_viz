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
using System.Collections.Generic;
using System.Linq;

namespace Mutation
{
    public class TypeCollisionList
    {
        private List< object > m_Objects = new List< object >();
        public List<object> Objects { get { return m_Objects; } private set { m_Objects = value; } }

        public static TypeCollisionList CollideValues( object value1, object value2 )
        {
            var colList1 = value1 as TypeCollisionList;
            var colList2 = value2 as TypeCollisionList;

            if ( colList1 != null && colList2 != null )
                return CombineCollisionLists( colList1, colList2 );

            if ( colList1 != null )
                return AddToCollisionList( colList1, value2 );

            if ( colList2 != null )
                return AddToCollisionList( colList2, value1 );

            return AddToCollisionList( AddToCollisionList( new TypeCollisionList(), value1), value2);
        }

        public static TypeCollisionList CombineCollisionLists( TypeCollisionList colList1, TypeCollisionList colList2 )
        {
            var newCollisionList = new TypeCollisionList();
            foreach (var obj in colList1.Objects)
                newCollisionList.Objects.Add( obj );
            foreach ( var obj in colList2.Objects )
                newCollisionList = AddToCollisionList( newCollisionList, obj );
            return newCollisionList;
        }

        public static TypeCollisionList AddToCollisionList( TypeCollisionList colList, object obj )
        {
            var newCollisionList = new TypeCollisionList();

            bool inserted = false;

            var mut = obj as MutableObject;
            if ( mut != null ) // inserting a mutable object.  Union it into existing mutable objects
            {
                var extantMutables = new List< MutableObject >();

                foreach ( var entry in colList.Objects )
                {
                    var entryMut = entry as MutableObject;
                    if ( entryMut != null )
                    {
                        extantMutables.Add( entryMut );
                    }
                }
                newCollisionList.Objects.Add(MutableObject.UnionSchemas(mut, extantMutables));
                return newCollisionList;
            }

            var mutList = obj as IEnumerable< MutableObject >;
            if ( mutList != null )
            {
                var extantEnums = new List< IEnumerable< MutableObject > >(); 

                foreach (var entry in colList.Objects)
                {
                    var entryMutList = entry as IEnumerable<MutableObject>;
                    if ( entryMutList != null )
                    {
                        extantEnums.Add( entryMutList );
                    }
                }
                newCollisionList.Objects.Add(new List<MutableObject>()
                {
                    MutableObject.UnionSchemas( 
                    extantEnums.Count>0?mutList.Concat( 
                    extantEnums.Aggregate( (a,b)=>a.Concat( b ) ))
                    : mutList)
                });

                return newCollisionList;
            }
            
            foreach ( var entry in colList.Objects )
            {
                if ( entry.GetType() == obj.GetType() )
                    inserted = true;
                newCollisionList.Objects.Add( entry );
            }
            if (!inserted)
                newCollisionList.Objects.Add( obj );

            return newCollisionList;
        }

        public bool ContainsType( Type type )
        {
            return Objects.Any( entry => type.IsInstanceOfType( entry ) );
        }

        public override string ToString()
        {
            return 
                Objects.Aggregate("TypeCollisionList with "+Objects.Count+" objects: ", (current, entry) => current + (
                    (entry is TypeCollisionList?(entry as TypeCollisionList).ToString():
                    (entry is MutableObject?MutableObject.DebugMutable(entry as MutableObject):
                    entry) + ", ")))+" | ";
        }
    }
}
