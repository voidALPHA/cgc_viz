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
using Newtonsoft.Json;
using Utility;

namespace Mutation
{
    [JsonObject( MemberSerialization = MemberSerialization.OptIn )]
    public class MutableTarget : IAbsoluteKeyAssignable
    {
        #region Absolute Key

        public event Action AbsoluteKeyChanged = delegate { };

        private string m_AbsoluteKey = "";
        [JsonProperty]
        public string AbsoluteKey { get { return m_AbsoluteKey; }
            set
            {
                if ( m_AbsoluteKey == value )
                    return;

                m_AbsoluteKey = value;
                
                InvalidateKeyDependencies();

                AbsoluteKeyChanged();
            }
        }

        public string LastKey
        {
            get { return AbsoluteKey.Split('.').LastOrDefault(); }
            set
            {
                AbsoluteKey = AbsoluteKey.TrimEnd(LastKey.ToCharArray()) + value;
            }
        }

        public string IntermediateKeyString
        {
            get { return string.Join(".", AbsoluteKey.Split('.').WithoutLast().ToArray()); }
            set { AbsoluteKey = value + (value!=""?".":"")+LastKey; }
        }

        public List<string> IntermediateKeys
        {
            get { return AbsoluteKey.Split('.').WithoutLast().ToList(); }
            set
            {
                string newKey = value.Aggregate("", (current, str) => current + (str + "."));
                newKey += LastKey;
                AbsoluteKey = newKey;
            }
        }

        public int NumberOfIntermediates { get { return IntermediateKeys.Count; } }

#endregion

        public SchemaSource SchemaSource { get { return SchemaSource.Mutable;} }

        public bool ValidateKey(MutableObject mutable)
        {
            KeyValid = ValidateParent() && AbsoluteResolvable(mutable);
            return KeyValid;
        }

        protected bool AbsoluteResolvable(MutableObject mutable)
        {
            try
            {
                return GetLeafEntriesBelow(mutable, 0).Any();
            }
            catch (Exception)
            {
                return false;
            }
        }

        private IAbsoluteKeyAssignable m_SchemaParent = null;

        public IAbsoluteKeyAssignable SchemaParent
        {
            get { return m_SchemaParent; }
            set
            {
                if (m_SchemaParent != null)
                {
                    m_SchemaParent.AbsoluteKeyChanged -= InvalidateKeyDependencies;
                }
                m_SchemaParent = value;
                m_SchemaParent.AbsoluteKeyChanged += InvalidateKeyDependencies;
                InvalidateKeyDependencies();
            }
        }

        private void InvalidateKeyDependencies()
        {
            KeyValid = ValidateParent();
        }

        private bool ValidateParent()
        {
            if (SchemaParent == null)
            {
                return true; // AbsoluteKey.Split('.').Count() > 1;
            }

            if ((SchemaParent.SchemaSource != SchemaSource)
                &&
                (SchemaParent.SchemaSource!=SchemaSource.Global 
                && SchemaParent.SchemaSource!=SchemaSource.Literal))
                return false;

            var parentParts = SchemaParent.AbsoluteKey.Split('.').WithoutLast().ToList();
            if (IntermediateKeys.Count < parentParts.Count)
                return false;

            for (int i = 0; i < parentParts.Count(); i++)
            {
                if (parentParts[i] != IntermediateKeys[i])
                {
                    return false;
                }
            }
            return true;
        }

        private bool m_KeyValid;

        public event Action<bool> KeyValidChanged = delegate { };

        public bool KeyValid
        {
            get
            {
                return m_KeyValid;
            }
            private set
            {
                if (m_KeyValid == value)
                    return;
                m_KeyValid = value;
                KeyValidChanged(m_KeyValid);
            }
        }

        public IEnumerable<List<MutableObject>> GetEntries(MutableObject mutable)
        {
            return GetEntriesInternal(mutable, new List<MutableObject>());
        }


        public IEnumerable<List<MutableObject>> GetEntries(IEnumerable<MutableObject> mutables)
        {
            if (AbsoluteKey=="")
                return new List<List<MutableObject>> { mutables.ToList() };
            var mutablesList = mutables.ToList();
            return GetEntriesInternal(mutablesList.Last(), mutablesList.WithoutLast().ToList());
        }

        private IEnumerable<List<MutableObject>> GetEntriesInternal(MutableObject mutable, List<MutableObject> arrityList)
        {
            arrityList.Add(mutable);

            if (arrityList.Count == IntermediateKeys.Count + 1)
            {
                yield return arrityList;
                arrityList.RemoveAt(arrityList.Count - 1);
                yield break;
            }

            var nextLevel = mutable[IntermediateKeys[arrityList.Count - 1]];

            if (nextLevel is MutableObject)
            {
                foreach (var entry in GetEntriesInternal(nextLevel as MutableObject, arrityList))
                    yield return entry;
            }
            else
            {
                var resolutionList = nextLevel as IEnumerable<MutableObject>;
                foreach (var resolution in resolutionList)
                    foreach (var entry in GetEntriesInternal(resolution, arrityList))
                        yield return entry;
            }

            arrityList.RemoveAt(arrityList.Count - 1);
        }

        public IEnumerable<MutableObject> GetLeafEntriesBelow(MutableObject mut, int arrityDepth)
        {
            arrityDepth++;

            if (arrityDepth == IntermediateKeys.Count + 1)
            {
                yield return mut;
                    //arrityDepth--;
                yield break;
            }

            var nextLevel = mut[IntermediateKeys[arrityDepth - 1]];

            if (nextLevel is MutableObject)
            {
                foreach (var entry in GetLeafEntriesBelow(nextLevel as MutableObject, arrityDepth))
                    yield return entry;
            }
            else
            {
                var resolutionList = nextLevel as IEnumerable<MutableObject>;
                foreach (var resolution in resolutionList)
                    foreach (var entry in GetLeafEntriesBelow(resolution, arrityDepth))
                        yield return entry;
            }

            //arrityDepth--;
        }

        public void SetValue( object value, MutableObject targetMutable)
        {
            foreach (var entry in GetLeafEntriesBelow(targetMutable, 0))
            {
                entry[LastKey] = value;
            }

            //SetValue( value, new List< MutableObject >() {targetMutable} );
        }

        public void SetValue( object value, List< MutableObject > targetMutables )
        {
            if ( !targetMutables.Any() )
                return;

            if ( targetMutables.Count() > IntermediateKeys.Count )
            {
                targetMutables[ IntermediateKeys.Count ][ LastKey ] = value;
                return;
            }

            //SetValue(value, targetMutables.Last(), targetMutables.Count()-1);

            foreach ( var entry in GetLeafEntriesBelow( targetMutables.Last(), targetMutables.Count() - 1 ) )
            {
                entry[ LastKey ] = value;
            }
        }

    }
}
