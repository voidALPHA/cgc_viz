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
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

public class InsertLicenseReferenceWindow : EditorWindow
{
    private string m_licenseRefToInsert = "";

    private string LicenseRefToInsert
    {
        get
        {
            if(string.IsNullOrEmpty(m_licenseRefToInsert))
                EditorPrefs.GetString("VA.License", "");
            if(string.IsNullOrEmpty(m_licenseRefToInsert))
                LicenseRefToInsert = "© " + DateTime.Today.Year + " " + PlayerSettings.companyName;
            return m_licenseRefToInsert;
        }
        set
        {
            if(m_licenseRefToInsert == value) return;
            EditorPrefs.SetString("VA.License", value);
            m_licenseRefToInsert = value;
        }
    }

    private int CSFileCount
    {
        get
        {
            if(FileList.Count == 0)
            {
                FileList.AddRange(Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories));
            }
            return FileList.Count;
        }
    }

    private readonly List<string> FileList = new List<string>();

    private bool DoInsert { get; set; }
    private readonly Stack<string> FilesLeftToDo = new Stack<string>();

    [MenuItem("Window/Insert License References")]
    static void ShowWindow()
    {
        var window = GetWindow<InsertLicenseReferenceWindow>();

        window.titleContent = new GUIContent("InsertLicRef");

        window.Show();
    }

    private Stopwatch m_sw = null;

    private Stopwatch Stopwatch
    {
        get { return m_sw ?? (m_sw = new Stopwatch()); }
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("License Information to Insert:");
        LicenseRefToInsert = EditorGUILayout.TextArea(LicenseRefToInsert, GUILayout.MinHeight(48));

        EditorGUILayout.LabelField("Some probably helpful characters that can be copy-pasted:");
        EditorGUILayout.SelectableLabel("©®™");

        
        GUILayout.FlexibleSpace();
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("The text above will be added to " + CSFileCount + " .cs files.");
        if(GUILayout.Button("Refresh Count"))
        {
            FileList.Clear();
        }
        if(GUILayout.Button("Insert Text"))
        {
            FileList.ForEach(s => FilesLeftToDo.Push(s));
            DoInsert = true;
        }
    }

    private void Update()
    {
        if(DoInsert)
        {
            if(EditorUtility.DisplayCancelableProgressBar("Inserting text to all .cs files...", FilesLeftToDo.Peek(),
                1f - (FilesLeftToDo.Count * 1f) / CSFileCount))
            {
                Debug.Log("Insertion cancelled.");
                EditorUtility.ClearProgressBar();
                FilesLeftToDo.Clear();
                DoInsert = false;
                return;
            }
            
            Stopwatch.Reset();
            Stopwatch.Start();
            while(Stopwatch.ElapsedMilliseconds < 30)
            {
                var file = FilesLeftToDo.Pop();

                var text = LicenseRefToInsert + "\n\n" + File.ReadAllText(file);

                File.WriteAllText(file, text);

                if(FilesLeftToDo.Count == 0) break;
            }
            Stopwatch.Stop();

            if(FilesLeftToDo.Count == 0)
            {
                Debug.Log("Insertion complete.");
                EditorUtility.ClearProgressBar();
                DoInsert = false;
            }
        }
    }
}