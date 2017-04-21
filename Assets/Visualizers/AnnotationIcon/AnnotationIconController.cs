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
using Chains;
using Mutation;
using UnityEngine;
using Visualizers.IsoGrid;

namespace Visualizers.AnnotationIcon
{
    public class AnnotationIconController : VisualizerController
    {
        public SelectionState DefaultState { get { return Router["Default"]; } }

        private MutableField<string> m_AnnotationType = new MutableField<string>()
        {LiteralValue = "default"};
        [Controllable(LabelText = "AnnotationType")]
        public MutableField<string> AnnotationType
        {
            get { return m_AnnotationType; }
        }

        private MutableField<string> m_AnnotationText = new MutableField<string>() 
        { LiteralValue = "" };
        [Controllable(LabelText = "AnnotationText")]
        public MutableField<string> AnnotationText { get { return m_AnnotationText; } }


        private MutableField<Color> m_RequestColorField = new MutableField<Color> { LiteralValue = Color.white };
        [Controllable( LabelText = "Request Color" )]
        private MutableField<Color> RequestColorField 
        {
            get { return m_RequestColorField; }
        }

        private MutableField<Color> m_ChallengeSetColorField = new MutableField<Color> { LiteralValue = Color.white };
        [Controllable( LabelText = "Challenge Set Color" )]
        private MutableField<Color> ChallengeSetColorField
        {
            get { return m_ChallengeSetColorField; }
        }


        public AnnotationIconController()
        {
            Router.AddSelectionState("Default");
        }

        protected override IEnumerator ProcessPayload(VisualPayload payload)
        {
            var annotationType = AnnotationType.GetLastKeyValue(payload.Data);
            var icon =
                VisualizerFactory.InstantiateAnnotationIcon(annotationType);

            if ( annotationType == "transmit" )
                icon.Color = ChallengeSetColorField.GetFirstValue( payload.Data );
            else if ( annotationType == "receive" )
                icon.Color = RequestColorField.GetFirstValue( payload.Data );

            icon.AnnotationText = AnnotationText.GetLastKeyValue( payload.Data );

            icon.Initialize(this, payload);

            yield return null;
        }
    }
}
