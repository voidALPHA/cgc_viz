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
using System.Linq;
using Mutation;
using Utility;
using Visualizers;

namespace Adapters.GlobalParameters
{
    public class CommandLineArgumentAdapter : Adapter
    {
        private MutableField<string> m_ParameterName = new MutableField<string>() 
        { LiteralValue = "Param" };
        [Controllable(LabelText = "Parameter Name")]
        public MutableField<string> ParameterName { get { return m_ParameterName; } }

        private MutableField<string> m_DefaultValue = new MutableField<string>() 
        { LiteralValue = "Default" };
        [Controllable(LabelText = "Default Param Value")]
        public MutableField<string> DefaultValue { get { return m_DefaultValue; } }

        private MutableTarget m_ParameterTarget = new MutableTarget() 
        { AbsoluteKey = "New Param" };
        [Controllable(LabelText = "Parameter Target")]
        public MutableTarget ParameterTarget { get { return m_ParameterTarget; } }

        public CommandLineArgumentAdapter() : base()
        {
            Router.AddSelectionState( "Default" );
        }

        public override IEnumerator ReceivePayload( VisualPayload payload )
        {
            string foundValue = CommandLineArgs.GetArgumentValue( ParameterName.GetFirstValue( payload.Data ) );
            bool found = !string.IsNullOrEmpty(foundValue);

            ParameterTarget.SetValue( !found
                ? DefaultValue.GetFirstValue( payload.Data )
                : foundValue,
                payload.Data);

            var iterator = Router.TransmitAll(payload);
            while ( iterator.MoveNext() )
                yield return null;
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            ParameterTarget.SetValue( DefaultValue.GetFirstValue( newSchema ), newSchema );

            Router.TransmitAllSchema(newSchema);
        }
    }
}
