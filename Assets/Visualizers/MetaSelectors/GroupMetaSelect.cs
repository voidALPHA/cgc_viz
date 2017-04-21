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
using ChainViews;
using Mutation;
using Utility.JobManagerSystem;

namespace Visualizers.MetaSelectors
{
    public class GroupMetaSelect : IMetaSelector
    {
        private IMetaSelectable TargetSelectable { get; set; }

        public InputModifiers Modifiers { get; set; }

        public void Setup(IMetaSelectable targetSelectable)
        {
            TargetSelectable = targetSelectable;

            SelectionMode.Selectable = TargetSelectable;
        }

        public void ApplySelection(IEnumerable<VisualPayload> payloads)
        {
            ApplySelection(payloads, InputModifiers.CurrentInputModifiers());
        }

        public void ApplySelection(IEnumerable<VisualPayload> payloads, InputModifiers inputModifiers)
        {
            if (!ChainView.Instance.IsBusy)
            {
                if (!inputModifiers.Equals(Modifiers))
                    return;

                ChainView.Instance.IsBusy = true;
                JobManager.Instance.StartJob(SelectionMode.ApplyMode(payloads),
                    jobName: "ApplyMode", startImmediately: true, completionHandler: ChainView.UnBusy);
            }
        }

        public MetaSelectionMode SelectionMode { get; set; }

        public void Destruct()
        {
        }
    }
}
