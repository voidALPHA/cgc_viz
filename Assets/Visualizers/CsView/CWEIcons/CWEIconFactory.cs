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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Visualizers.LabelController;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Visualizers.CsView.CWEIcons
{
    public class CWEIconFactory : MonoBehaviour
    {
        public static CWEIconFactory Instance { get; set; }

        [SerializeField]
        private List<CWEIcon> m_Icons = new List<CWEIcon>();
        private List<CWEIcon> Icons
        {
            get { return m_Icons; }
            set { m_Icons = value; } 
        }

        [SerializeField]
        private CWEIcon m_DefaultIcon = null;
        private CWEIcon DefaultIcon { get { return m_DefaultIcon; } }

        [SerializeField]
        private Material m_IconMaterialTemplate = null;

        public Material IconMaterialTemplate { get { return m_IconMaterialTemplate; } }


        public Dictionary<string, CWEIcon> AvailableIcons { get; set; }


        private static int MaxDescriptionSegmentLength { get { return 25; } }


        [UsedImplicitly]
        public void Awake()
        {
            Instance = this;

            AvailableIcons = new Dictionary<string, CWEIcon>();
            foreach (var icon in Icons)
            {
                AvailableIcons.Add(icon.Name, icon);
            }
        }

        public static CWEIcon GetIcon(string iconName)
        {
            if ( Instance.AvailableIcons.ContainsKey( iconName ) )
            {
                return Instance.AvailableIcons[iconName];
            }

            var newIcon = new CWEIcon( GetDefaultIcon() );
            newIcon.Name = iconName;
            newIcon.Description = "CWE-" + iconName;

            return newIcon;
        }

        public static CWEIcon GetDefaultIcon()
        {
            return Instance.DefaultIcon;
        }

        public void OrderByCweNumber()
        {
            try
            {
                Icons = Icons.OrderBy( i => Int32.Parse( i.Name ) ).ToList();
            }
            catch( FormatException )
            {
                Debug.LogError( "CWE name not in number form." );

                throw;
            }
        }

        public void Validate()
        {
            ValidateLengths();

            ValidateLineSegments();
        }

        public void ValidateLengths()
        {
            foreach ( var icon in Icons )
                if ( icon.Description.Length > 40 )
                    Debug.LogError("Icon " + icon.Name + "'s length is " + icon.Description.Length + ", but the max is 40."  );
        }

        public void ValidateLineSegments()
        {
            foreach ( var icon in Icons )
            {
                var wrappedLines = LabelVisualizer.WrapText( icon.Description, MaxDescriptionSegmentLength );

                if ( wrappedLines.Count > 2 )
                {
                    Debug.LogError( "Icon " + icon.Name + " has too many lines: (" + wrappedLines.Count + ", with max of 2)." );
                    continue;
                }

                var lineIndex = 0;
                foreach ( var line in wrappedLines )
                {
                    if ( line.Length > MaxDescriptionSegmentLength )
                        Debug.LogError( "Icon " + icon.Name + " has too many characters in line index " + lineIndex + ": (" + wrappedLines.Count + ", with max of 2)." );

                    lineIndex++;
                }
            }
        }

        public void Dump()
        {
            var stringBuilder = new StringBuilder();

            foreach ( var icon in Icons )
            {
                stringBuilder.AppendLine( String.Format( "{0,4} |{1,-40}{2}", icon.Name, icon.Description, icon.Description.Length > 40 ? "" : "|" ) );
            }

            Debug.Log( stringBuilder.ToString() );
        }

        public void DumpWrapped()
        {
            

            var stringBuilder = new StringBuilder();


            foreach ( var icon in Icons )
            {
                var wrappedLines = LabelVisualizer.WrapText( icon.Description, MaxDescriptionSegmentLength );
                
                var lineIndex = 0;
                
                foreach ( var line in wrappedLines )
                {
                    stringBuilder.AppendLine( String.Format( "{0,4} |{1,-" + MaxDescriptionSegmentLength + "}|", lineIndex == 0 ? icon.Name : "", line ) );

                    lineIndex++;
                }

                if ( lineIndex > 2 )
                    stringBuilder.AppendLine( "^^^^^^^^^^^^^^^^^^^^^^^^^^^" );

                stringBuilder.AppendLine();
            }


            Debug.Log( stringBuilder.ToString() );
        }

        public void Retrieve( string filename )
        {
            var weaknesses = GetWeaknesses( filename );

            var stringBuilder = new StringBuilder();

            foreach ( var icon in Icons )
            {
                var foundWeakness = weaknesses.FirstOrDefault( w => w.Attribute( "ID" ).Value == icon.Name );
                var foundDescription = "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!";

                if ( foundWeakness != null )
                    foundDescription = foundWeakness.Attribute( "Name" ).Value;
                
                stringBuilder.AppendLine( String.Format( "{0,4} |{1,-40}{2}", icon.Name, foundDescription, icon.Description.Length > 40 ? "" : "|" ) );
            }

            Debug.Log( stringBuilder.ToString() );
        }

        public void Overwrite( string filename )
        {
            var shiftDown = Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift );
            var controlDown = Input.GetKey( KeyCode.LeftControl ) || Input.GetKey( KeyCode.RightControl );
            if ( !shiftDown && controlDown )
                return;

            var weaknesses = GetWeaknesses( filename );

            foreach ( var icon in Icons )
                icon.Description = weaknesses.First( w => w.Attribute( "ID" ).Value == icon.Name ).Attribute( "Name" ).Value;
        }

        private static IEnumerable< XElement > GetWeaknesses( string filename )
        {
            var xdoc = XDocument.Load( filename );
            var weaknesses =
                xdoc.Descendants( "Weakness" )
                    .Union( xdoc.Descendants( "Compound_Element" ) )
                    .Union( xdoc.Descendants( "Category" ) );
            return weaknesses;
        }

        public void AssignTextures()
        {
#if UNITY_EDITOR
            //var imageRoot = Path.Combine( Application.dataPath, "Visualizers/CsView/CWEIcons/Textures" );
            var imageRoot = "Assets/Visualizers/CsView/CWEIcons/Textures";
            var defaultImagePath = Path.Combine( imageRoot, "CWE-Other.png" ).Replace( "\\", "/" );

            foreach ( var icon in Icons )
            {
                var imageName = "CWE-" + icon.Name + ".png";

                var imagePath = Path.Combine( imageRoot, imageName ).Replace( "\\", "/" );

                Debug.Log( "Image path is " + imagePath );

                if ( !File.Exists( imagePath ) )
                {
                    icon.Texture = AssetDatabase.LoadAssetAtPath< Texture2D >( defaultImagePath );
                    continue;
                }

                icon.Texture = AssetDatabase.LoadAssetAtPath< Texture2D >( imagePath );
                //icon.Texture = Resources.Load( imagePath ) as Texture2D;
                //icon.Texture = new Texture2D( 128,128 );
                //icon.Texture.LoadImage( File.ReadAllBytes( imagePath ) );
                //icon.Texture.Apply();
            }
#else
            Debug.LogError("This can only be used in Editor mode.");
#endif
        }
    }
}
