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
using System.Text;
using Mutation.Mutators.DefaultValue;
using UnityEngine;

namespace Assets.Experimental
{
    [ExecuteInEditMode]
    public class ExperimentalCurveTest : MonoBehaviour
    {

        [SerializeField]
        private AnimationCurve m_EventCurve;
        public AnimationCurve EventCurve { get { return m_EventCurve; } set { m_EventCurve = value; } }

        [SerializeField]
        private GameObject m_TargetObject;
        public GameObject TargetObject { get { return m_TargetObject; } set { m_TargetObject = value; } }

        [SerializeField]
        private float m_TimeScale = 1f;
        public float TimeScale { get { return m_TimeScale;} set { m_TimeScale = value; } }

        public void Update()
        {
            float time = Time.time*TimeScale % 1f;

            TargetObject.transform.position = Vector3.up*EventCurve.Evaluate(time);
        }
    }
}
