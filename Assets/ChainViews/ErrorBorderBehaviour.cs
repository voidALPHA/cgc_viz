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

public class ErrorBorderBehaviour : MonoBehaviour
{
    private Image m_Image;
    private Image Image { get { return m_Image ?? (m_Image = GetComponent<Image>()); }}


    [SerializeField]
    private float m_FadeTimeSeconds = 1.0f;
    private float FadeTimeSeconds { get { return m_FadeTimeSeconds; } }
    
    private void Update()
    {
        var timeInMs = Mathf.RoundToInt( Time.time * 1000 );

        var cycleIndex = timeInMs / Mathf.RoundToInt( FadeTimeSeconds * 1000 );

        if ( cycleIndex % 2 == 0 )
            Image.color = Color.red;
        else
            Image.color = new Color(0,0,0,0);
    }
}
