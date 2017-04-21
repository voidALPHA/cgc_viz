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
using FocusCamera;
using Mutation;
using UnityEngine;

namespace Visualizers.RectangularVolume
{
    public class RectangularVolumeVisualizer : Visualizer
    {
        [Header("Component References")]
        [SerializeField]
        private Transform m_Geometry;
        private Transform Geometry { get { return m_Geometry; } }

        public override event Action OnVisualizerDestroyed = delegate { };

        public void Destroy()
        {
            //Destroy(gameObject);

            OnVisualizerDestroyed();
        }

        public Material OpaqueMaterial;
        public Material TranslucentMaterial;

        private Material m_GeometryMaterial;

        private Material GeometryMaterial
        {
            get { return m_GeometryMaterial ?? (m_GeometryMaterial = Geometry.GetComponentInChildren<Renderer>().material); }
            set
            {
                m_GeometryMaterial = value;
                Geometry.GetComponentInChildren<Renderer>().material = m_GeometryMaterial;
            }
        }

        public Color Color
        {
            get { return GeometryMaterial.color; }
            set { GeometryMaterial.color = value; }
        }

        public float Xscale
        {
            get { return Geometry.localScale.x; }
            set
            {
                var newScale = Geometry.localScale;
                newScale.x = value;
                Geometry.localScale = newScale;
            }
        }

        public float Yscale
        {
            get { return Geometry.localScale.y; }
            set
            {
                var newScale = Geometry.localScale;
                newScale.y = value;
                Geometry.localScale = newScale;
            }
        }

        public float Zscale
        {
            get { return Geometry.localScale.z; }
            set
            {
                var newScale = Geometry.localScale;
                newScale.z = value;
                Geometry.localScale = newScale;
            }
        }

        protected override void SetVisible(bool visible)
        {
            throw new NotImplementedException();
        }

        public void SetOpaqueMaterial( bool useOpaque )
        {
            GeometryMaterial = Instantiate( useOpaque ? OpaqueMaterial : TranslucentMaterial );
        }

        #region FocusableTarget implementation

        public void UpdateInput()
        {
        }

        public Vector3 CameraLocation()
        {
            throw new NotImplementedException();
        }

        public List<CameraTarget> CameraTargets()
        {
            throw new NotImplementedException();
        }

        public void FocusTarget()
        {
            throw new NotImplementedException();
        }

        public void UnFocusTarget()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
