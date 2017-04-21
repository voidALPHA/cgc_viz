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

namespace Visualizers.MetaSelectors
{
    public class PayloadSelectorFactory : MonoBehaviour
    {
        private static PayloadSelectorFactory m_PayloadSelectorFactoryMaster = null;
        public static PayloadSelectorFactory PayloadSelectorFactoryMaster
        {
            get
            {
                if (m_PayloadSelectorFactoryMaster == null)
                    m_PayloadSelectorFactoryMaster = FindObjectOfType<PayloadSelectorFactory>();

                return m_PayloadSelectorFactoryMaster;
            }
        }

        #region Key-SelectAllPayloads

        [SerializeField]
        private KeyMetaSelectAll m_KeyMetaSelectAllPrefab;
        public KeyMetaSelectAll KeyMetaSelectAllPrefab { get { return m_KeyMetaSelectAllPrefab; } 
            set { m_KeyMetaSelectAllPrefab = value; } }

        public static KeyMetaSelectAll InstantiateSelectAll(IMetaSelectable selectable)
        {
            var selectAll = Instantiate(PayloadSelectorFactoryMaster.KeyMetaSelectAllPrefab);

            selectAll.Setup(selectable);

            return selectAll;
        }

        #endregion

        #region RowColumnSelectPayloads

        [SerializeField]
        private RowColumnMetaSelect m_SelectPrefab;
        public RowColumnMetaSelect SelectPrefab
        {
            get { return m_SelectPrefab; }
            set { m_SelectPrefab = value; }
        }

        public static RowColumnMetaSelect InstantiateRowColumnSelect(IMetaSelectable selectable)
        {
            var selectRow = Instantiate(PayloadSelectorFactoryMaster.SelectPrefab);

            selectRow.Setup(selectable);

            return selectRow;
        }

        #endregion

        #region FrustumSelectPayloads

        [SerializeField]
        private FrustumMetaSelector m_FrustumSelectPrefab;
        public FrustumMetaSelector FrustumSelectPrefab
        {
            get { return m_FrustumSelectPrefab; }
            set { m_FrustumSelectPrefab = value; }
        }

        public static FrustumMetaSelector InstantiateFrustumSelect(IMetaSelectable selectable)
        {
            var selectFrustum = Instantiate(PayloadSelectorFactoryMaster.FrustumSelectPrefab);

            selectFrustum.Setup(selectable);

            return selectFrustum;
        }

        #endregion

        #region Click-Select Receive Payloads

        public static ClickMetaSelectReceiver InstantiateClickSelect(IMetaSelectable selectable,
            GameObject receivingObject)
        {
            return InstantiateClickSelect(selectable, receivingObject, InputModifiers.CurrentInputModifiers());
        }

        public static ClickMetaSelectReceiver InstantiateClickSelect(IMetaSelectable selectable,
            GameObject receivingObject, InputModifiers modifiers)
        {
            var selectClick = receivingObject.AddComponent<ClickMetaSelectReceiver>();

            selectClick.Modifiers = modifiers;

            selectClick.Setup(selectable);

            return selectClick;
        }

        #endregion

        #region CriterionSelectEqualsPayloads

        public static CriterionEqualsMetaSelector InstantiateCriterionEqualsSelect(IMetaSelectable selectable)
        {
            var selectCriterion = new CriterionEqualsMetaSelector();

            selectCriterion.Setup(selectable);

            return selectCriterion;
        }

        #endregion

        #region CriterionSelectRangePayloads

        public static CriterionRangeMetaSelector InstantiateCriterionRangeSelect(IMetaSelectable selectable)
        {
            var selectCriterion = new CriterionRangeMetaSelector();

            selectCriterion.Setup(selectable);

            return selectCriterion;
        }

        #endregion
    }
}
