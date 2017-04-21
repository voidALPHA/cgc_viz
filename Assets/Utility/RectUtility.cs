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

namespace Utility
{
    public static class RectUtility
    {
        public static Rect CreateExpandedRect( Rect rect, float eachSide )
        {
            return CreateExpandedRect( rect, eachSide, eachSide, eachSide, eachSide );
        }

        public static Rect CreateExpandedRect( Rect rect, float l, float t, float r, float b )
        {
            rect.Set( rect.x - l, rect.y - t, rect.width + l + r, rect.height + t + b );

            return rect;
        }


        public static Rect RectTransformToScreenSpace( RectTransform transform )
        {
            Vector2 size = Vector2.Scale( transform.rect.size, transform.lossyScale );
            return new Rect( transform.position.x, Screen.height - transform.position.y, size.x, size.y );
        }
    }
}
