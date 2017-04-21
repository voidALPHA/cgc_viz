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
using System.Collections;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Utility.NetworkSystem;
using Visualizers;

namespace Mutation.Mutators
{
    [UsedImplicitly]
    public class NetworkRequestMutator : Mutator
    {
        private readonly MutableField<string> m_Url = new MutableField<string>() { LiteralValue = "localhost:8003" };
        [Controllable(LabelText = "URL")]
        public MutableField<string> Url { get { return m_Url; } }

        private readonly MutableField<string> m_Request = new MutableField<string>() { LiteralValue = "/Example/Foo/Bar" };
        [Controllable(LabelText = "Request")]
        public MutableField<string> Request { get { return m_Request; } }

        private readonly MutableField<bool> m_WaitForCompletion = new MutableField<bool>() { LiteralValue = false };
        [Controllable(LabelText = "Wait For Completion")]
        public MutableField<bool> WaitForCompletion { get { return m_WaitForCompletion; } }

        // TODO: timeout value?

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            var requestId = Network.Instance.SendRequest(Url.GetFirstValue(payload.Data) + Request.GetFirstValue(payload.Data));

            if(WaitForCompletion.GetFirstValue(payload.Data))
            {
                while(Network.Instance.PendingRequests.Any(x => x.Id == requestId))
                    yield return null;
            }

            var iterator = Router.TransmitAll(payload);
            while(iterator.MoveNext())
                yield return null;
        }
    }
}
