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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonobehaviorTester : MonoBehaviour
{

    private readonly HashSet<string> tested = new HashSet<string>();

    private void OnPostRender()
    {
        if(tested.Contains("OnPostRender")) return;
        tested.Add("OnPostRender");
        Debug.Log("OnPostRender");
    }

    private void OnPreCull()
    {
        if(tested.Contains("OnPreCull")) return;
        tested.Add("OnPreCull");
        Debug.Log("OnPreCull");
    }

    private void OnPreRender()
    {
        if(tested.Contains("OnPreRender")) return;
        tested.Add("OnPreRender");
        Debug.Log("OnPreRender");
    }

    private void OnRenderObject()
    {
        if(tested.Contains("OnRenderObject")) return;
        tested.Add("OnRenderObject");
        Debug.Log("OnRenderObject");
    }

    private void OnWillRenderObject()
    {
        if(tested.Contains("OnWillRenderObject")) return;
        tested.Add("OnWillRenderObject");
        Debug.Log("OnWillRenderObject");
    }

}
