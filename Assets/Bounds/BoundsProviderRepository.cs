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
using UnityEngine;

namespace Bounds
{
    public static class BoundsProviderRepository
    {

        public static event Action< IBoundsProvider > BoundsProviderAdded = delegate { };
        public static event Action< IBoundsProvider > BoundsProviderRemoved = delegate { };


        private static List< IBoundsProvider > m_BoundsProviders = new List< IBoundsProvider >();
        private static List< IBoundsProvider > BoundsProviders { get { return m_BoundsProviders; } }

        
        public static IEnumerable<string> BoundsProvidersKeys { get { return BoundsProviders.Select( e => e.BoundsProviderKey ); } }

        public static IEnumerable<IBoundsProvider> BoundsProvidersEnumerable { get {  return BoundsProviders.AsReadOnly(); } }
        
        public static void Add( IBoundsProvider boundsProvider )
        {
            if ( BoundsProviders.Contains( boundsProvider ) )
                throw new InvalidOperationException("This BoundsProvider is already registered. Key " + boundsProvider.BoundsProviderKey );

            if ( BoundsProviders.Any( p => p.BoundsProviderKey == boundsProvider.BoundsProviderKey ) )
                Debug.LogWarning( "A different BoundsProvider with the same key is already registered. Key " + boundsProvider.BoundsProviderKey + ". Proceeding to register second BoundsProvider." );
            //    throw new InvalidOperationException("A different BoundsProvider with the same key is already registered. Key " + boundsProvider.BoundsProviderKey );

            BoundsProviders.Add( boundsProvider );

            BoundsProviderAdded( boundsProvider );
        }

        public static void Remove( IBoundsProvider boundsProvider )
        {
            Debug.Log( "Removing bounds provider " + boundsProvider.BoundsProviderKey );

            if ( !BoundsProviders.Contains( boundsProvider ) )
                throw new InvalidOperationException( "This BoundsProvider is not registered." );

            BoundsProviders.Remove( boundsProvider );

            BoundsProviderRemoved( boundsProvider );
        }
    }
}
