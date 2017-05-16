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
using ChainViews;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Undo;
using Object = UnityEngine.Object;

namespace Ui
{
    public class Draggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {

        //TODO: Add Bounding Rect (internal-to, such as icons on a workspace; and external, such as a scrollview?)

        [Header("References")]

        [SerializeField]
        private List< Transform > m_Targets = new List< Transform >();
        public List< Transform > Targets { get { return m_Targets; } set { m_Targets = value; } }
        
        [SerializeField]
        private int m_SnapGridSize = 1;
        public int SnapGridSize { get { return m_SnapGridSize; } }

        [SerializeField]
        private Texture2D m_DragCursor = null;
        private Texture2D DragCursor { get { return m_DragCursor; } }

        [SerializeField]
        private bool m_DoLog;
        private bool DoLog { get { return m_DoLog; } set { m_DoLog = value; } }


        [SerializeField]
        private Object m_DropObject = null;
        private Object DropObject { get { return m_DropObject; } }


        [Header("Configuration")]

        [SerializeField]
        private bool m_DragRootOnMiddleMouse = false;
        private bool DragRootOnMiddleMouse { get { return m_DragRootOnMiddleMouse; } }

        [SerializeField]
        private bool m_DragInScreenSpace = false;
        private bool DragInScreenSpace { get { return m_DragInScreenSpace; } }

        //[SerializeField]
        //private bool m_MoveToTopWhenReleased;
        //private bool MoveToTopWhenReleased { get { return m_MoveToTopWhenReleased; } set { m_MoveToTopWhenReleased = value; } }

        private bool m_MouseDownOnMe;
        private bool MouseDownOnMe
        {
            get { return m_MouseDownOnMe; }
            set
            {
                if ( m_MouseDownOnMe == value )
                    return;

                m_MouseDownOnMe = value;
                MouseDownCount += m_MouseDownOnMe ? 1 : -1;
            }
        }

        private static int MouseDownCount { get; set; }
        public static bool IsMouseDown { get { return MouseDownCount > 0; } }

        private Vector3 StartMousePosition { get; set; }

        private Vector3 LastMousePosition { get; set; }

        private Dictionary< Transform, Vector3 > TargetStartPositions { get; set; }

        public bool IsDragging { get; private set; }

        private int DragThreshold { get { return EventSystem.current.pixelDragThreshold; } }

        public Vector3 OriginalPosition { get; set; }

        public static readonly Dictionary<int, Draggable> Draggables = new Dictionary<int, Draggable>();
        private static int LargestDragID { get; set; }
        private int m_DragID = -1;
        public int DragID
        {
            get
            {
                if(m_DragID > -1) return m_DragID;
                Draggables[m_DragID = LargestDragID++] = this;
                return m_DragID;
            }
            set
            {
                if(m_DragID > -1)
                {
                    Draggables.Remove(m_DragID);
                }
                Draggables[m_DragID = value] = this;
            }
        }

        //public bool ShouldReturn { get; set; }

        public event Action MouseDown = delegate { };
        public event Action MouseUp = delegate { };
        public event Action DragStarted = delegate { };
        public event Action DragEnded = delegate { };
        public event Action DragMoved = delegate { };

        private Draggable CurrentDragActor { get; set; }

        private PointerEventData.InputButton CurrentInputButton { get; set; }

        public void OnPointerDown( PointerEventData pointerEventData )
        {
            if ( MouseDownOnMe ) // if any mouse button is already pressed, ignore all others
                return;

            CurrentInputButton = pointerEventData.button;

            if ( DragRootOnMiddleMouse && pointerEventData.button == PointerEventData.InputButton.Middle )
            {
                CurrentDragActor = transform.root.GetComponentInChildren< Draggable >();

                CurrentDragActor.ProcessPointerDown( pointerEventData );
                
                return;
            }

            if ( pointerEventData.button == PointerEventData.InputButton.Left )
                ProcessPointerDown( pointerEventData );
        }

        private void ProcessPointerDown( PointerEventData pointerEventData )
        {
            MouseDownOnMe = true;

            MouseDown();

            OriginalPosition = transform.localPosition;

            TargetStartPositions = new Dictionary< Transform, Vector3 >();
            Targets.ForEach( t => TargetStartPositions.Add( t, t.position ) );

            Vector3 smp;
            if(DragInScreenSpace)
            {
                smp = Input.mousePosition;
            }
            else
            {
                RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform, Input.mousePosition, ChainView.Instance.Camera, out smp);
            }
            
            StartMousePosition = smp;

            pointerEventData.Use();
        }

        public void OnPointerUp( PointerEventData pointerEventData )
        {
            if ( CurrentInputButton != pointerEventData.button ) // if a mouse button is released that isn't the current, ignore it
                return;

            if ( DragRootOnMiddleMouse && CurrentDragActor != null )
            {
                CurrentDragActor.ProcessPointerUp( pointerEventData );

                CurrentDragActor = null;

                return;
            }

            ProcessPointerUp( pointerEventData );
        }

