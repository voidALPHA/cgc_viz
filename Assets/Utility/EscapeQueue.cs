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

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Utility
{
    public interface IEscapeQueueHandler
    {
        void HandleEscape();
    }

    public class EscapeQueue : MonoBehaviour
    {
        private static GameObject SceneObject { get; set; }
        private static void EnsureSceneExistence()
        {
            if ( SceneObject != null )
                return;

            SceneObject = new GameObject("Escape Handler (generated)");
            SceneObject.AddComponent< EscapeQueue >();
        }

        private static List< IEscapeQueueHandler > s_Handlers = new List< IEscapeQueueHandler >();
        private static List< IEscapeQueueHandler > Handlers
        {
            get { return s_Handlers; }
            set { s_Handlers = value; }
        }

        public static void AddHandler( IEscapeQueueHandler handler )
        {
            EnsureSceneExistence();

            Handlers.Add( handler );
        }

        public static void RemoveHandler( IEscapeQueueHandler handler )
        {
            if ( SceneObject == null )
                return;

            // Only remove the top-most reference to said handler...
            if ( !Handlers.Any() )
                return;

            var handlerIndex = Handlers.LastIndexOf( handler );

            if ( handlerIndex == -1 )
                return;
            
            //Debug.Log("Removing handler " + handlerIndex  );

            Handlers.RemoveAt( handlerIndex );
        }

        [UsedImplicitly]
        private void Start()
        {
            Handlers = new List< IEscapeQueueHandler >();
        }

        [UsedImplicitly]
        private void Update()
        {
            if ( Input.GetKeyDown( KeyCode.Escape ) )
            {
                var topCallback = Handlers.LastOrDefault();

                if ( topCallback != null )
                    topCallback.HandleEscape();

                RemoveHandler( topCallback );
            }
        }
    }
}
