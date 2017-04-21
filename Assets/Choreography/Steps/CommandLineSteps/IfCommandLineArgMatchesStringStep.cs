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

using System.Linq;
using Newtonsoft.Json;
using Utility;
using Visualizers;

namespace Choreography.Steps.CommandLineSteps
{
    public class IfCommandLineArgMatchesStringStep : IfCommandLineStep
    {
        private string m_StringsToMatch = "value1, value2";
        [Controllable (LabelText = "Comma-separated values to accept")]
        public string StringsToMatch { get { return m_StringsToMatch; } set { m_StringsToMatch = value; } }

        protected override bool CheckArgument()
        {
            var foundValue = CommandLineArgs.GetArgumentValue( ArgumentName );
            return StringsToMatch.ToLowerInvariant().Split( ',' ).Contains( foundValue );
        }
    }
}
