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
using UnityEngine;
using Visualizers;

namespace Experimental
{
    public class BoundingTest : MonoBehaviour
    {
        [SerializeField]
        private List<Vector3> m_TestPoints = new List<Vector3>();
        private List<Vector3> TestPoints { get { return m_TestPoints; } set { m_TestPoints = value; } }

        [SerializeField]
        private BoundingBox m_TestBox;
        private BoundingBox TestBox { get { return m_TestBox; } set { m_TestBox = value; } }

        [SerializeField]
        private int m_NumberOfPoints=5;
        private int NumberOfPoints { get { return m_NumberOfPoints; } set { m_NumberOfPoints = value; } }

        private float TimeCounter = 0f;

        private void Start()
        {
            //for (int i = 0; i < NumberOfPoints; i++)
            //{
            //    TestPoints.Add(UnityEngine.Random.insideUnitSphere * 3f);
            //}
        }

        private void Update()
        {
            TimeCounter += Time.deltaTime;

            if (TimeCounter < .5f)
                return;

            TimeCounter = 0;

            TestPoints.Clear();
            
            for (int i = 0; i < NumberOfPoints; i++)
            {
                TestPoints.Add(UnityEngine.Random.insideUnitSphere*3f);
            }

            TestBox.EnclosePoints(TestPoints.ToArray());
        }

        private void OnDrawGizmos()
        {
            if (TestPoints == null)
                return;

            Gizmos.color = Color.blue;
            foreach (var point in TestPoints)
                Gizmos.DrawWireSphere(point, .1f);
        }
    }
}
