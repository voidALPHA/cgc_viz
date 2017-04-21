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
using Assets.Visualizers;
using Ui.FilamentControl;
using UnityEngine;
using Visualizers.AnnotationIcon;
using Visualizers.CsView;
using Visualizers.DisassemblyWindow;
using Visualizers.FilledGraph;
using Visualizers.IsoGrid;
using Visualizers.LabelController;
using Visualizers.LineGraph;
using Visualizers.MemoryView;
using Visualizers.PrefabController;
using Visualizers.RectangularVolume;
using Visualizers.ScatterPlot;

namespace Visualizers
{
    public class VisualizerFactory : MonoBehaviour
    {
        private static VisualizerFactory m_VisualizerFactoryMaster = null;
        public static VisualizerFactory VisualizerFactoryMaster
        {
            get
            {
                if ( m_VisualizerFactoryMaster == null )
                    m_VisualizerFactoryMaster = FindObjectOfType< VisualizerFactory >();

                return m_VisualizerFactoryMaster;
            }
        }

        private void PostConstructVisualizer(Visualizer visualizer)
        {
            foreach ( var renderer in visualizer.GetComponentsInChildren< Renderer >() )
            {
                var rendererMat = renderer.materials;

                var newMats = new Material[rendererMat.Length];

                for (int i = 0; i < rendererMat.Length; i++)
                    newMats[ i ] = GameObject.Instantiate( rendererMat[ i ] );

                renderer.materials = newMats;
            }

            visualizer.OnVisualizerDestroyed += () => { };
        }

        #region IsoGrid

        [SerializeField]
        private IsoGridBehaviour m_IsoGridPrefab;
        private IsoGridBehaviour IsoGridPrefab { get { return m_IsoGridPrefab; } set { m_IsoGridPrefab = value; } }

        public static IsoGridBehaviour InstantiateIsoGrid()
        {
            var newObject = GameObject.Instantiate(VisualizerFactoryMaster.IsoGridPrefab.gameObject);
            var newGraph = newObject.GetComponent<IsoGridBehaviour>();
            VisualizerFactoryMaster.PostConstructVisualizer(newGraph);
            return newGraph;
        }
        #endregion

        #region Rectangular Volume
        [SerializeField]
        private RectangularVolumeVisualizer m_RectangularVolumePrefab;
        private RectangularVolumeVisualizer RectangularVolumePrefab { get { return m_RectangularVolumePrefab; } set { m_RectangularVolumePrefab = value; } }

        public static RectangularVolumeVisualizer InstantiateRectangularVolume()
        {
            var newObject = GameObject.Instantiate(VisualizerFactoryMaster.RectangularVolumePrefab.gameObject);
            var newVisualizer = newObject.GetComponent<RectangularVolumeVisualizer>();
            VisualizerFactoryMaster.PostConstructVisualizer(newVisualizer);
            return newVisualizer;
        }
        #endregion

        #region Scatter Plot

        [SerializeField]
        private ScatterPlotVisualizer m_ScatterPlotPrefab;
        private ScatterPlotVisualizer ScatterPlotPrefab { get { return m_ScatterPlotPrefab; } set { m_ScatterPlotPrefab = value; } }

        public static ScatterPlotVisualizer InstantiateScatterPlot()
        {
            var newObject = GameObject.Instantiate(VisualizerFactoryMaster.ScatterPlotPrefab.gameObject);
            var newGraph = newObject.GetComponent<ScatterPlotVisualizer>();
            VisualizerFactoryMaster.PostConstructVisualizer(newGraph);
            return newGraph;
        }
        #endregion

        #region Line Graph

        [SerializeField]
        private LineGraphVisualizer m_LineGraphPrefab;
        private LineGraphVisualizer LineGraphPrefab { get { return m_LineGraphPrefab; } set { m_LineGraphPrefab = value; } }

        public static LineGraphVisualizer InstantiateLineGraph()
        {
            var newObject = GameObject.Instantiate(VisualizerFactoryMaster.LineGraphPrefab.gameObject);
            var newGraph = newObject.GetComponent<LineGraphVisualizer>();
            VisualizerFactoryMaster.PostConstructVisualizer(newGraph);
            return newGraph;
        }
        #endregion
        
        #region Filled Graph

        [SerializeField]
        private FilledGraphVisualizer m_FilledGraphPrefab;
        private FilledGraphVisualizer FilledGraphPrefab { get { return m_FilledGraphPrefab; } set { m_FilledGraphPrefab = value; } }