        private void ProcessPointerUp( PointerEventData pointerEventData )
        {
            if ( !MouseDownOnMe )
                return;

            MouseDownOnMe = false;

            MouseUp();

            if ( !IsDragging ) //|| ShouldReturn )
            {
                foreach ( var pair in TargetStartPositions )
                    pair.Key.position = pair.Value;

                // Don't use the click if we weren't dragging... That's the point of the threshold!

                return;
            }

            IsDragging = false;

            SnapTargetsToGrid();

            DragEnded();

            pointerEventData.Use();

            DoDrop();
        }

        public void FireDragEnded()
        {
            DragEnded();
        }


        private void SnapTargetsToGrid()
        {
            foreach ( var target in Targets )
            {
                var p = target.localPosition;

                target.localPosition = SnapVector( p );
            }
        }

        private int SnapValue( float value )
        {
            if ( SnapGridSize == 0 )
                return Mathf.RoundToInt( value );

            var sign = Mathf.Sign( value );

            var halfSnapGridSize = SnapGridSize / 2.0f * sign;

            var snapped = ( Mathf.RoundToInt( value + halfSnapGridSize ) / SnapGridSize ) * SnapGridSize;

            //Debug.LogFormat( "Snapped {0} to {1}", value, snapped );

            return snapped;
        }

        private Vector3 SnapVector( Vector3 toSnap )
        {
            return new Vector3( SnapValue( toSnap.x ), SnapValue( toSnap.y ), SnapValue( toSnap.z ) );
        }

        [UsedImplicitly]
        private void Update()
        {
            if ( !MouseDownOnMe )
                return;

            if(m_DragID == -1) // Intentionally referencing backing field rather than property
            {
                var id = DragID; // Put self into Draggable Dictionary
            }

            Vector3 currentMousePosition;
            if(DragInScreenSpace)
            {
                currentMousePosition = Input.mousePosition;
            }
            else
            {
                RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform, Input.mousePosition, ChainView.Instance.Camera, out currentMousePosition);
            }
            

            if ( !IsDragging )
                if ( Vector3.Distance( currentMousePosition, StartMousePosition ) >= DragThreshold )
                {
                    IsDragging = true;

                    DragStarted();
                }


            var diff = currentMousePosition - StartMousePosition;

            foreach ( var pair in TargetStartPositions )
            {
                var pos = pair.Value + diff;
                
                pair.Key.position = pos;
            }

            if ( ( currentMousePosition - LastMousePosition ).sqrMagnitude > 0 )
                DragMoved();

            LastMousePosition = currentMousePosition;

            if ( IsDragging )
                UpdateDropTarget();
        }

        private GameObject m_DropTarget;
        private GameObject DropTarget
        {
            get { return m_DropTarget; }
            set
            {
                if ( m_DropTarget == value )
                    return;

                if ( m_DropTarget != null )
                {
                    m_DropTarget.SendMessage( "OnDragExit", DropObject, SendMessageOptions.DontRequireReceiver );
                }

                m_DropTarget = value;

                Log( "Setting DropTarget to " + m_DropTarget );

                if (m_DropTarget != null)
                {
                    m_DropTarget.SendMessage("OnDragEnter", DropObject, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        private void UpdateDropTarget()
        {
            PointerEventData cursor = new PointerEventData(EventSystem.current);
            cursor.position = Input.mousePosition;

            var Hits = GraphicRaycasterVA.Instance.LastResultList;

            var otherHits = Hits.Where( h => !Targets.Any( t => h.gameObject.transform.IsChildOf( t ) ) );

            if ( !otherHits.Any() )
                return;

            DropTarget = otherHits.First().gameObject;
        }

        private void DoDrop()
        {
            if ( DropObject == null )
                return;

            if ( DropTarget == null )
                return;

            // Hmm, should this class handle a failed drop by restoring original position, or is that implementation dependent?

            DropTarget.SendMessage( "OnDrop", DropObject, SendMessageOptions.DontRequireReceiver );

            DropTarget = null;
        }

        public void OnPointerEnter( PointerEventData eventData )
        {
            if ( DragCursor != null )
                Cursor.SetCursor( DragCursor, new Vector2( 16,16 ), CursorMode.Auto );
        }

        public void OnPointerExit( PointerEventData eventData )
        {
            if ( DragCursor != null )
                Cursor.SetCursor( null, Vector2.zero, CursorMode.Auto );
        }

        private void Log( string message, Object context = null )
        {
            if ( !DoLog )
                return;

            Debug.Log("<color=#993399>[DRAG] " + message + "</color>", context ?? this );
        }

        private void OnDestroy()
        {
            Draggables.Remove(DragID);
        }


    }
}