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

using UnityEngine;

namespace LabelSystem
{
    public class LabelSystemFactory : MonoBehaviour
    {
        private static LabelSystemFactory m_LabelSystemFactoryMaster = null;
        public static LabelSystemFactory LabelSystemFactoryMaster
        {
            get
            {
                if (m_LabelSystemFactoryMaster == null)
                    m_LabelSystemFactoryMaster = FindObjectOfType<LabelSystemFactory>( );

                return m_LabelSystemFactoryMaster;
            }
        }

        #region Label Prefab
        [SerializeField]
        private GameObject m_LabelPrefab;
        private GameObject LabelPrefab
        {
            get { return m_LabelPrefab; }
            set { m_LabelPrefab = value; }
        }

        public static GameObject InstantiateLabel()
        {
            var label = Instantiate( LabelSystemFactoryMaster.LabelPrefab );
            return label;
        }
        #endregion

        #region Axis Element Label Prefab
        [SerializeField]
        private GameObject m_AxisElementLabelPrefab;
        private GameObject AxisElementLabelPrefab
        {
            get { return m_AxisElementLabelPrefab; }
            set { m_AxisElementLabelPrefab = value; }
        }

        public static GameObject InstantiateAxisElementLabel()
        {
            var label = Instantiate(LabelSystemFactoryMaster.AxisElementLabelPrefab);
            return label;
        }
        #endregion

    }
}
