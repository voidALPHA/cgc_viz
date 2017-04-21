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
using Mutation;
using Assets.Utility;
using Chains;
using Visualizers;
using System.Collections;
using Utility.JobManagerSystem;
using UnityEngine;

namespace Adapters.InputAdapters
{
    public class KeyPressAdapter : Adapter
    {

        private MutableField<String> m_KeyCode = new MutableField<String>() { LiteralValue = "[" };
        [Controllable(LabelText = "Key Code")]
        public MutableField<String> KeyCode
        {
            get { return m_KeyCode; }
        }

        public SelectionState OnKeyDown { get { return Router["On Key Down"]; } }
        public SelectionState OnKeyHeld { get { return Router["On Key Held"]; } }
        public SelectionState OnKeyUp { get { return Router["On Key Up"]; } }


        public KeyPressAdapter()
        {
            Router.AddSelectionState("On Key Down");
            Router.AddSelectionState("On Key Held");
            Router.AddSelectionState("On Key Up");
        }

        protected override void OnProcessOutputSchema(MutableObject newSchema)
        {
            Router.TransmitAllSchema(newSchema);
        }

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var keyString = KeyCode.GetFirstValue(payload.Data);

            var localCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyString);

            SetUpChildSatellites(payload.VisualData.Bound, localCode, payload);

            yield return null;
        }



            
        private void SetUpChildSatellites(BoundingBox bound, KeyCode code, VisualPayload payload)
        {
            var newSatellite = bound.gameObject.AddComponent<KeyPressSatellite>();
            newSatellite.KeyCode = code;
            newSatellite.OnKeyDown += () => JobManager.Instance.StartJob(
                OnKeyDown.Transmit(payload), jobName: "Key Down", startImmediately: true,
                maxExecutionsPerFrame: 1);
            newSatellite.OnKeyHeld += () => JobManager.Instance.StartJob(
                OnKeyHeld.Transmit(payload), jobName: "Key Held", startImmediately: true,
                maxExecutionsPerFrame: 1);
            newSatellite.OnKeyUp += () => JobManager.Instance.StartJob(
                OnKeyUp.Transmit(payload), jobName: "Key Up", startImmediately: true,
                maxExecutionsPerFrame: 1);
        }
    }

}