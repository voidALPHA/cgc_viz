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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mutation;
using UnityEngine;

namespace Visualizers.MetaSelectors
{
    public interface IMetaSelectable
    {
        Transform GetTransform();

        List<VisualPayload> SelectablePayloads { get; }

        IEnumerator Select(VisualPayload payload);
        IEnumerator Select(IEnumerable<VisualPayload> payloads);
        IEnumerator Deselect(VisualPayload payload);
        IEnumerator Deselect(IEnumerable<VisualPayload> payloads);
        IEnumerator SelectOnly(IEnumerable<VisualPayload> payloads);
        IEnumerator ToggleFullySelected(IEnumerable<VisualPayload> payloads);

        IEnumerator TransmitAll();
        IEnumerator SelectAll();
        IEnumerator DeselectAll();

    }

    public class MetaSelectionMode
    {
        public IMetaSelectable Selectable { get; set; }

        public MetaSelectionMode(SelectionOperation operationToPerform = SelectionOperation.SelectOnly)
        {
            OperationToPerform = operationToPerform;
        }

        public SelectionOperation OperationToPerform { get; set; }



        public static IEnumerator ApplySelectionOperation( SelectionOperation operation, IMetaSelectable selectable,
            IEnumerable< VisualPayload > selected)
        {
            var selectMode = new MetaSelectionMode( operation ) 
                {Selectable = selectable};

            var iterator = selectMode.ApplyMode( selected );
            while ( iterator.MoveNext() )
                yield return null;
        }

        public IEnumerator ApplyMode(IEnumerable<VisualPayload> payloads)
        {
            switch (OperationToPerform)
            {
                case SelectionOperation.Select:
                {
                    var iterator = Selectable.Select(payloads);
                    while (iterator.MoveNext( ))
                        yield return null;
                    break;
                }
                case SelectionOperation.Deselect:
                {
                    var iterator = Selectable.Deselect(payloads);
                    while (iterator.MoveNext())
                        yield return null;
                    break;
                }
                case SelectionOperation.SelectOnly:
                {
                    var iterator = Selectable.SelectOnly(payloads);
                    while (iterator.MoveNext())
                        yield return null;
                    break;
                }
                case SelectionOperation.ToggleFullySelected:
                {
                    var iterator = Selectable.ToggleFullySelected(payloads);
                    while (iterator.MoveNext())
                        yield return null;
                    break;
                }
                case SelectionOperation.DoNothing:
                {
                    yield return null;
                    break;
                }
                default:
                {
                    throw new NotImplementedException("Selection mode not implemented");
                }
            }
        }
    }
}
