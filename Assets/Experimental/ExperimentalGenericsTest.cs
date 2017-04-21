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
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

namespace Experimental
{
    public class ExperimentalGenericsTest : MonoBehaviour
    {
        private class Holder<T>
        {
            public List<T> HeldData { get; set; }

            public bool IsHolder(Type givenType)
            {
                return givenType.GetGenericTypeDefinition() == typeof (Holder<int>).GetGenericTypeDefinition();
            }
        }

        public void Start()
        {
            Type stringType = typeof(string);
            Type floatType = typeof (float);

            Debug.Log("String is " +
                (stringType.IsAssignableFrom(floatType)
                ?"assignable" : "not assignable" ) + " from float");


            return;
            //Holder<string> stringHolder = new Holder<string>();
            //stringHolder.HeldData = new List<string>();
            //stringHolder.HeldData.Add("Test!");
            //Holder<float> floatHolder = new Holder<float>();
            //floatHolder.HeldData = new List<float>();
            //floatHolder.HeldData.Add(.2f);
            //floatHolder.HeldData.Add(.4f);

            //if (stringHolder.IsHolder(stringHolder.GetType()))
            //{
            //    Debug.Log("stringHolder is a holder!");
            //    Debug.Log("Its internal generic argument is " + stringHolder.GetType().GetGenericArguments().First());
            //}
            //else
            //{
            //    Debug.Log("stringHolder is _not_ a holder...");
            //}
        }

    }

}
