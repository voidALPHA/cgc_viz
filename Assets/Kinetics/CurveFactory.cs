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

namespace Kinetics
{
    [Serializable]
    public class CurvePair
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public AnimationCurve Curve;
    }

    public class CurveFactory : MonoBehaviour
    {
        public static CurveFactory Instance { get; set; }

        [SerializeField]
        private List<CurvePair> m_CurvePairs = new List<CurvePair>();
        private List<CurvePair> CurvePairs { get { return m_CurvePairs; } }

        [SerializeField] private AnimationCurve m_DefaultCurve;
        private AnimationCurve DefaultCurve { get { return m_DefaultCurve; } }

        public Dictionary<string, AnimationCurve> Curves;

        public void Awake()
        {
            Instance = this;

            Curves = new Dictionary<string, AnimationCurve>();
            foreach (var curvePair in CurvePairs)
                Curves.Add(curvePair.Name, curvePair.Curve);
        }

        public static AnimationCurve GetCurve(string curveName)
        {
            if (Instance.Curves.ContainsKey(curveName))
                return Instance.Curves[curveName];

            return Instance.DefaultCurve;
        }
    }
}
