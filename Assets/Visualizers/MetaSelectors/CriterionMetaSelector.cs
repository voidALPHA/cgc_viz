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
using System.Diagnostics;
using ChainViews;
using Mutation;
using UnityEngine;
using Utility.JobManagerSystem;
using Debug = UnityEngine.Debug;

namespace Visualizers.MetaSelectors
{
    public interface ICriterionMetaSelector : IMetaSelector
    {
        InputModifiers RequiredModifiers { get; set; }

        void SelectCriterion(object compareValue);

        Func<List<MutableObject>, object> CriterionField { get; set; }
        int ArrityLevels { get; set; }
    }

    public abstract class CriterionMetaSelector<T> : ICriterionMetaSelector
    {
        public void Setup(IMetaSelectable targetSelectable)
        {
            TargetSelectable = targetSelectable;

            SelectionMode.Selectable = targetSelectable;
        }

        public string FieldLastKey { get; set; }

        public InputModifiers RequiredModifiers { get; set; }

        [SerializeField]
        private MetaSelectionMode m_SelectionMode = new MetaSelectionMode(SelectionOperation.SelectOnly);
        public MetaSelectionMode SelectionMode { get { return m_SelectionMode; } set { m_SelectionMode = value; } }

        private IMetaSelectable TargetSelectable { get; set; }

        public Func<List<MutableObject>, object> CriterionField { get; set; }
        public int ArrityLevels { get; set; }

        public void SelectCriterion(object compareValue)
        {
            //if (!typeof(T).IsAssignableFrom(compareValue.GetType()))
            //    throw new Exception("Type invalid in a criterion selector!");

            var foundVal = (T)compareValue;

            SelectTypedCriterion(foundVal);
        }

        public void SelectTypedCriterion(T compareValue)
        {
            if (!InputModifiers.CurrentInputModifiers().Equals(RequiredModifiers))
                return;

            var selectionList = new List<VisualPayload>();
                //TargetSelectable.SelectablePayloads.Where(element => CriterionField(element.Data) == compareValue).ToList();

            List<MutableObject> resolutionList=new List<MutableObject>();
          
            for (var i = 0; i < ArrityLevels; i++)
            {
                resolutionList.Add(new MutableObject());
            }

            foreach (var element in TargetSelectable.SelectablePayloads)
            {
                resolutionList.Add(element.Data);
                var criterionValue = CriterionField(resolutionList);
                //if (!typeof(T).IsAssignableFrom(compareValue.GetType()))
                //    throw new Exception("Invalid type comparing values!");
                if (CompareValues((T)criterionValue, compareValue))
                    selectionList.Add(element);
                resolutionList.RemoveAt(ArrityLevels);
            }

            if (!ChainView.Instance.IsBusy)
            {
                ChainView.Instance.IsBusy = true;
                JobManager.Instance.StartJob(SelectionMode.ApplyMode(selectionList), jobName: "Select Criterion", startImmediately: true, completionHandler: ChainView.UnBusy);
            }
        }

        protected abstract bool CompareValues(T criterionValue, T compareValue);

        public void Destruct()
        {
        }
    }

    public class CriterionEqualsMetaSelector : CriterionMetaSelector<object>
    {
        protected override bool CompareValues(object criterionValue, object compareValue)
        {
            return criterionValue.Equals(compareValue);
        }
    }

    public class CriterionRangeMetaSelector : CriterionMetaSelector<float>
    {
        private float m_RangeWidth = .5f;
        public float RangeWidth
        {
            get { return m_RangeWidth; }
            set { m_RangeWidth = value; }
        }

        protected override bool CompareValues(float criterionValue, float compareValue)
        {
            return (Mathf.Abs(criterionValue - compareValue) <= RangeWidth);
        }
    }
}
