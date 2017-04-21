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
using System.Windows.Forms;
using UnityEngine;

namespace Choreography.Views.StepEditor.CuratedChoreography
{
    [AttributeUsage( AttributeTargets.Property )]
    public class SubStepContaining : Attribute
    {
        
    }

    public class CameraSubStepListView : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField]
        private RectTransform m_SubStepViewRoot = null;
        private RectTransform SubStepViewRoot { get { return m_SubStepViewRoot; } }

        [SerializeField]
        private RectTransform m_AddButtonRootTransform = null;
        private RectTransform AddButtonRootTransform { get { return m_AddButtonRootTransform; } }


        [Header("Prefab References")]
        [SerializeField]
        private GameObject m_SubStepViewPrefab = null;
        private GameObject SubStepViewPrefab { get { return m_SubStepViewPrefab; } }

        [Header("Parameter Defaults")]
        [SerializeField]
        private float m_DefaultDuration = 2.5f;
        private float DefaultDuration { get { return m_DefaultDuration; } }

        [SerializeField]
        private float m_DefaultDelay = 1.5f;
        private float DefaultDelay { get { return m_DefaultDelay; } }


        private readonly Dictionary<CameraSubStep, CameraSubStepView> m_SubStepsToSubStepViews = new Dictionary< CameraSubStep, CameraSubStepView >();
        private Dictionary< CameraSubStep, CameraSubStepView > SubStepsToCameraSubStepViews { get { return m_SubStepsToSubStepViews;} }

        private List< CameraSubStep > m_CameraSubSteps;

        public List< CameraSubStep > CameraSubSteps
        {
            get { return m_CameraSubSteps; }
            set
            {
                ClearSubSteps();
                m_CameraSubSteps = value;
                AssignSubSteps(m_CameraSubSteps);
            }
        }

        private void ClearSubSteps()
        {
            foreach ( var kvp in m_SubStepsToSubStepViews )
            {
                GameObject.Destroy( kvp.Value.gameObject );
            }

            SubStepsToCameraSubStepViews.Clear();

            if(CameraSubSteps != null)
                CameraSubSteps.Clear();
        }

        private void AssignSubSteps( List< CameraSubStep > subSteps )
        {
            foreach ( var subStep in subSteps )
            {
                if(!SubStepsToCameraSubStepViews.ContainsKey(subStep))
                {
                    AddNewSubStepView(subStep);
                }
            }

            RefreshAllNumberLabels();
        }

        private CameraSubStepView AddNewSubStepView( CameraSubStep subStep )
        {
            var subStepViewObj = GameObject.Instantiate( SubStepViewPrefab );

            var subStepView = subStepViewObj.GetComponent< CameraSubStepView >();

            subStepViewObj.transform.SetParent( SubStepViewRoot,false );
            subStepViewObj.transform.SetAsLastSibling();
            AddButtonRootTransform.transform.SetAsLastSibling();

            subStepView.SubStep = subStep;
            subStepView.RemoveButtonClicked += RemoveSubStepView;

            SubStepsToCameraSubStepViews.Add( subStep, subStepView );

            return subStepView;
        }

        public void HandleCopyToClipboardClicked()
        {
            DumpToClipboard();
        }

        private void DumpToClipboard()
        {
            Clipboard.SetText((from subStep in CameraSubSteps
                               select subStep.ToString())
                              .Aggregate((a, b) => a + "|" + b));
        }

        public void HandleRedrawFromClipboardClicked(bool clear)
        {
            DrawFromClipboard(clear);
        }

        private void DrawFromClipboard(bool clear)
        {
            var foundValue = Clipboard.GetText();
            //"(10.0,00.0,10.0):(1,0,0):2:1|10.0,2.0,10.0):(1,0,0):2:1|10.0,-2.0,10.0):(1,0,0):2:1|(10.0,1.0,10.0):(1,0,0):2:1";

            var tokens = foundValue.Split('|');

            if(clear)
                ClearSubSteps();
            
            if ( tokens.Length > 0 && tokens[ 0 ].Contains( ':' ) )
            {
                foreach ( var token in tokens )
                {
                    var newStep = CameraSubStep.GenerateFromString( token );

                    CameraSubSteps.Add( newStep );
                }
            }
            AssignSubSteps(CameraSubSteps);
        }

        public void HandleAddNewSubStep()
        {
            var subStepNumber = CameraSubSteps.Count>0?SubStepsToCameraSubStepViews[ CameraSubSteps.Last() ].GetNumberLabelValue()+1:0;

            var newSubStep = new CameraSubStep();
            CameraSubSteps.Add( newSubStep );
            newSubStep.Delay = DefaultDelay;
            newSubStep.Duration = DefaultDuration;

            var newStepView = AddNewSubStepView( newSubStep );

            newStepView.SaveCameraData();

            newStepView.SetNumberLabel(subStepNumber);
        }

        public void HandleSubStepClear()
        {
            ClearSubSteps();
        }

        private void RemoveSubStepView( CameraSubStep subStep )
        {
            if ( !SubStepsToCameraSubStepViews.ContainsKey( subStep ) )
                return;

            var subStepView = SubStepsToCameraSubStepViews[ subStep ];
            
            SubStepsToCameraSubStepViews.Remove(subStep);

            CameraSubSteps.Remove( subStep );

            GameObject.Destroy(subStepView.gameObject);

            //RefreshAllNumberLabels();
        }

        private void RefreshAllNumberLabels()
        {
            foreach(var substep in CameraSubSteps)
            {
                SubStepsToCameraSubStepViews[substep].RefreshNumberLabel();
            }
        }
    }
}
