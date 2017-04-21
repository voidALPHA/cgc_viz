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
using System.Reflection;
using Mutation;
using Newtonsoft.Json;
using UnityEngine;
using Visualizers;

namespace PayloadSelection.CriterionStatements
{
    [JsonObject( MemberSerialization.OptIn )]
    public abstract class CriterionStatement
    {
        public event Action< CriterionStatement > RemovalRequested = delegate {};

        public abstract string Name { get; }

        public event Action<CriterionStatement> CriterionChanged = delegate { }; 
        
        public abstract List< BoundingBox > GetMatchingBounds( List< BoundingBox > bounds );

        protected CriterionStatement()
        {
            BindToMutableFields();
        }

        private List<IMutableField> MutableFields { get { return GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => typeof(IMutableField).IsAssignableFrom(p.PropertyType))
                    .Select(p => p.GetValue(this, null)).Cast<IMutableField>().ToList();
            } }

        private void BindToMutableFields()
        {
            foreach (var mutableField in MutableFields)
                mutableField.ValueChanged += ()=> { CriterionChanged(this); };
        }

        public void UpdateSchema( MutableObject newSchema )
        {
            foreach ( var mutableField in MutableFields )
                mutableField.ValidateKey( newSchema );
        }

        public void RequestRemoval()
        {
            RemovalRequested( this );
        }
    }

    public abstract class PredicateCriterionStatement : CriterionStatement
    {
        protected abstract Func< MutableObject, bool > Predicate { get; }

        public override List< BoundingBox > GetMatchingBounds( List< BoundingBox > bounds )
        {
            return bounds.Where( bound => Predicate( bound.Data ) ).ToList();
        }
    }
}
