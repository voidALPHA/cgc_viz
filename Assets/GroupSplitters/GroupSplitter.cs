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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chains;
using JetBrains.Annotations;
using Mutation;
using Newtonsoft.Json;
using Visualizers;

namespace GroupSplitters
{
    public abstract class GroupSplitter : ChainNode
    {
        [JsonProperty]
        public string EntryFieldName { set { SelectedListTarget.AbsoluteKey = value; } }

        private MutableScope m_Scope = new MutableScope() {AbsoluteKey = ""};
        [Controllable(LabelText = "Scope", Order=-3)]
        public MutableScope Scope { get { return m_Scope; } }

        private MutableScope m_EntryField = new MutableScope();
        [Controllable(LabelText = "Selectable Entries", Order = -2)]
        public MutableScope EntryField { get { return m_EntryField; } }
        
        protected virtual string SelectedName { get { return "Selected"; } }
        protected virtual string UnSelectedName { get { return "Unselected"; } }

        private SelectionState SelectedSet { get { return Router[ SelectedName ]; } }
        private SelectionState UnSelectedSet { get { return Router[ UnSelectedName ]; } }

        protected List<MutableObject> SelectedList { get; set; }
        protected List<MutableObject> UnSelectedList { get; set; }



        private MutableField<bool> m_NewPayloadOnly = new MutableField<bool>()
        { LiteralValue = true };
        [Controllable(LabelText = "New Payload Only?")]
        public MutableField<bool> NewPayloadOnly { get { return m_NewPayloadOnly; } }

        private MutableTarget m_SelectedListTarget = new MutableTarget() 
        { AbsoluteKey = "Entries" };
        [Controllable(LabelText = "Selected List Target")]
        public MutableTarget SelectedListTarget { get { return m_SelectedListTarget; } }

        private MutableTarget m_UnSelectedListTarget = new MutableTarget()
        { AbsoluteKey = "Unselected Entries" };
        [Controllable(LabelText = "UnSelected List Target")]
        public MutableTarget UnSelectedListTarget { get { return m_UnSelectedListTarget; } }


        private MutableField<bool> m_AllowEmptyLists = new MutableField<bool>() 
        { LiteralValue = false };
        [Controllable(LabelText = "Allow Empty Lists")]
        public MutableField<bool> AllowEmptyLists { get { return m_AllowEmptyLists; } }



        protected GroupSplitter()
        {
            EntryField.SchemaParent = Scope;
            SelectedListTarget.SchemaParent = Scope;
            UnSelectedListTarget.SchemaParent = Scope;


            Router.AddSelectionState( SelectedName );
            Router.AddSelectionState( UnSelectedName );

            SelectedList = new List<MutableObject>();
            UnSelectedList = new List<MutableObject>();
        }

        protected abstract void SelectGroups(List<MutableObject> entry);
        
        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            foreach ( var entry in Scope.GetEntries( payload.Data ) )
            {
                SelectGroups( entry );

                //var firstEntry = EntryField.GetEntries(payload.Data).First().Last();
                //
                //if (firstEntry != null)
                //    firstEntry = firstEntry.CloneKeys();
                //else
                //    firstEntry = new MutableObject();
                //
                //if (!firstEntry.ContainsKey(ImplicitSchemaIndicator.KeyName))
                //    firstEntry.Add(ImplicitSchemaIndicator.KeyName, new ImplicitSchemaIndicator(this));
                //
                //
                //DenoteEmptySelectedList(payload.Data, firstEntry);
                //DenoteEmptyUnSelectedList(payload.Data, firstEntry);

                if ( NewPayloadOnly.GetFirstValue( payload.Data ) )
                {
                    if ( AllowEmptyLists.GetFirstValue( payload.Data ) || SelectedList.Any() )
                    {
                        var iterator = SelectedSet.Transmit(
                            new VisualPayload( new MutableObject
                            {
                                { SelectedListTarget.LastKey, SelectedList }
                            }, payload.VisualData ) );
                        while ( iterator.MoveNext() )
                            yield return null;
                    }

                    if (AllowEmptyLists.GetFirstValue(payload.Data) || UnSelectedList.Any() )
                    {
                        var iterator = UnSelectedSet.Transmit(
                            new VisualPayload( new MutableObject
                            {
                                { UnSelectedListTarget.LastKey, UnSelectedList }
                            }, payload.VisualData ) );
                        while ( iterator.MoveNext() )
                            yield return null;
                    }
                }
                else
                {
                    SelectedListTarget.SetValue( SelectedList, entry );
                    UnSelectedListTarget.SetValue( UnSelectedList, entry );

                }

            }

            if ( !NewPayloadOnly.GetFirstValue( payload.Data ) )
            {
                if (AllowEmptyLists.GetFirstValue(payload.Data) || SelectedList.Any() )
                {
                    var iterator = SelectedSet.Transmit( payload );
                    while ( iterator.MoveNext() )
                        yield return null;
                }

                if (AllowEmptyLists.GetFirstValue(payload.Data) || UnSelectedList.Any() )
                {
                    var iterator = UnSelectedSet.Transmit( payload );
                    while ( iterator.MoveNext() )
                        yield return null;
                }
            }
        }

        protected override void OnProcessOutputSchema( MutableObject newSchema )
        {
            foreach ( var entry in Scope.GetEntries( newSchema ) )
            {
                SelectGroups( entry );

                var firstEntry = EntryField.GetEntries( entry ).First().Last();

                if ( firstEntry != null )
                    firstEntry = firstEntry.CloneKeys();
                else
                    firstEntry = new MutableObject();

                if ( !firstEntry.ContainsKey( ImplicitSchemaIndicator.KeyName ) )
                    firstEntry.Add( ImplicitSchemaIndicator.KeyName, new ImplicitSchemaIndicator( this ) );

                DenoteEmptySelectedList( newSchema, firstEntry );
                DenoteEmptyUnSelectedList( newSchema, firstEntry );

                if ( !NewPayloadOnly.GetFirstValue( newSchema ) )
                {
                    SelectedListTarget.SetValue( SelectedList, entry );
                    UnSelectedListTarget.SetValue( UnSelectedList, entry );
                }
            }

            if ( !NewPayloadOnly.GetFirstValue( newSchema ) )
            {
                SelectedSet.TransmitSchema( newSchema );

                UnSelectedSet.TransmitSchema( newSchema );
            }
            else
            {
                SelectedSet.TransmitSchema( new MutableObject
                {
                    { SelectedListTarget.LastKey, SelectedList }
                } );
                UnSelectedSet.TransmitSchema( new MutableObject
                {
                    { UnSelectedListTarget.LastKey, UnSelectedList }
                } );
            }
        }

        protected virtual void DenoteEmptySelectedList( MutableObject mutable, MutableObject defaultObject)
        {
            if (!SelectedList.Any())
                SelectedList.Add( defaultObject );
        }

        protected virtual void DenoteEmptyUnSelectedList( MutableObject mutable, MutableObject defaultObject )
        {
            if (!UnSelectedList.Any())
                UnSelectedList.Add( defaultObject );
        }

        [UsedImplicitly]
        public class ImplicitSchemaIndicator
        {
            public static string KeyName { get { return "**Implicit Schema**"; } }

            public ChainNode Splitter { get; set; }

            public ImplicitSchemaIndicator( ChainNode splitter )
            {
                Splitter = splitter;
            }

            public override string ToString()
            {
                return string.Format( "Implicit Schema added by {0}. JSON ID* of {1}", Splitter.GetType().Name, Splitter.JsonId );
            }
        }
    }

    
}
