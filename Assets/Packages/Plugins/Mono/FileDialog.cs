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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Chains;
using ChainViews;
using UnityEngine;
using Application = UnityEngine.Application;


#if UNITY_EDITOR

using UnityEditor;    

#endif

namespace Packages.Plugins.Mono
{
    public static class FileDialog
    {
        //[DllImport( "user32.dll" )]
        //private static extern void SaveFileDialog();

        //[DllImport( "user32.dll" )]
        //private static extern void OpenFileDialog();

        //[DllImport( "user32.dll", EntryPoint = "GetActiveWindow" )]
        //private static extern IntPtr GetActiveWindow();

        //[DllImport( "user32.dll" )]
        //static extern bool IsZoomed( IntPtr hWnd );

        //[DllImport( "user32.dll" )]
        //private static extern bool ShowWindowAsync( IntPtr hWnd, int nCmdShow );


        private static string InitialDirectory
        {
            get
            {
                var localRootPath = HaxxisPackage.RootPath;
                var remoteRootPath = Environment.GetEnvironmentVariable( @"CGC_REMOTE_HP_ROOT" );

                var finalRootPath = ChainView.Instance.UseLocalPackageStore ? localRootPath : ( remoteRootPath ?? localRootPath );

                return Path.GetFullPath( finalRootPath );
            }
        }


        public static FilePicker.FilePickResult ShowOpenDialog(string title, FilePicker.FileFilter[] filters)
        {
            //Debug.Log( "Initial directory is " + InitialDirectory );
//#if UNITY_EDITOR
//            return EditorUtility.OpenFilePanel( "Open File", InitialDirectory, "json" );
//#else
//            using ( var diag = new OpenFileDialog() )
//            {
//                diag.Multiselect = false;
//                diag.Filter = "Json Haxxis Packages (*.json)|*.json|All files (*.*)|*.*";
//                diag.InitialDirectory = InitialDirectory;
//                diag.AutoUpgradeEnabled = true;

//                // Get window's maximized state
//                var windowHandle = GetActiveWindow();
//                var maximized = IsZoomed( windowHandle );

//                var result = diag.ShowDialog( new WindowWrapper { Handle = windowHandle } );

//                // Restore window's maximized state
//                ShowWindowAsync( windowHandle, maximized ? 3 : 1 );

//                if ( result == DialogResult.OK )
//                {
//                    return diag.FileName;
//                }
//                return string.Empty;
//            }
//#endif
            return FilePicker.ShowOpenDialog(InitialDirectory, title, filters);
        }


        public static FilePicker.FilePickResult ShowSaveDialog(string title, FilePicker.FileFilter[] filters)
        {
            return ShowSaveDialog( string.Empty, title, filters );
        }

        private class WindowWrapper : IWin32Window
        {
            public IntPtr Handle { get; set; }
        }

        public static FilePicker.FilePickResult ShowSaveDialog( string filename, string title, FilePicker.FileFilter[] filters )
        {
            // TODO: This should respect the Local/Remote button, even if there is an open package.
            var pathSansFilename = string.IsNullOrEmpty( filename ) ? InitialDirectory : Path.GetFullPath( filename );

//#if UNITY_EDITOR
//            var filenameSansPath = Path.GetFileName( filename );
            
//            return EditorUtility.SaveFilePanel( "Save File", pathSansFilename, filenameSansPath, "json" );
//#else
//            using ( var diag = new SaveFileDialog() )
//            {
//                diag.Filter = "Json Haxxis Packages (*.json)|*.json|All files (*.*)|*.*";
//                diag.InitialDirectory = pathSansFilename;

//                if ( !string.IsNullOrEmpty( filename ) )
//                    diag.FileName = filename;
                
//                // Get window's maximized state
//                var windowHandle = GetActiveWindow();
//                var maximized = IsZoomed( windowHandle );
                
//                var result = diag.ShowDialog( new WindowWrapper { Handle = windowHandle } );

//                // Restore window's maximized state
//                ShowWindowAsync( windowHandle, maximized ? 3 : 1 );

//                if ( result == DialogResult.OK )
//                {
//                    return diag.FileName;
//                }

//                return string.Empty;
//            }
//#endif
            return FilePicker.ShowSaveDialog(pathSansFilename, title, filters);
        }
    }
}