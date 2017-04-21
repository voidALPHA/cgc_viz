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
using System.Linq;
using System.Reflection;
using Chains;
using JetBrains.Annotations;
using Mutation;
using PayloadSelection;
using UnityEngine;
using Visualizers;

public class FindInvalidControllablesBehaviour : MonoBehaviour
{

    [SerializeField]
    private bool m_LogStringControllables = false;
    private bool LogStringControllables { get { return m_LogStringControllables; } }



    [UsedImplicitly]
    private void Start()
    {
        //var assy = Assembly.GetExecutingAssembly();

        //var allChainNodeTypes = assy.GetTypes().Where( t => typeof( ChainNode ).IsAssignableFrom( t ) && !t.IsAbstract ).OrderBy( t => t.Name );

        //var allControllableMembers = allChainNodeTypes.SelectMany( chainNodeType =>
        //    chainNodeType
        //        .GetMembers( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
        //        .Where( e => Attribute.IsDefined( e, typeof( ControllableAttribute ) ) ) );

        //var invalidControllableMembers =
        //    allControllableMembers.Where( controllable => !CheckControllableValidity( controllable ));


        var invalidControllableMembers =
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where( t => typeof( ChainNode ).IsAssignableFrom( t ) && !t.IsAbstract )
                .OrderBy( t => t.Name )
                .SelectMany( chainNodeType =>
                    chainNodeType
                        .GetMembers( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
                        .Where( e => Attribute.IsDefined( e, typeof( ControllableAttribute ) ) ) )
                .Where( controllable => !CheckControllableValidity( controllable ) );


        foreach ( var invalidControllableMember in invalidControllableMembers )
        {
            Debug.LogErrorFormat( "Assembly contains invalid Controllable member: {0}.{1}", invalidControllableMember.DeclaringType, invalidControllableMember.Name );
        }
    }

    private bool CheckControllableValidity( MemberInfo member )
    {
        if ( member is MethodInfo )
            return true;

        if ( member is PropertyInfo )
        {
            var property = member as PropertyInfo;

            if ( LogStringControllables )
                if ( property.PropertyType == typeof( string ) )
                    Debug.LogWarningFormat( "Found string controllable {0}.{1}", member.DeclaringType, member.Name );

            return property.PropertyType == typeof( string ) ||
                   property.PropertyType == typeof( LabelSystem.LabelSystem ) ||
                   property.PropertyType == typeof( PayloadExpression ) ||
                   property.PropertyType == typeof( MutableTarget ) ||
                   property.PropertyType == typeof( MutableScope ) ||
                   typeof( IMutableField ).IsAssignableFrom( property.PropertyType );
        }

        return false;
    }
}
