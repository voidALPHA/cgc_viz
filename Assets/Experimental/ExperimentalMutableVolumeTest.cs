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
using Mutation;
using UnityEngine;
using Utility.JobManagerSystem;
using Visualizers;
using Visualizers.RectangularVolume;

namespace Experimental
{
    public class ExperimentalMutableVolumeTest : MonoBehaviour
    {
        public void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 80, 20), "Volume Test"))
            {
                JobManager.Instance.StartJob(RunVolumeTest(), jobName: "RunVolumeTest", startImmediately: true);
            }
        }
        

        public IEnumerator RunVolumeTest()
        {
            MutableObject testObj = new MutableObject();

            testObj.Add("height", 3f);
            testObj.Add("heightMax", 5f);

            VisualPayload payload = new VisualPayload(
                testObj,
                new VisualDescription(BoundingBox.ConstructBoundingBox(GetType().Name)));


            // create a volume controller
            RectangularVolumeController volumeController = new RectangularVolumeController();
            volumeController.YAxis.AbsoluteKey = "height";
            volumeController.YMax.AbsoluteKey = "heightMax";

            // send the test mutable
            var iterator = volumeController.StartReceivePayload(payload);

            while (iterator.MoveNext( ))
                yield return null;
        }

    }
}
