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
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Choreography.Views.Editor
{
    public class TimelineHelperWindow : EditorWindow
    {
        [MenuItem( "Window/Timeline Helper" )]
        [UsedImplicitly]
        static void Init()
        {
            var window = (TimelineHelperWindow)GetWindow( typeof( TimelineHelperWindow ), false, "Timeline" );

            window.Show();
        }

        [UsedImplicitly]
        void OnGUI()
        {
            DrawToolbar();

            var timeline = TimelineViewBehaviour.Instance;

            if ( timeline == null )
            {
                DrawNoTimeline();
                return;
            }

            DrawTimeline( timeline );
        }

        private static void DrawToolbar()
        {
            var oldColor = GUI.color;

            GUILayout.BeginHorizontal( EditorStyles.toolbar );

            GUI.color = new Color( 0.5f, 0.75f, 1.0f );
            GUILayout.Label( "Steps    " );

            GUI.color = new Color( 0.75f, 0.5f, 1.0f );
            GUILayout.Label( "Events" );

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            GUI.color = oldColor;
        }

        private void DrawNoTimeline()
        {
            EditorGUILayout.HelpBox( "No Timeline Detected", MessageType.Info );
        }

        private Vector2 ScrollPosition { get; set; }

        private void DrawTimeline( TimelineViewBehaviour timelineView )
        {
            ScrollPosition = GUILayout.BeginScrollView( ScrollPosition );//, true, true );

            var steps = GetChildrenWithComponentFromTransform< TimelineStepViewBehaviour >( timelineView.StepViewParent );

            if ( !DrawButton( 0, "Timeline", timelineView, timelineView.StepViewParent, steps.Any() ) )
                return;

            foreach ( var step in steps )
                DrawStep( 1, step );

            GUILayout.EndScrollView();
        }

        private void DrawStep( int indent, TimelineStepViewBehaviour stepView )
        {
            var namePrefix = stepView.Step != null && TimelineViewBehaviour.Instance.SelectedStep == stepView.Step ? "*" : "";

            var events = GetChildrenWithComponentFromTransform< TimelineEventViewBehaviour >( stepView.EventAttachmentPoint );

            if ( !DrawButton( indent, namePrefix + stepView.name, stepView, stepView.EventAttachmentPoint, events.Any() ) )
                return;

            foreach ( var @event in events )
                DrawEvent( indent + 1, @event );
        }

        private void DrawEvent( int indent, TimelineEventViewBehaviour eventView )
        {
            var steps = GetChildrenWithComponentFromTransform< TimelineStepViewBehaviour >( eventView.StepAttachmentPoint );

            if ( !DrawButton( indent, eventView.name, eventView, eventView.StepAttachmentPoint, steps.Any() ) )
                return;

            foreach ( var step in steps )
                DrawStep( indent + 1, step );
        }

        private static IEnumerable< T > GetChildrenWithComponentFromTransform< T >( Transform transform ) where T : Component
        {
            var children =
                transform.Cast<Transform>()
                    .Select( child => child.GetComponent< T >() )
                    .Where( child => child != null );

            return children;
        }


        private readonly Dictionary<Component, bool> m_FoldoutStates = new Dictionary<Component, bool>();
        private Dictionary<Component, bool> FoldoutStates { get { return m_FoldoutStates; } }

        private bool DrawButton( int indent, string text, Component component, Transform attachmentPoint, bool hasChildren )
        {
            if (!FoldoutStates.ContainsKey( component ))
                FoldoutStates.Add( component, true );

            GUILayout.BeginHorizontal();

            GUILayout.Space( indent * 8 );
            
            if ( hasChildren )
                FoldoutStates[ component ] = GUILayout.Toggle( FoldoutStates[ component ], "", EditorStyles.foldout, GUILayout.Width( 16 ) );
            else
                GUILayout.Space( 16 );

            var oldColor = GUI.color;

            if ( component is TimelineViewBehaviour ) GUI.color = new Color( 0.65f, 1.0f, 0.65f );
            else if ( component is TimelineStepViewBehaviour ) GUI.color = new Color( 0.5f, 0.75f, 1.0f );
            else if ( component is TimelineEventViewBehaviour ) GUI.color = new Color( 0.75f, 0.5f, 1.0f );

            var componentStyle = Selection.activeGameObject == component.gameObject
                ? EditorStyles.whiteLabel
                : EditorStyles.label;

            if ( GUILayout.Button( text, componentStyle, GUILayout.ExpandWidth( true ) ) )
                Selection.activeGameObject = component.gameObject;


            if ( attachmentPoint != null )
            {
                var attachmentPointStyle = Selection.activeGameObject == attachmentPoint.gameObject
                    ? EditorStyles.whiteLabel
                    : EditorStyles.label;
                
                if ( GUILayout.Button( "(AP)", attachmentPointStyle, GUILayout.ExpandWidth( false ) ) )
                    Selection.activeGameObject = attachmentPoint.gameObject;
            }

            GUI.color = oldColor;

            GUILayout.EndHorizontal();

            return FoldoutStates[ component ];
        }

        [UsedImplicitly]
        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
