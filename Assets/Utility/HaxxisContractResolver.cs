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
using System;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Utility.JsonConverters;
using JsonDotNet.Extras.CustomConverters;

public class HaxxisContractResolver : DefaultContractResolver
{
    public new static readonly HaxxisContractResolver Instance = new HaxxisContractResolver();

    protected override JsonContract CreateContract( Type objectType )
    {
        JsonContract contract = base.CreateContract( objectType );

        if ( objectType == typeof(Color) )
        {
            contract.Converter = new ColorConverter();
        }
        else if ( objectType == typeof(Vector2) )
        {
            contract.Converter = new Vector2Converter();
        }
        else if ( objectType == typeof(Vector3) )
        {
            contract.Converter = new Vector3Converter();
        }
        else if ( objectType == typeof(Matrix4x4) )
        {
            contract.Converter = new Matrix4x4Converter();
        }
        else if ( objectType == typeof(Quaternion) )
        {
            contract.Converter = new QuaternionConverter();
        }

        return contract;
    }
}
