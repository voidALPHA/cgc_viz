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
using System.Reflection;
using System.Runtime.InteropServices;
using Assets.PayloadSelection.UI;
using JetBrains.Annotations;
using PayloadSelection.CriterionStatements;
using Ui.TypePicker;
using UnityEngine;

namespace PayloadSelection.UI
{
    public class AddCriterionStatementDialogBehaviour : MonoBehaviour
    {
        
        #region Inspectable Properties

        [Header( "Component References" )]
        [SerializeField]
        private RectTransform m_AddCriterionStatementButtonAttachmentPoint = null;
        private RectTransform AddCriterionStatementButtonAttachmentPoint { get { return m_AddCriterionStatementButtonAttachmentPoint; } }


        [SerializeField]
        private GameObject m_AddCriterionStatementButtonPrefab = null;
        private GameObject AddCriterionStatementButtonPrefab { get { return m_AddCriterionStatementButtonPrefab; } }

        #endregion

        
        private readonly List<AddCriterionStatementButtonBehaviour> m_Buttons = new List<AddCriterionStatementButtonBehaviour>();
        private List<AddCriterionStatementButtonBehaviour> Buttons { get { return m_Buttons; } }


        [UsedImplicitly]
        private void Start()
        {
            GenerateAddCriterionButtons();
        }
        
        private void GenerateAddCriterionButtons()
        {
            // TODO: Eventually this might as well be type picker?

            var assy = Assembly.GetExecutingAssembly();

            var typeToShow = assy.GetType( typeof( CriterionStatement ).FullName, throwOnError: false, ignoreCase: true );

            var allTypes = assy.GetTypes().Where(
                t => typeToShow.IsAssignableFrom( t )
                    && !t.IsAbstract
                    && !Attribute.IsDefined( t, typeof( TypePickerIgnore ) ) ).OrderBy( t => t.Name );

            foreach ( var t in allTypes )
            {
                var descriptiveName = t.Name;

                var instancedCriterionStatement = Activator.CreateInstance( t ) as CriterionStatement;
                if ( instancedCriterionStatement != null )
                    descriptiveName = instancedCriterionStatement.Name;

                AddCriterionButton( descriptiveName, t );
            }
        }

        // TODO: This is temp, probably would take type with attribute or static property defining symbol which would become model of CriterionButtonView
        private void AddCriterionButton( string symbol, Type criterionStatementType )
        {
            var buttonGo = Instantiate( AddCriterionStatementButtonPrefab );

            var button = buttonGo.GetComponent<AddCriterionStatementButtonBehaviour>();

            button.Symbol = symbol;

            button.Clicked += () =>
            {
                Group.AddCriterion( Activator.CreateInstance( criterionStatementType ) as CriterionStatement );

                Group = null;

                Hide();
            };

            button.transform.SetParent( AddCriterionStatementButtonAttachmentPoint, false );

            Buttons.Add( button );
        }

        private CriteriaGroup Group { get; set; }

        public void Show( CriteriaGroup group )
        {
            Group = group;

            if ( Group == null )
            {
                Visible = false;
                return;
            }

            Visible = true;
        }

        public void Hide()
        {
            Group = null;
            Visible = false;
        }

        [UsedImplicitly]
        public void HandleCloseButtonClicked()
        {
            Hide();
        }

        private bool m_Visible;
        private bool Visible
        {
            get { return m_Visible; }
            set
            {
                m_Visible = value;

                gameObject.SetActive( m_Visible );
            }
        }
    }
}