        public static FilledGraphVisualizer InstantiateFilledGraph()
        {
            var newObject = GameObject.Instantiate(VisualizerFactoryMaster.FilledGraphPrefab.gameObject);
            var newGraph = newObject.GetComponent<FilledGraphVisualizer>();
            VisualizerFactoryMaster.PostConstructVisualizer(newGraph);
            return newGraph;
        }
        #endregion

        #region Annotation Icon

        public static AnnotationIcon.AnnotationIcon InstantiateAnnotationIcon(string annotationType)
        {
            var newIcon = AnnotationMappingLord.GenerateAnnotationIcon(annotationType);
            //var newObject = GameObject.Instantiate(VisualizerFactoryMaster.LineGraphPrefab.gameObject);
            //var newObj = newIcon.gameObject;
            VisualizerFactoryMaster.PostConstructVisualizer(newIcon);
            return newIcon;
        }
        #endregion

        #region DisassemblyWindowVisualizer

        [SerializeField]
        private DisassemblyWindowVisualizer m_DisassemblyWindowPrefab = null;
        private DisassemblyWindowVisualizer DisassemblyWindowPrefab { get { return m_DisassemblyWindowPrefab; } }

        public static DisassemblyWindowVisualizer InstantiateDisassemblyWindow()
        {
            var visGo = Instantiate( VisualizerFactoryMaster.DisassemblyWindowPrefab.gameObject );
            var vis = visGo.GetComponent< DisassemblyWindowVisualizer >();
            
            // Not a vis... VisualizerFactoryMaster.PostConstructVisualizer( vis );

            return vis;
        }

        #endregion

        #region CommsItemVisualizer

        [SerializeField]
        private CommsEntryVisualizer m_CommsEntryPrefab = null;
        private CommsEntryVisualizer CommsEntryPrefab { get { return m_CommsEntryPrefab; } }

        public static CommsEntryVisualizer InstantiateCommsItem()
        {
            var visGo = Instantiate( VisualizerFactoryMaster.CommsEntryPrefab.gameObject );
            var vis = visGo.GetComponent<CommsEntryVisualizer>();

            // Not a vis... VisualizerFactoryMaster.PostConstructVisualizer( vis );

            return vis;
        }

        #endregion

        #region DisasmItemVisualizer

        [SerializeField]
        private DisasmEntryVisualizer m_DisasmEntryPrefab = null;
        private DisasmEntryVisualizer DisasmEntryPrefab { get { return m_DisasmEntryPrefab; } }

        public static DisasmEntryVisualizer InstantiateDisasmItem()
        {
            var visGo = Instantiate(VisualizerFactoryMaster.DisasmEntryPrefab.gameObject);
            var vis = visGo.GetComponent<DisasmEntryVisualizer>();
            
            return vis;
        }

        #endregion

        #region Label Visualizer
        [SerializeField]
        private LabelVisualizer m_LabelVisualizerPrefab;
        private LabelVisualizer LabelVisualizerPrefab { get { return m_LabelVisualizerPrefab; } set { m_LabelVisualizerPrefab = value; } }

        public static LabelVisualizer InstantiateLabelVisualizerPrefab()
        {
            var newObject = GameObject.Instantiate(VisualizerFactoryMaster.LabelVisualizerPrefab.gameObject);
            var newVisualizer = newObject.GetComponent<LabelVisualizer>();
            VisualizerFactoryMaster.PostConstructVisualizer(newVisualizer);
            return newVisualizer;
        }
        #endregion

        #region DataShared Tooltip Visualizer

        [SerializeField]
        private DataSharedTooltipVisualizer m_DataSharedTooltipVisualizerPrefab = null;
        private DataSharedTooltipVisualizer DataSharedTooltipVisualizerPrefab { get { return m_DataSharedTooltipVisualizerPrefab; } }

        public static DataSharedTooltipVisualizer InstantiateDataSharedTooltipVisualizer()
        {
            var newObject = GameObject.Instantiate( VisualizerFactoryMaster.DataSharedTooltipVisualizerPrefab.gameObject );
            var newVisualizer = newObject.GetComponent< DataSharedTooltipVisualizer >();
            VisualizerFactoryMaster.PostConstructVisualizer( newVisualizer );
            return newVisualizer;
        }

        #endregion

        #region DisassemblyGroupVisualizer

        // Note: Not strictly a visualizer...

        [SerializeField]
        private DisassemblyGroupVisualizer m_DisassemblyGroupPrefab = null;
        private DisassemblyGroupVisualizer DisassemblyGroupPrefab { get { return m_DisassemblyGroupPrefab; } }

