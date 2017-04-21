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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Choreography.CameraControl;
using Choreography.Steps;
using UnityEngine;
using Utility;
using Visualizers;

namespace Choreography.Views.StepEditor.CuratedChoreography
{
    public class CuratedChoreographyStep : Step
    {
        private string m_ArgumentName = "Curated";
        [Controllable(LabelText = "Cmd Argument To Read")]
        public string ArgumentName { get { return m_ArgumentName; } set { m_ArgumentName = value; } }
        
        private const string EndEventName = "EndMovement";

        private List<CameraSubStep> m_CameraSubSteps = new List< CameraSubStep >();
        [SubStepContaining]
        public List<CameraSubStep> CameraSubSteps { get { return m_CameraSubSteps; } set { m_CameraSubSteps = value; } }
        
        private GameObject m_FacingObject;
        protected GameObject FacingObject
        {
            get
            {
                return m_FacingObject ?? (m_FacingObject = new GameObject("Camera Facing Target"));
            }
            set { m_FacingObject = value; }
        }

        public CuratedChoreographyStep()
        {
            PopulateSubSteps();

            Router.AddEvent( EndEventName );
        }

        public override void CleanUp()
        {
            GameObject.Destroy(FacingObject);

            FacingObject = null;
        }
        
        protected override IEnumerator ExecuteStep()
        {
            var currentTime = Time.time;

            if (CameraSubSteps.Count==0)
                PopulateSubSteps();

            foreach ( var subStep in CameraSubSteps )
            {
                bool stepCompleted = false;
                bool lookCompleted = false;

                while ( Time.time < currentTime + subStep.Delay )
                    yield return null;

                currentTime += subStep.Delay;
                var stepMovement = subStep.GenerateTranslationMovement( currentTime );
                stepMovement.OnMovementCompleted += (f) => stepCompleted = true;

                var lookMovement = subStep.GenerateLookMovement( currentTime );
                lookMovement.OnMovementCompleted += ( f ) => lookCompleted = true;

                SplineCameraControlLord.EnqueueMovement( stepMovement );
                SplineCameraControlLord.EnqueueLookMovement( lookMovement );

                while ( !stepCompleted || !lookCompleted )
                    yield return null;

                currentTime += subStep.Duration;
            }

            Router.FireEvent(EndEventName);
        }

        private void PopulateSubSteps()
        {
            var foundValue = CommandLineArgs.GetArgumentValue(ArgumentName);
                //"(10.0,00.0,10.0):(1,0,0):2:1|10.0,2.0,10.0):(1,0,0):2:1|10.0,-2.0,10.0):(1,0,0):2:1|(10.0,1.0,10.0):(1,0,0):2:1";

            var tokens = foundValue.Split('|');
            
            CameraSubSteps.Clear();

            if (tokens.Length > 0 && tokens[0].Contains(':'))
                foreach (var token in tokens)
                {
                    CameraSubSteps.Add(CameraSubStep.GenerateFromString(token));
                }
        }

        public override float BaseDuration
        {
            get { return CameraSubSteps.Sum( subStep => subStep.Duration + subStep.Delay ); }
        }
    }
}
