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

namespace Adapters.TraceAdapters.CallGraphing
{
    public class FunctionDescriptor
    {
        public uint Address { get; set; }

        public string Name { get; set; }

        public FunctionDescriptor( uint address )
        {
            Address = address;

            Name = "F: " + address;
        }

        //public void UpdateTo( FunctionDescriptor other )
        //{
        //    if ( ! Address.Equals( other.Address ) )
        //        throw new InvalidOperationException("Cannot update function descriptor to match descriptor with different address.");

        //    if ( Name.Equals( other.Name ) )
        //        return;

        //    if ( !String.IsNullOrEmpty( Name ) )
        //        throw new InvalidOperationException("Cannot update function descriptor with a name to match descriptor with a different name.");

        //    Name = other.Name;
        //}

        public override bool Equals( object obj )
        {
            if ( ! (obj is FunctionDescriptor ) )
                return false;

            var other = obj as FunctionDescriptor;

            return Address.Equals( other.Address );
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }
    }
}