        [SerializeField]
        public Canvas RenderingCanvas;

        public static DisassemblyGroupVisualizer InstantiateDisassemblyGroup()
        {
            var visGo = Instantiate( VisualizerFactoryMaster.DisassemblyGroupPrefab.gameObject, VisualizerFactoryMaster.RenderingCanvas.transform);
            visGo.transform.localScale = Vector3.one;
            visGo.transform.localPosition = Vector3.zero;
            ((RectTransform)visGo.transform).sizeDelta = Vector2.zero;
            var vis = visGo.GetComponent<DisassemblyGroupVisualizer>();

            return vis;
        }

        #endregion

        #region Bound Highlight
        [SerializeField]
        private BoundsHighlightSatellite m_BoundHighlightPrefab;
        private BoundsHighlightSatellite BoundHighlightPrefab
        {
            get { return m_BoundHighlightPrefab; } 
            set { m_BoundHighlightPrefab = value; }}

        public static BoundsHighlightSatellite InstantiateBoundHighlightPrefab(BoundingBox bound)
        {
            var newObject = GameObject.Instantiate(VisualizerFactoryMaster.BoundHighlightPrefab.gameObject);

            
            newObject.transform.parent = bound.transform;
            newObject.transform.localScale = Vector3.one;
            newObject.transform.localRotation = Quaternion.identity;
            newObject.transform.localPosition = Vector3.zero;

            var clickResponder = newObject.GetComponent<BoundsHighlightSatellite>() ??
                                 newObject.AddComponent<BoundsHighlightSatellite>();

            clickResponder.SetUpChildSatellites();
            clickResponder.RelatedBound = bound;
            
            newObject.SetActive(false);

            return clickResponder;
        }
        #endregion

        #region Prefab Visualizer

        public static PrefabVisualizer InstantiatePrefabVisualizer(string prefabType)
        {
            prefabType = prefabType.ToLowerInvariant();

            if (!PrefabFactory.Instance.Prefabs.ContainsKey(prefabType))
            {
                throw new Exception("Cannot create a prefab of type " + prefabType + "!");
            }

            var newPrefab = GameObject.Instantiate(PrefabFactory.Instance.Prefabs[prefabType]);

            var newBound = newPrefab.GetComponent<BoundingBox>();
            if (newBound == null)
                newBound = newPrefab.AddComponent<BoundingBox>();

            //newBound.Data = payload.Data;

            var newVisualizer = newPrefab.GetComponent<PrefabVisualizer>();
            if (newVisualizer == null)
                newVisualizer = newPrefab.AddComponent<PrefabVisualizer>();

            VisualizerFactoryMaster.PostConstructVisualizer(newVisualizer);
            
            return newVisualizer;
        }
        #endregion

        #region Challenge Set Visualizer

        [SerializeField]
        private ChallengeSetVisualizer m_ChallengeSetVisualizerPrefab;
        private ChallengeSetVisualizer ChallengeSetVisualizerPrefab { get { return m_ChallengeSetVisualizerPrefab; } set { m_ChallengeSetVisualizerPrefab = value; } }

        public static ChallengeSetVisualizer InstantiateChallengeSetVisualizer()
        {
            var newObject = Instantiate(VisualizerFactoryMaster.ChallengeSetVisualizerPrefab.gameObject);
            var newGraph = newObject.GetComponent<ChallengeSetVisualizer>();
            VisualizerFactoryMaster.PostConstructVisualizer(newGraph);
            return newGraph;
        }

        [SerializeField]
        private ComponentAssembler m_ChallengeSetAssembler;
        private ComponentAssembler ChallengeSetAssembler { get { return m_ChallengeSetAssembler; } }
        public static ComponentAssembler GetChallengeSetAssembler { get { return VisualizerFactoryMaster.ChallengeSetAssembler; } }
        #endregion


        #region Label Visualizer
        [SerializeField]
        private MemoryViewVisualizer m_MemoryVisualizerPrefab;
        private MemoryViewVisualizer MemoryVisualizerPrefab { get { return m_MemoryVisualizerPrefab; } set { m_MemoryVisualizerPrefab = value; } }

        public static MemoryViewVisualizer InstantiateMemoryGraph()
        {
            var newObject = GameObject.Instantiate(VisualizerFactoryMaster.MemoryVisualizerPrefab.gameObject);
            var newVisualizer = newObject.GetComponent<MemoryViewVisualizer>();
            VisualizerFactoryMaster.PostConstructVisualizer(newVisualizer);
            return newVisualizer;
        }

        #endregion
    }
}
