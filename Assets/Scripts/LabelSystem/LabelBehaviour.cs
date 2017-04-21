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

namespace LabelSystem
{
    public class LabelBehaviour : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField]
        private TextMesh m_TextComponent = null;
        public TextMesh TextComponent { get { return m_TextComponent; } }

        [SerializeField]
        private Transform m_Geometry;
        public Transform Geometry { get { return m_Geometry; } set { m_Geometry = value; } }

        [SerializeField]
        private Transform m_TextRoot;
        private Transform TextRoot { get { return m_TextRoot; } set { m_TextRoot = value; } }

        [SerializeField]
        private LabelClickSender m_LabelClickSender;
        protected LabelClickSender LabelClickSender { get { return m_LabelClickSender; } set { m_LabelClickSender = value; } }

        private const float PressedLabelScaleMult = 0.90f;
        private Vector3 SavedScale { get; set; }


        public static readonly Dictionary<LabelOrientation, float> OrientationToDegreesZ = new Dictionary<LabelOrientation, float>
        {
            {LabelOrientation.Out, 0.0f},
            {LabelOrientation.Out90, 90.0f},
            {LabelOrientation.Out180, 180.0f},
            {LabelOrientation.Out270, 270.0f},
            {LabelOrientation.Up, 0.0f},
            {LabelOrientation.Up90, 90.0f},
            {LabelOrientation.Up180, 180.0f},
            {LabelOrientation.Up270, 270.0f}
        };

        public static readonly Dictionary<LabelOrientation, float> OrientationToDegreesX = new Dictionary<LabelOrientation, float>
        {
            {LabelOrientation.Out, 0.0f},
            {LabelOrientation.Out90, 0.0f},
            {LabelOrientation.Out180, 0.0f},
            {LabelOrientation.Out270, 0.0f},
            {LabelOrientation.Up,90.0f},
            {LabelOrientation.Up90, 90.0f},
            {LabelOrientation.Up180, 90.0f},
            {LabelOrientation.Up270, 90.0f}
        };

        public LabelOrientation Orientation
        {
            set
            {
                if (!OrientationToDegreesX.ContainsKey(value) || !OrientationToDegreesZ.ContainsKey(value))
                    throw new NotImplementedException();

                Geometry.transform.rotation = Quaternion.Euler(OrientationToDegreesX[value], 0.0f, OrientationToDegreesZ[value]);
                TextRoot.transform.rotation = Quaternion.Euler(OrientationToDegreesX[value], 0.0f, OrientationToDegreesZ[value]);
            }
        }

        public string Text
        {
            set { TextComponent.text = value; }
        }

        private void Start()
        {
            LabelClickSender.onLabelMouseDown += OnLabelMouseDown;
            LabelClickSender.onLabelMouseUp += OnLabelMouseUp;
        }

        private void OnLabelMouseDown()
        {
            SavedScale = transform.localScale;
            transform.localScale = new Vector3(SavedScale.x * PressedLabelScaleMult, SavedScale.y * PressedLabelScaleMult, SavedScale.z * PressedLabelScaleMult);
        }

        private void OnLabelMouseUp()
        {
            transform.localScale = SavedScale;
        }
    }
}
