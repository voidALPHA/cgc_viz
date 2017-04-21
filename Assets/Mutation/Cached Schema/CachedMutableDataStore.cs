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

namespace Mutation
{
    public class CachedMutableDataStore
    {
        private static CachedMutableDataStore s_Instance;
        public static CachedMutableDataStore Instance
        {
            get
            {
                return s_Instance ?? (s_Instance = new CachedMutableDataStore());
            }
        }

        private MutableObject m_DataStore;

        public static MutableObject DataStore
        {
            get { return Instance.m_DataStore; }
            set
            {
                Instance.m_DataStore = value;
            }
        }

        public static bool DataCached
        {
            get { return Instance.m_DataStore != null; }
        }

        public static void ClearDataCache()
        {
            DataStore = null;
        }
    }
}
