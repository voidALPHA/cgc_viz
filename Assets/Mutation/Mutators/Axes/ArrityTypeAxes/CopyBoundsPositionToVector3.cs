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
using System.Linq;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.Axes.ArrityTypeAxes
{
    public class CopyBoundsPositionToVector3 : TypeConversionAxis<BoundingBox, Vector3>
    {
        protected override Vector3 ConversionFunc( BoundingBox key, List<MutableObject> entry)
        {
            return key.transform.position;
        }
    }

    public class CopyFirstBoundsListPositionToVector3 : TypeConversionAxis<List<BoundingBox>, Vector3>
    {
        protected override Vector3 ConversionFunc(List<BoundingBox> key, List<MutableObject> entry)
        {
            return key.First().transform.position;
        }
    }

    // Could add 'center point' version ...
}
