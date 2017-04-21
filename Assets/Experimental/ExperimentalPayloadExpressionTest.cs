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
using System.Windows.Forms;
using Choreography.Steps;
using Mutation;
using PayloadSelection;
using PayloadSelection.CriterionStatements;
using UnityEngine;
using Visualizers;

namespace Experimental
{
    [ExecuteInEditMode]
    public class ExperimentalPayloadExpressionTest : MonoBehaviour
    {
        public bool Exec = false;

        private List< BoundingBox > SelectedList = new List< BoundingBox >();
        private List<BoundingBox> AllList = new List< BoundingBox >();

        public void Update()
        {
            if ( !Exec )
                return;
            Exec = false;

            // declare the bounds we'll test with.  Boring.
            var topBound = BoundingBox.ConstructBoundingBox( "Top Bound" );
            topBound.transform.parent = transform;

            AllList = new List< BoundingBox >();

            for ( int i = 0; i < 50; i++ )
            {
                for ( int k = 0; k < 50; k++ )
                {
                    var newBound = topBound.CreateDependingBound( "Second level " + i + ": Group" + i );
                    //newBound.DiscriminatingValues.Add( new KeyValuePair< string, object >( "Name", "Group" + i ) );
                    newBound.Data.Add( "Name", i );
                    newBound.Data.Add( "OtherName", k );
                    newBound.transform.position += transform.right * i;
                    newBound.transform.position += transform.forward * k;

                    var thirdBound = newBound.CreateDependingBound( "Third level" );
                    for ( var j = 0; j < 50; j++ )
                    {
                        var fourthBound = thirdBound.CreateDependingBound( "Fourth level " + j + ": Team " + j );
                        fourthBound.Data.Add( "Team", j );
                        fourthBound.transform.position += transform.up * j;
                        AllList.Add( fourthBound );
                    }
                }
            }


            //PayloadExpression newExpression = new PayloadExpression();

            //var step1 = new CriteriaGroup();
            //step1.Conjunction = CriteriaConjunction.Or;
            //step1.AddCriterion( new PredicateCriterionStatement(
            //    mut => CheckValueEquals(mut, "OtherName", 1)));
            //step1.AddCriterion(new PredicateCriterionStatement(
            //    mut => CheckValueEquals(mut, "Name", 2)));
            //newExpression.AddCriteriaGroup( step1 );

            //var step2 = new CriteriaGroup();
            //step2.Conjunction = CriteriaConjunction.Or;
            //step2.AddCriterion(new PredicateCriterionStatement(
            //    mut => CheckValueEquals(mut, "Team", 4)));
            //step2.AddCriterion(new PredicateCriterionStatement(
            //    mut => CheckValueEquals(mut, "Team", 12)));
            //newExpression.AddCriteriaGroup( step2 );

            //SelectedList = newExpression.ResolveExpression(new List<BoundingBox>() { topBound });
            //Debug.Log("Found " + SelectedList.Count + " valid objects.");
            
        }

        public void OnDrawGizmos()
        {
            if ( AllList == null )
                return;

          //  Gizmos.color = Color.red;
          //  foreach ( var obj in AllList )
          //  {
          //      Gizmos.DrawWireSphere(obj.transform.position, .1f);
          //  }

            Gizmos.color = Color.green;
            foreach (var obj in SelectedList)
                Gizmos.DrawWireSphere( obj.transform.position, .3f );
        }

        public bool CheckValueEquals(MutableObject mutable, string keyName, object comparisonValue)
        {
            object foundValue;
            if ( !mutable.TryGetValue( keyName, out foundValue ) )
                return false;

            return foundValue.Equals( comparisonValue );
        }

        public bool CheckLastCharacter(MutableObject mutable, string keyName, string sequence)
        {
            object foundValue;
            if (!mutable.TryGetValue(keyName, out foundValue))
                return false;
            var foundString = foundValue as string;
            if ( string.IsNullOrEmpty( foundString ) )
                return false;

            return foundString.EndsWith( sequence );
        }
        public bool CheckFirstCharacter(MutableObject mutable, string keyName, string sequence)
        {
            object foundValue;
            if (!mutable.TryGetValue(keyName, out foundValue))
                return false;
            var foundString = foundValue as string;
            if (string.IsNullOrEmpty(foundString))
                return false;

            return foundString.StartsWith(sequence);
        }

    }
}
