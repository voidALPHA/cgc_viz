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

namespace Visualizers 
{
    public class ControllableCondition
    {
        public event Action<bool> Changed = delegate { };

        private bool m_ConditionValid;
        public bool ConditionValid
        {
            get { return m_ConditionValid; }
            set
            {
                if ( m_ConditionValid == value )
                    return;

                m_ConditionValid = value;

                Changed( value );
            }
        }

        public ControllableCondition( bool conditionValid )
        {
            ConditionValid = conditionValid;
        }

        // Implicit ControllableCondition to bool convertsion operator
        public static implicit operator bool( ControllableCondition condition )
        {
            return condition.ConditionValid;
        }


        // Can we do this without overwriting the instance?
        //// Implicit ControllableCondition to bool convertsion operator
        //public static implicit operator ControllableCondition ( bool value )
        //{
        //    return new ControllableCondition( value );
        //}


        public override string ToString()
        {
            return ConditionValid.ToString();
        }
    }
}
