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
    public enum SplineEvaluationFunctionTypes
    {
        Bezier, 
        OffsetBezier, 
    }

    public class SplineTypes
    {
        public static Func< Vector3, Vector3, Vector3, float, Vector3 > GetEvaluationFunction(
            SplineEvaluationFunctionTypes functionType )
        {
            if (!EvaluationFunctionsDictionary.ContainsKey( functionType ))
                throw new Exception("Evaluation function not found.");
            return EvaluationFunctionsDictionary[ functionType ];
        }

        private static Dictionary<SplineEvaluationFunctionTypes, Func<Vector3, Vector3, Vector3, float, Vector3>> EvaluationFunctionsDictionary
        {
            get
            {
                return new Dictionary< SplineEvaluationFunctionTypes, Func< Vector3, Vector3, Vector3, float, Vector3 > >
                {
                    { SplineEvaluationFunctionTypes.Bezier, (current, next, focus, localTime)=> { return Vector3.Lerp( Vector3.Lerp( current, focus, localTime ), Vector3.Lerp( focus, next, localTime ), localTime ); }},
                    { SplineEvaluationFunctionTypes.OffsetBezier, (current, next, focus, localTime)=> { return Vector3.Lerp( Vector3.Lerp( current, focus, localTime ), Vector3.Lerp( current, next, localTime ), localTime ); }}
                };
            }
        }
    }
}
