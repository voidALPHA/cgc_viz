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
using JetBrains.Annotations;
using UnityEngine;

namespace ChainViews
{
    public class PendingChangesDialogBehaviour : MonoBehaviour
    {
        private Action OnDiscard { get; set; }

        private Action OnCancel { get; set; }

        [UsedImplicitly]
        public void HandleDiscardPressed()
        {
            if ( OnDiscard != null )
                OnDiscard();

            Hide();
        }

        [UsedImplicitly]
        public void HandleCancelPressed()
        {
            if ( OnCancel != null )
                OnCancel();

            Hide();
        }

        public void Show( Action onDiscard, Action onCancel )
        {
            OnDiscard = onDiscard;

            OnCancel = onCancel;

            Show();
        }

        private void Show()
        {
            gameObject.SetActive( true );
        }

        private void Hide()
        {
            gameObject.SetActive( false );
        }
    }
}
