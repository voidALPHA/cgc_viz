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
using UnityEngine;

namespace Choreography.CameraControl.SplineFunctionListings
{
    public enum SplineComputeFocusFunctionTypes
    {
        Projection,
        Linear,
        Straighten,
        Widen,
        NormalizedProjection,
    }

    public class SplineFocusTypes
    {
        public static Func<Vector3, Vector3, Vector3, float, Vector3> GetFocusFunction(
            SplineComputeFocusFunctionTypes functionType)
        {
            if (!FocusFunctionsDictionary.ContainsKey(functionType))
                throw new Exception("Focus function not found.");
            return FocusFunctionsDictionary[functionType];
        }

        private static Dictionary<SplineComputeFocusFunctionTypes, Func<Vector3, Vector3, Vector3, float, Vector3>> FocusFunctionsDictionary
        {
            get
            {
                return new Dictionary<SplineComputeFocusFunctionTypes, Func<Vector3, Vector3, Vector3, float, Vector3>>
                {
                    { SplineComputeFocusFunctionTypes.Projection, (prior, current, next, localTime)=> { return 2*current-prior; }},
                    { SplineComputeFocusFunctionTypes.Linear, (prior, current, next, localTime)=> { return (current+next)/2f; }},
                    { SplineComputeFocusFunctionTypes.Straighten, (prior, current, next, localTime)=> { return Vector3.Lerp(2*current-prior, current, localTime); }},
                    { SplineComputeFocusFunctionTypes.Widen, (prior, current, next, localTime)=> { return Vector3.Lerp(2*current-prior, next, localTime); }},
                    { SplineComputeFocusFunctionTypes.NormalizedProjection, (prior, current, next, localTime)=> { return current+(current-prior).normalized; }}
                };
            }
        }
    }
}
