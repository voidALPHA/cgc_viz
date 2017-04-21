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
using Chains;
using Mutation;
using Newtonsoft.Json;
using UnityEngine;
using Visualizers;
using Visualizers.MetaSelectors;

namespace LabelSystem
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AxisLabelSet
    {
        public event Action DeletionRequested = delegate { };

        private LabelSystem m_LabelSystem;
        public LabelSystem LabelSystem { get { return m_LabelSystem; } set { m_LabelSystem = value; } }


        private MutableField<bool> m_ShowAxisLabel = new MutableField<bool>() { LiteralValue = false };
        [Controllable(LabelText = "Show Axis Label")]
        private MutableField<bool> ShowAxisLabel { get { return m_ShowAxisLabel; } }

        private MutableField<string> m_AxisLabelText = new MutableField<string>() { LiteralValue = "" };
        [Controllable(LabelText = "Axis Label Text")]
        private MutableField<string> AxisLabelText { get { return m_AxisLabelText; } }

        private MutableField<Vector3> m_AxisLabelOffset = new MutableField<Vector3> { LiteralValue = Vector3.zero };
        [Controllable(LabelText = "Axis Label Offset")]
        private MutableField<Vector3> AxisLabelOffset { get { return m_AxisLabelOffset; } }

        private MutableField<LabelOrientation> m_AxisLabelOrientation = new MutableField<LabelOrientation> { LiteralValue = LabelOrientation.Out };
        [Controllable(LabelText = "Axis Label Orientation")]
        private MutableField<LabelOrientation> AxisLabelOrientation { get { return m_AxisLabelOrientation; } }


        private MutableField<Vector3> m_StartingOffset = new MutableField<Vector3> { LiteralValue = Vector3.zero };
        [Controllable(LabelText = "Starting Offset")]
        private MutableField<Vector3> StartingOffset { get { return m_StartingOffset; } }

        private MutableField<Vector3> m_AxisOrientation = new MutableField<Vector3> { LiteralValue = Vector3.right };
        [Controllable(LabelText = "Direction")]
        private MutableField<Vector3> AxisOrientation { get { return m_AxisOrientation; } }

        private MutableField<LabelOrientation> m_ItemLabelOrientation = new MutableField<LabelOrientation> { LiteralValue = LabelOrientation.Out };
        [Controllable(LabelText = "Item Label Orientation")]
        private MutableField<LabelOrientation> ItemLabelOrientation { get { return m_ItemLabelOrientation; } }

        private MutableField<AxialLabelAxisMode> m_AxisMode = new MutableField<AxialLabelAxisMode> { LiteralValue = AxialLabelAxisMode.Discrete };
        [Controllable(LabelText = "Axis Mode")]
        private MutableField<AxialLabelAxisMode> AxisMode { get { return m_AxisMode; } }


        // For discrete mode:
        private MutableField<float> m_SizePeriod = new MutableField<float> { LiteralValue = 1.0f };
        [Controllable(LabelText = "Disc: Periodic Dist")]
        private MutableField<float> SizePeriod { get { return m_SizePeriod; } }

        private MutableField<string> m_ItemText = new MutableField<string>() { LiteralValue = "" };
        [Controllable(LabelText = "Disc: Item Text")]
        private MutableField<string> ItemText { get { return m_ItemText; } }

        private MutableField<int> m_AxisIndexVariable = new MutableField<int>() { LiteralValue = 0 };
        [Controllable(LabelText = "Disc: Axis Index Var")]
        private MutableField<int> AxisIndexVariable { get { return m_AxisIndexVariable; } }


        // For continuous mode:
        private MutableField<float> m_AxisLength = new MutableField<float> { LiteralValue = 30.0f };
        [Controllable(LabelText = "Cont: Axis Length")]
        private MutableField<float> AxisLength { get { return m_AxisLength; } }

        private MutableField<int> m_LabelCount = new MutableField<int> { LiteralValue = 5 };
        [Controllable(LabelText = "Cont: Label Count")]
        private MutableField<int> LabelCount { get { return m_LabelCount; } }

        private MutableField<float> m_ComparisonValue = new MutableField<float> { LiteralValue = 0.0f };
        [Controllable(LabelText = "Cont: Comparison Value")]
        private MutableField<float> ComparisonValue { get { return m_ComparisonValue; } }

        private MutableField<float> m_MaxValue = new MutableField<float> { LiteralValue = 30.0f };
        [Controllable(LabelText = "Cont: Max Value")]
        private MutableField<float> MaxValue { get { return m_MaxValue; } }

        private MutableField<float> m_MinValue = new MutableField<float> { LiteralValue = 0.0f };
        [Controllable(LabelText = "Cont: Min Value")]
        private MutableField<float> MinValue { get { return m_MinValue; } }

        // (end of mode-specific fields)

        [JsonProperty]
        public ChainNode ChainNode { get; set; }

        private float MinimumValue { get; set; }
        private float MaximumValue { get; set; }


        public void RequestDeletion()
        {
            DeletionRequested( );
        }

        public void Render(VisualPayload payload, IMetaSelectable selectable)
        {
            RenderAxisLabel(payload);   // Optional 'Axis title'

            // Labels along an axis:
            var mode = AxisMode.GetLastKeyValue( payload.Data );

            if (mode == AxialLabelAxisMode.Discrete)
            {
                // create equality criteria
                var normalSelector = PayloadSelectorFactory.InstantiateCriterionEqualsSelect(selectable);
                normalSelector.SelectionMode.OperationToPerform = SelectionOperation.SelectOnly;
                normalSelector.CriterionField = (mut) => AxisIndexVariable.GetValue(mut);
                normalSelector.ArrityLevels = AxisIndexVariable.NumberOfIntermediates;

                var toggleSelector = PayloadSelectorFactory.InstantiateCriterionEqualsSelect(selectable);
                toggleSelector.SelectionMode.OperationToPerform = SelectionOperation.ToggleFullySelected;
                toggleSelector.RequiredModifiers = new InputModifiers() { Control = true };
                toggleSelector.CriterionField = (mut) => AxisIndexVariable.GetValue(mut);
                toggleSelector.ArrityLevels = AxisIndexVariable.NumberOfIntermediates;

                normalSelector.FieldLastKey = AxisIndexVariable.AbsoluteKey;
                toggleSelector.FieldLastKey = AxisIndexVariable.AbsoluteKey;

                var discreteCriterionSelectors = new List<ICriterionMetaSelector>()
                {
                    normalSelector, toggleSelector
                };

                RenderItemLabelsDiscrete(payload, discreteCriterionSelectors);
            }
            else if (mode == AxialLabelAxisMode.Continuous)
            {
                // create range criteria
                var normalRangeSelector = PayloadSelectorFactory.InstantiateCriterionRangeSelect(selectable);
                normalRangeSelector.SelectionMode.OperationToPerform = SelectionOperation.SelectOnly;
                normalRangeSelector.CriterionField = (mut) => ComparisonValue.GetValue(mut);
                normalRangeSelector.ArrityLevels = ComparisonValue.NumberOfIntermediates;

                var toggleRangeSelector = PayloadSelectorFactory.InstantiateCriterionRangeSelect(selectable);
                toggleRangeSelector.SelectionMode.OperationToPerform = SelectionOperation.ToggleFullySelected;
                toggleRangeSelector.RequiredModifiers = new InputModifiers() { Control = true };
                toggleRangeSelector.CriterionField = (mut) => ComparisonValue.GetValue(mut);
                toggleRangeSelector.ArrityLevels = ComparisonValue.NumberOfIntermediates;

                normalRangeSelector.FieldLastKey = ComparisonValue.AbsoluteKey;
                toggleRangeSelector.FieldLastKey = ComparisonValue.AbsoluteKey;

                var continuousCriterionSelectors = new List<ICriterionMetaSelector>()
                {
                    normalRangeSelector, toggleRangeSelector
                };

                RenderItemLabelsContinuous(payload, continuousCriterionSelectors);
            }
        }

        private void RenderAxisLabel(VisualPayload payload)
        {            
            if (ShowAxisLabel.GetLastKeyValue( payload.Data ))
            {
                var labelComponent = LabelSystem.CreateLabel(AxisLabelText.GetLastKeyValue(payload.Data), AxisLabelOffset.GetLastKeyValue(payload.Data), AxisLabelOrientation.GetLastKeyValue(payload.Data));

                var targetSize = LabelSystem.GetDesiredLabelLength(labelComponent);

                LabelSystem.SetLabelLength( labelComponent, targetSize );
            }
        }

        private void RenderItemLabelsDiscrete(VisualPayload payload, List<ICriterionMetaSelector> criteria)
        {
            var sizePeriod = SizePeriod.GetLastKeyValue(payload.Data);

            const float shiftRatio = 0.50f; // 0.50 means 'centered'    todo: make this a setting (mutable field)

            // Determine how many labels to create, based on the maximum 'index' (coordinate) of the axis in question
            var entries = AxisIndexVariable.GetEntries(payload.Data);

            var maxVar = -1;
            foreach ( var e in entries )
                maxVar = Mathf.Max( maxVar, AxisIndexVariable.GetValue( e ) );
            if ( maxVar == -1 )
                return;
            maxVar += 1;

            //var numLabels = 
            //    //!entries.Any() ? 0 : 
            //    entries.Max( e => AxisIndexVariable.GetValue( e ) ) + 1;

            RenderItemLabelsInternal(payload, maxVar, sizePeriod, shiftRatio, true, criteria);
        }

        private void RenderItemLabelsContinuous(VisualPayload payload, List<ICriterionMetaSelector> criteria)
        {
            if (MinValue.UseLiteralValue)
                MinimumValue = MinValue.GetLastKeyValue( payload.Data );
            else
            {
                var entries = MinValue.GetEntries( payload.Data );
                MinimumValue = entries.Min( e => MinValue.GetValue( e ) );
            }

            if (MaxValue.UseLiteralValue)
                MaximumValue = MaxValue.GetLastKeyValue( payload.Data );
            else
            {
                var entries = MaxValue.GetEntries(payload.Data);
                MaximumValue = entries.Max(e => MaxValue.GetValue(e));
            }

            var numLabels = LabelCount.GetLastKeyValue(payload.Data);

            var totalLength = AxisLength.GetLastKeyValue(payload.Data);

            if (numLabels > 0)
            {
                float sizePeriod;

                if (numLabels > 1)
                    sizePeriod = totalLength / (float)( numLabels - 1 );
                else
                    sizePeriod = totalLength;

                
                foreach (var criterion in criteria)
                {
                    var rangeCriterion = criterion as CriterionRangeMetaSelector;
                    if (rangeCriterion == null)
                        return;

                    rangeCriterion.RangeWidth = (MaximumValue - MinimumValue) / ((numLabels - 1)*2f);
                }


                RenderItemLabelsInternal(payload, numLabels, sizePeriod, 0.0f, false, criteria);
            }
        }

        private void RenderItemLabelsInternal(VisualPayload payload, int numLabels, float sizePeriod, float shiftRatio, bool discrete, List<ICriterionMetaSelector> criteria)
        {
            var startingOffset = StartingOffset.GetLastKeyValue( payload.Data );
            var axisOrientation = AxisOrientation.GetLastKeyValue( payload.Data );

            var startX = startingOffset.x + ( sizePeriod * shiftRatio * axisOrientation.x );
            var startY = startingOffset.y + ( sizePeriod * shiftRatio * axisOrientation.y );
            var startZ = startingOffset.z + ( sizePeriod * shiftRatio * axisOrientation.z );

            var localOffset = new Vector3(startX, startY, startZ);
            var localOffsetAdd = new Vector3(sizePeriod * axisOrientation.x, sizePeriod * axisOrientation.y, sizePeriod * axisOrientation.z);

            var currentContinuousValue = MinimumValue;
            var continuousValueIncrement = 0.0f;
            if (numLabels > 1)
                continuousValueIncrement = ( MaximumValue - MinimumValue ) / (float)(numLabels - 1);

            var maxSize = 0.0f;
            List<LabelBehaviour> labelsCreated = new List<LabelBehaviour>();



            for (var labelIndex = 0; labelIndex < numLabels; labelIndex++)
            {
                var text = String.Empty;
                object rowIndex = 0;

                if (discrete)
                {
                    var entries = AxisIndexVariable.GetEntries( payload.Data );

                    // Mind you, this is super inefficient.
                    foreach (var entry in entries)
                    {
                        if (labelIndex == AxisIndexVariable.GetValue( entry ))
                        {
                            text = ItemText.GetValue( entry );
                            rowIndex = AxisIndexVariable.GetValue(entry);
                            break;
                        }
                    }
                }
                else
                {
                    text = currentContinuousValue.ToString("F2");
                    rowIndex = (float)currentContinuousValue;
                    currentContinuousValue += continuousValueIncrement;
                }

                var axisElementObj = LabelSystemFactory.InstantiateAxisElementLabel();

                var labelComponent = axisElementObj.GetComponent<AxisElementLabel>();
                    
                LabelSystem.CreateLabel(text, localOffset, ItemLabelOrientation.GetLastKeyValue(payload.Data), labelComponent);

                labelComponent.SetCriteria(criteria, rowIndex);


                labelsCreated.Add(labelComponent);
                maxSize = Math.Max(maxSize, LabelSystem.GetDesiredLabelLength(labelComponent));

                localOffset += localOffsetAdd;
            }

            foreach (var label in labelsCreated)
                LabelSystem.SetLabelLength(label, maxSize);
        }
    }
}
