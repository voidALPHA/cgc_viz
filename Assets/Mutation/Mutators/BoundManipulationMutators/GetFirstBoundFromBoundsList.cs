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
using UnityEngine;
using Visualizers;

namespace Mutation.Mutators.BoundManipulationMutators
{
    public class GetFirstBoundFromBoundsList : DataMutator
    {
        private MutableField<IEnumerable<BoundingBox>> m_BoundsList = new MutableField<IEnumerable<BoundingBox>>() 
        { AbsoluteKey = "BoundsList" };
        [Controllable(LabelText = "Bounds List")]
        public MutableField<IEnumerable<BoundingBox>> BoundsList { get { return m_BoundsList; } }

        private MutableTarget m_FirstBoundTarget = new MutableTarget() 
        { AbsoluteKey = "First Bound" };
        [Controllable(LabelText = "First Bound Target")]
        public MutableTarget FirstBoundTarget { get { return m_FirstBoundTarget; } }

        private static BoundingBox SchemaBound { get; set; }

        protected override MutableObject Mutate(MutableObject mutable)
        {
            var boundsList = BoundsList.GetFirstValue(mutable).ToList();

            BoundingBox outgoingBound;

            if ( boundsList.Any() )
                outgoingBound = boundsList.First();
            else outgoingBound = SchemaBound ?? ( SchemaBound = BoundingBox.ConstructBoundingBox( "Schema Box" ) );

            FirstBoundTarget.SetValue(outgoingBound, mutable);

            return mutable;
        }

        public override void Unload()
        {
            if (SchemaBound!=null)
                GameObject.Destroy(SchemaBound.gameObject);

            base.Unload();
        }
    }
}
