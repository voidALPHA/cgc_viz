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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Application = UnityEngine.Application;

public class FilePicker
{
    private enum Platform
    {
        Win, Mac, Linux
    }

    private static readonly Platform currPlatform =
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        Platform.Win;
#elif UNITY_STANDALONE_OSX
        Platform.Mac;
#elif UNITY_STANDALONE_LINUX
        Platform.Linux;
#endif

    /// <summary>
    /// A struct containing information on the result of the picking.
    /// </summary>
    public struct FilePickResult
    {
        /// <summary>
        /// Did the user actually choose a location or file?
        /// </summary>
        public bool PickSuccess;
        /// <summary>
        /// Where did the user choose?
        /// </summary>
        public string FileLocation;
    }

    /// <summary>
    /// A struct for choosing filters for the file pickers.
    /// </summary>
    public struct FileFilter
    {
        /// <summary>
        /// The name of the filter (ie "music files" or "JSON files")
        /// </summary>
        public string FilterName;
        /// <summary>
        /// An array of supported extensions (ie wav,mp3,ogg or json); only include the part after the period
        /// </summary>
        public string[] FilterExtensions;
    }

#if UNITY_STANDALONE_OSX
    [DllImport("StandaloneFileBrowser")]
    private static extern IntPtr DialogOpenFilePanel(string title, string directory, string extension, bool multiselect);

    [DllImport("StandaloneFileBrowser")]
    private static extern IntPtr DialogSaveFilePanel(string title, string directory, string defaultName, string extension); 
#endif

    public static FilePickResult ShowOpenDialog(Environment.SpecialFolder specialFolder, string title = "Open a File", FileFilter[] filters = null)
    {
        return ShowOpenDialog(Environment.GetFolderPath(specialFolder), title, filters);
    }

