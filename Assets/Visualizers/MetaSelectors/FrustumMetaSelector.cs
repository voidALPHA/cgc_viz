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

using System.Collections;
using System.Linq;
using ChainViews;
using UnityEngine;
using Utility.InputManagement;
using Utility.JobManagerSystem;

namespace Visualizers.MetaSelectors
{
    public class FrustumMetaSelector : MonoBehaviour, IMetaSelector
    {
        private IMetaSelectable TargetSelectable { get; set; }

        [SerializeField]
        private MetaSelectionMode m_SelectionMode = new MetaSelectionMode(SelectionOperation.SelectOnly);
        public MetaSelectionMode SelectionMode { get { return m_SelectionMode; } set { m_SelectionMode = value; } }

        public void Setup(IMetaSelectable targetSelectable)
        {
            TargetSelectable = targetSelectable;

            SelectionMode.Selectable = targetSelectable;

            transform.parent = targetSelectable.GetTransform();
        }

        public IEnumerator SelectFrustum(Camera selectCamera, Rect screenSpaceSelect)
        {
            var selectList = (from payload in TargetSelectable.SelectablePayloads let screenPosition = selectCamera.WorldToScreenPoint(payload.VisualData.Bound.transform.position) where screenSpaceSelect.Contains(screenPosition, false) && screenPosition.z > 0 select payload).ToList();
            yield return null;

            var iterator = TargetSelectable.SelectOnly(selectList);
            while (iterator.MoveNext())
                yield return null;
        }

        private void Update()
        {
            if (Input.GetKeyDown("l") && !InputFocusManager.Instance.IsAnyInputFieldInFocus())
            {
                if (!ChainView.Instance.IsBusy)
                {
                    var mouseRect = new Rect(Input.mousePosition.x - 50, Input.mousePosition.y - 50, 100, 100);

                    ChainView.Instance.IsBusy = true;
                    JobManager.Instance.StartJob(SelectFrustum(Camera.main, mouseRect), jobName: "SelectFrustrum", startImmediately: true, completionHandler: ChainView.UnBusy);
                }
            }
        }

        public void Destruct()
        {
            DestroyImmediate(gameObject);
        }
    }
}
