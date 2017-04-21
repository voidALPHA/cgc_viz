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

namespace Visualizers.PrefabController
{
    public class PrefabDiscriminateChild : MonoBehaviour
    {
        [SerializeField]
        private string m_ChildName = null;
        public string ChildName { get { return (m_ChildName=string.IsNullOrEmpty( m_ChildName )?gameObject.name:m_ChildName); } }

        [SerializeField]
        private List<string> m_DiscriminatedValues = new List<string>();
        private List<string> DiscriminatedValues { get { return m_DiscriminatedValues; } }

        public BoundingBox FulfillValueDiscrimination()
        {
            var foundBound = gameObject.GetComponent< BoundingBox >() ?? gameObject.AddComponent< BoundingBox >();

            foreach ( var value in DiscriminatedValues )
            {
                foundBound.Data[ value ] = "Value";
            }

            return foundBound;
        }
    }
}
