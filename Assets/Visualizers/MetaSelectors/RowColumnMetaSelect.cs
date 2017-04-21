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
using ChainViews;
using UnityEngine;
using Utility.InputManagement;
using Utility.JobManagerSystem;

namespace Visualizers.MetaSelectors
{
    public class RowColumnMetaSelect : MonoBehaviour, IMetaSelector
    {
        private IMetaSelectable TargetSelectable { get; set; }

        [SerializeField]
        private Transform m_TargetObject;
        private Transform TargetObject { get { return m_TargetObject; } set { m_TargetObject = value; } }

        [SerializeField]
        private MetaSelectionMode m_SelectionMode = new MetaSelectionMode();
        public MetaSelectionMode SelectionMode { get { return m_SelectionMode; } set { m_SelectionMode = value; } }

        private bool ColumnSelect { get; set; }

        public void Setup(IMetaSelectable targetSelectable)
        {
            TargetSelectable = targetSelectable;

            SelectionMode.Selectable = targetSelectable;

            transform.parent = targetSelectable.GetTransform();

            TargetObject.transform.position = transform.position + Vector3.one * 5f;
        }

        private void Update()
        {
            if (Input.GetKeyDown("r") && !InputFocusManager.Instance.IsAnyInputFieldInFocus())
                if (!ChainView.Instance.IsBusy)
                {
                    ChainView.Instance.IsBusy = true;
                    JobManager.Instance.StartJob(SelectPlane(TargetObject.position, transform.right), jobName: "SelectPlane", startImmediately: true, completionHandler: OnCompletion );
                }

            if (Input.GetKeyDown("t") && !InputFocusManager.Instance.IsAnyInputFieldInFocus())
                if (!ChainView.Instance.IsBusy)
                {
                    ChainView.Instance.IsBusy = true;
                    JobManager.Instance.StartJob(SelectPlane(TargetObject.position, transform.forward), jobName: "SelectPlane", startImmediately: true, completionHandler: OnCompletion);
                }

            if (Input.GetKeyDown("y") && !InputFocusManager.Instance.IsAnyInputFieldInFocus())
                if (!ChainView.Instance.IsBusy)
                {
                    ChainView.Instance.IsBusy = true;
                    JobManager.Instance.StartJob( SelectPlane( TargetObject.position, transform.up ), jobName: "SelectPlane", startImmediately: true, completionHandler: OnCompletion );
                }
        }

        private void OnCompletion(uint jobId)
        {
            ChainView.Instance.IsBusy = false;
        }

        public IEnumerator SelectPlane(Vector3 includedPosition, Vector3 planeNormal)
        {
            BoundingBox selectVolume = BoundingBox.ConstructBoundingBox( GetType().Name, includedPosition,
                Quaternion.LookRotation(planeNormal), Vector3.one);

            selectVolume.transform.localScale = new Vector3(999f, 999f, 1f);

            selectVolume.transform.position -= selectVolume.transform.right*500f + selectVolume.transform.up*500f;

            var iterator = PayloadSelectVolume.SelectVolumeByCenter(SelectionMode, selectVolume);
            while (iterator.MoveNext( ))
                yield return null;

            //selectVolume.name = "Select Volume";

            DestroyImmediate(selectVolume.gameObject);
        }

        public void Destruct()
        {
            DestroyImmediate(gameObject);
        }
    }
}
