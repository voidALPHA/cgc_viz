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
using Chains;
using Mutation;
using UnityEngine;
using Visualizers;
using Visualizers.IsoGrid;

namespace Ui.FilamentControl
{
    public class DataSharedTooltipController : VisualizerController
    {
        private MutableField<string> m_GroupId = new MutableField<string>() 
        { LiteralValue = "Group Id" };
        [Controllable(LabelText = "Group Id")]
        public MutableField<string> GroupId { get { return m_GroupId; } }

        private MutableField<string> m_Tooltip = new MutableField<string>() 
        { LiteralValue = "Tooltip" };
        [Controllable(LabelText = "Tooltip Text")]
        public MutableField<string> Tooltip { get { return m_Tooltip; } }

        private MutableField<Vector3> m_ScreenPosition = new MutableField<Vector3>() 
        { AbsoluteKey = "Screen Position"};
        [Controllable(LabelText = "Screen Position")]
        public MutableField<Vector3> ScreenPosition { get { return m_ScreenPosition; } }
        
        private MutableField<bool> m_ShowTooltip = new MutableField<bool>() 
        { LiteralValue = true };
        [Controllable(LabelText = "Show Tooltip")]
        public MutableField<bool> ShowTooltip { get { return m_ShowTooltip; } }
        
        private MutableField<int> m_FontSize = new MutableField<int>() 
        { LiteralValue = 18 };
        [Controllable(LabelText = "Font Size")]
        public MutableField<int> FontSize { get { return m_FontSize; } }

        private MutableField<Color> m_FontColor = new MutableField<Color>() 
        { LiteralValue = Color.black };
        [Controllable(LabelText = "Text Color")]
        public MutableField<Color> FontColor { get { return m_FontColor; } }

        private MutableField<Color> m_BackgroundColor = new MutableField<Color>() 
        { LiteralValue = Color.gray };
        [Controllable(LabelText = "Background Color")]
        public MutableField<Color> BackgroundColor { get { return m_BackgroundColor; } }
        

        private static NodeDataShare<DataSharedTooltipVisualizer> m_DataShare = new NodeDataShare< DataSharedTooltipVisualizer >();
        private static NodeDataShare<DataSharedTooltipVisualizer> DataShare {
            get { return m_DataShare; }
            set{m_DataShare = value;}
        }

        protected override IEnumerator ProcessPayload( VisualPayload payload )
        {
            var groupId = GroupId.GetFirstValue( payload.Data );

            DataSharedTooltipVisualizer newVisualizer;

            if ( !DataShare.ContainsKey( groupId ) )
            {
                newVisualizer = VisualizerFactory.InstantiateDataSharedTooltipVisualizer();

                newVisualizer.Initialize( this, payload );

                DataShare[ groupId ] = newVisualizer;
            }
            else
                newVisualizer = DataShare[ groupId ];


            newVisualizer.TextColor = FontColor.GetFirstValue(payload.Data);
            newVisualizer.FontSize = FontSize.GetFirstValue(payload.Data);
            newVisualizer.BackgroundColor = BackgroundColor.GetFirstValue(payload.Data);
            newVisualizer.CheckRedeclareStyle();
            

            newVisualizer.DisplayText = 
                ShowTooltip.GetFirstValue( payload.Data )?
                Tooltip.GetFirstValue( payload.Data ):"";

            newVisualizer.DrawPosition = ScreenPosition.GetFirstValue( payload.Data );

            var iterator = Router.TransmitAll( payload );
            while ( iterator.MoveNext() )
                yield return null;
        }

        public override void Unload()
        {
            DataShare.Clear();
        }
    }
}
