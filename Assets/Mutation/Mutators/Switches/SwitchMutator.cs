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

using JetBrains.Annotations;
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.Switches
{
    public abstract class SwitchMutator : DataMutator { }
    public abstract class SwitchMutator< T > : SwitchMutator
    {

        private MutableScope m_Scope = new MutableScope();
        [Controllable( LabelText = "Scope" )]
        protected MutableScope Scope { get { return m_Scope; } }


        private MutableField<bool> m_ConditionField = new MutableField<bool>() { LiteralValue = true };
        [Controllable( LabelText = "Condition" )]
        protected MutableField<bool> ConditionField { get { return m_ConditionField; } }
        

        private MutableField<T> m_IfTrueField = new MutableField<T> { AbsoluteKey = "" };
        [Controllable( LabelText = "IfTrueField" )]
        protected MutableField<T> IfTrueField { get { return m_IfTrueField; } }

        private MutableField<T> m_IfFalseField = new MutableField<T> { AbsoluteKey = "" };
        [Controllable( LabelText = "IfFalseField" )]
        protected MutableField<T> IfFalseField { get { return m_IfFalseField; } }


        private MutableTarget m_OutputTarget = new MutableTarget() { AbsoluteKey = "Switch Output" };
        [Controllable( LabelText = "Output Target" )]
        protected MutableTarget OutputTarget { get { return m_OutputTarget; } }

        protected SwitchMutator()
        {
            ConditionField.SchemaParent = Scope;
            IfTrueField.SchemaParent = Scope;
            IfFalseField.SchemaParent = Scope;
            OutputTarget.SchemaParent = Scope;

            IfTrueField.LiteralValue = Default();
            IfFalseField.LiteralValue = Default();
        }

        protected virtual T Default()
        {
            return default ( T );
        }

        protected override MutableObject Mutate( MutableObject mutable )
        {
            foreach ( var entry in Scope.GetEntries( mutable ) )
            {
                if ( ConditionField.GetValue( entry ) )
                    OutputTarget.SetValue( IfTrueField.GetValue( entry ), entry );
                else
                    OutputTarget.SetValue( IfFalseField.GetValue( entry ), entry );
            }
            return mutable;
        }
    }

    [UsedImplicitly]
    public class BoolToIntSwitch : SwitchMutator<int>
    {
    }

    [UsedImplicitly]
    public class BoolToLongSwitch : SwitchMutator<long>
    {
    }

    [UsedImplicitly]
    public class BoolToFloatSwitch : SwitchMutator<float>
    {
    }

    [UsedImplicitly]
    public class BoolToColorSwitch : SwitchMutator< Color >
    {
        protected override Color Default()
        {
            return new Color(1,0,1,1);
        }
    }

    [UsedImplicitly]
    public class BoolToStringSwitch : SwitchMutator< string >
    {
        protected override string Default()
        {
            return "";
        }
    }

    [UsedImplicitly]
    public class BoolToVector3Switch : SwitchMutator< Vector3 >
    {
    }

    [UsedImplicitly]
    public class BoolToQuaternionSwitch : SwitchMutator< Quaternion >
    {
    }
}