    public static FilePickResult ShowOpenDialog(string startingDirectory = "", string title = "Open a File", FileFilter[] filters = null)
    {
        if(string.IsNullOrEmpty(startingDirectory))
        {
            startingDirectory = Environment.CurrentDirectory;
        }

        Process p;
        string res;
        string[] parts;
        FilePickResult rtn;

        switch(currPlatform)
        {
            case Platform.Win:
                p = new Process
                {
                    StartInfo =
                    {
                        Arguments = "open \"" + startingDirectory + "\" \"" + title + "\" \"" + GetFilterFromFilterList(filters) + "\"",
                        FileName = Application.dataPath + "/StreamingAssets/FilePicker/WinFileDialog.exe",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
                };

                p.Start();
                p.WaitForExit();
                res = p.StandardOutput.ReadLine();
                parts = res.Split(':');
                rtn = new FilePickResult
                {
                    PickSuccess = parts[0] == "OK"
                };
                if(rtn.PickSuccess)
                {
                    rtn.FileLocation = res.Substring(3);
                }
                return rtn;
            case Platform.Mac:
#if UNITY_STANDALONE_OSX
                UnityEngine.Debug.Log("StartingDirectory: " + startingDirectory);
                //var paths = Marshal.PtrToStringAnsi(DialogOpenFilePanel(title, startingDirectory, GetFilterFromFilterList(filters), false)); // A current issue in the Mac picker means that no filters are allowed on open.
                var paths = Marshal.PtrToStringAnsi(DialogOpenFilePanel(title, startingDirectory, "", false));
#else
                var paths = "";
#endif
                res = paths.Split((char)28)[0];
                if(res.StartsWith("file://"))
                {
                    res = res.Substring(7);
                }
                rtn = new FilePickResult
                {
                    PickSuccess = paths.Length > 0,
                    FileLocation = res
                };
                return rtn;
            case Platform.Linux:
                p = new Process
                {
                    StartInfo =
                    {
                        Arguments = "open \"" + startingDirectory + "\" \"" + title + "\" \"" + GetFilterFromFilterList(filters) + "\"",
                        FileName = Application.dataPath + "/StreamingAssets/FilePicker/LinuxFileDialog",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
                };

                p.Start();
                p.WaitForExit();
                res = p.StandardOutput.ReadLine();
                parts = res.Split(':');
                rtn = new FilePickResult
                {
                    PickSuccess = parts[0] == "OK"
                };
                if(rtn.PickSuccess)
                {
                    rtn.FileLocation = res.Substring(3);
                }

                return rtn;
            default:
                throw new NotImplementedException();
        }
    }

    public static FilePickResult ShowSaveDialog(Environment.SpecialFolder specialFolder, string title = "Save a File", FileFilter[] filters = null)
    {
        return ShowSaveDialog(Environment.GetFolderPath(specialFolder), title, filters);
    }

    public static FilePickResult ShowSaveDialog(string startingDirectory = "", string title = "Save a File", FileFilter[] filters = null)
    {
        if(string.IsNullOrEmpty(startingDirectory))
        {
            startingDirectory = Environment.CurrentDirectory;
        }

        Process p;
        string res;
        string[] parts;
        FilePickResult rtn;

        switch(currPlatform)
        {
            case Platform.Win:
                p = new Process
                {
                    StartInfo =
                    {
                        Arguments = "save \"" + startingDirectory + "\" \"" + title + "\" \"" + GetFilterFromFilterList(filters) + "\"",
                        FileName = Application.dataPath + "/StreamingAssets/FilePicker/WinFileDialog.exe",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
                };

                p.Start();
                p.WaitForExit();
                res = p.StandardOutput.ReadLine();
                parts = res.Split(':');
                rtn = new FilePickResult
                {
                    PickSuccess = parts[0] == "OK"
                };
                if(rtn.PickSuccess)
                {
                    rtn.FileLocation = res.Substring(3);
                }

                return rtn;
            case Platform.Mac:
#if UNITY_STANDALONE_OSX
                var paths = Marshal.PtrToStringAnsi(DialogSaveFilePanel(title, startingDirectory, "", GetFilterFromFilterList(filters)));
#else
                var paths = "";
#endif
                if(paths.StartsWith("file://"))
                {
                    paths = paths.Substring(7);
                }
                rtn = new FilePickResult
                {
                    PickSuccess = paths.Length > 0,
                    FileLocation = paths
                };
                return rtn;
            case Platform.Linux:
                p = new Process
                {
                    StartInfo =
                    {
                        Arguments = "save \"" + startingDirectory + "\" \"" + title + "\" \"" + GetFilterFromFilterList(filters) + "\"",
                        FileName = Application.dataPath + "/StreamingAssets/FilePicker/LinuxFileDialog",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
                };

                p.Start();
                p.WaitForExit();
                res = p.StandardOutput.ReadLine();
                parts = res.Split(':');
                rtn = new FilePickResult
                {
                    PickSuccess = parts[0] == "OK"
                };
                if(rtn.PickSuccess)
                {
                    rtn.FileLocation = res.Substring(3);
                }

                return rtn;
            default:
                throw new NotImplementedException();
        }
    }

    public static string GetFilterFromFilterList(FileFilter[] filters)
    {
        if(filters == null || filters.Length == 0)
        {
            //UnityEngine.Debug.Log("Sending filter \"\" to the picker.");
            return "";
        }
        string rtn = "";
        var filterStrings = new string[filters.Length];
        switch(currPlatform)
        {
            case Platform.Win:
            case Platform.Linux:
                for(int i = 0; i < filters.Length; i++)
                {
                    filterStrings[i] = filters[i].FilterName + "|" + string.Join(";", filters[i].FilterExtensions.Select(s => "*." + s).ToArray());
                }
                break;
            case Platform.Mac:
                for(int i = 0; i < filters.Length; i++)
                {
                    filterStrings[i] = filters[i].FilterName + ";" + string.Join(",", filters[i].FilterExtensions);
                }
                break;
        }
        rtn = string.Join("|", filterStrings);

        //UnityEngine.Debug.Log("Sending filter \"" + rtn + "\" to the picker.");

        return rtn;
    }
}
