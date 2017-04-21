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
using UnityEngine.UI;

namespace Ui
{
    [ExecuteInEditMode]
    [RequireComponent( typeof( Image ) )]
    public class ImageSourceFlasher : MonoBehaviour
    {

        [SerializeField]
        private float m_HoldTime = 0.5f;
        private float HoldTime { get { return m_HoldTime; } }


        private Image m_ImageComponent = null;
        private Image ImageComponent { get { return m_ImageComponent ?? (m_ImageComponent = GetComponent< Image >()); } }


        [SerializeField]
        private Sprite m_Sprite1 = null;
        private Sprite Sprite1 { get { return m_Sprite1; } }

        [SerializeField]
        private Color m_Color1 = Color.magenta;
        private Color Color1 { get { return m_Color1; } }


        [SerializeField]
        private Sprite m_Sprite2 = null;
        private Sprite Sprite2 { get { return m_Sprite2; } }

        [SerializeField]
        private Color m_Color2 = Color.magenta;
        private Color Color2 { get { return m_Color2; } }


        bool UseFirstImageAndColor { get; set; }
        

        private void Start()
        {
            InvokeRepeating( "Flash", HoldTime, HoldTime );
        }

        private void Flash()
        {
            var newImage = UseFirstImageAndColor ? Sprite1 : Sprite2;
            var newColor = UseFirstImageAndColor ? Color1 : Color2;

            ImageComponent.sprite = newImage;
            ImageComponent.color = newColor;

            UseFirstImageAndColor = !UseFirstImageAndColor;
        }
    }
}
