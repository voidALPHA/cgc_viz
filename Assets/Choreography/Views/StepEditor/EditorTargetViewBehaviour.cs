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
using Choreography.Steps;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Choreography.Views.StepEditor
{
    public class EditorTargetViewBehaviour : MonoBehaviour, IPointerClickHandler
    {
        public event Action< EditorTargetViewBehaviour > Clicked = delegate { };


        [Header("Component References")]

        [SerializeField]
        private Image m_BackgroundImageComponent = null;
        private Image BackgroundImageComponent { get { return m_BackgroundImageComponent; } }

        [SerializeField]
        private Text m_NameTextComponent = null;
        private Text NameTextComponent { get { return m_NameTextComponent; } }



        private Step m_Target;
        public Step Target
        {
            get
            {
                return m_Target;
            }
            set
            {
                if ( m_Target != null )
                {
                    if ( value != null )
                        throw new InvalidOperationException( "Cannot reuse this view." );
                }

                m_Target = value;

                if ( m_Target != null )
                {
                    NameTextComponent.text = m_Target.Name;

                    //BackgroundImageComponent.color = TimelineColorManager.GetStepColor( m_Target );
                }
            }
        }

        public void OnPointerClick( PointerEventData eventData )
        {
            Clicked( this );
        }

        [UsedImplicitly]
        public void HandleRemoveButtonPressed()
        {
            //RemovalRequested( this );
            Target.RequestRemoval();
        }

        [UsedImplicitly]
        public void HandleMoveUpArrowPressed()
        {
            Target.RequestMoveUp();
        }

        [UsedImplicitly]
        public void HandleMoveDownArrowPressed()
        {
            Target.RequestMoveDown();
        }
    }
}
