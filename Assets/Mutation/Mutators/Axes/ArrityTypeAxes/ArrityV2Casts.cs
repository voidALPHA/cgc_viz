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
using Adapters.GameEvents.Model;
using Adapters.TraceAdapters.Traces;
using Mutation.Mutators.ArithmeticOperators;
using UnityEngine;
using Visualizers;
using Visualizers.LabelController;

namespace Mutation.Mutators.Axes.ArrityTypeAxes
{
    public class CastIntToString : TypeConversionAxis< int, string >
    {
        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            TargetField.SetValue( "", newSchema );
            Router.TransmitAllSchema( newSchema );
        }
        
        protected override string ConversionFunc( int key, List<MutableObject> entry)
        {
            return key + "";
        }
    }

    public class CastFloatToString : TypeConversionAxis< float, string >
    {
        private string m_FormatString = "{0:0.##}";

        [Controllable( LabelText = "Format String" )]
        public string FormatString
        {
            get { return m_FormatString; }
            set { m_FormatString = value; }
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            TargetField.SetValue( "", newSchema );
            Router.TransmitAllSchema( newSchema );
        }

        protected override string ConversionFunc( float key, List<MutableObject> entry)
        {
            return string.Format( FormatString, key);
        }
    }

    public class CastIntToFloat : TypeConversionAxis< int, float >
    {
        protected override float ConversionFunc( int key, List<MutableObject> entry)
        {
            return key;
        }
    }

    public class TruncateLongToInt : TypeConversionAxis< long, int >
    {
        protected override int ConversionFunc( long key, List<MutableObject> entry)
        {
            return Convert.ToInt32( key % int.MaxValue );
        }
    }

    public class CastIntToLong : TypeConversionAxis< int, long >
    {
        protected override long ConversionFunc( int key, List<MutableObject> entry)
        {
            return Convert.ToInt64( key);
        }
    }

    public class ParseIntToHexString : TypeConversionAxis< int, string >
    {
        protected override string ConversionFunc( int key, List<MutableObject> entry)
        {
            return key.ToString( "X" );
        }

        protected override string DefaultValue()
        {
            return "00";
        }
    }

    public class CastUintToInt : TypeConversionAxis< uint, int >
    {
        protected override int ConversionFunc( uint key, List<MutableObject> entry)
        {
            return (int)key;
        }
    }

    public class CastIntToUint : TypeConversionAxis< int, uint >
    {
        protected override uint ConversionFunc( int key, List<MutableObject> entry)
        {
            return (uint)key;
        }
    }

    public class ParseUIntToHexString : TypeConversionAxis< uint, string >
    {
        protected override string ConversionFunc( uint key, List<MutableObject> entry)
        {
            return key.ToString( "X" );
        }

        protected override string DefaultValue()
        {
            return "00";
        }
    }

    public class CastFloatToInt : TypeConversionAxis< float, int >
    {
        protected override int ConversionFunc( float key, List<MutableObject> entry)
        {
            return Mathf.FloorToInt( key);
        }
    }

    public class CastDoubleToFloat : TypeConversionAxis< double, float >
    {
        protected override float ConversionFunc( double key, List<MutableObject> entry)
        {
            return Convert.ToSingle( key);
        }
    }

    public class CastFloatToDouble : TypeConversionAxis< float, double >
    {
        protected override double ConversionFunc( float key, List<MutableObject> entry )
        {
            return Convert.ToDouble( key);
        }
    }

    public class ParseStringToInt : TypeConversionAxis< string, int >
    {
        protected override int ConversionFunc( string key, List<MutableObject> entry )
        {
            int foundValue = -1;
            bool success = int.TryParse( key, out foundValue );

            if ( !success )
                throw new Exception( "Cannot parse string " + key + " into an int!" );

            return foundValue;
        }
    }

    public class CountCharactersInString : TypeConversionAxis< string, int >
    {
        protected override int ConversionFunc( string key, List<MutableObject> entry )
        {
            return key.Length;
        }
    }

    public class ParseStringToFloat : TypeConversionAxis< string, float >
    {
        protected override float ConversionFunc( string key, List<MutableObject> entry )
        {
            float foundValue = -1f;
            bool success = float.TryParse( key, out foundValue );

            if ( !success )
                throw new Exception( "Cannot parse string " + key + " into a float!" );

            return foundValue;
        }
    }

    public class ParseStringToBool : TypeConversionAxis< string, bool >
    {
        protected override bool ConversionFunc( string key, List<MutableObject> entry )
        {
            bool value;

            var foundBool = bool.TryParse( key, out value );

            if ( !foundBool )
                value = false;

            return value;
        }
    }

    public class ParseStringToRequestNature : TypeConversionAxis< string, RequestNature >
    {
        protected override RequestNature ConversionFunc( string key, List<MutableObject> entry )
        {
            var testString = key.ToLower();

            if ( testString.StartsWith( "poll" ) || testString.StartsWith( "servicepoll" ) )
                return RequestNature.ServicePoll;
            if ( testString.StartsWith( "pov" ) )
                return RequestNature.Pov;
            Debug.LogError( "Unknown request nature " + testString );
            return RequestNature.Pov;
        }
    }

    public class ParseStringToLateralJustificationOption : TypeConversionAxis< string, LateralJustificationOption >
    {
        protected override LateralJustificationOption ConversionFunc( string key, List< MutableObject > entry )
        {
            return (LateralJustificationOption)Enum.Parse( typeof(LateralJustificationOption), key );
        }
    }


    public class ParseSuccessToBoolSucceeded : TypeConversionAxis< EventResult, bool >
    {
        protected override bool ConversionFunc( EventResult key, List<MutableObject> entry )
        {
            if ( key == EventResult.Succeed )
                return true;
            return false;
        }
    }

    public class CastBoolToInt : TypeConversionAxis< bool, int >
    {
        protected override int ConversionFunc( bool key, List<MutableObject> entry )
        {
            return key?1:0;
        }
    }

    public class ParseIntToBool : TypeConversionAxis< int, bool >
    {
        protected override bool ConversionFunc( int key, List<MutableObject> entry )
        {
            return key != 0;
        }
    }

    public class ParseStringToUnaryOperator : TypeConversionAxis< string, UnaryOperators >
    {
        protected override UnaryOperators ConversionFunc( string key, List<MutableObject> entry )
        {
            try
            {
                return (UnaryOperators)Enum.Parse( typeof( UnaryOperators ), key, true );
            }
            catch ( ArgumentException )
            {
                return UnaryOperators.Value;
            }
        }
    }

    public class ParseStringToColorOperator : TypeConversionAxis< string, Color >
    {
        protected override Color ConversionFunc( string key, List<MutableObject> entry )
        {
            try
            {
                var tokens = key.Split( ',' );

                var floatForms = (from token in tokens
                    select
                        float.Parse( token.Trim() )).ToList();
                if (floatForms.Count>=3)
                    return new Color(floatForms[0], floatForms[1], floatForms[2]);
            }
            catch ( Exception )
            {
                Debug.Log( "Invalid color string " + key);
            }
            return Color.magenta;
        }
    }
}
