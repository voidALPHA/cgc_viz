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
using Visualizers;

namespace Assets.PayloadSelection.UI
{
    public class BoundsClickSelectionOperation
    {
        private static BoundsClickSelectionOperation m_Instance;

        public static BoundsClickSelectionOperation Instance
        {
            get { return m_Instance ??(m_Instance = new BoundsClickSelectionOperation()); }
        }

        private bool m_IsSelectingBoundsSequence = false;

        public bool IsSelectingBoundsSequence
        {
            get { return m_IsSelectingBoundsSequence; }
            private set
            {
                if (m_IsSelectingBoundsSequence == value)
                    return;

                m_IsSelectingBoundsSequence = value;

                if (m_IsSelectingBoundsSequence)
                {
                    SelectedBounds = new List<BoundingBox>();
                    CurrentlyActiveBounds = new List<BoundingBox>();
                }
            }
        }

        public List<BoundingBox> SelectedBounds { get; private set; }

        public List<BoundingBox> CurrentlyActiveBounds { get; set; }

        public void StartSelectionOperation(IEnumerable<BoundingBox> topLevelBounds)
        {
            if (IsSelectingBoundsSequence)
                return;

            IsSelectingBoundsSequence = true;

            foreach (var bound in topLevelBounds)
            {
                CurrentlyActiveBounds.Add(bound);
                bound.Highlight = true;
            }
        }

        public void FinishSelectionOperation()
        {
            IsSelectingBoundsSequence = false;
        }

        public void SelectBound(BoundingBox selectedBound)
        {
            if (!IsSelectingBoundsSequence)
                return;

            SelectedBounds.Add(selectedBound);

            foreach (var bound in CurrentlyActiveBounds)
                bound.Highlight = false;

            CurrentlyActiveBounds.Clear();

            // activate discriminated child bounds of this one
            foreach (var bound in BoundingBox.DescendThroughNondiscriminatedBounds(new[] {selectedBound}))
            {
                CurrentlyActiveBounds.Add(bound);
                bound.Highlight = true;
            }
        }
    }
}
