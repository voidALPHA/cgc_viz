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

using UnityEngine;
using UnityEditor;

/// <summary>
/// This script exits play mode whenever script compilation is detected during an editor update.
/// </summary>
[InitializeOnLoad] // Make static initialiser be called as soon as the scripts are initialised in the editor (rather than just in play mode).
public class ExitPlayModeOnScriptCompile {
    
    // Static initialiser called by Unity Editor whenever scripts are loaded (editor or play mode)
    static ExitPlayModeOnScriptCompile () {
        Unused (_instance);
        _instance = new ExitPlayModeOnScriptCompile ();
    }

    private ExitPlayModeOnScriptCompile () {
        EditorApplication.update += OnEditorUpdate;
    }
    
    ~ExitPlayModeOnScriptCompile () {
        EditorApplication.update -= OnEditorUpdate;
        // Silence the unused variable warning with an if.
        _instance = null;
    }
    
    // Called each time the editor updates.
    private static void OnEditorUpdate () {
        if (EditorApplication.isPlaying && EditorApplication.isCompiling) {
            Debug.Log ("Exiting play mode due to script compilation.");
            EditorApplication.isPlaying = false;
        }
    }

    // Used to silence the 'is assigned by its value is never used' warning for _instance.
    private static void Unused<T> (T unusedVariable) {}

    private static ExitPlayModeOnScriptCompile _instance = null;
}
