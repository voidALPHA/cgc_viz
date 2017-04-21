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

using ChainViews;
using UnityEngine;
using Utility.InputManagement;
using Utility.JobManagerSystem;

namespace Visualizers.MetaSelectors
{
    public class KeyMetaSelectAll : MonoBehaviour, IMetaSelector
    {
        private IMetaSelectable TargetSelectable { get; set; }

        [SerializeField]
        private MetaSelectionMode m_SelectionMode = new MetaSelectionMode();
        public MetaSelectionMode SelectionMode { get { return m_SelectionMode; } set { m_SelectionMode = value; } }

        public void Setup(IMetaSelectable targetSelectable)
        {
            TargetSelectable = targetSelectable;

            SelectionMode.Selectable = targetSelectable;

            transform.parent = targetSelectable.GetTransform();
        }


        private void Update()
        {
            if (Input.GetKeyDown( "]" ) && !InputFocusManager.Instance.IsAnyInputFieldInFocus( ))
            {
                if (!ChainView.Instance.IsBusy)
                {
                    ChainView.Instance.IsBusy = true;
                    JobManager.Instance.StartJob( TargetSelectable.SelectAll( ), jobName: "SelectAll", startImmediately: true, completionHandler: OnCompletion );
                }
            }

            if (Input.GetKeyDown( "[" ) && !InputFocusManager.Instance.IsAnyInputFieldInFocus( ))
            {
                if (!ChainView.Instance.IsBusy)
                {
                    ChainView.Instance.IsBusy = true;
                    JobManager.Instance.StartJob(TargetSelectable.DeselectAll(), jobName: "DeselectAll", startImmediately: true, completionHandler: OnCompletion );
                }
            }
        }

        private void OnCompletion(uint jobId)
        {
            ChainView.Instance.IsBusy = false;
        }

        public void Destruct()
        {
            DestroyImmediate(gameObject);
        }
    }
}
