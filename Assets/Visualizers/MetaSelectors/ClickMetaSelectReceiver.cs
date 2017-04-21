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
using ChainViews;
using Mutation;
using UnityEngine;
using Utility.JobManagerSystem;

namespace Visualizers.MetaSelectors
{
    public class ClickMetaSelectReceiver : MonoBehaviour, IMetaSelector
    {
        [SerializeField]
        private MetaSelectionMode m_SelectionMode = new MetaSelectionMode();
        public MetaSelectionMode SelectionMode { get { return m_SelectionMode; } set { m_SelectionMode = value; } }

        public InputModifiers Modifiers { get; set; }

        public void Setup(IMetaSelectable targetSelectable)
        {
            SelectionMode.Selectable = targetSelectable;
        }

        public void SendClickEvent(VisualPayload payload, InputModifiers inputModifiers)
        {
            if (!ChainView.Instance.IsBusy)
            {
                if (!inputModifiers.Equals(Modifiers))
                    return;

                ChainView.Instance.IsBusy = true;
                JobManager.Instance.StartJob(SelectionMode.ApplyMode(new List<VisualPayload>() 
                    { payload }), jobName: "ApplyMode", startImmediately: true, completionHandler: ChainView.UnBusy);
            }
        }

        public void Destruct()
        {
        }
    }
}
