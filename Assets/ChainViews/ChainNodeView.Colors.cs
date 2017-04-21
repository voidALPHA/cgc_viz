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
using Adapters;
using Adapters.GlobalParameters;
using Chains;
using GroupSplitters;
using JetBrains.Annotations;
using Kinetics;
using Mutation.Mutators;
using Mutation.Mutators.Axes;
using Mutation.Mutators.BoundManipulationMutators;
using Mutation.Mutators.Enumeration;
using Mutation.Mutators.IfMutator;
using Mutation.Mutators.MutableDataManipulation;
using Mutation.Mutators.SocketConnection;
using Mutation.Mutators.Switches;
using Mutation.Mutators.VisualModifiers;
using Sequencers;
using UnityEngine;
using Visualizers.IsoGrid;

namespace ChainViews
{
    public partial class ChainNodeView
    {
        [Serializable]
        public class TypeColorPair
        {
            [SerializeField]
            private List<Type> m_Type = new List< Type > { typeof( object )};
            public List<Type> Type
            {
                get { return m_Type; }
                set { m_Type = value; }
            }

            [SerializeField]
            private Color m_Color = Color.magenta;
            public Color Color
            {
                get { return m_Color; }
                set { m_Color = value; }
            }
        }

        [Serializable]
        private class ColorMapDefinition
        {
            // TO ADD A CHAIN NODE COLOR:
            //
            // Add its corresponding property here, optionally modify its color on prefab in inspector...
            //
            // DERIVED TYPES MUST COME LATER IN THE LIST

            #pragma warning disable 0414

            [SerializeField, UsedImplicitly]
            private TypeColorPair m_Adapter = new TypeColorPair
            {
                Type = new List < Type >{ typeof( Adapter )},
                Color = Color.blue
            };

            [SerializeField, UsedImplicitly]
            private TypeColorPair m_Mutator = new TypeColorPair
            {
                Type = new List<Type>{ typeof( Mutator )},
                Color = Color.red
            };

            [SerializeField, UsedImplicitly]
            private TypeColorPair m_AxisMutator = new TypeColorPair
            {
                Type = new List<Type>{ typeof( AxisMutator )},
                Color = Color.gray
            };

            [SerializeField, UsedImplicitly]
            private TypeColorPair m_Splitter = new TypeColorPair
            {
                Type = new List<Type>{ typeof( GroupSplitter ), typeof(SelectedOnlySplitter)},
                Color = Color.yellow
            };



            [SerializeField, UsedImplicitly]
            private TypeColorPair m_Iterator = new TypeColorPair
            {
                Type = new List<Type>{ typeof(EnumeratorIterator),
                    typeof(BoundsEnumerator),
                    typeof(ExecutionToTraceIterator),
                    typeof(EnumeratorDecombiner),
                    typeof(EnumeratorRecombiner),
                    typeof(CsvEnumerator),
                    typeof(EnumeratorRecombineObjects),
                    typeof(IntCsvOrRangeEnumerator)},
                Color = Color.cyan
            };

            [SerializeField, UsedImplicitly]
            private TypeColorPair m_Controller = new TypeColorPair
            {
                Type = new List<Type>{ typeof( VisualizerController )},
                Color = Color.green
            };

            [SerializeField, UsedImplicitly]
            private TypeColorPair m_WriteParameterNode = new TypeColorPair
            {
                Type = new List<Type>{ typeof(WriteGlobalValueNode)},
                Color = Color.green
            };

            [SerializeField, UsedImplicitly]
            private TypeColorPair m_KineticNode = new TypeColorPair
            {
                Type = new List<Type>{ typeof(KineticNode)},
                Color = Color.green
            };

            [SerializeField, UsedImplicitly]
            private TypeColorPair m_SequencerNode = new TypeColorPair
            {
                Type = new List<Type>{ typeof(SequencerNode)},
                Color = Color.green
            };

            [SerializeField, UsedImplicitly]
            private TypeColorPair m_IfNode = new TypeColorPair
            {
                Type = new List<Type>{ typeof(IfMutator)},
                Color = Color.green
            };

            [SerializeField, UsedImplicitly]
            private TypeColorPair m_DestroyBound = new TypeColorPair
            {
                Type = new List<Type>{ typeof(DestroyBoundNode)},
                Color = Color.red
            };
            
            

            [SerializeField, UsedImplicitly]
            private TypeColorPair m_SwitchMutator = new TypeColorPair
            {
                Type = new List<Type>{ typeof( SwitchMutator )},
                Color = Color.yellow
            };
            

            [SerializeField, UsedImplicitly]
            private TypeColorPair m_RemoveData = new TypeColorPair
            {
                Type = new List<Type> { typeof(RemoveDataMutator), typeof(RemoveSchemaMutator) },
                Color = Color.red
            };

            [SerializeField, UsedImplicitly]
            private TypeColorPair m_TextureToMaterial = new TypeColorPair
            {
                Type = new List<Type> { typeof(TextureToMaterialMutator)},
                Color = Color.red
            };


            [SerializeField, UsedImplicitly]
            private TypeColorPair m_NetworkingNodes= new TypeColorPair
            {
                Type = new List<Type> { typeof(OpenSocketAdapter), typeof(ConnectToDataSharedSocketMutator), typeof(SendStringThroughDataSharedSocketMutator) },
                Color = Color.yellow
            };


            //#pragma warning restore 0414

            //
            // Here down need not be modified for new types...
            //

            private static List< TypeColorPair > s_TypeColorPairs;
            private List< TypeColorPair > TypeColorPairs
            {
                get
                {
                    if ( s_TypeColorPairs == null )
                    {
                        s_TypeColorPairs = GetType()
                                .GetFields( BindingFlags.NonPublic | BindingFlags.Instance )
                                .Where( p => typeof( TypeColorPair ).IsAssignableFrom( p.FieldType ) )
                                .Select( p => p.GetValue( this ) ).Cast< TypeColorPair >().ToList();
                    }

                    return s_TypeColorPairs;
                }
            }


            public Color Resolve( Type type )
            {
                var pair = TypeColorPairs.LastOrDefault( 
                    tcp => tcp.Type.Any( subtype=>subtype.IsAssignableFrom(type) ));

                if ( pair == null )
                    return Color.magenta;

                return pair.Color;
            }
        }
    }
}
