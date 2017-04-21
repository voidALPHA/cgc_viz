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
using System.Collections;
using System.Collections.Generic;
using Mutation.Mutators;
using UnityEngine;
using Visualizers;

namespace Mutation
{
    public class MutableToDebugBoundBoxMutator : Mutator
    {
        private List<Vector3> m_BoundPoints = new List<Vector3>();
        private List<Vector3> BoundPoints { get { return m_BoundPoints; } set { m_BoundPoints = value; } }

        private List<Vector3> m_BoundSizes = new List<Vector3>();
        private List<Vector3> BoundSizes { get { return m_BoundSizes; } set { m_BoundSizes = value; } }

        private BoundingBox DrawBound { get; set; }

        private void OnDrawGizmos()
        {
            if (DrawBound == null)
                return;

            Gizmos.color = Color.green;

            Gizmos.DrawWireCube(DrawBound.transform.position, Vector3.one * 2f);

        }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            DrawBound = payload.VisualData.Bound;

            var iterator = DebugElements(payload.Data);
            while (iterator.MoveNext())
                yield return null;

            iterator = Router.TransmitAll(payload);
            while (iterator.MoveNext())
                yield return null;
        }

        private IEnumerator DebugElements(MutableObject mutable)
        {
            foreach (var kvp in mutable)
            {
                var subList = kvp.Value as IEnumerable<MutableObject>;

                if (subList != null)
                    foreach (var subMutable in subList)
                        DebugElements(subMutable);

                yield return null;
            }
        }
    }
}
